using Latios;
using Survivors.Play.Systems.Items;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class ItemsMotionSuperSystem : SuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<XpMoveToPlayerSystem>();
        }
    }
}