using Latios;
using Latios.Kinemation;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Animations;
using Survivors.Play.Authoring.Player.Actions;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Player
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct PlayerActionSystem : ISystem
    {
        LatiosWorldUnmanaged m_worldUnmanaged;
        EntityQuery          m_Query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
            state.RequireForUpdate<PlayerTag>();

            m_Query = state.Fluent()
                .WithAspect<OptimizedSkeletonAspect>()
                .With<ActionClipComponent>()
                .With<ActionClipState>()
                .With<AvatarMasks>()
                .With<PlayerTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inpuState = m_worldUnmanaged.sceneBlackboardEntity.GetComponentData<PlayerInputState>();

            var ecb = m_worldUnmanaged.syncPoint.CreateEntityCommandBuffer();

            state.Dependency = new AnimationJob
            {
                DeltaTime  = SystemAPI.Time.DeltaTime,
                InputState = inpuState,
                ECB        = ecb.AsParallelWriter()
            }.ScheduleParallel(m_Query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }


        [BurstCompile]
        partial struct AnimationJob : IJobEntity
        {
            [ReadOnly] public float                              DeltaTime;
            [ReadOnly] public PlayerInputState                   InputState;
            public            EntityCommandBuffer.ParallelWriter ECB;

            void Execute(
                Entity entity,
                [ChunkIndexInQuery] int chunkIndexInQuery,
                OptimizedSkeletonAspect skeleton,
                in ActionClipComponent actionClipComponent,
                ref ActionClipState actionClipState,
                ref AvatarMasks masks)
            {
                ref var weaponThrowClip = ref actionClipComponent.ClipSet.Value.clips[0];
                ref var weaponThrowState = ref actionClipState.ClipState;

                if (InputState.AttackTriggered || weaponThrowState.PreviousTime < weaponThrowState.Time)
                {
                    weaponThrowState.Update(DeltaTime * weaponThrowState.SpeedMultiplier);
                    weaponThrowState.Time = weaponThrowClip.LoopToClipTime(weaponThrowState.Time);
                    weaponThrowClip.SamplePose(ref skeleton, masks.Blob.Value.masks[(int)EAction.Throw].AsSpan(), weaponThrowState.Time, 1f);
                    skeleton.EndSamplingAndSync();
                }


                if (weaponThrowClip.events.TryGetEventsRange(weaponThrowState.PreviousTime, weaponThrowState.Time, out var index, out var eventCount))
                    for (var i = index; i < eventCount; i++)
                    {
                        var eventHash = weaponThrowClip.events.nameHashes[i];

                        if (eventHash == weaponThrowState.EventHash) ECB.SetComponentEnabled<RightHandSlotThrowTag>(chunkIndexInQuery, entity, true);
                    }
            }
        }
    }
}