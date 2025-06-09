using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Enemies;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Enemies.Boids
{
    public partial struct BoidsFollowSystem : ISystem
    {
        EntityQuery          m_query;
        LatiosWorldUnmanaged m_world;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
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
            var environmentLayer = m_world.sceneBlackboardEntity.GetCollectionComponent<EnvironmentCollisionLayer>(true)
                .layer;

            var playerPosition = m_world.sceneBlackboardEntity.GetComponentData<PlayerPosition>();
            var grid = m_world.GetCollectionAspect<VectorFieldAspect>(m_world.sceneBlackboardEntity);

            state.Dependency = new FollowJob
            {
                Grid             = grid,
                EnvironmentLayer = environmentLayer,
                PlayerPosition   = playerPosition
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        partial struct FollowJob : IJobEntity
        {
            [ReadOnly] public CollisionLayer    EnvironmentLayer;
            [ReadOnly] public VectorFieldAspect Grid;
            [ReadOnly] public PlayerPosition    PlayerPosition;

            void Execute(
                TransformAspect transformAspect,
                in BoidSettings boidSettings,
                ref BoidForces boidForces)
            {
                float2 direction;

                var deltaToPlayer = math.normalizesafe(PlayerPosition.Position - transformAspect.worldPosition);
                var rayStart = transformAspect.worldPosition + math.up();
                var rayEnd = transformAspect.worldPosition + math.up() + deltaToPlayer * 25f;

                // Check if the raycast to the player hits the environment
                // If it does, we just follow the vector field
                if (!Latios.Psyshock.Physics.Raycast(rayStart, rayEnd, in EnvironmentLayer, out _, out _))
                    direction = deltaToPlayer.xz;
                else
                    direction = Grid.InterpolatedVectorAt(transformAspect.worldPosition.xz);

                direction = math.normalizesafe(direction);

                var nan = math.isnan(direction);

                if (nan.x || nan.y)
                {
                    UnityEngine.Debug.Log($"Nan detected in vecDelta: {direction}");
                    direction.x = math.select(direction.x, 0f, nan.x);
                    direction.y = math.select(direction.y, 0f, nan.y);
                }

                var followForce = float3.zero;
                followForce.xz = direction;
                followForce.y  = transformAspect.worldPosition.y;
                followForce    = math.normalizesafe(followForce);

                boidForces.FollowForce = followForce * boidSettings.followStrength;
            }
        }
    }
}