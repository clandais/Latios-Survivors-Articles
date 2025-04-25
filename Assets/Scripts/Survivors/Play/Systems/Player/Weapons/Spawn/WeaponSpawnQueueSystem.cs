using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring.Player.Weapons;
using Survivors.Play.Authoring.SceneBlackBoard;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player.Weapons.Spawn
{
    [BurstCompile]
    public partial struct WeaponSpawnQueueSystem : ISystem
    {
        LatiosWorldUnmanaged m_worldUnmanaged;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spawnQueue = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<WeaponSpawnQueue>()
                .WeaponQueue;

            if (spawnQueue.IsEmpty()) return;

            var sfxQueue = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<SfxSpawnQueue>()
                .SfxQueue;

            var icb = m_worldUnmanaged.syncPoint
                .CreateInstantiateCommandBuffer<ThrownWeaponComponent, WorldTransform>();

            state.Dependency = new WeaponSpawnJob
            {
                SpawnQueue            = spawnQueue,
                WeaponComponentLookup = SystemAPI.GetComponentLookup<ThrownWeaponConfigComponent>(true),
                SpawnQueueWriter      = icb.AsParallelWriter()
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }


    [BurstCompile]
    internal struct WeaponSpawnJob : IJob
    {
        public NativeQueue<WeaponSpawnQueue.WeaponSpawnData> SpawnQueue;
        [ReadOnly] public ComponentLookup<ThrownWeaponConfigComponent> WeaponComponentLookup;
        public InstantiateCommandBuffer<ThrownWeaponComponent, WorldTransform>.ParallelWriter SpawnQueueWriter;

        public void Execute()
        {
            var sortKey = 0;

            while (!SpawnQueue.IsEmpty())
                if (SpawnQueue.TryDequeue(out var weapon))
                {
                    var transform = new WorldTransform
                    {
                        worldTransform = TransformQvvs.identity
                    };

                    transform.worldTransform.position = weapon.Position;
                    transform.worldTransform.rotation = quaternion.LookRotation(weapon.Direction, math.up());

                    var config = WeaponComponentLookup[weapon.WeaponPrefab];

                    SpawnQueueWriter.Add(
                        weapon.WeaponPrefab, new ThrownWeaponComponent
                        {
                            Direction     = weapon.Direction,
                            Speed         = config.Speed,
                            RotationSpeed = config.RotationSpeed,
                            RotationAxis  = config.RotationAxis
                        }, transform,
                        ++sortKey);
                }
        }
    }
}