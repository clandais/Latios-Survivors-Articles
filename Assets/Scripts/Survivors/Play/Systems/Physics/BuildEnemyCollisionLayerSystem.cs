using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Physics
{
    public partial struct BuildEnemyCollisionLayerSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged           m_latiosWorldUnmanaged;
        BuildCollisionLayerTypeHandles m_typeHandles;
        EntityQuery                    m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_typeHandles          = new BuildCollisionLayerTypeHandles(ref state);
            m_query                = state.Fluent().With<EnemyTag>(true).PatchQueryForBuildingCollisionLayer().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_typeHandles.Update(ref state);


            var entities = m_query.ToEntityArray(state.WorldUpdateAllocator);

            var min = float3.zero;
            var max = float3.zero;

            foreach (var entity in entities)
            {
                var collider = SystemAPI.GetComponent<Collider>(entity);
                var transform = SystemAPI.GetComponent<WorldTransform>(entity);
                var aabb = Latios.Psyshock.Physics.AabbFrom(collider, transform.worldTransform);
                min = math.min(aabb.min, min);
                max = math.max(aabb.max, max);
            }


            // add a small padding to the AABB
            min -= new float3(5f);
            max += new float3(5f);

            var settings = new CollisionLayerSettings
            {
                worldAabb                = new Aabb(min, max),
                worldSubdivisionsPerAxis = new int3(1, 1, 8)
            };

            state.Dependency = Latios.Psyshock.Physics.BuildCollisionLayer(m_query,
                    in m_typeHandles).WithSettings(settings)
                .ScheduleParallel(out var enemyCollisionLayer, Allocator.TempJob, state.Dependency);


            m_latiosWorldUnmanaged.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(new EnemyCollisionLayer
            {
                Layer = enemyCollisionLayer
            });
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        public void OnNewScene(ref SystemState state)
        {
            m_latiosWorldUnmanaged.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld<EnemyCollisionLayer>(default);
        }
    }
}