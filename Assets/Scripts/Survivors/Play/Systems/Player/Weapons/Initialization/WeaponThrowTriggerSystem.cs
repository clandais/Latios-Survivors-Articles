using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Player.Actions;
using Survivors.Play.Authoring.SceneBlackBoard;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player.Weapons.Initialization
{
    [RequireMatchingQueriesForUpdate]
    public partial struct WeaponThrowTriggerSystem : ISystem
    {
        LatiosWorldUnmanaged m_worldUnmanaged;
        EntityQuery          _rightHandQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();

            _rightHandQuery = state.Fluent()
                .WithEnabled<RightHandSlotThrowTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_rightHandQuery.IsEmpty) return;


            var mousePosition = m_worldUnmanaged.sceneBlackboardEntity.GetComponentData<PlayerInputState>().MousePosition;
            var spawnQueue = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<WeaponSpawnQueue>().WeaponQueue;
            var prefab = m_worldUnmanaged.sceneBlackboardEntity.GetBuffer<PrefabBufferElement>()[(int)EWeaponType.ThrowableAxe].Prefab;

            var ecb = m_worldUnmanaged.syncPoint.CreateEntityCommandBuffer();

            foreach (var (transform, slot, entity) in
                     SystemAPI.Query<RefRO<WorldTransform>, RefRO<RightHandSlot>>()
                         .WithEntityAccess())
            {
                var rHandSlotTransform = SystemAPI.GetComponent<WorldTransform>(slot.ValueRO.RightHandSlotEntity);

                var direction2d = math.normalizesafe(mousePosition.xz - rHandSlotTransform.position.xz);
                var direction = new float3(direction2d.x, 0f, direction2d.y);

                spawnQueue.Enqueue(new WeaponSpawnQueue.WeaponSpawnData
                {
                    WeaponPrefab = prefab,
                    Position     = rHandSlotTransform.position,
                    Direction    = direction
                });


                ecb.SetComponentEnabled<RightHandSlotThrowTag>(entity, false);
            }
        }
    }
}