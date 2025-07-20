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


        [SerializeField] bool invincible;

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

                AddComponent(entity, new Velocity
                {
                    Value = float3.zero
                });
                

                if (authoring.invincible) AddComponent<InvincibleTag>(entity);
            }
        }
    }


    public struct InvincibleTag : IComponentData { }

    [Serializable]
    public struct MovementSettings : IComponentData
    {
        public float moveSpeed;
        public float maxAngleDelta;
        public float speedChangeRate;
        public float speedMultiplier;
    }
}