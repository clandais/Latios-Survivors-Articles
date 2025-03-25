using System;
using Cysharp.Threading.Tasks;
using Survivors.GameScope.Commands;
using VitalRouter;

namespace Survivors.MainMenu
{
    public struct StartButtonClickedCommand : ICommand { }

    [Routes]
    public partial class MainMenuRouter : IDisposable
    {
        public ICommandPublisher ParentPublisher;

        public void Dispose()
        {
            UnmapRoutes();
        }

        [Route]
        void On(StartButtonClickedCommand _)
        {
            ParentPublisher.PublishAsync(new PlayStateCommand()).AsUniTask().Forget();
        }
    }
}