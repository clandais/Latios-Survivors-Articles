using Latios;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Play.Systems.SFX
{
    [BurstCompile]
    public partial struct SfxSpawnQueueSystem : ISystem
    {
        LatiosWorldUnmanaged m_worldUnmanaged;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var sfxQueue = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<SfxSpawnQueue>().SfxQueue;

            if (sfxQueue.IsEmpty()) return;

            var icb = m_worldUnmanaged.syncPoint.CreateInstantiateCommandBuffer<WorldTransform>();

            state.Dependency = new SfxSpawnJob
            {
                SfxQueue         = sfxQueue,
                SpawnQueueWriter = icb.AsParallelWriter()
            }.Schedule(state.Dependency);
        }


        [BurstCompile]
        struct SfxSpawnJob : IJob
        {
            public NativeQueue<SfxSpawnQueue.SfxSpawnData> SfxQueue;
            public InstantiateCommandBuffer<WorldTransform>.ParallelWriter SpawnQueueWriter;

            public void Execute()
            {
                var sortKey = 0;

                while (!SfxQueue.IsEmpty())
                    if (SfxQueue.TryDequeue(out var data))
                    {
                        var transform = new WorldTransform
                        {
                            worldTransform = TransformQvvs.identity
                        };

                        transform.worldTransform.position = data.Position;

                        SpawnQueueWriter.Add(
                            data.SfxPrefab, transform, ++sortKey);
                    }
            }
        }
    }
}