using Latios;
using Latios.Kinemation;
using Survivors.Play.Components;
using Unity.Burst;
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
                .With<DeathClipsStates>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}