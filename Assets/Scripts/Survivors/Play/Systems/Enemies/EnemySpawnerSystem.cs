using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring.Enemies;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Enemies
{
    [BurstCompile]
    public partial struct EnemySpawnerSystem : ISystem
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

            var ecb = m_worldUnmanaged.syncPoint.CreateEntityCommandBuffer();
            
            state.Dependency = new SpawnerJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb       = ecb.AsParallelWriter(),
            }.ScheduleParallel(state.Dependency);
        }

        
        [BurstCompile]
        partial struct SpawnerJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;
            public EntityCommandBuffer.ParallelWriter Ecb;

            void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery, in WorldTransform transform, ref EnemySpawnerComponent spawnerComponent)
            {
                spawnerComponent.CurrentTime += DeltaTime;

                if (spawnerComponent.CurrentTime >= spawnerComponent.SpawnInterval)
                {
                    spawnerComponent.CurrentTime = 0f;

                    var spawnPosition = transform.position + spawnerComponent.RelativeSpawnPosition;
                    var e = Ecb.Instantiate(chunkIndexInQuery, spawnerComponent.EnemyPrefab);
                    var worldTransform = TransformQvvs.identity;
                    worldTransform.position = spawnPosition;
                    Ecb.SetComponent(chunkIndexInQuery, e, new WorldTransform
                    {
                        worldTransform = worldTransform,
                    });


                }
            }
        }
    }
}