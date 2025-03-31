using Latios;
using Survivors.Play.Systems.Player.Weapons.Physics;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class WeaponUpdateSuperSystem : SuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<ThrownWeaponUpdateSystem>();
        }
    }
}