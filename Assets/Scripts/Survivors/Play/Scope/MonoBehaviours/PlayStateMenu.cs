using UnityEngine;
using UnityEngine.UI;

namespace Survivors.Play.Scope.MonoBehaviours
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayStateMenu : MonoBehaviour
    {
        [SerializeField] Button resumeButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button quitButton;

        public Button ResumeButton => resumeButton;
        public Button MainMenuButton => mainMenuButton;
        public Button QuitButton => quitButton;

        CanvasGroup m_canvasGroup;

        void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            m_canvasGroup.alpha          = 1;
            m_canvasGroup.blocksRaycasts = true;
            m_canvasGroup.interactable   = true;
        }

        public void Hide()
        {
            m_canvasGroup.alpha          = 0;
            m_canvasGroup.blocksRaycasts = false;
            m_canvasGroup.interactable   = false;
        }
    }
}