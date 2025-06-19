using Latios;
using Latios.Psyshock;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Environment;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.Physics
{
    [RequireMatchingQueriesForUpdate]
    public partial struct BuildEnvironmentCollisionLayerSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged latiosWorld;

        BuildCollisionLayerTypeHandles m_handles;
        EntityQuery                    m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            latiosWorld = state.GetLatiosWorldUnmanaged();
            m_handles   = new BuildCollisionLayerTypeHandles(ref state);
            m_query = state.Fluent()
                .With<LevelTag>(true)
                .PatchQueryForBuildingCollisionLayer()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_handles.Update(ref state);
            var physicsSettings = latiosWorld.GetPhysicsSettings();

            state.Dependency = Latios.Psyshock.Physics.BuildCollisionLayer(m_query, in m_handles)
                .WithSettings(physicsSettings.CollisionLayerSettings)
                .ScheduleParallel(out var layer, state.WorldUpdateAllocator, state.Dependency);

            latiosWorld.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(new EnvironmentCollisionLayer
            {
                layer = layer
            });
        }

        [BurstCompile]
        public void OnNewScene(ref SystemState state)
        {
            latiosWorld.sceneBlackboardEntity
                .AddOrSetCollectionComponentAndDisposeOld<EnvironmentCollisionLayer>(default);
        }
    }
}