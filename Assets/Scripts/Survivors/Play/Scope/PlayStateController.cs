using System;
using R3;
using Survivors.GameScope.Commands;
using Survivors.GameScope.MonoBehaviours;
using Survivors.Play.Scope.MonoBehaviours;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using VitalRouter;

namespace Survivors.Play.Scope
{
    public class PlayStateController : IStartable, IDisposable
    {
        [Inject] CinemachineBehaviour m_cinemachineCamera;
        [Inject] ICommandPublisher    m_commandPublisher;
        [Inject] ICommandSubscribable m_commandSubscribable;
        [Inject] Image                m_crosshair;

        DisposableBag          m_disposable;
        [Inject] PlayStateMenu m_playStateMenu;

        public void Dispose()
        {
            m_disposable.Dispose();
        }

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

            m_commandSubscribable.Subscribe<MousePositionChangedCommand>(OnMousePositionChanged)
                .AddTo(ref m_disposable);

            m_commandSubscribable.Subscribe<MouseScrollChangedCommand>(OnMouseScrollChanged)
                .AddTo(ref m_disposable);

            m_commandSubscribable.Subscribe<PlayerDeadCommand>(OnPlayerDead)
                .AddTo(ref m_disposable);

            m_playStateMenu.Hide();
        }

        void OnPlayerDead(PlayerDeadCommand _, PublishContext ctx)
        {
            m_playStateMenu.ShowDead();
        }


        void OnPauseStateRequested(RequestPauseStateCommand _,
            PublishContext ctx)
        {
            m_playStateMenu.Show();
        }


        void OnResumeStateRequested(RequestResumeStateCommand _,
            PublishContext ctx)
        {
            m_playStateMenu.Hide();
        }

        void OnMousePositionChanged(MousePositionChangedCommand cmd,
            PublishContext ctx)
        {
            m_crosshair.rectTransform.position = new Vector3(cmd.Position.x, cmd.Position.y, 0);
        }

        void OnMouseScrollChanged(MouseScrollChangedCommand cmd,
            PublishContext arg2)
        {
            m_cinemachineCamera.Zoom(cmd.ScrollDelta);
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