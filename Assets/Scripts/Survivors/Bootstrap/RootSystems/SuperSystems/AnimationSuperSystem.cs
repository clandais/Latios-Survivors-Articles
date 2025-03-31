using Latios;
using Survivors.Play.Systems.Animations;
using Survivors.Play.Systems.Player;
using Survivors.Play.Systems.Player.Weapons.Initialization;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class AnimationSuperSystem : SuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<FourDirectionsAnimationSystem>();
            GetOrCreateAndAddUnmanagedSystem<PlayerActionSystem>();
            GetOrCreateAndAddUnmanagedSystem<WeaponThrowTriggerSystem>();
        }
    }
}