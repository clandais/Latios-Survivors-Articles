using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Survivors.Play.Authoring.SceneBlackBoard;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Physics
{
    
    
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct BuildGridCollisionLayerSystem : ISystem, ISystemNewScene
    {
    
        LatiosWorldUnmanaged m_latiosWorldUnmanaged;
        BuildCollisionLayerTypeHandles m_typeHandles;
        EntityQuery m_query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_typeHandles          = new BuildCollisionLayerTypeHandles(ref state);
            m_query = state.Fluent()
                .With<GridCollisionSettings>()
                .With<Collider>()
                .PatchQueryForBuildingCollisionLayer()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_typeHandles.Update(ref state);


           var settings = m_latiosWorldUnmanaged.GetPhysicsSettings();
            
            state.Dependency = Latios.Psyshock.Physics.BuildCollisionLayer(m_query, in m_typeHandles )
                .WithSettings( new CollisionLayerSettings
                {
                    worldAabb = settings.collisionLayerSettings.worldAabb,
                    worldSubdivisionsPerAxis = settings.collisionLayerSettings.worldSubdivisionsPerAxis,
                })
                .ScheduleParallel(out var gridCollisionLayer, state.WorldUpdateAllocator, state.Dependency);
            
            m_latiosWorldUnmanaged.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld( new GridCollisionLayer
            {
                Layer = gridCollisionLayer,
            });
            
            
            state.Enabled = false;
            
            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        public void OnNewScene(ref SystemState state)
        {
            m_latiosWorldUnmanaged.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld<GridCollisionLayer>(default);
        }
    }
}