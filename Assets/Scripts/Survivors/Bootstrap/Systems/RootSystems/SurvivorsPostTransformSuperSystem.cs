using Latios;
using Latios.Transforms.Systems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Bootstrap.Systems.SuperSystems;
using Survivors.Play.Components;
using Survivors.Play.Systems.BlackBoard;
using Survivors.Play.Systems.Camera;
using Survivors.Play.Systems.Lifecycle;
using Survivors.Play.Systems.Player;
using Unity.Entities;

namespace Survivors.Bootstrap.Systems.RootSystems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSuperSystem))]
    public partial class SurvivorsPostTransformSuperSystem : RootSuperSystem
    {
        EntityQuery m_shouldUpdateQuery;

        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();

            GetOrCreateAndAddManagedSystem<PostTransformSubSystem>();

            GetOrCreateAndAddManagedSystem<WeaponUpdateSuperSystem>();

            GetOrCreateAndAddUnmanagedSystem<PlayerPositionUpdater>();
            GetOrCreateAndAddUnmanagedSystem<PlayerProcessExpSystem>();
            GetOrCreateAndAddUnmanagedSystem<PlayerProcessHpSystem>();

            GetOrCreateAndAddManagedSystem<CinemachineTargetUpdater>();
            // GetOrCreateAndAddManagedSystem<EnemyCounterSystem>();

            GetOrCreateAndAddUnmanagedSystem<DestroyTaggedEntitiesSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter;
    }
}