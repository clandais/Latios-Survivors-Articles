using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.SFX
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Survivors/Play/SFX/SFX List Ref")]
    public class SfxListRefAuthoring : MonoBehaviour
    {
        [SerializeField] SfxListRef sfxListRef;

        class SfxListRefAuthoringBaker : Baker<SfxListRefAuthoring>
        {
            public override void Bake(SfxListRefAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                if (authoring.sfxListRef.oneShotSfxPrefabs == null) return;

                AddComponent(entity, new OneShotSfxSpawnerRef
                {
                    EventType = authoring.sfxListRef.eventType,
                    SfxPrefab = GetEntity(authoring.sfxListRef.oneShotSfxPrefabs, TransformUsageFlags.None)
                });

                AddComponent<SfxTriggeredTag>(entity);
                SetComponentEnabled<SfxTriggeredTag>(entity, false);
            }
        }
    }
}