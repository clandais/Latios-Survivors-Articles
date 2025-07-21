using Latios;
using Survivors.Bootstrap.RootSystems.SuperSystems;
using Survivors.Play.Components;
using Unity.Entities;

namespace Survivors.Bootstrap.Systems.SuperSystems
{
    public partial class PostTransformSubSystem : SuperSystem
    {
        EntityQuery m_shouldUpdateQuery;

        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();




            GetOrCreateAndAddManagedSystem<PlayerMotionSuperSystem>();
            GetOrCreateAndAddManagedSystem<EnemiesMotionSuperSystem>();
            GetOrCreateAndAddManagedSystem<ItemsMotionSuperSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter;
    }
}