using Latios;
using Latios.Systems;
using Latios.Transforms.Systems;
using Survivors.Play.Components;
using Survivors.Play.Systems.Enemies;
using Survivors.Play.Systems.Player.Weapons.Spawn;
using Survivors.Play.Systems.SFX;
using Survivors.Play.Systems.VFX;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(LatiosWorldSyncGroup))]
    public partial class SyncRootSystems : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<WeaponSpawnQueueSystem>();
            GetOrCreateAndAddUnmanagedSystem<SfxSpawnQueueSystem>();
            GetOrCreateAndAddUnmanagedSystem<VfxSpawnQueueSystem>();
            GetOrCreateAndAddUnmanagedSystem<DisableDeadCollidersSystem>();
        }
    }

    [UpdateInGroup(typeof(PostSyncPointGroup))]
    [UpdateBefore(typeof(MotionHistoryUpdateSuperSystem))]
    public partial class SkeletonSpawnSystem : RootSuperSystem
    {
        EntityQuery m_pauseQuery;

        void CreateQueries()
        {
            m_pauseQuery = Fluent.WithAnyEnabled<PauseRequestedTag>(true).Build();
        }

        protected override void CreateSystems()
        {
            CreateQueries();
            GetOrCreateAndAddUnmanagedSystem<EnemySpawnerSystem>();
        }

        public override bool ShouldUpdateSystem()
        {
            return m_pauseQuery.IsEmptyIgnoreFilter;
        }
    }
}