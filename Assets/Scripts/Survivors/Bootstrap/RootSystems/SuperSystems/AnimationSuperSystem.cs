using Latios;
using Survivors.Play.Components;
using Survivors.Play.Systems.Animations;
using Unity.Entities;

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