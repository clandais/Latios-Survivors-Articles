using System.Collections.Generic;
using UnityEngine;

namespace Survivors.Play.Authoring.Debug
{
    public class Playbacker : MonoBehaviour
    {
        [Tooltip("If this is set, then the inspector string is being live generated from the CaptureAuthoring on this same GameObject")]
        public bool isLive;
        [Tooltip("Check this to cause everything to be cleared and potentially recalculated")]
        public bool                                   reset;
        public string                                 hex;
        public SerializedColliderDistanceResult       closestColliderDistance;
        public SerializedUnityContactsResult          closestContacts;
        public List<SerializedColliderDistanceResult> allColliderDistances;
        public List<SerializedUnityContactsResult>    allContacts;

        private void OnValidate()
        {
            if (reset)
            {
                reset                   = false;
                hex                     = default;
                closestColliderDistance = default;
                closestContacts         = default;
                allColliderDistances    = default;
                allContacts             = default;
            }
        }
    }
}