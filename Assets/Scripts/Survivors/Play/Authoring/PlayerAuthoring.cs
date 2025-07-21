using Survivors.Play.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] bool invincible;

        class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);

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
}