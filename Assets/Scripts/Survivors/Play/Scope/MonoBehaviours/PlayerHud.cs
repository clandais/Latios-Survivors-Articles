using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Survivors.Play.Scope.MonoBehaviours
{
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] Image    healthBar;
        [SerializeField] TMP_Text healthText;

        [SerializeField] TMP_Text experienceText;

        public void SetHealth(float health, float maxHealth)
        {
            healthBar.fillAmount = health / maxHealth;
            healthText.text      = $"{health}/{maxHealth}";
        }

        public void SetExperience(int expAmount)
        {
            experienceText.text = $"XP: {expAmount}";
        }
    }
}