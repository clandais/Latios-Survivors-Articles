using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Collider = Latios.Psyshock.Collider;

namespace Survivors.Play.Systems.Debug
{
    [RequireMatchingQueriesForUpdate]
    public partial struct PhysicsDebugSystem : ISystem
    {
        LatiosWorldUnmanaged m_world;
        EntityQuery m_Query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
            m_Query = state.Fluent()
                .With<EnvironmentCollisionTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var layer = m_world.sceneBlackboardEntity
                .GetCollectionComponent<EnvironmentCollisionLayer>().layer;
            state.Dependency = PhysicsDebug.DrawLayer(layer).ScheduleParallel(state.Dependency);

            foreach (var (collider, transformAspect) in SystemAPI.Query<RefRO<Collider>, TransformAspect>()
                         .WithAll<PlayerTag>())
            {
                var t = transformAspect.worldTransform;
                PhysicsDebug.DrawCollider(in collider.ValueRO, in t, Color.green);
            }


            foreach (var (collider, transformAspect) in SystemAPI.Query<RefRO<Collider>, TransformAspect>()
                         .WithAll<EnvironmentCollisionTag>())
            {
                var t = transformAspect.worldTransform;
                PhysicsDebug.DrawCollider(in collider.ValueRO, in t, Color.red);
            }
        }
    }
}