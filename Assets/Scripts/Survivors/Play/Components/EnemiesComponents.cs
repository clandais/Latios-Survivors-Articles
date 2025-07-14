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

        public JobHandle TryDispose(JobHandle inputDeps) => inputDeps; // Uses WorldUpdateAllocator
    }

    public struct XpDropPrefab : IComponentData
    {
        public EntityWith<Prefab> Prefab;
    }
    
    public struct HpDropPrefab : IComponentData
    {
        public EntityWith<Prefab> Prefab;
    }
    
    public struct ItemDropChance : IComponentData
    {
        public int HpDropChance;
        public int XpDropChance;
    }
}