using Latios;
using Latios.Psyshock;
using Latios.Transforms;
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
                .With<Velocity>()
                .With<Collider>()
                .With<PlayerTag>()
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
                MovementSettings = m_world.sceneBlackboardEntity.GetComponentData<MovementSettings>(),
                DeltaTime        = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        partial struct MoveJob : IJobEntity
        {
            [ReadOnly] public CollisionLayer   EnvironmentLayer;
            [ReadOnly] public float            DeltaTime;
            [ReadOnly] public MovementSettings MovementSettings;

            void Execute(TransformAspect transform,
                ref Velocity currentVelocity,
                in Collider collider)
            {
                // var steering = boidAspect.GetSteering(DeltaTime);
                // boidAspect.Velocity += steering;

                // see https://github.com/Dreaming381/Hack-Labs/blob/main/Hack%20Lab%20-%20A1/Assets/_Code/Systems/FirstPersonCharacter/FirstPersonControllerSystem.cs#L251
                var startPosition = transform.worldPosition;




                var moveVector = currentVelocity.Value * DeltaTime;
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

                collisionAvoidanceForce *= 10f;
                var newForce = collisionAvoidanceForce;

                currentVelocity.Value += newForce; //boidAspect.Velocity;

                var newPosition = startPosition + currentVelocity.Value * DeltaTime;
                newPosition.y = 0.1f; // Keep boids on the ground plane
                var lookDirection = math.normalizesafe(currentVelocity.Value);
                var lookRotation = quaternion.LookRotationSafe(lookDirection, math.up());

                transform.worldPosition = newPosition;
                transform.worldRotation =
                    transform.worldRotation.RotateTowards(lookRotation, MovementSettings.maxAngleDelta * DeltaTime);
            }
        }
    }
}