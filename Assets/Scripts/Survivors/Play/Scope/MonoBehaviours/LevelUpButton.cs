using Survivors.Play.Scope.Perks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Survivors.Play.Scope.MonoBehaviours
{
    [RequireComponent(typeof(Button))]
    public class LevelUpButton : MonoBehaviour
    {
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Button   button;

        public Button Button => button;
        public Perk Perk { get; private set; }


        public void SetPerk(Perk perk)
        {
            Perk                 = perk;
            descriptionText.text = perk.description;
        }
    }
}