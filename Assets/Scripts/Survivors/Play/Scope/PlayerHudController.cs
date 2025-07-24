using System;
using R3;
using Survivors.Play.Scope.Commands;
using Survivors.Play.Scope.MonoBehaviours;
using VContainer;
using VContainer.Unity;
using VitalRouter;

namespace Survivors.Play.Scope
{
    public class PlayerHudController : IStartable, IDisposable
    {
        [Inject] ICommandSubscribable m_commandSubscribable;

        DisposableBag      m_disposable;
        [Inject] PlayerHud m_playerHud;

        public void Dispose()
        {
            m_disposable.Dispose();
        }

        public void Start()
        {
            m_disposable = new DisposableBag();

            m_commandSubscribable.Subscribe<PlayerHealthCommand>(OnPlayerHealthChanged)
                .AddTo(ref m_disposable);

            m_commandSubscribable.Subscribe<PlayerExperienceCommand>(OnPlayerExperienceChanged)
                .AddTo(ref m_disposable);

            m_commandSubscribable.Subscribe<UpdateTimeCommand>(OnUpdateTime)
                .AddTo(ref m_disposable);
        }

        void OnUpdateTime(UpdateTimeCommand updateTimeCommand, PublishContext ctx)
        {
            m_playerHud.SetTimeRemaining(updateTimeCommand.TimeRemaining);
        }

        void OnPlayerHealthChanged(PlayerHealthCommand cmd, PublishContext ctx)
        {
            m_playerHud.SetHealth(cmd.CurrentHealth, cmd.MaxHealth);
        }

        void OnPlayerExperienceChanged(PlayerExperienceCommand cmd, PublishContext ctx)
        {
            m_playerHud.SetExperience(cmd);
        }
    }
}