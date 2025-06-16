using Latios;
using Survivors.Play.Components;
using Survivors.Play.Systems.Input;
using Survivors.Play.Systems.SFX;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class PlayerInputSuperSystem : SuperSystem
    {
        EntityQuery m_playerDeadQuery;
        EntityQuery m_shouldUpdateQuery;

        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent
                .With<PauseRequestedTag>()
                .Build();

            m_playerDeadQuery = Fluent
                .With<PlayerTag>()
                .With<DeadTag>()
                .Build();

            GetOrCreateAndAddManagedSystem<PlayerInputSystem>();
            GetOrCreateAndAddManagedSystem<MainAudioListenerUpdateSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter &&
                                                     m_playerDeadQuery.IsEmptyIgnoreFilter;
    }
}