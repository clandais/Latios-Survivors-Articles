using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.GameScope.MonoBehaviours
{
    [AddComponentMenu("Survivors/Cinemachine Behaviour")]
    public class CinemachineBehaviour : MonoBehaviour
    {
        [SerializeField] Transform playerPositionTarget;
        [SerializeField] Transform playerAimTarget;

        [SerializeField] CinemachineSplineDolly dolly;

        [SerializeField] float scrollSensitivity = 1f;
        [SerializeField] float maxZoomDistanceDelta = 0.1f;
        [SerializeField] float defaultZoomDistance = .5f;


        float targetZoom;

        void Awake()
        {
            dolly.CameraPosition = defaultZoomDistance;
            targetZoom           = defaultZoomDistance;
        }


        void Update()
        {
            dolly.CameraPosition =
                Mathf.MoveTowards(dolly.CameraPosition, targetZoom, maxZoomDistanceDelta * Time.deltaTime);
        }


        public void SetTargetsPositions(Vector3 playerPosition, Vector3 aimPosition)
        {
            playerPositionTarget.position = playerPosition;
            playerAimTarget.position      = aimPosition;
        }

        public void Zoom(float rawDelta)
        {
            targetZoom += rawDelta * scrollSensitivity;
            targetZoom =  math.clamp(targetZoom, 0f, 1f);
        }
    }
}