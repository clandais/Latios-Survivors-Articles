using VitalRouter;

namespace Survivors.Play.Scope.Commands
{
    public struct PlayerHealthCommand : ICommand
    {
        public int CurrentHealth;
        public int MaxHealth;
    }
}