using Latios.Myri.Authoring;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.SFX
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioListenerAuthoring))]
    [AddComponentMenu("Survivors/Audio/Main Audio Listener")]
    public class MainAudioListenerAuthoring : MonoBehaviour
    {
        class MainAudioListenerAuthoringBaker : Baker<MainAudioListenerAuthoring>
        {
            public override void Bake(MainAudioListenerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<MainAudioListener>(entity);
            }
        }
    }

    public struct MainAudioListener : IComponentData { }
}