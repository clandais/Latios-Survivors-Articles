using Latios;
using Latios.Transforms.Systems;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Survivors.Play.Systems.VFX;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    [UpdateBefore(typeof(TransformSuperSystem))]
    public partial class PreTransformRootSystem : RootSuperSystem
    {
        EntityQuery m_pauseQuery;

        protected override void CreateSystems()
        {
            CreateQueries();
            GetOrCreateAndAddManagedSystem<AnimationSuperSystem>();
            GetOrCreateAndAddUnmanagedSystem<VfxPositionEventSpawnerSystem>();
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