using Survivors.Play.Components;
using Survivors.ScriptableObjects;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.Enemies
{
    public class SkeletonMinionAuthoring : MonoBehaviour
    {
        [SerializeField] MovementSettingsData movementSettings;
        [SerializeField] SfxListRef           sfxListRef;

        class SkeletonMinionAuthoringBaker : Baker<SkeletonMinionAuthoring>
        {
            public override void Bake(SkeletonMinionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<EnemyTag>(entity);
                AddComponent(entity, authoring.movementSettings.movementSettings);
                AddComponent(entity, new PreviousVelocity
                {
                    Value = float3.zero
                });

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