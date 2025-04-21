using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Player.Actions;
using Survivors.Play.Authoring.SceneBlackBoard;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player.Weapons.Initialization
{
    [RequireMatchingQueriesForUpdate]
    public partial struct WeaponThrowTriggerSystem : ISystem, ISystemNewScene
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

        public void OnNewScene(ref SystemState state)
        {
            state.InitSystemRng(new FixedString128Bytes("WeaponThrowTriggerSystem"));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_rightHandQuery.IsEmpty) return;


            var mousePosition = m_worldUnmanaged.sceneBlackboardEntity.GetComponentData<PlayerInputState>()
                .MousePosition;
            var spawnQueue = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<WeaponSpawnQueue>()
                .WeaponQueue;

            var prefab =
                m_worldUnmanaged.sceneBlackboardEntity.GetBuffer<PrefabBufferElement>()[(int)EWeaponType.ThrowableAxe]
                    .Prefab;

            var ecb = m_worldUnmanaged.syncPoint.CreateEntityCommandBuffer();


            state.Dependency = new WeaponThrowTriggerJob
            {
                Transforms    = SystemAPI.GetComponentLookup<WorldTransform>(),
                MousePosition = mousePosition,
                Prefab        = prefab,
                SpawnQueue    = spawnQueue,
                CommandBuffer = ecb.AsParallelWriter(),
                Rng           = state.GetJobRng()
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        partial struct WeaponThrowTriggerJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            [ReadOnly]                            public ComponentLookup<WorldTransform>               Transforms;
            [ReadOnly]                            public float3                                        MousePosition;
            [ReadOnly]                            public Entity                                        Prefab;
            [NativeDisableParallelForRestriction] public NativeQueue<WeaponSpawnQueue.WeaponSpawnData> SpawnQueue;

            public EntityCommandBuffer.ParallelWriter CommandBuffer;
            public SystemRng                          Rng;


            void Execute(Entity entity, [EntityIndexInQuery] int index, in RightHandSlot slot)
            {
                var rHandSlotTransform = Transforms[slot.RightHandSlotEntity];

                var direction2d = math.normalizesafe(MousePosition.xz - rHandSlotTransform.position.xz);
                var direction = new float3(direction2d.x, 0f, direction2d.y);

                SpawnQueue.Enqueue(new WeaponSpawnQueue.WeaponSpawnData
                {
                    WeaponPrefab = Prefab,
                    Position     = rHandSlotTransform.position,
                    Direction    = direction
                });


                CommandBuffer.SetComponentEnabled<RightHandSlotThrowTag>(index, entity, false);
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                Rng.BeginChunk(unfilteredChunkIndex);
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask,
                bool chunkWasExecuted) { }
        }
    }
}