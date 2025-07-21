using Survivors.Play.Components;
using UnityEngine;

namespace Survivors.Play.Scope.Perks
{
    [CreateAssetMenu(menuName = "Survivors/Play/Perk/Movement Speed Increase", fileName = "MovementSpeedIncreasePerk",
        order = 0)]
    public class MovementSpeedIncreasePerk : Perk
    {
        [SerializeField] int movementSpeedIncreasePercentage;

        public MovementSettings Apply(MovementSettings movementSettings)
        {
            var speedIncrease = movementSettings.moveSpeed * movementSpeedIncreasePercentage / 100;
            movementSettings.moveSpeed += speedIncrease;
            return movementSettings;
        }
    }
}