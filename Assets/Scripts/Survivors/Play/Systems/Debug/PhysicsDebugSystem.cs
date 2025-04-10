using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
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
            // var layer = m_world.sceneBlackboardEntity
            //     .GetCollectionComponent<EnemyCollisionLayer>().Layer;
            // state.Dependency = PhysicsDebug.DrawLayer(layer).ScheduleParallel(state.Dependency);

            var envLayer = m_world.sceneBlackboardEntity
                .GetCollectionComponent<EnvironmentCollisionLayer>().layer;

            state.Dependency = PhysicsDebug.DrawLayer(envLayer).ScheduleParallel(state.Dependency);

            
            if (m_world.sceneBlackboardEntity.HasCollectionComponent<FloorGrid>())
            {
                var grid = m_world.sceneBlackboardEntity.GetCollectionComponent<FloorGrid>();
                FloorGrid.Draw(grid);
            }
            
            
            
            // Collider colliderA = default;
            // TransformQvvs transformA = default;
            //
            // foreach (var (collider, transform) in SystemAPI.Query<RefRO<Collider>, RefRO<WorldTransform>>().WithAll<PlayerTag>())
            // {
            //     colliderA  = collider.ValueRO;
            //     transformA = transform.ValueRO.worldTransform;
            // }
            //
            //
            // Collider colliderB = default;
            // TransformQvvs transformB = default;
            //
            // foreach (var (collider, transform) in SystemAPI.Query<RefRO<Collider>, RefRO<WorldTransform>>().WithAll<EnvironmentCollisionTag>())
            // {
            //     colliderB  = collider.ValueRO;
            //     transformB = transform.ValueRO.worldTransform;
            // }


            // var log = PhysicsDebug.LogDistanceBetween(in colliderA, in transformA, in colliderB, in transformB,
            //     100f);
            //
            // UnityEngine.Debug.Log($"{log}");



            // var gridCollisionLayer = m_world.sceneBlackboardEntity
            //     .GetCollectionComponent<GridCollisionLayer>().Layer;

            // state.Dependency = PhysicsDebug.DrawLayer(gridCollisionLayer).ScheduleParallel(state.Dependency);

            // foreach (var (c, transformAspect) in SystemAPI.Query<RefRO<Collider>, TransformAspect>().WithAll<GridCollisionSettings>())
            // {
            //     var t = transformAspect.worldTransform;
            //
            //     if (c.ValueRO.type == ColliderType.TriMesh)
            //     {
            //         PhysicsDebug.DrawCollider(in c.ValueRO, in t, Color.green);
            //     }
            //     
            //     
            // }

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