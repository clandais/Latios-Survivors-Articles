using Latios;
using Latios.Transforms.Systems;
using Survivors.Play.Components;
using Survivors.Play.Systems.Debug;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(PostTransformSuperSystem))]
    public partial class PostTransformRootSystem : RootSuperSystem
    {
        EntityQuery m_shouldUpdateQuery;

        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();

            GetOrCreateAndAddUnmanagedSystem<PhysicsDebugSystem>();
        }

        public override bool ShouldUpdateSystem()
        {
            return m_shouldUpdateQuery.IsEmptyIgnoreFilter;
        }
    }
}