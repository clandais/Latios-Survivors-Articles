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
        }

        void OnPlayerHealthChanged(PlayerHealthCommand cmd, PublishContext ctx)
        {
            m_playerHud.SetHealth(cmd.CurrentHealth, cmd.MaxHealth);
        }
    }
}