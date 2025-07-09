using Latios;
using Latios.Psyshock;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.Physics
{
    [RequireMatchingQueriesForUpdate]
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
            m_query = state.Fluent()
                .With<PlayerTag>(true)
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


            state.Dependency = Latios.Psyshock.Physics.BuildCollisionLayer(m_query,
                    in m_typeHandles).WithSettings(physicsSettings.CollisionLayerSettings)
                .ScheduleParallel(out var playerCollisionLayer, state.WorldUpdateAllocator, state.Dependency);

            m_latiosWorldUnmanaged.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(new PlayerCollisionLayer
            {
                Layer = playerCollisionLayer
            });
        }


        [BurstCompile]
        public void OnNewScene(ref SystemState state)
        {
            m_latiosWorldUnmanaged.sceneBlackboardEntity
                .AddOrSetCollectionComponentAndDisposeOld<PlayerCollisionLayer>(default);
        }
    }
}