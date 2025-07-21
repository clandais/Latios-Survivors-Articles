using Cysharp.Threading.Tasks;
using Survivors.GameScope.Commands;
using Survivors.Play.Scope.Perks;
using Unity.Mathematics;
using VContainer;
using VitalRouter;
#if UNITY_EDITOR
using UnityEditor;

#else
using UnityEngine;
#endif

namespace Survivors.Play.Scope
{
    public struct MenuClosedCommand : ICommand { }

    public struct BackToMainMenuClicked : ICommand { }

    public struct ExitGameClicked : ICommand { }

    public struct PerkSelectedCommand : ICommand
    {
        public Perk Perk;
    }

    public struct MousePositionChangedCommand : ICommand
    {
        public float2 Position;
    }

    public struct MouseScrollChangedCommand : ICommand
    {
        public float ScrollDelta;
    }

    [Routes]
    public partial class PlayStateRouter
    {
        [Inject] ICommandPublisher commandPublisher;

        public ICommandPublisher ParentPublisher { get; set; }

        [Route]
        async UniTask On(MenuClosedCommand _)
        {
            await commandPublisher.PublishAsync(new RequestResumeStateCommand());
        }

        [Route]
        async UniTask On(BackToMainMenuClicked _)
        {
            await ParentPublisher.PublishAsync(new MainMenuStateCommand());
        }

        [Route]
        async UniTask On(MousePositionCommand cmd)
        {
            await commandPublisher.PublishAsync(new MousePositionChangedCommand
            {
                Position = cmd.MousePosition
            });
        }

        [Route]
        async UniTask On(MouseScrollCommand cmd)
        {
            await commandPublisher.PublishAsync(new MouseScrollChangedCommand
            {
                ScrollDelta = cmd.ScrollDelta
            });
        }


        [Route]
        void On(ExitGameClicked _)
        {
#if UNITY_EDITOR
            // pretty handy when you want to stop the game in the editor
            EditorApplication.ExitPlaymode();
#else
			Application.Quit(0);
#endif
        }
    }
}