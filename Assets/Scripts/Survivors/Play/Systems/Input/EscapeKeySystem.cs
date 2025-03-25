using Latios;
using R3;
using Survivors.GameScope.Commands;
using Survivors.Input;
using Survivors.Play.Components;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Systems.Input
{
    public partial class EscapeKeySystem : SubSystem
    {
        readonly ReactiveProperty<bool> m_isEscapePressed = new();
        ICommandPublisher m_commandPublisher;
        ICommandSubscribable m_commandSubscriber;
        DisposableBag m_disposableBag;
        InputSystem_Actions m_inputSystemActions;

        [Inject]
        public void Construct(ICommandPublisher commandPublisher,
            ICommandSubscribable commandSubscriber)
        {
            m_commandPublisher  = commandPublisher;
            m_commandSubscriber = commandSubscriber;

            m_commandSubscriber.Subscribe<RequestPauseStateCommand>((command,
                ctx) =>
            {
                sceneBlackboardEntity.AddComponent<PauseRequestedTag>();
            }).AddTo(ref m_disposableBag);

            m_commandSubscriber.Subscribe<RequestResumeStateCommand>((command,
                context) =>
            {
                sceneBlackboardEntity.RemoveComponent<PauseRequestedTag>();
            }).AddTo(ref m_disposableBag);

            m_isEscapePressed.Subscribe(OnEscapePressed)
                .AddTo(ref m_disposableBag);
        }

        void OnEscapePressed(bool isEscapePressed)
        {
            if (!isEscapePressed) return;

            if (sceneBlackboardEntity.HasComponent<PauseRequestedTag>())
                m_commandPublisher.PublishAsync(new RequestResumeStateCommand());
            else
                m_commandPublisher.PublishAsync(new RequestPauseStateCommand());
        }


        protected override void OnCreate()
        {
            m_inputSystemActions = new InputSystem_Actions();
            m_inputSystemActions.Enable();
        }

        protected override void OnUpdate()
        {
            m_isEscapePressed.Value = m_inputSystemActions.UI.Escape.ReadValue<float>() > .1f;
        }

        protected override void OnDestroy()
        {
            m_disposableBag.Dispose();
            m_inputSystemActions.Disable();
        }
    }
}