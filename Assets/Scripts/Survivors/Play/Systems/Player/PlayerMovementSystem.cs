using Latios;
using Latios.Anna;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player
{
    [RequireMatchingQueriesForUpdate]
    public partial struct PlayerMovementSystem : ISystem
    {
        LatiosWorldUnmanaged m_world;
        EntityQuery          m_Query;
        EntityQuery          m_jobQuery;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
            m_Query = state.Fluent()
                .With<PlayerInputState>()
                .Build();

            m_jobQuery = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<RigidBody>()
                .With<MovementSettings>()
                .With<PreviousVelocity>()
                .With<PlayerTag>()
                .Build();
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerInputState = m_world.sceneBlackboardEntity.GetComponentData<PlayerInputState>();

            state.Dependency = new MovementJob
            {
                DeltaTime        = SystemAPI.Time.DeltaTime,
                PlayerInputState = playerInputState
            }.ScheduleParallel(m_jobQuery, state.Dependency);
        }
    }


    [WithAll(typeof(PlayerTag))]
    [BurstCompile]
    internal partial struct MovementJob : IJobEntity
    {
        [ReadOnly] public float            DeltaTime;
        [ReadOnly] public PlayerInputState PlayerInputState;

        void Execute(TransformAspect transformAspect,
            in MovementSettings movementSettings,
            ref RigidBody rigidBody,
            ref PreviousVelocity previousVelocity)
        {
            var move = PlayerInputState.Direction;

            var currentVelocity = rigidBody.velocity.linear;
            var desiredVelocity = new float3(move.x, 0f, move.y) * movementSettings.moveSpeed;

            // We don't want to change the gravity force
            desiredVelocity.y = currentVelocity.y;

            previousVelocity.Value = currentVelocity;

            currentVelocity           = currentVelocity.MoveTowards(desiredVelocity, movementSettings.speedChangeRate);
            rigidBody.velocity.linear = currentVelocity;

            var lookDir = PlayerInputState.MousePosition - transformAspect.worldPosition;
            var lookRotation = quaternion.LookRotationSafe(lookDir, math.up());
            transformAspect.worldRotation = transformAspect.worldRotation.RotateTowards(lookRotation, movementSettings.maxAngleDelta * DeltaTime);
        }
    }
}