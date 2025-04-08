using Latios;
using Latios.Kinemation;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Animations
{
    public partial struct SkeletonDeathSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged m_latiosWorldUnmanaged;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_query = state.Fluent().With<EnemyTag>().With<DeadTag>().WithAspect<OptimizedSkeletonAspect>().With<DeathClips>().With<DeathClipsStates>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new DeathAnimationJob
            {
                Rng = state.GetJobRng(), DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(m_query, state.Dependency);
        }

        public void OnNewScene(ref SystemState state)
        {
            state.InitSystemRng(new FixedString128Bytes("SkeletonDeathSystem"));
        }

        [BurstCompile]
        partial struct DeathAnimationJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public SystemRng Rng;
            public float     DeltaTime;

            void Execute(OptimizedSkeletonAspect skeleton,
                in DeathClips clips,
                ref DeathClipsStates clipStates)
            {
                if (clipStates.ChosenState == -1) clipStates.ChosenState = Rng.NextInt(0, 3);
                ref var state = ref clipStates.StateA;

                switch (clipStates.ChosenState)
                {
                    case 0:
                        state = ref clipStates.StateA;
                        break;
                    case 1:
                        state = ref clipStates.StateB;
                        break;
                    case 2:
                        state = ref clipStates.StateC;
                        break;
                }


                state.Update(DeltaTime * state.SpeedMultiplier);


                // One shot clips
                if (state.Time >= clips.ClipSet.Value.clips[clipStates.ChosenState].duration)
                {
                    state.Time = clips.ClipSet.Value.clips[clipStates.ChosenState].duration;
                    return;
                }


                clips.ClipSet.Value.clips[clipStates.ChosenState].SamplePose(ref skeleton, state.Time, 1f);
                skeleton.EndSamplingAndSync();
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk,
                int unfilteredChunkIndex,
                bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                Rng.BeginChunk(unfilteredChunkIndex);
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk,
                int unfilteredChunkIndex,
                bool useEnabledMask,
                in v128 chunkEnabledMask,
                bool chunkWasExecuted) { }
        }
    }
}