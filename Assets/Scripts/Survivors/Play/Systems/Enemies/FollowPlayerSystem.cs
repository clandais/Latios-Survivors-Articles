using Boids.Components;
using Latios;
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
    [RequireMatchingQueriesForUpdate]
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
                .WithAspect<BoidAspect>()
                .With<CurrentVelocity>()
                .With<MovementSettings>()
                .With<SkeletonMinionAttackAnimationState>()
                .With<PreviousVelocity>()
                .With<EnemyTag>()
                .Without<DeadTag>()
                .Build();

            // state.RequireForUpdate<FloorGridConstructedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerPosition = m_world.sceneBlackboardEntity.GetComponentData<PlayerPosition>();
            state.Dependency = new FollowPlayerJob
            {
                DeltaTime                = SystemAPI.Time.DeltaTime,
                PlayerPosition           = playerPosition,
                AttackAnimationTagLookup = SystemAPI.GetComponentLookup<SkeletonMinionAttackAnimationTag>()
            }.ScheduleParallel(m_query, state.Dependency);
        }


        [BurstCompile]
        partial struct FollowPlayerJob : IJobEntity
        {
            [ReadOnly] public float          DeltaTime;
            [ReadOnly] public PlayerPosition PlayerPosition;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<SkeletonMinionAttackAnimationTag> AttackAnimationTagLookup;

            void Execute(
                Entity entity,
                [EntityIndexInQuery] int entityIndexInQuery,
                TransformAspect transformAspect,
                BoidAspect boidAspect,
                in MovementSettings movementSettings,
                ref CurrentVelocity currentVelocityComponent,
                ref PreviousVelocity previousVelocity,
                ref SkeletonMinionAttackAnimationState attackAnimationState)
            {
                var worldTransform = transformAspect.worldTransform;
                var lookRotation = transformAspect.worldRotation;

                if (math.distance(transformAspect.worldPosition, PlayerPosition.Position) >
                    attackAnimationState.DistanceToTarget)
                {
                    var steering = boidAspect.GetSteering(DeltaTime);
                    boidAspect.Velocity += steering;

                    currentVelocityComponent.Value = boidAspect.Velocity;


                    worldTransform.position   += boidAspect.Velocity * DeltaTime;
                    worldTransform.position.y =  0f; // Keep boids on the ground plane


                    var lookDirection = math.normalize(boidAspect.Velocity);
                    lookRotation = quaternion.LookRotationSafe(lookDirection, math.up());
                    worldTransform.rotation =
                        worldTransform.rotation.RotateTowards(lookRotation,
                            movementSettings.maxAngleDelta * DeltaTime);
                }
                else
                {
                    if (AttackAnimationTagLookup.IsComponentEnabled(entity))
                    {
                        var vectorToPlayer = math.normalize(PlayerPosition.Position - transformAspect.worldPosition);
                        worldTransform.rotation = quaternion.LookRotationSafe(
                            vectorToPlayer,
                            math.up());

                        var velocity = vectorToPlayer * boidAspect.Settings.maxSpeed;
                        currentVelocityComponent.Value = velocity;

                        lookRotation = quaternion.LookRotationSafe(vectorToPlayer, math.up());
                        worldTransform.rotation =
                            worldTransform.rotation.RotateTowards(lookRotation,
                                movementSettings.maxAngleDelta * DeltaTime);

                        worldTransform.position   += velocity * DeltaTime;
                        worldTransform.position.y =  0f;
                    }
                    else
                    {
                        AttackAnimationTagLookup.SetComponentEnabled(entity, true);
                    }
                }

                transformAspect.worldTransform = worldTransform;
            }
        }
    }
}