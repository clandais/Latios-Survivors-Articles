using Latios;
using Latios.Transforms;
using LatiosNavigation.Authoring;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Systems.Enemies
{
    public partial struct EnemiesPathDebugSystem : ISystem
    {
        LatiosWorldUnmanaged m_latiosWorld;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorld = state.GetLatiosWorldUnmanaged();
            m_query = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<NavmeshAgentTag>()
                .With<AgentPathPoint>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new Job().ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        partial struct Job : IJobEntity
        {
            void Execute(in DynamicBuffer<AgentPathPoint> pathPoints)
            {
                if (pathPoints.Length > 1)
                    for (var i = 0; i < pathPoints.Length - 1; i++)
                    {
                        var color = Color.blue;

                        var p1 = pathPoints[i].Position;
                        var p2 = pathPoints[i + 1].Position;

                        UnityEngine.Debug.DrawLine(p1, p2, color);
                    }
            }
        }
    }
}