using Latios;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Survivors.Play.Systems.Debug;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    public partial class PreTransformMotionSuperSystem : SuperSystem
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
            GetOrCreateAndAddManagedSystem<ItemsMotionSuperSystem>();

            GetOrCreateAndAddUnmanagedSystem<PhysicsDebugSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter &&
                                                     m_playerQuery.IsEmptyIgnoreFilter;
    }
}