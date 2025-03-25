using Latios;
using Survivors.Play.Systems.Player;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class PlayerMotionSuperSystem : SuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<PlayerMovementSystem>();
        }
    }
}