using Latios;
using R3;
using Survivors.GameScope.Commands;
using Survivors.Play.Components;
using Survivors.Play.Scope.Commands;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Systems.UI
{
    public partial class PlayerHudSystem : SubSystem
    {
        readonly ReactiveProperty<PlayerExperience> m_playerExperience = new();
        readonly ReactiveProperty<PlayerHealth>     m_playerHealth     = new();

        ICommandPublisher m_commandPublisher;

        DisposableBag m_disposableBag;

        int m_lastLevel = 1;


        [Inject]
        public void Construct(ICommandPublisher commandPublisher)
        {
            m_commandPublisher = commandPublisher;

            m_playerHealth.Subscribe(OnHealthChanged).AddTo(ref m_disposableBag);
            m_playerExperience.Subscribe(OnExpChanged).AddTo(ref m_disposableBag);
        }


        void OnHealthChanged(PlayerHealth playerHealth)
        {
            m_commandPublisher.PublishAsync(new PlayerHealthCommand
            {
                CurrentHealth = playerHealth.CurrentHealth,
                MaxHealth     = playerHealth.MaxHealth
            });

            if (playerHealth.CurrentHealth == 0)
                m_commandPublisher.PublishAsync(new PlayerDeadCommand());
        }

        void OnExpChanged(PlayerExperience playerExperience)
        {
            m_commandPublisher.PublishAsync(new PlayerExperienceCommand
            {
                CurrentExperience     = playerExperience.CurrentExperience,
                CurrentLevel          = playerExperience.CurrentLevel,
                ExperienceToNextLevel = playerExperience.ExperienceToNextLevel
            });

            if (m_lastLevel < m_playerExperience.Value.CurrentLevel)
            {
                m_lastLevel = m_playerExperience.Value.CurrentLevel;
                m_commandPublisher.PublishAsync(new PlayerLevelUpCommand
                {
                    Level = m_playerExperience.Value.CurrentLevel
                });

                sceneBlackboardEntity.AddComponent<PauseRequestedTag>();
                sceneBlackboardEntity.AddComponent<PlayerLevelUpScreenRequestedTag>();
            }
        }


        protected override void OnCreate()
        {
            RequireForUpdate<PlayerHealth>();
            RequireForUpdate<PlayerExperience>();
        }

        protected override void OnUpdate()
        {
            var playerHealth = latiosWorldUnmanaged.sceneBlackboardEntity.GetComponentData<PlayerHealth>();
            var playerExperience = latiosWorldUnmanaged.sceneBlackboardEntity.GetComponentData<PlayerExperience>();

            m_playerHealth.Value     = playerHealth;
            m_playerExperience.Value = playerExperience;
        }

        protected override void OnDestroy()
        {
            m_disposableBag.Dispose();
        }
    }
}