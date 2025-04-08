using System;
using Survivors.ScriptableObjects;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Survivors.Play.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("playerData")] [SerializeField]
        public MovementSettingsData movementSettingsData;
        // [SerializeField] MovementSettings movementSettings;

        class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                AddComponent(entity, authoring.movementSettingsData.movementSettings);
                AddComponent<PreviousVelocity>(entity);
            }
        }
    }

    public struct PlayerTag : IComponentData { }

    [Serializable]
    public struct MovementSettings : IComponentData
    {
        public float moveSpeed;
        public float maxAngleDelta;
        public float speedChangeRate;
    }

    public struct PreviousVelocity : IComponentData
    {
        public float3 Value;
    }
}