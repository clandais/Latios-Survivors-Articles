using Latios;
using Latios.Psyshock;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.Physics
{
    [RequireMatchingQueriesForUpdate]
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
            m_query = state.Fluent()
                .With<EnemyTag>(true)
                .Without<DeadTag>()
                .PatchQueryForBuildingCollisionLayer()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_typeHandles.Update(ref state);


            if (!m_latiosWorldUnmanaged.GetPhysicsSettings(out var physicsSettings))
                return;

            var settings = new CollisionLayerSettings
            {
                worldAabb                = physicsSettings.CollisionLayerSettings.worldAabb,
                worldSubdivisionsPerAxis = physicsSettings.CollisionLayerSettings.worldSubdivisionsPerAxis
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
        public void OnNewScene(ref SystemState state)
        {
            m_latiosWorldUnmanaged.sceneBlackboardEntity
                .AddOrSetCollectionComponentAndDisposeOld<EnemyCollisionLayer>(default);
        }
    }
}