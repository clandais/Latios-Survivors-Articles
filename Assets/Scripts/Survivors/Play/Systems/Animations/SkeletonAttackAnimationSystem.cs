using Latios;
using Latios.Kinemation;
using Survivors.Play.Authoring.Enemies;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Animations
{
    [BurstCompile]
    public partial struct SkeletonAttackAnimationSystem : ISystem
    {
        LatiosWorldUnmanaged m_latiosWorldUnmanaged;
        EntityQuery          m_entityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_entityQuery = state.Fluent()
                .With<EnemyTag>()
                .Without<DeadTag>()
                .WithEnabled<SkeletonMinionAttackAnimationTag>()
                .WithAspect<OptimizedSkeletonAspect>()
                .With<SkeletonMinionAttackAnimation>()
                .With<SkeletonMinionAttackAnimationState>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new AttackJob
            {
                AttackAnimationTagLookup = SystemAPI.GetComponentLookup<SkeletonMinionAttackAnimationTag>(),
                DeltaTime                = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(m_entityQuery, state.Dependency);
        }


        [BurstCompile]
        partial struct AttackJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<SkeletonMinionAttackAnimationTag> AttackAnimationTagLookup;

            void Execute(
                Entity entity,
                [EntityIndexInQuery] int entityIndexInQuery,
                OptimizedSkeletonAspect skeleton,
                in SkeletonMinionAttackAnimation attackAnimation,
                ref SkeletonMinionAttackAnimationState attackAnimationState
            )
            {
                ref var state = ref attackAnimationState.State;
                state.Update(DeltaTime * state.SpeedMultiplier);

                if (state.Time > attackAnimation.ClipSet.Value.clips[0].duration)
                {
                    AttackAnimationTagLookup.SetComponentEnabled(entity, false);
                    state.Time = 0f;
                    return;
                }

                state.CurrentWeight = 1f;
                attackAnimation.ClipSet.Value.clips[0].SamplePose(ref skeleton, state.Time, state.CurrentWeight);
                skeleton.EndSamplingAndSync();
            }
        }
    }
}