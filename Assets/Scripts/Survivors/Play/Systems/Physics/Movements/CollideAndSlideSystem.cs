using Boids.Components;
using Latios;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Environment;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Physics.Movements
{
    [RequireMatchingQueriesForUpdate]
    public partial struct CollideAndSlideSystem : ISystem
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
                .With<CurrentVelocity>()
                .With<Collider>()
                .With<MovementSettings>()
                .With<EnemyTag>()
                .Without<DeadTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var environmentLayer = m_world.sceneBlackboardEntity.GetCollectionComponent<EnvironmentCollisionLayer>(true)
                .layer;

            state.Dependency = new MoveJob
            {
                EnvironmentLayer = environmentLayer,
                DeltaTime        = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        partial struct MoveJob : IJobEntity
        {
            [ReadOnly] public CollisionLayer EnvironmentLayer;
            [ReadOnly] public float          DeltaTime;

            void Execute(TransformAspect transform,
                BoidAspect boidAspect,
                ref CurrentVelocity currentVelocity,
                in Collider collider,
                in MovementSettings movementSettings)
            {
                var steering = boidAspect.GetSteering(DeltaTime);
                boidAspect.Velocity += steering;

                // see https://github.com/Dreaming381/Hack-Labs/blob/main/Hack%20Lab%20-%20A1/Assets/_Code/Systems/FirstPersonCharacter/FirstPersonControllerSystem.cs#L251
                var startPosition = transform.worldPosition;


                var moveVector = (boidAspect.Velocity + steering) * DeltaTime;
                var distanceRemaining = math.length(moveVector);
                var currentTransform = new TransformQvvs(startPosition, quaternion.identity);
                var moveDirection = math.normalize(moveVector);
                var end = currentTransform.position + moveDirection * distanceRemaining;
                var collisionAvoidanceForce = float3.zero;

                if (Latios.Psyshock.Physics.ColliderCast(
                        in collider,
                        in currentTransform,
                        end,
                        in EnvironmentLayer,
                        out var hitInfos,
                        out _))
                    collisionAvoidanceForce = math.mul(quaternion.LookRotation(hitInfos.normalOnCaster, moveDirection),
                        math.up());

                // for (var i = 0; i < 16; i++)
                // {
                //     if (distanceRemaining < math.EPSILON) break;
                //
                //     var end = currentTransform.position + moveDirection * distanceRemaining;
                //     if (Latios.Psyshock.Physics.ColliderCast(
                //             in collider,
                //             in currentTransform,
                //             end,
                //             in EnvironmentLayer,
                //             out var hitInfos,
                //             out _))
                //     {
                //         currentTransform.position += moveDirection * (hitInfos.distance - 0.01f);
                //         distanceRemaining         -= hitInfos.distance;
                //         if (math.dot(hitInfos.normalOnTarget, moveDirection) <
                //             -.9f) // If the obstacle directly opposes our movement
                //             break;
                //
                //         // LookRotation corrects an "up" vector to be perpendicular to the "forward" vector.
                //         // We cheat this to get a new moveDirection perpendicular to the normal.
                //         moveDirection = math.mul(quaternion.LookRotation(hitInfos.normalOnCaster, moveDirection),
                //             math.up());
                //     }
                //     else
                //     {
                //         currentTransform.position += moveDirection * distanceRemaining;
                //         distanceRemaining         =  0f; // No more distance to move
                //     }
                // }

                // boidAspect.Velocity   = (currentTransform.position - startPosition) / DeltaTime;


                collisionAvoidanceForce *= 10f;
                var newForce = collisionAvoidanceForce;

                newForce = math.length(newForce) > boidAspect.Settings.maxForce
                    ? math.normalize(newForce) * boidAspect.Settings.maxForce
                    : newForce;

                boidAspect.Velocity   += newForce;
                currentVelocity.Value =  boidAspect.Velocity;

                var newPosition = startPosition + currentVelocity.Value * DeltaTime;
                newPosition.y = 0.1f; // Keep boids on the ground plane
                var lookDirection = math.normalizesafe(currentVelocity.Value);
                var lookRotation = quaternion.LookRotationSafe(lookDirection, math.up());

                // if (math.length(currentTransform.position - startPosition) < math.EPSILON)
                //     currentVelocity.Value = float3.zero;

                transform.worldPosition = newPosition;
                transform.worldRotation =
                    transform.worldRotation.RotateTowards(lookRotation, movementSettings.maxAngleDelta * DeltaTime);
            }
        }
    }
}