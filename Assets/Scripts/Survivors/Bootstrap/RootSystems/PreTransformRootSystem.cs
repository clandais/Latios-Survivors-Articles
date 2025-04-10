using Latios;
using Latios.Anna.Systems;
using Latios.Transforms.Systems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateInGroup(typeof(PreTransformSuperSystem))]
    public partial class PreTransformRootSystem : RootSuperSystem
    {
        EntityQuery m_pauseQuery;

        protected override void CreateSystems()
        {
            CreateQueries();
            GetOrCreateAndAddManagedSystem<AnimationSuperSystem>();
        }

        void CreateQueries()
        {
            m_pauseQuery = Fluent.WithAnyEnabled<PauseRequestedTag>(true).Build();
        }

        public override bool ShouldUpdateSystem()
        {
            return m_pauseQuery.IsEmptyIgnoreFilter;
        }
    }
}