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

        CanvasGroup m_canvasGroup;

        public Button ResumeButton => resumeButton;
        public Button MainMenuButton => mainMenuButton;
        public Button QuitButton => quitButton;

        void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
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
    }
}