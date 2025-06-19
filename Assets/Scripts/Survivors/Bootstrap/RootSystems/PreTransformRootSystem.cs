using Latios;
using Latios.Transforms.Systems;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(PreTransformSuperSystem))]
    public partial class PreTransformRootSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            // GetOrCreateAndAddUnmanagedSystem<SceneBlackBoardInitializationSystem>();
            GetOrCreateAndAddManagedSystem<PreTransformMotionSuperSystem>();
            GetOrCreateAndAddManagedSystem<PreTransformAnimationSuperSystem>();
        }
    }
}