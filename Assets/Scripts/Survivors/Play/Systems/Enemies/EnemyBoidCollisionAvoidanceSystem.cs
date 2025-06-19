using Boids.Components;
using Boids.Systems.Boids;
using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring.Environment;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.Enemies
{
    [RequireMatchingQueriesForUpdate]
    public partial struct EnemyBoidCollisionAvoidanceSystem : ISystem
    {
        LatiosWorldUnmanaged m_world;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
            m_query = state.Fluent()
                .WithAspect<TransformAspect>()
                .WithAspect<BoidAspect>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var collisionLayer = m_world.sceneBlackboardEntity
                .GetCollectionComponent<EnvironmentCollisionLayer>()
                .layer;

            state.Dependency = new CollisionAvoidanceJob
            {
                CollisionLayer = collisionLayer
            }.ScheduleParallel(m_query, state.Dependency);
        }
    }
}