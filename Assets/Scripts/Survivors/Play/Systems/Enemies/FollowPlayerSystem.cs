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
                .WithDisabled<SkeletonMinionAttackAnimationTag>()
                .With<PreviousVelocity>()
                .With<EnemyTag>()
                .Without<DeadTag>()
                .Build();

            // state.RequireForUpdate<FloorGridConstructedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // var environmentLayer = m_world.sceneBlackboardEntity.GetCollectionComponent<EnvironmentCollisionLayer>(true)
            //     .layer;

            var playerPosition = m_world.sceneBlackboardEntity.GetComponentData<PlayerPosition>();
            //    var grid = m_world.GetCollectionAspect<VectorFieldAspect>(m_world.sceneBlackboardEntity);

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
                BoidAspect boidAspect,
                in MovementSettings movementSettings,
                ref CurrentVelocity currentVelocityComponent,
                ref PreviousVelocity previousVelocity,
                ref SkeletonMinionAttackAnimationState attackAnimationState)
            {
                var steering = boidAspect.GetSteering(DeltaTime);
                boidAspect.Velocity += steering;

                currentVelocityComponent.Value = boidAspect.Velocity;

                var worldTransform = transformAspect.worldTransform;
                worldTransform.position   += boidAspect.Velocity * DeltaTime;
                worldTransform.position.y =  0f; // Keep boids on the ground plane


                var lookDirection = math.normalize(boidAspect.Velocity);
                var lookRotation = quaternion.LookRotationSafe(lookDirection, math.up());
                worldTransform.rotation =
                    worldTransform.rotation.RotateTowards(lookRotation,
                        movementSettings.maxAngleDelta * DeltaTime);

                if (math.distance(transformAspect.worldPosition, PlayerPosition.Position) <=
                    attackAnimationState.DistanceToTarget)
                    AttackAnimationTagLookup.SetComponentEnabled(entity, true);


                transformAspect.worldRotation  = lookRotation;
                transformAspect.worldTransform = worldTransform;
            }
        }
    }
}