using Latios;
using Latios.Transforms.Systems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Systems.Input;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(PreTransformSuperSystem))]
    public partial class InputRootSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddManagedSystem<EscapeKeySystem>();
            GetOrCreateAndAddManagedSystem<PlayerInputSuperSystem>();
        }
    }
}