using Latios;
using Survivors.Play.Authoring.Environment;
using Survivors.Play.Components;
using Survivors.Play.Systems.Physics.FindPairs;
using Unity.Entities;

namespace Survivors.Bootstrap.Systems.SuperSystems.PostTransformSubSystems
{
    public partial class FindPairsSubSystem : SuperSystem
    {
        EntityQuery m_shouldUpdateQuery;
        EntityQuery m_hasPhysicsSettingsQuery;

        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PlayerTag>()
                .With<DeadTag>()
                .Build();

            m_hasPhysicsSettingsQuery = Fluent
                .With<SceneBlackboardTag>()
                .With<PhysicsSettings>()
                .Build();

            GetOrCreateAndAddUnmanagedSystem<PlayerTakeDamageSystem>();
            GetOrCreateAndAddUnmanagedSystem<PlayerVsXpSystem>();
            GetOrCreateAndAddUnmanagedSystem<PlayerVsHpSystem>();
        }

        public override bool ShouldUpdateSystem() =>
            m_shouldUpdateQuery.IsEmptyIgnoreFilter &&
            !m_hasPhysicsSettingsQuery.IsEmptyIgnoreFilter;
    }
}