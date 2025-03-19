using UnityEngine;
using UnityEngine.UI;

namespace Survivors.MainMenu
{
    [AddComponentMenu("Survivors/Main Menu/Main Menu Behavior")]
    public class MainMenuBehavior : MonoBehaviour
    {
        [SerializeField] Button startButton;
        public Button StartButton => startButton;
    }
}