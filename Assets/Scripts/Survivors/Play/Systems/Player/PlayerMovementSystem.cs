using Latios;
using Latios.Anna;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Anna;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player
{
    [RequireMatchingQueriesForUpdate]
    public partial struct PlayerMovementSystem : ISystem
    {
        LatiosWorldUnmanaged m_world;
        EntityQuery m_Query;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
            m_Query = state.Fluent()
                .With<PlayerInputState>()
                .Build();

        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            var playerInputState = m_world.sceneBlackboardEntity.GetComponentData<PlayerInputState>();
            var move = playerInputState.Direction;

            foreach (var (rb,  movementSettings)
                     in SystemAPI.Query<RefRW<RigidBody>,  RefRO<MovementSettings>>()
                         .WithAll<PlayerTag>())
            {
                var currentVelocity = rb.ValueRO.velocity.linear;
                var desiredVelocity = new float3(move.x, 0f, move.y) * movementSettings.ValueRO.moveSpeed;
                
                // We don't want to change the gravity force
                desiredVelocity.y = currentVelocity.y;

                currentVelocity            = currentVelocity.MoveTowards(desiredVelocity, movementSettings.ValueRO.speedChangeRate);
                rb.ValueRW.velocity.linear = currentVelocity;
            }
        }
    }
}