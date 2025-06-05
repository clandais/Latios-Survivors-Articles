using Latios;
using Latios.Psyshock;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Play.Components
{
    public struct EnemyTag : IComponentData { }


    public struct BoidTag : IComponentData { }

    public struct BoidNeighbor : IBufferElementData
    {
        public EntityWith<BoidTag> Neighbor;
    }

    public partial struct EnemyCollisionLayer : ICollectionComponent
    {
        public CollisionLayer Layer;

        public JobHandle TryDispose(JobHandle inputDeps) => inputDeps; // Uses WorldUpdateAllocator
    }
}