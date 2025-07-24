using VitalRouter;

namespace Survivors.Play.Scope.Commands
{
    public struct PlayerHealthCommand : ICommand
    {
        public int CurrentHealth;
        public int MaxHealth;
    }

    public struct PlayerExperienceCommand : ICommand
    {
        public int CurrentExperience;
        public int ExperienceToNextLevel;
        public int CurrentLevel;
    }

    public struct PlayerLevelUpCommand : ICommand
    {
        public int Level;
    }

    public struct UpdateTimeCommand : ICommand
    {
        public float TimeRemaining;
    }

    public struct TimerEndedCommand : ICommand
    {
        public int Kills;
    }
}