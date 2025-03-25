using Latios;
using Latios.Anna.Systems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateBefore(typeof(AnnaSuperSystem))]
    public partial class  PreRigidBodyRootSystem : RootSuperSystem
    {
        
        EntityQuery m_shouldUpdateQuery;
        
        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();
            
            GetOrCreateAndAddManagedSystem<PlayerMotionSuperSystem>();
        }

        public override bool ShouldUpdateSystem()
        {
            return m_shouldUpdateQuery.IsEmptyIgnoreFilter;
        }
    }
}