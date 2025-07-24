using System.Collections;
using Survivors.Play.Scope.Commands;
using Survivors.Utilities;
using TMPro;
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


        [Header("Game Over State")] [SerializeField]
        TMP_Text gameOverText;

        [SerializeField] Color gameOverBackgroundColor;

        [SerializeField] Image backgroundImage;

        [Header("Player Dead State")] [SerializeField]
        Color deadBackgroundColor = new(0.2f, 0.2f, 0.2f, 0.8f);

        [SerializeField] TMP_Text deadText;

        [SerializeField] float fadeDuration = 1f;

        CanvasGroup m_canvasGroup;
        Color       m_initialBackgroundColor;


        public Button ResumeButton => resumeButton;
        public Button MainMenuButton => mainMenuButton;
        public Button QuitButton => quitButton;


        void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
            deadText.gameObject.SetActive(false);
            gameOverText.gameObject.SetActive(false);
            m_initialBackgroundColor = backgroundImage.color;
        }

        public void Show()
        {
            Cursor.visible = true;
            m_canvasGroup.Show();
        }

        public void Hide()
        {
            Cursor.visible = false;
            m_canvasGroup.Hide();
        }

        public void ShowDead()
        {
            Cursor.visible = true;
            m_canvasGroup.Show();

            resumeButton.gameObject.SetActive(false);
            deadText.gameObject.SetActive(true);

            StartCoroutine(FadeToDeath());
        }

        IEnumerator FadeToDeath()
        {
            var elapsedTime = 0f;

            var targetColor = deadBackgroundColor;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / fadeDuration;
                backgroundImage.color = Color.Lerp(m_initialBackgroundColor, targetColor, t);
                yield return null;
            }
        }

        public void ShowGameOver(TimerEndedCommand cmd)
        {
            Cursor.visible = true;
            m_canvasGroup.Show();

            resumeButton.gameObject.SetActive(false);
            gameOverText.gameObject.SetActive(true);

            gameOverText.text = $"Game Over!\nYou killed {cmd.Kills} enemies.";

            StartCoroutine(FadeToGameOver());
        }

        IEnumerator FadeToGameOver()
        {
            var elapsedTime = 0f;

            var targetColor = gameOverBackgroundColor;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / fadeDuration;
                backgroundImage.color = Color.Lerp(m_initialBackgroundColor, targetColor, t);
                yield return null;
            }
        }
    }
}