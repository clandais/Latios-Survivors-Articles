using Latios.Kinemation;
using Survivors.Play.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Animations
{
    public partial struct PlayerIdleAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new IdleClipJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        partial struct IdleClipJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;

            void Execute(OptimizedSkeletonAspect skeleton,
                in IdleClip idleClip,
                ref IdleClipState idleClipState)
            {
                ref var clipState = ref idleClipState.State;
                ref var skeletonClip = ref idleClip.ClipSet.Value.clips[0];

                // Update the internal tracked state
                clipState.Update(DeltaTime * clipState.SpeedMultiplier);
                // make sure the time is looped
                clipState.Time = skeletonClip.LoopToClipTime(clipState.Time);

                // Sample the clip and sync the skeleton
                skeletonClip.SamplePose(ref skeleton, clipState.Time, 1f);
                skeleton.EndSamplingAndSync();
            }
        }
    }
}