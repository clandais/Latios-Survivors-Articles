using Latios;
using Latios.Psyshock;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Play.Components
{
    public struct EnemyTag : IComponentData { }

    public partial struct EnemyCollisionLayer : ICollectionComponent
    {
        public CollisionLayer Layer;

        public JobHandle TryDispose(JobHandle inputDeps) => Layer.IsCreated ? Layer.Dispose(inputDeps) : inputDeps;
    }
}