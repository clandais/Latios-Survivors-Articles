using Latios;
using Latios.Anna;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Enemies;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Enemies
{
    [BurstCompile]
    public partial struct FollowPlayerSystem : ISystem
    {
        LatiosWorldUnmanaged m_world;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
            m_query = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<RigidBody>()
                .With<MovementSettings>()
                .With<PreviousVelocity>()
                .With<SkeletonMinionAttackAnimationState>()
                .WithDisabled<SkeletonMinionAttackAnimationTag>()
                .With<EnemyTag>()
                .With<BoidSettings>()
                .With<BoidForces>()
                .Without<DeadTag>()
                .Build();

            // state.RequireForUpdate<FloorGridConstructedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var environmentLayer = m_world.sceneBlackboardEntity.GetCollectionComponent<EnvironmentCollisionLayer>(true)
                .layer;

            var playerPosition = m_world.sceneBlackboardEntity.GetComponentData<PlayerPosition>();
            var grid = m_world.GetCollectionAspect<VectorFieldAspect>(m_world.sceneBlackboardEntity);

            state.Dependency = new FollowPlayerJob
            {
                // EnvironmentLayer         = environmentLayer,
                // Grid                     = grid,
                DeltaTime                = SystemAPI.Time.DeltaTime,
                PlayerPosition           = playerPosition,
                AttackAnimationTagLookup = SystemAPI.GetComponentLookup<SkeletonMinionAttackAnimationTag>()
            }.ScheduleParallel(m_query, state.Dependency);
        }


        [BurstCompile]
        partial struct FollowPlayerJob : IJobEntity
        {
            // [ReadOnly] public CollisionLayer    EnvironmentLayer;
            // [ReadOnly] public VectorFieldAspect Grid;
            [ReadOnly] public float          DeltaTime;
            [ReadOnly] public PlayerPosition PlayerPosition;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<SkeletonMinionAttackAnimationTag> AttackAnimationTagLookup;

            void Execute(
                Entity entity,
                [EntityIndexInQuery] int entityIndexInQuery,
                TransformAspect transformAspect,
                in MovementSettings movementSettings,
                ref RigidBody rigidBody,
                in BoidSettings boidSettings,
                in BoidForces boidForces,
                ref PreviousVelocity previousVelocity,
                ref SkeletonMinionAttackAnimationState attackAnimationState)
            {
                var d = boidForces.AlignmentForce * boidSettings.alignmentStrength +
                        boidForces.AvoidanceForce * boidSettings.avoidanceStrength +
                        boidForces.CenteringForce * boidSettings.centeringStrength +
                        boidForces.FollowForce * boidSettings.followStrength;


                var currentVelocity = rigidBody.velocity.linear;
                var desiredVelocity = math.normalizesafe(d) * movementSettings.moveSpeed;

                desiredVelocity.y      = currentVelocity.y;
                previousVelocity.Value = currentVelocity;

                currentVelocity = currentVelocity.MoveTowards(desiredVelocity, movementSettings.speedChangeRate);

                rigidBody.velocity.linear = currentVelocity;

                var lookDirection = math.length(currentVelocity) > math.EPSILON
                    ? math.normalize(currentVelocity)
                    : math.normalize(desiredVelocity);

                var lookRotation = quaternion.LookRotationSafe(lookDirection, math.up());
                transformAspect.worldRotation =
                    transformAspect.worldRotation.RotateTowards(lookRotation,
                        movementSettings.maxAngleDelta * DeltaTime);

                if (math.distance(transformAspect.worldPosition, PlayerPosition.Position) <=
                    attackAnimationState.DistanceToTarget)
                {
                    rigidBody.velocity.linear = float3.zero;
                    AttackAnimationTagLookup.SetComponentEnabled(entity, true);
                    transformAspect.worldRotation = lookRotation;
                }
            }
        }
    }
}