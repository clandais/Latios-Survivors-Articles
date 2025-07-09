using Latios;
using Survivors.Play.Systems.Initialization;
using Unity.Entities;

namespace Survivors.Bootstrap.Systems.RootSystems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class SurvivorsInitializationSuperSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<SceneBlackBoardInitializationSystem>();
            // GetOrCreateAndAddUnmanagedSystem<BuildEnvironmentCollisionLayerSystem>();
        }
    }
}