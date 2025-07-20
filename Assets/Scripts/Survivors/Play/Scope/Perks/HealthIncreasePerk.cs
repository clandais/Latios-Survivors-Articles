using Survivors.Play.Components;
using UnityEngine;

namespace Survivors.Play.Scope.Perks
{
    public abstract class Perk : ScriptableObject
    {
        public string description;
    }

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

    // public interface IPerk<in T> where T : IComponentData
    // {
    //     public void Execute(T component);
    // }
    //
    // public struct HealthIncreasePerk : IPerk<PlayerHealth>
    // {
    //     public int HealthIncreasePercentage;
    //
    //     public void Execute(PlayerHealth component)
    //     {
    //         var healthIncrease = component.MaxHealth * HealthIncreasePercentage / 100;
    //         component.MaxHealth += healthIncrease;
    //     }
    // }
}