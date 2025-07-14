using Survivors.Play.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.Collectibles
{
    public class HpSphereAuthoring : MonoBehaviour
    {
        [SerializeField] int hpValue = 2;
        [SerializeField] GameObject hpSphereVfxPrefab;
        
        [Header("Movement")] [SerializeField] float maxSpeed = 10f;

        [SerializeField] float maxForce          = 10f;
        [SerializeField] float startFollowRadius = 5f;
        
        private class HpSphereAuthoringBaker : Baker<HpSphereAuthoring>
        {
            public override void Bake(HpSphereAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                
                AddComponent<CollectibleTag>(entity);
                
                AddComponent(entity, new HpItem { Value = authoring.hpValue });
                
                AddComponent(entity, new HpSphereVfx
                {
                    Prefab = GetEntity(authoring.hpSphereVfxPrefab, TransformUsageFlags.Dynamic)
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
    
    public struct HpItem : IComponentData
    {
        public int Value;
    }
    
    public struct HpSphereVfx : IComponentData
    {
        public Entity Prefab;
    }
}