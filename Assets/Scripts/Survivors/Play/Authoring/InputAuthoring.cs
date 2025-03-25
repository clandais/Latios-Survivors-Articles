using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring
{
    public class InputAuthoring : MonoBehaviour
    {
        class InputAuthoringBaker : Baker<InputAuthoring>
        {
            public override void Bake(InputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<PlayerInputState>(entity);
            }
        }
    }

    public struct PlayerInputState : IComponentData
    {
        public float2 Direction;
    }
}