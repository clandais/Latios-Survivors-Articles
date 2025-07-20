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
        // EntityQuery m_playerQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorld = state.GetLatiosWorldUnmanaged();
            // m_playerQuery = state.Fluent()
            //     .With<PlayerTag>()
            //     .With<PlayerExperience>()
            //     .Without<DeadTag>()
            //     .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            var expQueue = m_latiosWorld.sceneBlackboardEntity.GetCollectionComponent<PlayerExpQueue>()
                .ExpQueue;
            
            if (expQueue.IsEmpty())
                return;

            var playerExperience = m_latiosWorld.sceneBlackboardEntity.GetComponentData<PlayerExperience>();
            // Process the experience queue
            while (expQueue.TryDequeue(out int xp))
            {
                playerExperience.CurrentExperience += xp;

                if (playerExperience.CurrentExperience >= playerExperience.ExperienceToNextLevel)
                {
                    playerExperience.CurrentExperience -= playerExperience.ExperienceToNextLevel;
                    playerExperience.CurrentLevel++;
                }
            }
            
            m_latiosWorld.sceneBlackboardEntity.SetComponentData(playerExperience);
            
            // state.Dependency = new ProcessXepJob
            // {
            //     ExpQueue = expQueue
            // }.ScheduleParallel(m_playerQuery, state.Dependency);
            
            
        }

        // [BurstCompile]
        // partial struct ProcessXepJob : IJobEntity
        // {
        //     [NativeDisableParallelForRestriction] public NativeQueue<int> ExpQueue;
        //
        //     void Execute(ref PlayerExperience playerExperience)
        //     {
        //         if (ExpQueue.TryDequeue(out int xp))
        //         {
        //             playerExperience.CurrentExperience += xp;
        //
        //             if (playerExperience.CurrentExperience >= playerExperience.ExperienceToNextLevel)
        //             {
        //                 playerExperience.CurrentExperience -= playerExperience.ExperienceToNextLevel;
        //                 playerExperience.CurrentLevel++;
        //             }
        //         }
        //     }
        // }
        
    }
}