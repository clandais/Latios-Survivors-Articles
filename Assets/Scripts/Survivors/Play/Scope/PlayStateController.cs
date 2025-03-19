using System;
using R3;
using Survivors.GameScope.Commands;
using Survivors.Play.Scope.MonoBehaviours;
using VContainer;
using VContainer.Unity;
using VitalRouter;

namespace Survivors.Play.Scope
{
    public class PlayStateController : IStartable, IDisposable
    {
        [Inject] PlayStateMenu m_playStateMenu;
        [Inject] ICommandSubscribable m_commandSubscribable;
        [Inject] ICommandPublisher m_commandPublisher;

        DisposableBag m_disposable;

        public void Start()
        {
            m_disposable = new DisposableBag();

            m_playStateMenu.ResumeButton.OnClickAsObservable().Subscribe(OnResumeClicked)
                .AddTo(ref m_disposable);
            m_playStateMenu.MainMenuButton.OnClickAsObservable().Subscribe(OnMainMenuClicked)
                .AddTo(ref m_disposable);
            m_playStateMenu.QuitButton.OnClickAsObservable().Subscribe(OnQuitClicked)
                .AddTo(ref m_disposable);

            m_commandSubscribable.Subscribe<RequestPauseStateCommand>(OnPauseStateRequested)
                .AddTo(ref m_disposable);
            m_commandSubscribable.Subscribe<RequestResumeStateCommand>(OnResumeStateRequested)
                .AddTo(ref m_disposable);

            m_playStateMenu.Hide();
        }

        public void Dispose()
        {
            m_disposable.Dispose();
        }


        void OnPauseStateRequested(RequestPauseStateCommand _, PublishContext ctx)
        {
            m_playStateMenu.Show();
        }

        void OnResumeStateRequested(RequestResumeStateCommand _, PublishContext ctx)
        {
            m_playStateMenu.Hide();
        }

        void OnResumeClicked(Unit _)
        {
            m_commandPublisher.PublishAsync(new ResumeButtonClicked());
        }

        void OnMainMenuClicked(Unit _)
        {
            m_commandPublisher.PublishAsync(new BackToMainMenuClicked());
        }

        void OnQuitClicked(Unit _)
        {
            m_commandPublisher.PublishAsync(new ExitGameClicked());
        }
    }
}