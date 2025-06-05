using Latios;
using Latios.Anna.Systems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateBefore(typeof(AnnaSuperSystem))]
    public partial class PreRigidBodyRootSystem : RootSuperSystem
    {
        EntityQuery m_playerQuery;
        EntityQuery m_shouldUpdateQuery;

        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();

            m_playerQuery = Fluent.With<PlayerTag>()
                .With<DeadTag>()
                .Build();


            GetOrCreateAndAddManagedSystem<PlayerMotionSuperSystem>();
            GetOrCreateAndAddManagedSystem<EnemiesMotionSuperSystem>();
            GetOrCreateAndAddManagedSystem<WeaponUpdateSuperSystem>();

            // GetOrCreateAndAddUnmanagedSystem<PhysicsDebugSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter &&
                                                     m_playerQuery.IsEmptyIgnoreFilter;
    }
}