using System.Collections;
using System.Collections.Generic;
using Survivors.Play.Scope.Commands;
using Survivors.Play.Scope.Perks;
using Survivors.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Survivors.Play.Scope.MonoBehaviours
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayStateLevelUpMenu : MonoBehaviour
    {
        [SerializeField] List<LevelUpButton> perkButtons;

        [SerializeField] List<Perk>       perkList;
        public           UnityEvent<Perk> OnPerkSelected;
        CanvasGroup                       m_canvasGroup;

        public bool IsShown { get; private set; }


        void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
            m_canvasGroup.Hide();
            IsShown = false;
        }

        public void Show(PlayerLevelUpCommand playerLevelUpCommand)
        {
            if (IsShown)
                return;

            Cursor.visible = true;
            m_canvasGroup.Show();
            m_canvasGroup.interactable = false;
            IsShown                    = true;

            foreach (var perkButton in perkButtons)
            {
                // pick a random perk from the player's level up options
                perkButton.Button.onClick.RemoveAllListeners();
                perkButton.Button.onClick.AddListener(() => { OnPerkSelected.Invoke(perkButton.Perk); });

                // perkButton.gameObject.SetActive(true);

                // You can also set the text or image of the button to reflect the perk
                var perk = perkList[Random.Range(0, perkList.Count)];
                perkButton.SetPerk(perk);
            }

            // Delay interaction to avoid immediate clicks
            StartCoroutine(DelayInteraction(.25f));
        }

        IEnumerator DelayInteraction(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_canvasGroup.interactable = true;
        }


        public void Hide()
        {
            if (!IsShown)
                return;


            foreach (var perkButton in perkButtons) perkButton.Button.onClick.RemoveAllListeners();
            // perkButton.gameObject.SetActive(false);
            Cursor.visible = false;
            m_canvasGroup.Hide();
            IsShown = false;
        }
    }
}