using Latios;
using Latios.Anna.Systems;
using Survivors.Play.Components;
using Survivors.Play.Systems.Enemies;
using Survivors.Play.Systems.Physics;
using Survivors.Play.Systems.Physics.FindPairs;
using Survivors.Play.Systems.Player.Weapons.Physics;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateAfter(typeof(AnnaSuperSystem))]
    public partial class PostAnnaRootSystem : RootSuperSystem
    {
        EntityQuery m_shouldUpdateQuery;

        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();

            // GetOrCreateAndAddUnmanagedSystem<BuildGridCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<BuildEnemyCollisionLayerSystem>();

            GetOrCreateAndAddUnmanagedSystem<PlayerTakeDamageSystem>();

            GetOrCreateAndAddUnmanagedSystem<BuildWeaponCollisionLayerSystem>();
            // GetOrCreateAndAddUnmanagedSystem<FlowGridSystem>();
            // GetOrCreateAndAddUnmanagedSystem<FlowFieldSystem>();
            GetOrCreateAndAddUnmanagedSystem<SkeletonHitInfosUpdateSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter;
    }
}