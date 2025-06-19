using Latios;
using Survivors.Play.Systems.Initialization;
using Survivors.Play.Systems.Physics;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class InitializationRootSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<SceneBlackBoardInitializationSystem>();
            GetOrCreateAndAddUnmanagedSystem<BuildEnvironmentCollisionLayerSystem>();
        }
    }
}