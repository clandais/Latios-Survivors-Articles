using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring
{
    public class PlayerPositionAuthoring : MonoBehaviour
    {
        class PlayerPositionAuthoringBaker : Baker<PlayerPositionAuthoring>
        {
            public override void Bake(PlayerPositionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<PlayerPosition>(entity);
            }
        }
    }

    public struct PlayerPosition : IComponentData
    {
        public float3 LastPosition;
        public float3 Position;
    }
}