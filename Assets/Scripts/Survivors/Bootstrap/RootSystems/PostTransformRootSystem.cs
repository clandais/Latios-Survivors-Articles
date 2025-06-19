using Latios;
using Latios.Transforms.Systems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Survivors.Play.Systems.BlackBoard;
using Survivors.Play.Systems.Camera;
using Survivors.Play.Systems.Debug;
using Survivors.Play.Systems.UI;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(PostTransformSuperSystem))]
    public partial class PostTransformRootSystem : RootSuperSystem
    {
        EntityQuery m_shouldUpdateQuery;

        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();

            GetOrCreateAndAddManagedSystem<PostTransformSubSystem>();

            GetOrCreateAndAddManagedSystem<WeaponUpdateSuperSystem>();

            GetOrCreateAndAddUnmanagedSystem<PlayerPositionUpdater>();
            GetOrCreateAndAddManagedSystem<CinemachineTargetUpdater>();
            GetOrCreateAndAddManagedSystem<EnemyCounterSystem>();
            GetOrCreateAndAddManagedSystem<PlayerHudSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter;
    }
}