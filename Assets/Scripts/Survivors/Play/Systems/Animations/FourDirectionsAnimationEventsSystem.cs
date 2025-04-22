using Latios;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Animations
{
    [BurstCompile]
    public partial struct FourDirectionsAnimationEventsSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged m_worldUnmanaged;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();

            m_query = state.Fluent()
                .With<Clips>()
                .With<WorldTransform>()
                .With<FourDirectionClipStates>()
                .With<OneShotSfxSpawnerRef>()
                .Build();
        }

        public void OnNewScene(ref SystemState state)
        {
            m_worldUnmanaged.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new SfxSpawnQueue
            {
                SfxQueue = new NativeQueue<SfxSpawnQueue.SfxSpawnData>(Allocator.Persistent)
            });

            state.InitSystemRng(new FixedString128Bytes("FourDirectionsAnimationEventsSystem"));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var sfxQueue = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<SfxSpawnQueue>().SfxQueue;
            state.Dependency = new AnimationEventsJob
            {
                SfxQueue               = sfxQueue,
                Rng                    = state.GetJobRng(),
                SpawnerLookup          = SystemAPI.GetComponentLookup<OneShotSfxSpawner>(true),
                FootstepsPrefabsLookup = SystemAPI.GetBufferLookup<OneShotSfxElement>(true)
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        partial struct AnimationEventsJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            [NativeDisableParallelForRestriction] public NativeQueue<SfxSpawnQueue.SfxSpawnData> SfxQueue;
            public                                       SystemRng                               Rng;

            [ReadOnly] public ComponentLookup<OneShotSfxSpawner> SpawnerLookup;
            [ReadOnly] public BufferLookup<OneShotSfxElement>    FootstepsPrefabsLookup;

            void Execute(
                in FourDirectionClipStates clipStates,
                in Clips clips,
                in WorldTransform worldTransform,
                in OneShotSfxSpawnerRef oneShotSfxSpawner)
            {
                var clipStatesArray = new NativeArray<ClipState>(5, Allocator.Temp);
                clipStatesArray[(int)EDirections.Center] = clipStates.Center;
                clipStatesArray[(int)EDirections.Up]     = clipStates.Up;
                clipStatesArray[(int)EDirections.Down]   = clipStates.Down;
                clipStatesArray[(int)EDirections.Left]   = clipStates.Left;
                clipStatesArray[(int)EDirections.Right]  = clipStates.Right;

                var heaviestIndex = 0;

                for (var i = 0; i < clipStatesArray.Length; i++)
                {
                    var state = clipStatesArray[i];

                    if (state.CurrentWeight > clipStatesArray[heaviestIndex].CurrentWeight)
                        heaviestIndex = i;
                }

                var heaviestState = clipStatesArray[heaviestIndex];


                ref var clip = ref clips.ClipSet.Value.clips[heaviestIndex];
                if (!clip.events.TryGetEventsRange(heaviestState.PreviousTime, heaviestState.Time, out var index,
                        out var count)) return;

                if (index == -1) return;

                for (var j = index; j < index + count; j++)
                {
                    var evt = clip.events.parameters[j];
                    if (evt != (int)ESfxEventType.Footstep) continue;

                    var sfxPrefab = GetSfx(oneShotSfxSpawner);
                    if (sfxPrefab == Entity.Null) continue;

                    SfxQueue.Enqueue(new SfxSpawnQueue.SfxSpawnData
                    {
                        EventType = ESfxEventType.Footstep,
                        Position  = worldTransform.position,
                        SfxPrefab = sfxPrefab
                    });
                }
            }

            Entity GetSfx(in OneShotSfxSpawnerRef oneShotSfxSpawner)
            {
                var spawner = SpawnerLookup[oneShotSfxSpawner.SfxPrefab];
                var buffer = FootstepsPrefabsLookup[oneShotSfxSpawner.SfxPrefab];
                if (buffer.Length == 0) return Entity.Null;

                var prefabIndex = Rng.NextInt(0, spawner.SfxCount);
                return buffer[prefabIndex].Prefab;
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