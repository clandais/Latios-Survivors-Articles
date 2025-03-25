using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Survivors.GameScope.MonoBehaviours
{
    [AddComponentMenu("Survivors/CurtainBehaviour")]
    public class CurtainBehaviour : MonoBehaviour
    {
        [SerializeField] Image image;
		

        public IEnumerator FadeAlpha(float from, float to, float time)
        {
            float elapsedTime = 0;
            Color color = image.color;
            color.a     = from;
            image.color = color;

            while (elapsedTime < time)
            {
                elapsedTime += Time.deltaTime;
                color.a     =  Mathf.Lerp(from, to, elapsedTime / time);
                image.color =  color;
                yield return null;
            }

            color.a     = to;
            image.color = color;
        }
    }
}