using Survivors.Play.Scope.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Survivors.Play.Scope.MonoBehaviours
{
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] Image    healthBar;
        [SerializeField] TMP_Text healthText;

        [SerializeField] Image experienceBar;
        [SerializeField] TMP_Text experienceText;

        public void SetHealth(float health, float maxHealth)
        {
            healthBar.fillAmount = health / maxHealth;
            healthText.text      = $"{health}/{maxHealth}";
        }

        public void SetExperience(PlayerExperienceCommand cmd)
        {
            experienceBar.fillAmount = (float)cmd.CurrentExperience / cmd.ExperienceToNextLevel;
            experienceText.text = $"Lvl : {cmd.CurrentLevel} | Exp : {cmd.CurrentExperience}/{cmd.ExperienceToNextLevel}";
        }
    }
}