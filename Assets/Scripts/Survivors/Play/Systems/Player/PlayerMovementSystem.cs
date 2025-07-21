using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
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
                .With<Velocity>()
                .With<PreviousVelocity>()
                .With<PlayerTag>()
                .Without<DeadTag>()
                .Build();
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerInputState = m_world.sceneBlackboardEntity.GetComponentData<PlayerInputState>();

            state.Dependency = new MovementJob
            {
                DeltaTime        = SystemAPI.Time.DeltaTime,
                PlayerInputState = playerInputState,
                MovementSettings = m_world.sceneBlackboardEntity.GetComponentData<MovementSettings>()
            }.ScheduleParallel(m_jobQuery, state.Dependency);
        }
    }


    [WithAll(typeof(PlayerTag))]
    [BurstCompile]
    internal partial struct MovementJob : IJobEntity
    {
        [ReadOnly] public float            DeltaTime;
        [ReadOnly] public PlayerInputState PlayerInputState;
        [ReadOnly] public MovementSettings MovementSettings;

        void Execute(TransformAspect transformAspect,
            ref Velocity currentVelocityComponent,
            ref PreviousVelocity previousVelocity)
        {
            var move = PlayerInputState.Direction;

            var currentVelocity = currentVelocityComponent.Value;
            var desiredVelocity = new float3(move.x, 0f, move.y) * MovementSettings.moveSpeed;

            // We don't want to change the gravity force
            desiredVelocity.y = currentVelocity.y;

            previousVelocity.Value = currentVelocity;
            currentVelocityComponent.Value =
                currentVelocity.MoveTowards(desiredVelocity, MovementSettings.speedChangeRate);

            transformAspect.worldPosition += currentVelocity * DeltaTime;

            var lookDir = PlayerInputState.MousePosition - transformAspect.worldPosition;
            var lookRotation = quaternion.LookRotationSafe(lookDir, math.up());
            transformAspect.worldRotation =
                transformAspect.worldRotation.RotateTowards(lookRotation, MovementSettings.maxAngleDelta * DeltaTime);
        }
    }
}