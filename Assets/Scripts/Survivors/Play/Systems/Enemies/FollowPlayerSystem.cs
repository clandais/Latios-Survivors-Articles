﻿using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring;
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
                .With<EnemyTag>()
                .Without<DeadTag>()
                .Build();
            
            state.RequireForUpdate<FloorGridConstructedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var environmentLayer = m_world.sceneBlackboardEntity.GetCollectionComponent<EnvironmentCollisionLayer>(true).layer;
            var playerPosition = m_world.sceneBlackboardEntity.GetComponentData<PlayerPosition>();
            var grid = m_world.GetCollectionAspect<VectorFieldAspect>( m_world.sceneBlackboardEntity);

            state.Dependency = new FollowPlayerJob
            {
                EnvironmentLayer = environmentLayer,
                Grid             = grid,
                DeltaTime        = SystemAPI.Time.DeltaTime,
                PlayerPosition   = playerPosition
            }.ScheduleParallel(m_query, state.Dependency);
        }


        [BurstCompile]
        partial struct FollowPlayerJob : IJobEntity
        {
            [ReadOnly] public CollisionLayer    EnvironmentLayer;
            [ReadOnly] public VectorFieldAspect Grid;
            [ReadOnly] public float             DeltaTime;
            [ReadOnly] public PlayerPosition    PlayerPosition;

            void Execute(TransformAspect transformAspect,
                in MovementSettings movementSettings,
                ref RigidBody rigidBody,
                ref PreviousVelocity previousVelocity)
            {
                var targetDelta = float3.zero;
                
                var vecDelta = Grid.InterpolatedVectorAt(transformAspect.worldPosition.xz);
                var deltaToPlayer = math.normalizesafe(PlayerPosition.Position - transformAspect.worldPosition);

                var rayStart = transformAspect.worldPosition + math.up();
                var rayEnd = transformAspect.worldPosition + math.up() + deltaToPlayer * 25f;

                // Check if the raycast to the player hits the environment
                // If it does, we just follow the vector field
                if (!Latios.Psyshock.Physics.Raycast(rayStart, rayEnd, in EnvironmentLayer, out RaycastResult result, out _))
                    vecDelta += deltaToPlayer.xz;


                vecDelta = math.normalizesafe(vecDelta);


                var nan = math.isnan(vecDelta);

                if (nan.x || nan.y)
                {
                    vecDelta = float2.zero;
                    UnityEngine.Debug.Log($"Nan detected in vecDelta: {vecDelta}");
                }


                targetDelta.xz = vecDelta;
                targetDelta.y  = transformAspect.worldPosition.y;


                var currentVelocity = rigidBody.velocity.linear;
                var desiredVelocity = math.normalizesafe(targetDelta) * movementSettings.moveSpeed;

                var isDesiredVelocityNan = math.isnan(desiredVelocity);

                if (isDesiredVelocityNan.x || isDesiredVelocityNan.y)
                {
                    desiredVelocity = float3.zero;
                    UnityEngine.Debug.Log($"Nan detected in desiredVelocity: {desiredVelocity}");
                }
                

                desiredVelocity.y      = currentVelocity.y;
                previousVelocity.Value = currentVelocity;

                currentVelocity           = currentVelocity.MoveTowards(desiredVelocity, movementSettings.speedChangeRate);
                rigidBody.velocity.linear = currentVelocity;


                var lookDirection = math.length(currentVelocity) > math.EPSILON ? math.normalize(currentVelocity) : math.normalize(desiredVelocity);
                var lookRotation = quaternion.LookRotationSafe(lookDirection, math.up());
                transformAspect.worldRotation = transformAspect.worldRotation.RotateTowards(lookRotation, movementSettings.maxAngleDelta * DeltaTime);
            }
        }
    }
}