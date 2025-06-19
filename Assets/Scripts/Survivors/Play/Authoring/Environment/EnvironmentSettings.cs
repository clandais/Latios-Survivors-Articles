using Latios;
using Latios.Psyshock;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.Environment
{
    public class EnvironmentSettings : MonoBehaviour
    {
        [SerializeField] Bounds worldBounds         = new(float3.zero, new float3(1f));
        [SerializeField] int3   subdivisionsPerAxis = new(2, 2, 2);


        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
        }

        class EnvironmentSettingsBaker : Baker<EnvironmentSettings>
        {
            public override void Bake(EnvironmentSettings authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PhysicsSettings
                {
                    CollisionLayerSettings = new CollisionLayerSettings
                    {
                        worldAabb                = new Aabb(authoring.worldBounds.min, authoring.worldBounds.max),
                        worldSubdivisionsPerAxis = math.max(1, authoring.subdivisionsPerAxis)
                    }
                });
            }
        }
    }

    public struct PhysicsSettings : IComponentData
    {
        public CollisionLayerSettings CollisionLayerSettings;
    }


    public partial struct EnvironmentCollisionLayer : ICollectionComponent
    {
        public CollisionLayer layer;

        public JobHandle TryDispose(JobHandle inputDeps) => inputDeps; // Uses WorldUpdateAllocator
    }
}