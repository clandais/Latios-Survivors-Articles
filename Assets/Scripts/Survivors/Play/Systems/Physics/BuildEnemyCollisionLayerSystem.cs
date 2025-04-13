using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.Physics
{
    [BurstCompile]
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


            var physicsSettings = m_latiosWorldUnmanaged.GetPhysicsSettings();


            var settings = new CollisionLayerSettings
            {
                worldAabb                = physicsSettings.collisionLayerSettings.worldAabb,
                worldSubdivisionsPerAxis = physicsSettings.collisionLayerSettings.worldSubdivisionsPerAxis
            };

            state.Dependency = Latios.Psyshock.Physics.BuildCollisionLayer(m_query,
                    in m_typeHandles).WithSettings(settings)
                .ScheduleParallel(out var enemyCollisionLayer, state.WorldUpdateAllocator, state.Dependency);


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