using Latios;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Enemies
{
    public partial struct EnemyXpSpawnQueueSystem : ISystem
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
            var xpSpawnQueue = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<XpSpawnQueue>().XpQueue;
            var qToArray = xpSpawnQueue.ToArray(Allocator.TempJob);



            var icb = m_worldUnmanaged.syncPoint
                .CreateInstantiateCommandBuffer<WorldTransform>();

            state.Dependency = new SpawnXpJob
            {
                XpSpawnQueue     = qToArray,
                SpawnQueueWriter = icb.AsParallelWriter()
            }.Schedule(xpSpawnQueue.Count, 128, state.Dependency);

            qToArray.Dispose(state.Dependency);
            xpSpawnQueue.Clear();
        }


        [BurstCompile]
        struct SpawnXpJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<XpSpawnQueue.XpSpawnData> XpSpawnQueue;
            public InstantiateCommandBuffer<WorldTransform>.ParallelWriter SpawnQueueWriter;

            public void Execute(int index)
            {
                if (index >= XpSpawnQueue.Length) return;

                var xpDrop = XpSpawnQueue[index];
                var transform = new WorldTransform
                {
                    worldTransform = TransformQvvs.identity
                };

                transform.worldTransform.position = xpDrop.Position + math.up();

                SpawnQueueWriter.Add(
                    xpDrop.XpPrefab,
                    transform,
                    index
                );
            }
        }
    }
}