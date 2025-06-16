using Latios;
using Latios.Anna;
using Latios.Kinemation;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Animations
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FourDirectionsAnimationSystem : ISystem
    {
        EntityQuery m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_query = state.Fluent()
                .WithAspect<OptimizedSkeletonAspect>()
                .With<Clips>()
                .With<FourDirectionClipStates>()
                .With<RigidBody>()
                .With<PreviousVelocity>()
                .With<InertialBlendState>()
                .Without<DeadTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new AnimationJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(m_query, state.Dependency);
        }


        [WithNone(typeof(DeadTag))]
        [BurstCompile]
        internal partial struct AnimationJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;

            void Execute(
                OptimizedSkeletonAspect skeleton,
                in WorldTransform worldTransform,
                in RigidBody rigidBody,
                in Clips clips,
                in PreviousVelocity previousVelocity,
                ref FourDirectionClipStates clipStates,
                ref InertialBlendState inertialBlendState
            )
            {
                // Get Local Velocity
                var velocity = math.mul(math.inverse(worldTransform.rotation), rigidBody.velocity.linear);
                var magnitude = math.length(velocity);
                var rotatedVelocity = math.normalizesafe(velocity);

                // Calculate blend weights
                var centerWeight = math.max(0, 1f - magnitude * 2f); // Idle when not moving
                centerWeight = math.clamp(centerWeight, 0f, 1f);

                var upWeight = math.smoothstep(0.0f, 0.5f, rotatedVelocity.z);
                var downWeight = math.smoothstep(0.0f, 0.5f, -rotatedVelocity.z);
                var rightWeight = math.smoothstep(0.0f, 0.5f, rotatedVelocity.x);
                var leftWeight = math.smoothstep(0.0f, 0.5f, -rotatedVelocity.x);

                // Normalize directional weights (excluding center)
                var directionalSum = upWeight + downWeight + leftWeight + rightWeight;

                if (directionalSum > math.EPSILON)
                {
                    var normalizer = (1f - centerWeight) / directionalSum;
                    upWeight    *= normalizer;
                    downWeight  *= normalizer;
                    leftWeight  *= normalizer;
                    rightWeight *= normalizer;
                }


                // Update and sample animations
                UpdateClipState(ref clipStates.Center, ref clips.ClipSet.Value.clips[(int)EDirections.Center],
                    DeltaTime, centerWeight);

                UpdateClipState(ref clipStates.Up, ref clips.ClipSet.Value.clips[(int)EDirections.Up], DeltaTime,
                    upWeight);

                UpdateClipState(ref clipStates.Down, ref clips.ClipSet.Value.clips[(int)EDirections.Down], DeltaTime,
                    downWeight);

                UpdateClipState(ref clipStates.Left, ref clips.ClipSet.Value.clips[(int)EDirections.Left], DeltaTime,
                    leftWeight);

                UpdateClipState(ref clipStates.Right, ref clips.ClipSet.Value.clips[(int)EDirections.Right], DeltaTime,
                    rightWeight);

                // Sample animations
                SampleAnimation(ref skeleton, ref clips.ClipSet.Value.clips[(int)EDirections.Center], clipStates.Center,
                    centerWeight);

                SampleAnimation(ref skeleton, ref clips.ClipSet.Value.clips[(int)EDirections.Up], clipStates.Up,
                    upWeight);

                SampleAnimation(ref skeleton, ref clips.ClipSet.Value.clips[(int)EDirections.Down], clipStates.Down,
                    downWeight);

                SampleAnimation(ref skeleton, ref clips.ClipSet.Value.clips[(int)EDirections.Left], clipStates.Left,
                    leftWeight);

                SampleAnimation(ref skeleton, ref clips.ClipSet.Value.clips[(int)EDirections.Right], clipStates.Right,
                    rightWeight);


                // Detect significant direction/movement change (for starting new blend)
                var significantChange = math.abs(
                                            math.length(velocity.xz) - math.length(previousVelocity.Value.xz)) >
                                        inertialBlendState.VelocityChangeThreshold;

                // Start new inertial blend when movement changes significantly
                if (significantChange && inertialBlendState.PreviousDeltaTime > 0f)
                {
                    skeleton.StartNewInertialBlend(inertialBlendState.PreviousDeltaTime, inertialBlendState.Duration);
                    inertialBlendState.TimeInCurrentState = 0f;
                }


                // Apply inertial blend with current time since blend started
                if (!skeleton.IsFinishedWithInertialBlend(inertialBlendState.TimeInCurrentState))
                {
                    inertialBlendState.TimeInCurrentState += DeltaTime;
                    skeleton.InertialBlend(inertialBlendState.TimeInCurrentState);
                }


                // Finish sampling
                skeleton.EndSamplingAndSync();



                inertialBlendState.PreviousDeltaTime = DeltaTime;
            }

            void UpdateClipState(ref ClipState state,
                ref SkeletonClip clip,
                float deltaTime,
                float weight)
            {
                state.CurrentWeight = weight;
                if (weight > math.EPSILON)
                {
                    state.Update(deltaTime * state.SpeedMultiplier);
                    state.Time = clip.LoopToClipTime(state.Time);
                }
            }

            void SampleAnimation(ref OptimizedSkeletonAspect skeleton,
                ref SkeletonClip clip,
                ClipState state,
                float weight)
            {
                if (weight > math.EPSILON) clip.SamplePose(ref skeleton, state.Time, weight);
            }
        }
    }
}