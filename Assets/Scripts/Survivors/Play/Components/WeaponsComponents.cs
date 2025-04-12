using Latios;
using Latios.Psyshock;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Play.Components
{
    
    public struct WeaponTag : IComponentData { }
    
    public enum EWeaponType
    {
        ThrowableAxe,
        BFG9000,
        Chainsaw
    }

    public partial struct WeaponCollisionLayer : ICollectionComponent
    {
        public CollisionLayer Layer;
        public JobHandle TryDispose(JobHandle inputDeps) => inputDeps;  // Uses WorldUpdateAllocator
    }
}