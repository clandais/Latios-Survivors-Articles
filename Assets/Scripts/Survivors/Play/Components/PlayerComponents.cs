using Latios;
using Latios.Psyshock;
using Unity.Collections;
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

    public struct PlayerExperience : IComponentData
    {
        public int   CurrentExperience;
    }

    public partial struct PlayerExpQueue : ICollectionComponent
    {
        public NativeQueue<int> ExpQueue;

        public JobHandle TryDispose(JobHandle inputDeps)
        {
            if (!ExpQueue.IsCreated)
                return inputDeps;
            
            return ExpQueue.Dispose(inputDeps);
        }
    }
    
    public partial struct PlayerHpQueue : ICollectionComponent
    {
        public NativeQueue<int> HpQueue;

        public JobHandle TryDispose(JobHandle inputDeps)
        {
            if (!HpQueue.IsCreated)
                return inputDeps;
            
            return HpQueue.Dispose(inputDeps);
        }
    }
    
    
    public partial struct PlayerCollisionLayer : ICollectionComponent
    {
        public CollisionLayer Layer;

        public JobHandle TryDispose(JobHandle inputDeps) => inputDeps;
    }
    
    
}