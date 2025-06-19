using Latios;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Survivors.Play.Systems.VFX;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems
{
    public partial class PreTransformAnimationSuperSystem : SuperSystem
    {
        EntityQuery m_pauseQuery;

        protected override void CreateSystems()
        {
            CreateQueries();
            GetOrCreateAndAddManagedSystem<AnimationSuperSystem>();
            GetOrCreateAndAddManagedSystem<PlayerAnimationSuperSystem>();
            GetOrCreateAndAddUnmanagedSystem<VfxPositionEventSpawnerSystem>();
        }

        void CreateQueries()
        {
            m_pauseQuery = Fluent.WithAnyEnabled<PauseRequestedTag>(true).Build();
        }

        public override bool ShouldUpdateSystem() => m_pauseQuery.IsEmptyIgnoreFilter;
    }
}