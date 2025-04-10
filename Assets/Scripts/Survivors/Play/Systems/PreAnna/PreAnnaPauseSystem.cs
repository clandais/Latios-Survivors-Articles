using Latios;
using Latios.Anna;
using Survivors.Play.Authoring.Anna;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.PreAnna
{
    [BurstCompile]
    public partial struct PreAnnaPauseSystem : ISystem, ISystemStartStop
    {
        EntityQuery m_shouldUpdateQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_shouldUpdateQuery = state.Fluent().Without<PauseRequestedTag>()
                .Build();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var world = state.GetLatiosWorldUnmanaged();
            var ecb = world.syncPoint.CreateEntityCommandBuffer();

            foreach (var (rb, last, entity) in
                     SystemAPI.Query<RefRO<RigidBody>,
                             RefRW<SavedRigidBodyState>>()
                         .WithEntityAccess())
            {
                last.ValueRW.Velocity                 = rb.ValueRO.velocity;
                last.ValueRW.InverseMass              = rb.ValueRO.inverseMass;
                last.ValueRW.CoefficientOfFriction    = rb.ValueRO.coefficientOfFriction;
                last.ValueRW.CoefficientOfRestitution = rb.ValueRO.coefficientOfRestitution;

                ecb.RemoveComponent<RigidBody>(entity);
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
            var world = state.GetLatiosWorldUnmanaged();
            var ecb = world.syncPoint.CreateEntityCommandBuffer();

            foreach (var (last, entity)
                     in SystemAPI.Query<RefRO<SavedRigidBodyState>>()
                         .WithEntityAccess())
                ecb.AddComponent(entity, new RigidBody
                {
                    velocity                 = last.ValueRO.Velocity,
                    inverseMass              = last.ValueRO.InverseMass,
                    coefficientOfFriction    = last.ValueRO.CoefficientOfFriction,
                    coefficientOfRestitution = last.ValueRO.CoefficientOfRestitution
                });
        }
    }
}