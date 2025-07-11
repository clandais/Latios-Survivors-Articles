using Latios;
using R3;
using Survivors.GameScope.Commands;
using Survivors.Play.Components;
using Survivors.Play.Scope.Commands;
using Unity.Entities;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Systems.UI
{
    public partial class PlayerHudSystem : SubSystem
    {
        readonly ReactiveProperty<PlayerHealth> m_playerHealth = new();
        readonly ReactiveProperty<PlayerExperience> m_playerExperience = new();
        
        ICommandPublisher                       m_commandPublisher;

        DisposableBag m_disposableBag;

        [Inject]
        public void Construct(ICommandPublisher commandPublisher)
        {
            m_commandPublisher = commandPublisher;
            RequireForUpdate<PlayerHealth>();

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
            
            UnityEngine.Debug.Log($"Player Health Changed: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
        }

        void OnExpChanged(PlayerExperience playerExperience)
        {
            m_commandPublisher.PublishAsync(new PlayerExperienceCommand
            {
                CurrentExperience = playerExperience.CurrentExperience
            });
            
            UnityEngine.Debug.Log($"Player Experience Changed: {playerExperience.CurrentExperience}");
        }
        
        protected override void OnUpdate()
        {
            foreach (var (health, xp) in SystemAPI.Query<RefRO<PlayerHealth>, RefRO<PlayerExperience>>().WithAll<PlayerTag>())
            {
                m_playerHealth.Value = health.ValueRO;
                m_playerExperience.Value = xp.ValueRO;
            }
        }

        protected override void OnDestroy()
        {
            m_disposableBag.Dispose();
        }
    }
}