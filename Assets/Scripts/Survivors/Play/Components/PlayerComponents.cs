using Latios;
using Latios.Psyshock;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Survivors.Play.Components
{
    public struct PlayerTag : IComponentData { }

    public struct PreviousVelocity : IComponentData
    {
        public float3 Value;
    }

    public struct PlayerHealth : IComponentData
    {
        public int   CurrentHealth;
        public int   MaxHealth;
        public float DamageDelay;
        public float LastDamageTime;
    }

    public partial struct PlayerCollisionLayer : ICollectionComponent
    {
        public CollisionLayer Layer;

        public JobHandle TryDispose(JobHandle inputDeps) => inputDeps;
    }
}