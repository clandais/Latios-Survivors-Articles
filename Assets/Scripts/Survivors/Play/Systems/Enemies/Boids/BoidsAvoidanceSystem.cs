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
    public partial struct BoidsAvoidanceSystem : ISystem
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
            state.Dependency = new AvoidJob
            {
                WorldTransformLookup = SystemAPI.GetComponentLookup<WorldTransform>()
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    internal partial struct AvoidJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<WorldTransform> WorldTransformLookup;

        void Execute(in DynamicBuffer<BoidNeighbor> neighbors,
            in WorldTransform transform,
            in BoidSettings boidSettings,
            ref BoidForces boidForces)
        {
            if (neighbors.Length == 0) return;

            var avoidance = float3.zero;
            var neighborCount = 0;
            foreach (var neighbor in neighbors)
                if (WorldTransformLookup.HasComponent(neighbor.Neighbor.entity))
                {
                    var neighborTransform = WorldTransformLookup[neighbor.Neighbor.entity];
                    var distance = math.distance(transform.position, neighborTransform.position);
                    if (distance < boidSettings.avoidanceRadius)
                    {
                        var direction =
                                math.normalizesafe(transform.position - neighborTransform.position)
                            ; //* (
                        //boidSettings.avoidanceRadius - distance);

                        avoidance += direction;
                        neighborCount++;
                    }
                }

            if (math.lengthsq(avoidance) > 0f && neighborCount > 0)
            {
                // Average the avoidance vector
                avoidance /= neighborCount;

                // var mag = math.clamp(math.length(avoidance), 0f, boidSettings.avoidanceStrength);

                // avoidance                 = math.normalizesafe(avoidance) * mag; // * boidSettings.avoidanceStrength;
                avoidance = math.normalizesafe(avoidance);
                // Set the avoidance force
                boidForces.AvoidanceForce = avoidance;


                UnityEngine.Debug.DrawLine(
                    transform.position + math.up(),
                    transform.position + math.up() + boidForces.AvoidanceForce,
                    Color.red,
                    0.1f);
            }
            else
            {
                boidForces.AvoidanceForce = float3.zero;
            }
        }
    }
}