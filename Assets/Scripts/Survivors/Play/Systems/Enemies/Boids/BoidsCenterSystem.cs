using Latios;
using Latios.Anna;
using Latios.Transforms;
using Survivors.Play.Authoring.Enemies;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Systems.Enemies.Boids
{
    public partial struct BoidsCenterSystem : ISystem
    {
        EntityQuery m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_query = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<RigidBody>()
                .With<BoidTag>()
                .With<BoidSettings>()
                .With<BoidNeighbor>()
                .With<BoidForces>()
                .Without<DeadTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new CenterJob
            {
                WorldTransformLookup = SystemAPI.GetComponentLookup<WorldTransform>()
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        partial struct CenterJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<WorldTransform> WorldTransformLookup;

            void Execute(in DynamicBuffer<BoidNeighbor> neighbors,
                in WorldTransform transform,
                in BoidSettings boidSettings,
                ref BoidForces boidForces)
            {
                if (neighbors.Length == 0)
                    return;

                var center = float3.zero;
                var neighborCount = 0;
                foreach (var neighbor in neighbors)
                    if (WorldTransformLookup.HasComponent(neighbor.Neighbor.entity))
                    {
                        center += WorldTransformLookup[neighbor.Neighbor.entity].position;
                        neighborCount++;
                    }

                if (math.lengthsq(center) > 0f && neighborCount > 0)
                {
                    center /= neighborCount;
                    var direction = math.normalizesafe(center - transform.position);
                    boidForces.CenteringForce = direction;

                    UnityEngine.Debug.DrawLine(
                        transform.position + math.up(),
                        transform.position + math.up() + boidForces.CenteringForce,
                        Color.blue,
                        0.1f);
                }
                else
                {
                    boidForces.CenteringForce = float3.zero;
                }
            }
        }
    }
}