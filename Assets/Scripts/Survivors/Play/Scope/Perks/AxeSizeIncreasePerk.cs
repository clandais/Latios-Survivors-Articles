using Survivors.Play.Authoring.SceneBlackBoard;
using UnityEngine;

namespace Survivors.Play.Scope.Perks
{
    [CreateAssetMenu(menuName = "Survivors/Play/Perk/Axe Size Increase", fileName = "AxeSizeIncreasePerk")]
    public class AxeSizeIncreasePerk : Perk
    {
        [SerializeField] float axeSizeIncreasePercentage;

        public WeaponPerks Apply(WeaponPerks weaponPerks)
        {
            var sizeIncrease = weaponPerks.Size * axeSizeIncreasePercentage / 100;
            weaponPerks.Size += sizeIncrease;
            return weaponPerks;
        }
    }
}