using Latios;
using Latios.Systems;
using Latios.Transforms.Systems;
using Survivors.Play.Systems.Enemies;
using Survivors.Play.Systems.Player.Weapons.Initialization;
using Survivors.Play.Systems.Player.Weapons.Spawn;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(LatiosWorldSyncGroup))]
    public partial class SyncRootSystems : RootSuperSystem
    {
        protected override void CreateSystems()
        {

            GetOrCreateAndAddUnmanagedSystem<WeaponSpawnQueueSystem>();
            GetOrCreateAndAddUnmanagedSystem<DisableDeadCollidersSystem>();
        }
    }

    [UpdateInGroup(typeof(PostSyncPointGroup))]
    [UpdateBefore(typeof(MotionHistoryUpdateSuperSystem))]
    public partial class SkeletonSpawnSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<EnemySpawnerSystem>();
        }
    }
}