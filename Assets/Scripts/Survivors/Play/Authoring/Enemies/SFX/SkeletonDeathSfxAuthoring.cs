using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Enemies.SFX
{
    [AddComponentMenu("Survivors/Enemies/Skeleton/SkeletonDeathSfx")]
    public class SkeletonDeathSfxAuthoring : MonoBehaviour
    {
        [SerializeField] SfxListRef sfxListRef;

        class SkeletonDeathSfxBaker : Baker<SkeletonDeathSfxAuthoring>
        {
            public override void Bake(SkeletonDeathSfxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new OneShotSfxSpawnerRef
                {
                    SfxPrefab = GetEntity(authoring.sfxListRef.oneShotSfxPrefabs, TransformUsageFlags.None),
                    EventType = authoring.sfxListRef.eventType
                });

                AddComponent<SfxTriggeredTag>(entity);
                SetComponentEnabled<SfxTriggeredTag>(entity, false);
            }
        }
    }
}