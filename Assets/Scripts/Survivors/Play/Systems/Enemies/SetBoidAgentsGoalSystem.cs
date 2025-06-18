using Boids.Components;
using Latios;
using Latios.Transforms;
using LatiosNavigation.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Enemies
{
    public partial struct SetBoidAgentsGoalSystem : ISystem
    {
        EntityQuery m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_query = state.Fluent()
                .WithAspect<TransformAspect>()
                .WithAspect<BoidAspect>()
                .With<NavmeshAgentTag>()
                .With<NavMeshAgent>()
                .With<AgentPathPoint>()
                .With<AgentPath>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FollowPathJob
                {
                    DeltaTime = SystemAPI.Time.DeltaTime
                }
                .ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        partial struct FollowPathJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;

            void Execute(
                TransformAspect transformAspect,
                BoidAspect boidAspect,
                ref AgentPath pathState,
                in DynamicBuffer<AgentPathPoint> pathPoints,
                in NavMeshAgent navMeshAgent)
            {
                var currentIndex = pathState.PathIndex;
                var pointCount = pathState.PathLength;
                if (currentIndex >= pointCount || pointCount == 0)
                {
                    boidAspect.SetGoal(transformAspect.worldPosition);
                    return;
                }

                // Get the current target position from the path
                var targetPosition = pathPoints[currentIndex].Position;
                // Calculate the distance to the target position
                var distanceToTarget = math.distancesq(transformAspect.worldPosition, targetPosition);
                // If the distance to the target is less than the stopping distance, move to the next point
                if (distanceToTarget <= navMeshAgent.Radius * navMeshAgent.Radius)
                {
                    // Increment the path index to move to the next point
                    currentIndex++;
                    pathState.PathIndex = currentIndex;

                    // If we have reached the end of the path, stop moving
                    if (currentIndex >= pointCount)
                    {
                        boidAspect.SetGoal(pathPoints[^1].Position);
                        return;
                    }

                    // Get the new target position from the path
                    targetPosition = pathPoints[currentIndex].Position;
                }

                // Set the boid's goal to the target position
                boidAspect.SetGoal(targetPosition);
            }
        }
    }
}