using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Survivors.Play.Scope.MonoBehaviours
{
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] Image    healthBar;
        [SerializeField] TMP_Text healthText;


        public void SetHealth(float health, float maxHealth)
        {
            healthBar.fillAmount = health / maxHealth;
            healthText.text      = $"{health}/{maxHealth}";
        }
    }
}