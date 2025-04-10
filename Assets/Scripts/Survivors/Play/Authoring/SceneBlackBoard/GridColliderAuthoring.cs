using Latios;
using Latios.Psyshock;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.SceneBlackBoard
{
    public class GridColliderAuthoring : MonoBehaviour
    {
        
        public Bounds worldBounds         = new Bounds(float3.zero, new float3(1f));
        public int3   subdivisionsPerAxis = new int3(2, 2, 2);
        
        private class GridColliderAuthoringBaker : Baker<GridColliderAuthoring>
        {
            public override void Bake(GridColliderAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GridCollisionSettings
                {
                    WorldAabb                = new Aabb(authoring.worldBounds.min, authoring.worldBounds.max),
                    WorldSubdivisionsPerAxis = math.max(1, authoring.subdivisionsPerAxis)
                });
            }
        }
    }


    public struct GridCollisionSettings : IComponentData
    {
        public Aabb WorldAabb;
        public int3 WorldSubdivisionsPerAxis;
    }
    
    public partial struct GridCollisionLayer : ICollectionComponent
    {
        public CollisionLayer Layer;
        
        public JobHandle TryDispose(JobHandle inputDeps) => Layer.IsCreated ? Layer.Dispose(inputDeps) : inputDeps;
    }
}