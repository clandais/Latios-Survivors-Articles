using Latios;
using Latios.Psyshock;
using Survivors.Play.Authoring.Collectibles;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Physics.FindPairs
{
    [RequireMatchingQueriesForUpdate]
    public partial struct PlayerVsHpSystem : ISystem
    {
        BuildCollisionLayerTypeHandles m_typeHandles;
        LatiosWorldUnmanaged           m_world;
        EntityQuery                    m_hpQuery;
        EntityQuery                    m_playerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world       = state.GetLatiosWorldUnmanaged();
            m_typeHandles = new BuildCollisionLayerTypeHandles(ref state);
            

            m_hpQuery = state.Fluent()
                .With<HpItem>(true)
                .PatchQueryForBuildingCollisionLayer()
                .Build();
            
            m_playerQuery = state.Fluent()
                .With<PlayerTag>()
                .Without<DeadTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_typeHandles.Update(ref state);
            
            if (!m_world.GetPhysicsSettings(out var physicsSettings))
                return;
            
            var playerCollisionLayer = m_world.sceneBlackboardEntity.GetCollectionComponent<PlayerCollisionLayer>()
                .Layer;
            
            var hpLayerJh = Latios.Psyshock.Physics.BuildCollisionLayer(m_hpQuery, in m_typeHandles)
                .WithSettings(physicsSettings.CollisionLayerSettings)
                .ScheduleParallel(out var hpLayer, state.WorldUpdateAllocator, state.Dependency);

            var acb = m_world.syncPoint.CreateEntityCommandBuffer();
            
            var hpQueue = m_world.sceneBlackboardEntity.GetCollectionComponent<PlayerHpQueue>()
                .HpQueue;
            
            var playerVsHpFindPairs = new PlayerVsHpFindPairs
            {
                HpItemLookup    = SystemAPI.GetComponentLookup<HpItem>(),
                HpCubeVfxLookup = SystemAPI.GetComponentLookup<HpSphereVfx>(),
                VfxQueue = m_world.sceneBlackboardEntity.GetCollectionComponent<VfxSpawnQueue>()
                    .VfxQueue.AsParallelWriter(),
                CommandBuffer = acb.AsParallelWriter(),
                HpQueue       = hpQueue.AsParallelWriter()
            };
            
            state.Dependency = Latios.Psyshock.Physics
                .FindPairs(playerCollisionLayer, hpLayer, playerVsHpFindPairs)
                .ScheduleParallel( hpLayerJh);
            
            state.Dependency = hpLayer.Dispose(state.Dependency);
        }
        
        
        struct PlayerVsHpFindPairs : IFindPairsProcessor
        {
            public PhysicsComponentLookup<HpItem>                         HpItemLookup;
            public PhysicsComponentLookup<HpSphereVfx>                    HpCubeVfxLookup;
            public NativeQueue<VfxSpawnQueue.VfxSpawnData>.ParallelWriter VfxQueue;
            public EntityCommandBuffer.ParallelWriter                     CommandBuffer;
            public NativeQueue<int>.ParallelWriter                        HpQueue;
            
            public void Execute(in FindPairsResult result)
            {
                // var playerEntity = result.entityA;
                var hpEntity = result.entityB;
                var hpItem = HpItemLookup.GetRW(hpEntity).ValueRO;
                var hpVfx = HpCubeVfxLookup.GetRW(hpEntity).ValueRO;

                VfxQueue.Enqueue(new VfxSpawnQueue.VfxSpawnData
                {
                    Position  = result.transformB.position,
                    VfxPrefab = hpVfx.Prefab
                });
                
                HpQueue.Enqueue(hpItem.Value);

                CommandBuffer.AddComponent<ShouldDestroyTag>(result.bodyIndexB, result.entityB);
            }
        }
    }
}