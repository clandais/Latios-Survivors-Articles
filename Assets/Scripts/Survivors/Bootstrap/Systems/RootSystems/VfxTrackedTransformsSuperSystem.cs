using Latios;
using Latios.Kinemation.Systems;
using Unity.Entities;

namespace Survivors.Bootstrap.Systems.RootSystems
{
    [UpdateInGroup(typeof(KinemationCustomGraphicsSetupSuperSystem))]
    public partial class VfxTrackedTransformsSuperSystem : RootSuperSystem
    {
        protected override void CreateSystems() { }
    }
}