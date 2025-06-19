using Latios;
using Latios.Systems;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(PostSyncPointGroup), OrderFirst = true)]
    public partial class PreRenderTransformSuperSystem : RootSuperSystem
    {
        protected override void CreateSystems()
        {
            // GetOrCreateAndAddSystem(typeof(TransformSystemGroup));
        }
    }
}