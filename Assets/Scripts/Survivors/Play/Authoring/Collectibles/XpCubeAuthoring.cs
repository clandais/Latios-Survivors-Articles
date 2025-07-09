using Latios;
using Survivors.Play.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.Collectibles
{
    [AddComponentMenu("Survivors/Collectibles/XpCube")]
    public class XpCubeAuthoring : MonoBehaviour
    {
        [SerializeField] int xpValue = 10;

        [SerializeField] GameObject xpCubeVfxPrefab;

        [Header("Movement")] [SerializeField] float maxSpeed = 10f;

        [SerializeField] float maxForce          = 10f;
        [SerializeField] float startFollowRadius = 5f;

        class XpCubeAuthoringBaker : Baker<XpCubeAuthoring>
        {
            public override void Bake(XpCubeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new XpItem { Value = authoring.xpValue });


                AddComponent(entity, new XpCubeVfx
                {
                    Prefab = GetEntity(authoring.xpCubeVfxPrefab, TransformUsageFlags.Dynamic)
                });

                AddComponent(entity, new Velocity
                {
                    Value = float3.zero
                });

                AddComponent(entity, new MaxSpeed
                {
                    Value = authoring.maxSpeed
                });

                AddComponent(entity, new MaxForce
                {
                    Value = authoring.maxForce
                });

                AddComponent(entity, new FollowRadius
                {
                    Value = authoring.startFollowRadius
                });
            }
        }
    }

    public struct XpItem : IComponentData
    {
        public int Value;
    }

    public struct XpCubeVfx : IComponentData
    {
        public EntityWith<Prefab> Prefab;
    }
}