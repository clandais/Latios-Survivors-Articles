using Survivors.Play.Components;
using Survivors.ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Enemies
{
    public class SkeletonMinionAuthoring : MonoBehaviour
    {
        [SerializeField] MovementSettingsData movementSettings;

        class SkeletonMinionAuthoringBaker : Baker<SkeletonMinionAuthoring>
        {
            public override void Bake(SkeletonMinionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<EnemyTag>(entity);
                AddComponent(entity, authoring.movementSettings.movementSettings);
                AddComponent<PreviousVelocity>(entity);
            }
        }
    }
}