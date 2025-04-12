using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.GameScope.MonoBehaviours
{
    [AddComponentMenu("Survivors/Cinemachine Behaviour")]
    public class CinemachineBehaviour : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] CinemachineSplineDolly dolly;

        [SerializeField] float scrollSensitivity = 1f;
        [SerializeField] float maxZoomDistanceDelta = 0.1f;
        [SerializeField] float defaultZoomDistance = .5f;
        
        
        float targetZoom;

        void Awake()
        {
            dolly.CameraPosition = defaultZoomDistance;
            targetZoom = defaultZoomDistance;
        }


        public void SetTargetPosition(Vector3 position)
        {
            target.position = position;
        }

        public void Zoom(float rawDelta)
        {
            targetZoom += rawDelta * scrollSensitivity;
            targetZoom = math.clamp(targetZoom, 0f, 1f);
            
            Debug.Log(targetZoom);
        }


        void Update()
        {
            dolly.CameraPosition = Mathf.MoveTowards(dolly.CameraPosition, targetZoom, maxZoomDistanceDelta * Time.deltaTime);
        }
    }
}