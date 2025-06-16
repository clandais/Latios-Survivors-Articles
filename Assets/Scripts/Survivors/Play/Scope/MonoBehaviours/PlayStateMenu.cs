using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Survivors.Play.Scope.MonoBehaviours
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayStateMenu : MonoBehaviour
    {
        [SerializeField] Button   resumeButton;
        [SerializeField] Button   mainMenuButton;
        [SerializeField] Button   quitButton;
        [SerializeField] TMP_Text deadText;

        CanvasGroup m_canvasGroup;

        public Button ResumeButton => resumeButton;
        public Button MainMenuButton => mainMenuButton;
        public Button QuitButton => quitButton;

        void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
            deadText.gameObject.SetActive(false);
        }

        public void Show()
        {
            Cursor.visible               = true;
            m_canvasGroup.alpha          = 1;
            m_canvasGroup.blocksRaycasts = true;
            m_canvasGroup.interactable   = true;
        }

        public void Hide()
        {
            Cursor.visible               = false;
            m_canvasGroup.alpha          = 0;
            m_canvasGroup.blocksRaycasts = false;
            m_canvasGroup.interactable   = false;
        }

        public void ShowDead()
        {
            Cursor.visible               = true;
            m_canvasGroup.alpha          = 1;
            m_canvasGroup.blocksRaycasts = true;
            m_canvasGroup.interactable   = true;

            resumeButton.gameObject.SetActive(false);
            deadText.gameObject.SetActive(true);
        }
    }
}