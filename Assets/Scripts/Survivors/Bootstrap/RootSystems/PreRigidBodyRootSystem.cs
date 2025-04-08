using Latios;
using Latios.Anna.Systems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Survivors.Play.Systems.Pathfinding;
using Survivors.Play.Systems.Physics;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateAfter(typeof(AnnaSuperSystem))]
    public partial class BuildCollisionLayersSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<BuildEnemyCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<FlowGridSystem>();
            GetOrCreateAndAddUnmanagedSystem<FlowFieldSystem>();
        }
    }
    
    
    [UpdateBefore(typeof(AnnaSuperSystem))]
    public partial class  PreRigidBodyRootSystem : RootSuperSystem
    {
        
        EntityQuery m_shouldUpdateQuery;
        
        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();



            
            GetOrCreateAndAddManagedSystem<PlayerMotionSuperSystem>();
            GetOrCreateAndAddManagedSystem<EnemiesMotionSuperSystem>();
            GetOrCreateAndAddManagedSystem<WeaponUpdateSuperSystem>();
            
        }

        public override bool ShouldUpdateSystem()
        {
            return m_shouldUpdateQuery.IsEmptyIgnoreFilter;
        }
    }
}