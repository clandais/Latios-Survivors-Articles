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


            var qToArray = sfxQueue.ToArray(Allocator.TempJob);
            state.Dependency = new SfxSpawnJobParallelFor
            {
                SfxQueue         = qToArray,
                SpawnQueueWriter = icb.AsParallelWriter()
            }.Schedule(sfxQueue.Count, 128, state.Dependency);

            qToArray.Dispose(state.Dependency);
            sfxQueue.Clear();
        }

        [BurstCompile]
        struct SfxSpawnJobParallelFor : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<SfxSpawnQueue.SfxSpawnData> SfxQueue;
            public InstantiateCommandBuffer<WorldTransform>.ParallelWriter SpawnQueueWriter;

            public void Execute(int index)
            {
                if (index < SfxQueue.Length)
                {
                    var data = SfxQueue[index];

                    var transform = new WorldTransform
                    {
                        worldTransform = TransformQvvs.identity
                    };

                    transform.worldTransform.position = data.Position;

                    SpawnQueueWriter.Add(
                        data.SfxPrefab, transform, index);
                }
            }
        }
    }
}