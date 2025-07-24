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

        [SerializeField] Image    experienceBar;
        [SerializeField] TMP_Text experienceText;

        [SerializeField] TMP_Text timeText;

        public void SetHealth(float health, float maxHealth)
        {
            healthBar.fillAmount = health / maxHealth;
            healthText.text      = $"{health}/{maxHealth}";
        }

        public void SetExperience(PlayerExperienceCommand cmd)
        {
            experienceBar.fillAmount = (float)cmd.CurrentExperience / cmd.ExperienceToNextLevel;
            experienceText.text =
                $"Lvl : {cmd.CurrentLevel} | Exp : {cmd.CurrentExperience}/{cmd.ExperienceToNextLevel}";
        }

        public void SetTimeRemaining(float timeRemaining)
        {
            var minutes = Mathf.FloorToInt(timeRemaining / 60);
            var seconds = Mathf.FloorToInt(timeRemaining % 60);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}