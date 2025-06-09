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
    public partial struct BoidsAlignmentSystem : ISystem
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
            state.Dependency = new AlignJob
            {
                RigidBodyLookup      = SystemAPI.GetComponentLookup<RigidBody>(),
                WorldTransformLookup = SystemAPI.GetComponentLookup<WorldTransform>()
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        partial struct AlignJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<RigidBody>      RigidBodyLookup;
            [ReadOnly] public ComponentLookup<WorldTransform> WorldTransformLookup;

            void Execute(in DynamicBuffer<BoidNeighbor> neighbors,
                in WorldTransform transform,
                in BoidSettings boidSettings,
                ref BoidForces boidForces)
            {
                if (neighbors.Length == 0) return;

                var alignment = float3.zero;
                var neighborCount = 0;
                foreach (var neighbor in neighbors)
                    if (WorldTransformLookup.HasComponent(neighbor.Neighbor.entity) &&
                        RigidBodyLookup.HasComponent(neighbor.Neighbor.entity))
                    {
                        var neighborTransform = WorldTransformLookup[neighbor.Neighbor.entity];
                        var distance = math.distance(transform.position, neighborTransform.position);
                        if (distance < boidSettings.alignmentRadius)
                        {
                            // Only add the velocity of the neighbor if it's within the alignment radius
                            alignment += RigidBodyLookup[neighbor.Neighbor.entity].velocity.linear;
                            neighborCount++;
                        }
                    }

                if (math.lengthsq(alignment) > 0f && neighborCount > 0)
                {
                    // Average the alignment vector
                    alignment                 /= neighborCount;
                    alignment                 =  math.normalizesafe(alignment);
                    boidForces.AlignmentForce =  alignment;

                    UnityEngine.Debug.DrawLine(
                        transform.position + math.up(),
                        transform.position + math.up() + boidForces.AlignmentForce,
                        Color.green);
                }
                else
                {
                    boidForces.AlignmentForce = float3.zero;
                }
            }
        }
    }
}