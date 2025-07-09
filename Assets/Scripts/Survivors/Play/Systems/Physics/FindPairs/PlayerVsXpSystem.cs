using Latios;
using Latios.Psyshock;
using Survivors.Play.Authoring.Collectibles;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Play.Systems.Physics.FindPairs
{
    [RequireMatchingQueriesForUpdate]
    public partial struct PlayerVsXpSystem : ISystem
    {
        BuildCollisionLayerTypeHandles m_typeHandles;
        LatiosWorldUnmanaged m_world;
        EntityQuery m_xpQuery;
        EntityQuery m_playerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world       = state.GetLatiosWorldUnmanaged();
            m_typeHandles = new BuildCollisionLayerTypeHandles(ref state);

            m_xpQuery = state.Fluent()
                .With<XpItem>(true)
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
            var xpLayerJh = Latios.Psyshock.Physics.BuildCollisionLayer(m_xpQuery, in m_typeHandles)
                .WithSettings(physicsSettings.CollisionLayerSettings)
                .ScheduleParallel(out var xpLayer, state.WorldUpdateAllocator, state.Dependency);


            // state.Dependency = JobHandle.CombineDependencies(playerLayerJh, xpLayerJh);
            //
            // // var dcb = m_world.syncPoint.CreateDestroyCommandBuffer();
            // // var destroyList = new NativeList<Entity>(Allocator.TempJob);
            // //

            var acb = m_world.syncPoint.CreateEntityCommandBuffer();
            
            var playerVsXpFindPairs = new PlayerVsXpFindPairs
            {
                XpCubeVfxLookup = SystemAPI.GetComponentLookup<XpCubeVfx>(),
                VfxQueue = m_world.sceneBlackboardEntity.GetCollectionComponent<VfxSpawnQueue>()
                    .VfxQueue.AsParallelWriter(),
                CommandBuffer = acb.AsParallelWriter(),
            };
            
            state.Dependency = Latios.Psyshock.Physics
                .FindPairs(playerCollisionLayer, xpLayer, playerVsXpFindPairs)
                .ScheduleParallel( xpLayerJh);

            state.Dependency = xpLayer.Dispose(state.Dependency);
            

            // state.Dependency = new DestroyJob
            // {
            //     EntitiesToDestroy = destroyList,
            //     CommandBuffer     = dcb.AsParallelWriter()
            // }.Schedule(destroyList.Length, 128, findPairJh);
            //
            // destroyList.Dispose(state.Dependency);
        }


        struct PlayerVsXpFindPairs : IFindPairsProcessor
        {
            public PhysicsComponentLookup<XpCubeVfx> XpCubeVfxLookup;
            public NativeQueue<VfxSpawnQueue.VfxSpawnData>.ParallelWriter VfxQueue;
            public EntityCommandBuffer.ParallelWriter CommandBuffer;
            public void Execute(in FindPairsResult result)
            {
                // var playerEntity = result.entityA;
                var xpEntity = result.entityB;
                var xpVfx = XpCubeVfxLookup.GetRW(xpEntity).ValueRO;

                VfxQueue.Enqueue(new VfxSpawnQueue.VfxSpawnData
                {
                    Position  = result.transformB.position,
                    VfxPrefab = xpVfx.Prefab
                });
                
                CommandBuffer.AddComponent<ShouldDestroyTag>(result.bodyIndexB, result.entityB);
            }
        }

        [BurstCompile]
        struct DestroyJob : IJobParallelFor
        {
            [ReadOnly] public NativeList<Entity> EntitiesToDestroy;
            public DestroyCommandBuffer.ParallelWriter CommandBuffer;

            public void Execute(int index)
            {
                var entity = EntitiesToDestroy[index];
                if (entity != Entity.Null)
                {
                    CommandBuffer.Add(entity, index);
                }
            }
        }
    }
}
