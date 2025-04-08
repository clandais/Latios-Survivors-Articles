using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Components;
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
        EntityQuery          m_Query;

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
                .GetCollectionComponent<EnemyCollisionLayer>().Layer;
            state.Dependency = PhysicsDebug.DrawLayer(layer).ScheduleParallel(state.Dependency);

            var envLayer = m_world.sceneBlackboardEntity
                .GetCollectionComponent<EnvironmentCollisionLayer>().layer;

            var grid = m_world.sceneBlackboardEntity.GetCollectionComponent<FloorGrid>();
            FloorGrid.Draw(grid);
            //
            // for (int i = 0; i < envLayer.count; i++)
            // {
            //     var aabb = envLayer.GetAabb(i);
            //     PhysicsDebug.DrawAabb(aabb, Color.yellow);
            // }
            //
            // foreach (var (collider, transformAspect) in 
            //          SystemAPI.Query<RefRO<Collider>, TransformAspect>())
            // {
            //    // var t = transformAspect.worldTransform;
            //    // PhysicsDebug.DrawCollider(in collider.ValueRO, in t, Color.green);
            //   PhysicsDebug.DrawAabb( Latios.Psyshock.Physics.AabbFrom(collider.ValueRO, transformAspect.worldTransform), Color.green );
            // }

            // foreach (var (collider, transformAspect) in SystemAPI.Query<RefRO<Collider>, TransformAspect>()
            //              .WithAll<EnvironmentCollisionTag>())
            // {
            //     var t = transformAspect.worldTransform;
            //     PhysicsDebug.DrawCollider(in collider.ValueRO, in t, Color.red);
            // }
        }
    }
}