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
        [SerializeField] GameObject           xpDropPrefab;

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

                AddComponent(entity, new Velocity
                {
                    Value = float3.zero
                });

                AddComponent<HitInfos>(entity);
                SetComponentEnabled<HitInfos>(entity, false);

                AddComponent(entity, new XpDropPrefab
                {
                    Prefab = GetEntity(authoring.xpDropPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}