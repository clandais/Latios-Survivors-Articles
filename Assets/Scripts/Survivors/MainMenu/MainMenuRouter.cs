using System;
using Cysharp.Threading.Tasks;
using Survivors.GameScope;
using Survivors.GameScope.Commands;
using VContainer;
using VitalRouter;

namespace Survivors.MainMenu
{
    public struct StartButtonClickedCommand : ICommand
    {
    }

    [Routes]
    public partial class MainMenuRouter : IDisposable
    {
        public ICommandPublisher ParentPublisher;

        [Route]
        void On(StartButtonClickedCommand _)
        {
            ParentPublisher.PublishAsync(new PlayStateCommand()).AsUniTask().Forget();
        }
        
        public void Dispose()
        {
            UnmapRoutes();
        }
    }
}