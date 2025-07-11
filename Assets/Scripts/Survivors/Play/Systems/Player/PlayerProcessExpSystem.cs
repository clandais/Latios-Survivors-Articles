using Latios;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Player
{
    public partial struct PlayerProcessExpSystem : ISystem
    {

        LatiosWorldUnmanaged m_latiosWorld;
        EntityQuery m_playerQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorld = state.GetLatiosWorldUnmanaged();
            m_playerQuery = state.Fluent()
                .With<PlayerTag>()
                .With<PlayerExperience>()
                .Without<DeadTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            var expQueue = m_latiosWorld.sceneBlackboardEntity.GetCollectionComponent<PlayerExpQueue>()
                .ExpQueue;
            
            if (expQueue.IsEmpty())
                return;
            
            state.Dependency = new ProcessXepJob
            {
                ExpQueue = expQueue
            }.ScheduleParallel(m_playerQuery, state.Dependency);
            
            
        }

        [BurstCompile]
        partial struct ProcessXepJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeQueue<int> ExpQueue;

            void Execute(ref PlayerExperience playerExperience)
            {
                if (ExpQueue.TryDequeue(out int xp))
                {
                    playerExperience.CurrentExperience += xp;
                }
            }
        }
        
    }
}