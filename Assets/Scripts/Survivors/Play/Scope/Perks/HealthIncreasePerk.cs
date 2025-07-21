using Survivors.Play.Components;
using UnityEngine;

namespace Survivors.Play.Scope.Perks
{
    [CreateAssetMenu(menuName = "Survivors/Play/Perk/Health Increase", fileName = "HealthIncreasePerk")]
    public class HealthIncreasePerk : Perk
    {
        [SerializeField] int healthIncreasePercentage;

        public PlayerHealth Apply(PlayerHealth playerHealth)
        {
            var healthIncrease = playerHealth.MaxHealth * healthIncreasePercentage / 100;
            playerHealth.MaxHealth += healthIncrease;
            return playerHealth;
        }
    }
}