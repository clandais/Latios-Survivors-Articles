using System;
using Survivors.ScriptableObjects;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] public PlayerData playerData;
        // [SerializeField] MovementSettings movementSettings;

        class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                AddComponent(entity, authoring.playerData.movementSettings);
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