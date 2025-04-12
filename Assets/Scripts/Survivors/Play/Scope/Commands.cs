using Unity.Mathematics;
using VitalRouter;

namespace Survivors.Play.Scope
{
    public struct MousePositionCommand : ICommand
    {
        public float2 MousePosition;
    }
    
    public struct MouseScrollCommand : ICommand
    {
        public float ScrollDelta;
    }
}