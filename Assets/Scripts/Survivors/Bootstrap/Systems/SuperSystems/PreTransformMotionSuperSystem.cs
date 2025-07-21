using Latios;
using Survivors.Bootstrap.Systems.SuperSystems.PostTransformSubSystems;
using Survivors.Play.Components;
using Survivors.Play.Systems.Debug;
using Survivors.Play.Systems.Enemies;
using Survivors.Play.Systems.Physics;
using Survivors.Play.Systems.Physics.Movements;
using Survivors.Play.Systems.Player.Weapons.Physics;
using Unity.Entities;

namespace Survivors.Bootstrap.Systems.SuperSystems
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


            GetOrCreateAndAddUnmanagedSystem<BuildEnvironmentCollisionLayerSystem>();

            GetOrCreateAndAddUnmanagedSystem<BuildPlayerCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<BuildEnemyCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<BuildWeaponCollisionLayerSystem>();


            GetOrCreateAndAddUnmanagedSystem<CollideAndSlideSystem>();

            GetOrCreateAndAddUnmanagedSystem<SkeletonHitInfosUpdateSystem>();



            GetOrCreateAndAddManagedSystem<FindPairsSubSystem>();



            GetOrCreateAndAddUnmanagedSystem<PhysicsDebugSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter &&
                                                     m_playerQuery.IsEmptyIgnoreFilter;
    }
}