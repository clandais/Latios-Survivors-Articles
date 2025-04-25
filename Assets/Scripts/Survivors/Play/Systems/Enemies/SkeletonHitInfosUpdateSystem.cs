using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring.Enemies.SFX;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Enemies
{
    public partial struct SkeletonHitInfosUpdateSystem : ISystem, ISystemNewScene
    {
        EntityQuery          m_query;
        LatiosWorldUnmanaged m_world;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_query = state.Fluent()
                .With<WorldTransform>()
                .With<EnemyTag>()
                .WithEnabled<HitInfos>()
                .With<SkeletonHitSfxBufferElement>()
                .Build();

            m_world = state.GetLatiosWorldUnmanaged();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var sfxQueue = m_world.sceneBlackboardEntity.GetCollectionComponent<SfxSpawnQueue>()
                .SfxQueue;

            state.Dependency = new HitInfosUpdateJob
            {
                Rng            = state.GetJobRng(),
                SfxQueue       = sfxQueue.AsParallelWriter(),
                HitInfosLookup = SystemAPI.GetComponentLookup<HitInfos>()
            }.Schedule(m_query, state.Dependency);
        }


        public void OnNewScene(ref SystemState state)
        {
            state.InitSystemRng("SkeletonHitInfosUpdateSystem");
        }

        [BurstCompile]
        partial struct HitInfosUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public SystemRng Rng;
            public NativeQueue<SfxSpawnQueue.SfxSpawnData>.ParallelWriter SfxQueue;
            [NativeDisableParallelForRestriction] public ComponentLookup<HitInfos> HitInfosLookup;

            void Execute(
                Entity entity,
                [EntityIndexInQuery] int index,
                in WorldTransform worldTransform,
                in DynamicBuffer<SkeletonHitSfxBufferElement> sfxBuffer)
            {
                var prefabIndex = Rng.NextInt(0, sfxBuffer.Length);
                SfxQueue.Enqueue(new SfxSpawnQueue.SfxSpawnData
                {
                    SfxPrefab = sfxBuffer[prefabIndex].SfxPrefab,
                    Position  = worldTransform.position
                });

                // TODO: Add logic to handle HitInfos component
                
                HitInfosLookup.SetComponentEnabled(entity, false);
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