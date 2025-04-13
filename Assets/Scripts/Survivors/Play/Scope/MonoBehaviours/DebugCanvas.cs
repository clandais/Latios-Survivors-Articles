using TMPro;
using UnityEngine;

namespace Survivors.Play.Scope.MonoBehaviours
{
    public class DebugCanvas : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        
        public void SetText(string newText)
        {
            text.text = newText;
        }
    }
}