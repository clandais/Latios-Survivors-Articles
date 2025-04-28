using Latios;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Play.Systems.VFX
{
    [BurstCompile]
    public partial struct VfxSpawnQueueSystem : ISystem
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
            var vfxQueue = m_worldUnmanaged.sceneBlackboardEntity
                .GetCollectionComponent<VfxSpawnQueue>().VfxQueue;

            var qToArray = vfxQueue.ToArray(Allocator.TempJob);
            var vfxCommandBuffer = m_worldUnmanaged.syncPoint
                .CreateInstantiateCommandBuffer<WorldTransform>();


            state.Dependency = new VfxSpawnJobParallelFor
            {
                VfxQueue         = qToArray,
                VfxCommandBuffer = vfxCommandBuffer.AsParallelWriter()
            }.Schedule(vfxQueue.Count, 128, state.Dependency);

            qToArray.Dispose(state.Dependency);
            vfxQueue.Clear();
        }


        [BurstCompile]
        struct VfxSpawnJobParallelFor : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<VfxSpawnQueue.VfxSpawnData> VfxQueue;
            public InstantiateCommandBuffer<WorldTransform>.ParallelWriter VfxCommandBuffer;

            public void Execute(int index)
            {
                if (index < VfxQueue.Length)
                {
                    var data = VfxQueue[index];

                    var transform = new WorldTransform
                    {
                        worldTransform = TransformQvvs.identity
                    };

                    transform.worldTransform.position = data.Position;

                    VfxCommandBuffer.Add(data.VfxPrefab, transform, index);
                }
            }
        }
    }
}