using Latios;
using Latios.Anna.Systems;
using Survivors.Play.Systems.Enemies;
using Survivors.Play.Systems.Pathfinding;
using Survivors.Play.Systems.Physics;
using Survivors.Play.Systems.Physics.FindPairs;
using Survivors.Play.Systems.Player.Weapons.Physics;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateAfter(typeof(AnnaSuperSystem))]
    public partial class PostAnnaRootSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<BuildGridCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<BuildEnemyCollisionLayerSystem>();

            GetOrCreateAndAddUnmanagedSystem<PlayerTakeDamageSystem>();

            // GetOrCreateAndAddUnmanagedSystem<BuildPlayerCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<BuildWeaponCollisionLayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<FlowGridSystem>();
            GetOrCreateAndAddUnmanagedSystem<FlowFieldSystem>();
            GetOrCreateAndAddUnmanagedSystem<SkeletonHitInfosUpdateSystem>();
        }
    }
}