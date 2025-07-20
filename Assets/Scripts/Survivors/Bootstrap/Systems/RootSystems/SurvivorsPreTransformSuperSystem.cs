using Latios;
using Latios.Transforms.Systems;
using Survivors.Bootstrap.RootSystems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Systems.Input;
using Survivors.Play.Systems.Player;
using Survivors.Play.Systems.UI;
using Unity.Entities;

namespace Survivors.Bootstrap.Systems.RootSystems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSuperSystem))]
    public partial class SurvivorsPreTransformSuperSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddManagedSystem<EscapeKeySystem>();
            GetOrCreateAndAddManagedSystem<PlayerHudSystem>();
            GetOrCreateAndAddManagedSystem<PlayerApplyPerkSystem>();
            GetOrCreateAndAddManagedSystem<PlayerInputSuperSystem>();
            GetOrCreateAndAddManagedSystem<PreTransformMotionSuperSystem>();
            GetOrCreateAndAddManagedSystem<PreTransformAnimationSuperSystem>();
        }
    }
}