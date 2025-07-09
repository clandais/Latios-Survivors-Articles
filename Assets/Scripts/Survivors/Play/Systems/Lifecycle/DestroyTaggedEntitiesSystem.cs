using Latios;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.Lifecycle
{
    public partial struct DestroyTaggedEntitiesSystem : ISystem
    {
        LatiosWorldUnmanaged m_worldUnmanaged;
        EntityQuery          m_entityQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_entityQuery = state.Fluent()
                .With<ShouldDestroyTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new DestroyTaggedEntitiesJob
            {
                ecb = m_worldUnmanaged.syncPoint.CreateEntityCommandBuffer().AsParallelWriter()
            }.ScheduleParallel(m_entityQuery, state.Dependency);
        }

        
        [BurstCompile]
        partial struct DestroyTaggedEntitiesJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ecb;

            void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex)
            {
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }
        }
        
    }
}
