using Latios;
using Survivors.Play.Systems.Enemies;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class EnemiesMotionSuperSystem : SuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<FollowPlayerSystem>();
        }
    }
}