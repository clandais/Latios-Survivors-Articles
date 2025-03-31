using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.Actions
{
    public class RightHandSlotAuthoring : MonoBehaviour
    {
        [SerializeField] Transform rightHandSlot;

        class RightHandSlotAuthoringBaker : Baker<RightHandSlotAuthoring>
        {
            public override void Bake(RightHandSlotAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RightHandSlot
                {
                    RightHandSlotEntity = GetEntity(authoring.rightHandSlot, TransformUsageFlags.Dynamic)
                });

                AddComponent<RightHandSlotThrowTag>(entity);
                SetComponentEnabled<RightHandSlotThrowTag>(entity, false);
            }
        }
    }

    public struct RightHandSlot : IComponentData
    {
        public Entity RightHandSlotEntity;
    }

    public struct RightHandSlotThrowTag : IComponentData, IEnableableComponent { }
}