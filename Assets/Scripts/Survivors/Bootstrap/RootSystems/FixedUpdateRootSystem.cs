using Latios;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class FixedUpdateRootSystem : RootSuperSystem
    {
        protected override void CreateSystems() { }
    }
}