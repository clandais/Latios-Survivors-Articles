using Latios;
using Survivors.Play.Systems.Animations;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class AnimationSuperSystem : SuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<PlayerIdleAnimationSystem>();
        }
    }
}