using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Survivors.Play.Authoring;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.Physics
{
    public partial struct BuildPlayerCollisionLayerSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged           m_latiosWorldUnmanaged;
        BuildCollisionLayerTypeHandles m_typeHandles;
        EntityQuery                    m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_typeHandles          = new BuildCollisionLayerTypeHandles(ref state);
            m_query                = state.Fluent().With<PlayerTag>(true).PatchQueryForBuildingCollisionLayer().Build();
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
                .ScheduleParallel(out var playerCollisionLayer, state.WorldUpdateAllocator, state.Dependency);

            m_latiosWorldUnmanaged.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(new PlayerCollisionLayer
            {
                Layer = playerCollisionLayer
            });
        }


        public void OnNewScene(ref SystemState state)
        {
            m_latiosWorldUnmanaged.sceneBlackboardEntity
                .AddOrSetCollectionComponentAndDisposeOld<PlayerCollisionLayer>(default);
        }
    }
}