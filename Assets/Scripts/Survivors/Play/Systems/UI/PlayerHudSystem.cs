using Latios;
using R3;
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
        ICommandPublisher                       m_commandPublisher;

        DisposableBag m_disposableBag;

        [Inject]
        public void Construct(ICommandPublisher commandPublisher)
        {
            m_commandPublisher = commandPublisher;
            RequireForUpdate<PlayerHealth>();

            m_playerHealth.Subscribe(OnHealthChanged).AddTo(ref m_disposableBag);
        }


        void OnHealthChanged(PlayerHealth playerHealth)
        {
            m_commandPublisher.PublishAsync(new PlayerHealthCommand
            {
                CurrentHealth = playerHealth.CurrentHealth,
                MaxHealth     = playerHealth.MaxHealth
            });
        }

        protected override void OnUpdate()
        {
            foreach (var health in SystemAPI.Query<RefRO<PlayerHealth>>().WithAll<PlayerTag>())
                m_playerHealth.Value = health.ValueRO;
        }

        protected override void OnDestroy()
        {
            m_disposableBag.Dispose();
        }
    }
}