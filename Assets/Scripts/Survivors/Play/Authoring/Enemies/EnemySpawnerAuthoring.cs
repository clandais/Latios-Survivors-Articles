using Latios;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.Enemies
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] GameObject enemyPrefab;
        [SerializeField] float      spawnInterval = 1f;
        [SerializeField] Vector3    spawnPosition;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + spawnPosition, 0.5f);
        }

        class EnemySpawnerAuthoringBaker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemySpawnerComponent
                {
                    EnemyPrefab           = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic),
                    RelativeSpawnPosition = authoring.spawnPosition,
                    SpawnInterval         = authoring.spawnInterval,
                    CurrentTime           = 0f
                });
            }
        }

    }
    
    public struct EnemySpawnerComponent : IComponentData
    {
        public EntityWith<Prefab> EnemyPrefab;
        public float3             RelativeSpawnPosition;
        public float              SpawnInterval;
        public float              CurrentTime;
    }
}