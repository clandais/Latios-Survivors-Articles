using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] MovementSettings movementSettings;

        class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                AddComponent(entity, authoring.movementSettings);

            }
        }
    }

    public struct PlayerTag : IComponentData { }

    [Serializable]
    public struct MovementSettings : IComponentData
    {
        public float moveSpeed;
        public float rotationSpeed;
        public float speedChangeRate;
    }


}