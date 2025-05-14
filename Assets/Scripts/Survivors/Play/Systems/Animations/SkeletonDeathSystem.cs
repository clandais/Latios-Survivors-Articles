using Latios;
using Latios.Kinemation;
using Survivors.Play.Authoring.Materials;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Animations
{
    [BurstCompile]
    public partial struct SkeletonDeathSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged m_latiosWorldUnmanaged;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_query = state.Fluent()
                .With<EnemyTag>()
                .With<DeadTag>()
                .WithAspect<OptimizedSkeletonAspect>()
                .With<DeathClips>()
                .With<DeathClipsStates>()
                .With<LinkedEntityGroup>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dcb = m_latiosWorldUnmanaged.syncPoint.CreateDestroyCommandBuffer();

            state.Dependency = new DeathAnimationJob
            {
                Rng                  = state.GetJobRng(),
                DeltaTime            = SystemAPI.Time.DeltaTime,
                DissolveAmountLookup = SystemAPI.GetComponentLookup<MaterialDissolveAmount>(),
                DissolveSpeedLookup  = SystemAPI.GetComponentLookup<MaterialDissolveSpeed>(true),
                Dcb                  = dcb.AsParallelWriter()
            }.ScheduleParallel(m_query, state.Dependency);
        }

        public void OnNewScene(ref SystemState state)
        {
            state.InitSystemRng(new FixedString128Bytes("SkeletonDeathSystem"));
        }

        [BurstCompile]
        partial struct DeathAnimationJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public                                       SystemRng                               Rng;
            public                                       float                                   DeltaTime;
            [NativeDisableParallelForRestriction] public ComponentLookup<MaterialDissolveAmount> DissolveAmountLookup;
            [ReadOnly]                            public ComponentLookup<MaterialDissolveSpeed>  DissolveSpeedLookup;
            public                                       DestroyCommandBuffer.ParallelWriter     Dcb;

            void Execute(
                Entity entity,
                [EntityIndexInQuery] int idx,
                OptimizedSkeletonAspect skeleton,
                in DeathClips clips,
                ref DeathClipsStates clipStates,
                in DynamicBuffer<LinkedEntityGroup> linkedEntityGroup)
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
                    // Clamp to duration
                    state.Time = clips.ClipSet.Value.clips[clipStates.ChosenState].duration;

                    var dissolved = false;

                    // apply dissolve to linked entities' materials
                    foreach (var entityGroup in linkedEntityGroup)
                    {
                        if (!DissolveAmountLookup.HasComponent(entityGroup.Value)) continue;

                        var dissolve = DissolveAmountLookup.GetRefRW(entityGroup.Value);
                        var dissolveSpeed = DissolveSpeedLookup.GetRefRO(entityGroup.Value);
                        dissolve.ValueRW.Value =
                            math.min(dissolve.ValueRW.Value + DeltaTime * dissolveSpeed.ValueRO.Value, 1f);

                        dissolved = dissolve.ValueRW.Value >= 1f;
                    }

                    if (dissolved)
                        // Destroy the skeleton entity
                        Dcb.Add(entity, idx);

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