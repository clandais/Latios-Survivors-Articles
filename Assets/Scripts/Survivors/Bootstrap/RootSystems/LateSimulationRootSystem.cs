using Latios;
using Survivors.Play.Systems.BlackBoard;
using Survivors.Play.Systems.Camera;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class LateSimulationRootSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<PlayerPositionUpdater>();
            GetOrCreateAndAddManagedSystem<CinemachineTargetUpdater>();
        }
    }
}