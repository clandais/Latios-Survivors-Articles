using Unity.Cinemachine;
using UnityEngine;

namespace Survivors.GameScope.MonoBehaviours
{
    [AddComponentMenu("Survivors/Cinemachine Behaviour")]
    public class CinemachineBehaviour : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] CinemachineSplineDolly dolly;

        public void SetTargetPosition(Vector3 position)
        {
            target.position = position;
        }
    }
}