using Cysharp.Threading.Tasks;
using Survivors.GameScope.Commands;
using Unity.Mathematics;
using UnityEditor;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Scope
{
    public struct ResumeButtonClicked : ICommand { }
    public struct BackToMainMenuClicked : ICommand { }
    public struct ExitGameClicked : ICommand { }

    public struct MousePositionChangedCommand : ICommand
    {
        public float2 Position;
    }

    [Routes]
    public partial class PlayStateRouter
    {
        [Inject] ICommandPublisher commandPublisher;

        public ICommandPublisher ParentPublisher { get; set; }

        [Route]
        async UniTask On(ResumeButtonClicked _)
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
        void On(ExitGameClicked _)
        {
#if UNITY_EDITOR
            // pretty handy when you want to stop the game in the editor
            EditorApplication.ExitPlaymode();
#else
			Application.Quit(0)	;
#endif
        }
    }
}