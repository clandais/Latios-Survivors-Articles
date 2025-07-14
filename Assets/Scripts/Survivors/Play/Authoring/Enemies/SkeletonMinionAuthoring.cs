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
        [SerializeField] GameObject hpDropPrefab;
        
        [Header("Drops")]
        [Tooltip("Drop chances for XP and HP items. The first value is the HP weight, the second is the XP weight.")]
        [SerializeField] Vector2Int hpXpDropRange = new Vector2Int(1, 3);
        
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
                
                AddComponent(entity, new HpDropPrefab
                {
                    Prefab = GetEntity(authoring.hpDropPrefab, TransformUsageFlags.Dynamic)
                });
                
                AddComponent(entity, new ItemDropChance
                {
                    HpDropChance = authoring.hpXpDropRange.x,
                    XpDropChance = authoring.hpXpDropRange.y
                });
            }
        }
    }
}