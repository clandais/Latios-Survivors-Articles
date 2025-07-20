using Latios;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player
{
    public partial struct PlayerProcessHpSystem : ISystem
    {
        LatiosWorldUnmanaged m_latiosWorld;
        EntityQuery          m_playerQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorld = state.GetLatiosWorldUnmanaged();
            // m_playerQuery = state.Fluent()
            //     .With<PlayerTag>()
            //     // .With<PlayerHealth>()
            //     // .Without<DeadTag>()
            //     .Build();
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var hpQueue = m_latiosWorld.sceneBlackboardEntity.GetCollectionComponent<PlayerHpQueue>()
                .HpQueue;
            
            if (hpQueue.IsEmpty())
                return;
            
            var playerHealth = m_latiosWorld.sceneBlackboardEntity.GetComponentData<PlayerHealth>();
            // Process the HP queue
            while (hpQueue.TryDequeue(out int hp))
            {
                if (hp < 0)
                {
                    playerHealth.LastDamageTime = (float)SystemAPI.Time.ElapsedTime;
                }
                playerHealth.CurrentHealth += hp;
                playerHealth.CurrentHealth = math.clamp(playerHealth.CurrentHealth, 0, playerHealth.MaxHealth);
            }
            
            m_latiosWorld.sceneBlackboardEntity.SetComponentData(playerHealth);
            
            // state.Dependency = new ProcessHpJob
            // {
            //     HpQueue = hpQueue
            // }.ScheduleParallel(m_playerQuery, state.Dependency);
        }

        
        // [BurstCompile]
        // partial struct ProcessHpJob : IJobEntity
        // {
        //     [NativeDisableParallelForRestriction] public NativeQueue<int> HpQueue;
        //
        //     void Execute(ref PlayerHealth playerHealth)
        //     {
        //         if (HpQueue.TryDequeue(out int hp))
        //         {
        //             playerHealth.CurrentHealth += hp;
        //             playerHealth.CurrentHealth = math.clamp(playerHealth.CurrentHealth, 0, playerHealth.MaxHealth);
        //         }
        //     }
        // }
        
    }
}