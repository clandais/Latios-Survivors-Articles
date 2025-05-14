using System;
using Survivors.Play.Components;
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

        [SerializeField] int   playerStartingHealth = 100;
        [SerializeField] float damageDelay          = 0.5f;

        class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                AddComponent(entity, authoring.movementSettingsData.movementSettings);
                AddComponent(entity, new PreviousVelocity
                {
                    Value = float3.zero
                });

                AddComponent(entity, new PlayerHealth
                {
                    CurrentHealth  = authoring.playerStartingHealth,
                    MaxHealth      = authoring.playerStartingHealth,
                    DamageDelay    = authoring.damageDelay,
                    LastDamageTime = 0
                });
            }
        }
    }


    [Serializable]
    public struct MovementSettings : IComponentData
    {
        public float moveSpeed;
        public float maxAngleDelta;
        public float speedChangeRate;
    }
}