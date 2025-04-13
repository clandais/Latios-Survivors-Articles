using Survivors.Play.Scope.MonoBehaviours;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Scope
{
    
    public struct DebugTextCommand : ICommand
    {
        public string Text;
    }
    
    [Routes]
    public partial class DebugRouter
    {
        [Inject] DebugCanvas  debugCanvas;
        
        [Route]
        void On(DebugTextCommand cmd)
        {
            debugCanvas.SetText(cmd.Text);
        }
    }
}