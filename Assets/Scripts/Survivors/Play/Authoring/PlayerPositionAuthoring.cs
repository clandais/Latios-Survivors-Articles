using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring
{
    public class PlayerPositionAuthoring : MonoBehaviour
    {
        [SerializeField] Transform playerGameObject;

        class PlayerPositionAuthoringBaker : Baker<PlayerPositionAuthoring>
        {
            public override void Bake(PlayerPositionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlayerPosition
                {
                    LastPosition     = authoring.playerGameObject.position,
                    Position         = authoring.playerGameObject.position,
                    ForwardDirection = authoring.playerGameObject.forward
                });
            }
        }
    }

    public struct PlayerPosition : IComponentData
    {
        public float3 LastPosition;
        public float3 Position;
        public float3 ForwardDirection;
    }
}