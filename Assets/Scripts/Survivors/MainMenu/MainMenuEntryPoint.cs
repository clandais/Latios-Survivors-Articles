using System;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using VitalRouter;

namespace Survivors.MainMenu
{
    public class MainMenuEntryPoint : IStartable, IDisposable
    {
        [Inject] ICommandPublisher m_commandPublisher;
        [Inject] MainMenuBehavior m_menuBehavior;

        public void Dispose()
        {
            m_menuBehavior.StartButton.onClick.RemoveListener(OnStartClicked);
        }

        public void Start()
        {
            m_menuBehavior.StartButton.onClick.AddListener(OnStartClicked);
        }

        void OnStartClicked()
        {
            m_commandPublisher.PublishAsync(new StartButtonClickedCommand())
                .AsUniTask().Forget();
        }
    }
}