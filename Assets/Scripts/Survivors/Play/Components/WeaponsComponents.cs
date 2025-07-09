using Latios;
using Latios.Psyshock;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Survivors.Play.Components
{
    public struct WeaponTag : IComponentData
    {
    }

    public enum EWeaponType
    {
        ThrowableAxe,
        BFG9000,
        Chainsaw
    }

    public partial struct WeaponCollisionLayer : ICollectionComponent
    {
        public CollisionLayer Layer;

        public JobHandle TryDispose(JobHandle inputDeps)
        {
            return inputDeps;
            // Uses WorldUpdateAllocator
        }
    }

    public struct ThrownWeaponComponent : IComponentData
    {
        public float Speed;
        public float RotationSpeed;
        public float3 RotationAxis;
        public float3 Direction;
    }

    public struct ThrownWeaponConfigComponent : IComponentData
    {
        public readonly float Speed;
        public readonly float RotationSpeed;
        public readonly float3 RotationAxis;

        public ThrownWeaponConfigComponent(float speed,
            float rotationSpeed,
            float3 rotationAxis)
        {
            Speed         = speed;
            RotationSpeed = rotationSpeed;
            RotationAxis  = rotationAxis;
        }
    }

    public struct ThrownWeaponHitVfx : IComponentData
    {
        public EntityWith<Prefab> Prefab;
    }
}
