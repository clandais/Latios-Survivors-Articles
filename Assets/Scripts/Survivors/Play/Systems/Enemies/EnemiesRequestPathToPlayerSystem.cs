using Latios;
using LatiosNavigation.Authoring;
using Survivors.Play.Authoring;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Enemies
{
    [RequireMatchingQueriesForUpdate]
    public partial struct EnemiesRequestPathToPlayerSystem : ISystem
    {
        EntityQuery          m_query;
        LatiosWorldUnmanaged m_latiosWorld;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorld = state.GetLatiosWorldUnmanaged();
            m_query = state.Fluent()
                .With<EnemyTag>()
                .With<NavmeshAgentTag>()
                .With<AgentDestination>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = m_latiosWorld.syncPoint.CreateEntityCommandBuffer();
            var playerPosition = m_latiosWorld.sceneBlackboardEntity.GetComponentData<PlayerPosition>();

            state.Dependency = new Job
            {
                CommandBuffer  = ecb.AsParallelWriter(),
                PlayerPosition = playerPosition
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        partial struct Job : IJobEntity
        {
            [ReadOnly] public PlayerPosition                     PlayerPosition;
            public            EntityCommandBuffer.ParallelWriter CommandBuffer;

            void Execute(Entity entity, [EntityIndexInQuery] int index,
                ref AgentDestination destination)
            {
                destination.Position = PlayerPosition.Position;
                CommandBuffer.SetComponentEnabled<AgenPathRequestedTag>(index, entity, true);
            }
        }
    }
}