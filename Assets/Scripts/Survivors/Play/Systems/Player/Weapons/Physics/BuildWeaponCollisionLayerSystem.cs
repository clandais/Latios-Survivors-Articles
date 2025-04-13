using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.Player.Weapons.Physics
{
    public partial struct BuildWeaponCollisionLayerSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged latiosWorld;
        BuildCollisionLayerTypeHandles m_handles;
        EntityQuery                    m_query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            latiosWorld = state.GetLatiosWorldUnmanaged();
            m_handles = new BuildCollisionLayerTypeHandles(ref state);
            m_query = state.Fluent()
                .With<WeaponTag>(true)
                .With<Collider>( true)
                .PatchQueryForBuildingCollisionLayer()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_handles.Update(ref state);
            var physicsSettings = latiosWorld.GetPhysicsSettings();
            state.Dependency = Latios.Psyshock.Physics.BuildCollisionLayer(m_query, in m_handles)
                .WithSettings(physicsSettings.collisionLayerSettings)
                .ScheduleParallel(out var layer, state.WorldUpdateAllocator, state.Dependency);
            
            
            latiosWorld.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(new WeaponCollisionLayer 
            {
                Layer = layer
            });
        }


        public void OnNewScene(ref SystemState state)
        {
            latiosWorld.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld<WeaponCollisionLayer>(default);
        }
    }
}