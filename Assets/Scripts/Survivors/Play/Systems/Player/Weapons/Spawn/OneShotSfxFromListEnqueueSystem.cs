using Latios;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Player.Weapons.Spawn
{
    [BurstCompile]
    public partial struct OneShotSfxFromListEnqueueSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged m_worldUnmanaged;
        EntityQuery          m_entityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_entityQuery = state.Fluent()
                .With<WorldTransform>()
                .With<OneShotSfxSpawnerRef>()
                .WithEnabled<SfxTriggeredTag>()
                .Build();
        }

        public void OnNewScene(ref SystemState state)
        {
            state.InitSystemRng("WeaponSfxSpawnSystem");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new SfxEnqueueJob
            {
                SfxQueue = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<SfxSpawnQueue>().SfxQueue
                    .AsParallelWriter(),
                CommandBuffer    = m_worldUnmanaged.syncPoint.CreateEntityCommandBuffer().AsParallelWriter(),
                SpawnerLookup    = SystemAPI.GetComponentLookup<OneShotSfxSpawner>(true),
                SfxPrefabsLookup = SystemAPI.GetBufferLookup<OneShotSfxElement>(true),
                Rng              = state.GetJobRng()
            }.ScheduleParallel(m_entityQuery, state.Dependency);

            state.Dependency.Complete();
        }


        [BurstCompile]
        partial struct SfxEnqueueJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            [NativeDisableParallelForRestriction]
            public NativeQueue<SfxSpawnQueue.SfxSpawnData>.ParallelWriter SfxQueue;

            [ReadOnly] public ComponentLookup<OneShotSfxSpawner> SpawnerLookup;
            [ReadOnly] public BufferLookup<OneShotSfxElement>    SfxPrefabsLookup;
            public            EntityCommandBuffer.ParallelWriter CommandBuffer;

            public SystemRng Rng;

            void Execute(Entity entity, [EntityIndexInQuery] int index,
                in WorldTransform worldTransform,
                in OneShotSfxSpawnerRef sfxSpawnerRef)
            {
                var sfxSpawner = SpawnerLookup[sfxSpawnerRef.SfxPrefab];
                var sfxPrefabs = SfxPrefabsLookup[sfxSpawnerRef.SfxPrefab];

                var sfxPrefab = sfxPrefabs[Rng.NextInt(0, sfxSpawner.SfxCount)].Prefab;

                SfxQueue.Enqueue(new SfxSpawnQueue.SfxSpawnData
                {
                    SfxPrefab = sfxPrefab,
                    Position  = worldTransform.position,
                    EventType = sfxSpawnerRef.EventType
                });

                CommandBuffer.SetComponentEnabled<SfxTriggeredTag>(index, entity, false);
            }


            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                Rng.BeginChunk(unfilteredChunkIndex);
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask,
                bool chunkWasExecuted) { }
        }
    }
}