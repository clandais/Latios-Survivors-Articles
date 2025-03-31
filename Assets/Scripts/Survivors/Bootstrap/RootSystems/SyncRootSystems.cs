using Latios;
using Latios.Systems;
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
        }
    }
}