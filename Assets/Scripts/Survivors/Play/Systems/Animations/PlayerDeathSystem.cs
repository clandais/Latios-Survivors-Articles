using Latios;
using Latios.Kinemation;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Animations
{
    public partial struct PlayerDeathSystem : ISystem
    {
        LatiosWorldUnmanaged m_latiosWorldUnmanaged;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_query = state.Fluent()
                .With<PlayerTag>()
                .With<DeadTag>()
                .WithAspect<OptimizedSkeletonAspect>()
                .With<DeathClips>()
                .With<DeathClipState>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new DeathAnimationJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }


        [WithAll(typeof(DeadTag))]
        [BurstCompile]
        partial struct DeathAnimationJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;

            void Execute(OptimizedSkeletonAspect optimizedSkeletonAspect,
                ref DeathClips deathClips,
                ref DeathClipState deathClipState)
            {
                ref var state = ref deathClipState.State;
                state.Update(DeltaTime * state.SpeedMultiplier);

                if (state.Time >= deathClips.ClipSet.Value.clips[0].duration)
                    state.Time = deathClips.ClipSet.Value.clips[0].duration;


                deathClips.ClipSet.Value.clips[0].SamplePose(ref optimizedSkeletonAspect,
                    state.Time, 1f);

                optimizedSkeletonAspect.EndSamplingAndSync();
            }
        }
    }
}