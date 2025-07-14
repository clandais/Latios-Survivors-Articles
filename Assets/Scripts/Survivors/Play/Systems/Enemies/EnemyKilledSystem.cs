using Latios;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Enemies
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct EnemyKilledSystem : ISystem, ISystemNewScene
    {
        EntityQuery          _query;
        LatiosWorldUnmanaged _world;
        Rng                  m_rng;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _query = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<EnemyTag>()
                .With<DeadTag>()
                .With<Collider>()
                .With<XpDropPrefab>()
                .With<HpDropPrefab>()
                .With<ItemDropChance>()
                .Build();

            _world = state.GetLatiosWorldUnmanaged();
        }

        public void OnNewScene(ref SystemState state)
        {
            state.InitSystemRng("DisableDeadCollidersSystem");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var rcb = _world.syncPoint.CreateEntityCommandBuffer();

            var xpSpawnQueue = _world.sceneBlackboardEntity.GetCollectionComponent<CollectibleSpawnQueue>()
                .XpQueue;

            state.Dependency = new RemoveCollidersJob
            {
                CommandBuffer = rcb.AsParallelWriter(),
                XpSpawnQueue  = xpSpawnQueue.AsParallelWriter(),
                Rng           = state.GetJobRng(),
            }.ScheduleParallel(_query, state.Dependency);
        }

        [BurstCompile]
        partial struct RemoveCollidersJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public NativeQueue<CollectibleSpawnQueue.CollectibleSpawnData>.ParallelWriter XpSpawnQueue;
            public EntityCommandBuffer.ParallelWriter                   CommandBuffer;
            public SystemRng                                        Rng;

            void Execute(Entity entity,
                [EntityIndexInQuery] int index,
                TransformAspect transform,
                in XpDropPrefab xpDropPrefab,
                in HpDropPrefab hpDropPrefab,
                in ItemDropChance itemDropChance)
            {
                CommandBuffer.RemoveComponent<Collider>(index, entity);


                var chance = Rng.NextInt(0, (itemDropChance.HpDropChance + itemDropChance.XpDropChance));
                var xpChance = itemDropChance.XpDropChance;
                var hpChance = itemDropChance.HpDropChance;
                
                
                var prefab = chance < hpChance
                    ? hpDropPrefab.Prefab
                    : chance < (xpChance + hpChance)
                        ? xpDropPrefab.Prefab
                        : Entity.Null;
                
                
                XpSpawnQueue.Enqueue(new CollectibleSpawnQueue.CollectibleSpawnData
                {
                    Position = transform.worldPosition,
                    Prefab = prefab,
                });
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                Rng.BeginChunk(unfilteredChunkIndex);
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask,
                bool chunkWasExecuted)
            {
            }
        }
    }
}