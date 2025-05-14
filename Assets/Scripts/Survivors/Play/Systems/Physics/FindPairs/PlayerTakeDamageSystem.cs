using Latios;
using Latios.Psyshock;
using Survivors.Play.Authoring.Enemies;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Play.Systems.Physics.FindPairs
{
    [BurstCompile]
    public partial struct PlayerTakeDamageSystem : ISystem
    {
        BuildCollisionLayerTypeHandles m_handles;
        LatiosWorldUnmanaged           m_world;
        EntityQuery                    m_playerQuery;
        EntityQuery                    m_enemyAttackingQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world   = state.GetLatiosWorldUnmanaged();
            m_handles = new BuildCollisionLayerTypeHandles(ref state);
            m_playerQuery = state.Fluent().With<PlayerTag>().Without<DeadTag>()
                .PatchQueryForBuildingCollisionLayer().Build();

            m_enemyAttackingQuery = state.Fluent().With<EnemyTag>().Without<DeadTag>()
                .WithEnabled<SkeletonMinionAttackAnimationTag>()
                .PatchQueryForBuildingCollisionLayer().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_handles.Update(ref state);

            var playerLayerJh = Latios.Psyshock.Physics.BuildCollisionLayer(m_playerQuery, m_handles)
                .ScheduleParallel(out var playerLayer, state.WorldUpdateAllocator, state.Dependency);

            var enemyLayerJh = Latios.Psyshock.Physics.BuildCollisionLayer(m_enemyAttackingQuery, m_handles)
                .ScheduleParallel(out var attackingEnemyLayer, state.WorldUpdateAllocator, state.Dependency);


            var ecb = m_world.syncPoint.CreateEntityCommandBuffer();

            var findPairProcessor = new DamagePlayerProcessor
            {
                PlayerHealthLookup = SystemAPI.GetComponentLookup<PlayerHealth>(),
                Time               = (float)SystemAPI.Time.ElapsedTime,
                Ecb                = ecb.AsParallelWriter()
            };

            state.Dependency = Latios.Psyshock.Physics.FindPairs(playerLayer, attackingEnemyLayer, findPairProcessor)
                .ScheduleParallelByA(JobHandle.CombineDependencies(playerLayerJh, enemyLayerJh));
        }


        struct DamagePlayerProcessor : IFindPairsProcessor
        {
            public PhysicsComponentLookup<PlayerHealth> PlayerHealthLookup;
            public float                                Time;
            public EntityCommandBuffer.ParallelWriter   Ecb;

            public void Execute(in FindPairsResult result)
            {
                ref var playerHealth = ref PlayerHealthLookup.GetRW(result.entityA).ValueRW;

                if (playerHealth.LastDamageTime + playerHealth.DamageDelay < Time)
                {
                    playerHealth.CurrentHealth  -= 1;
                    playerHealth.LastDamageTime =  Time;
                }

                if (playerHealth.CurrentHealth <= 0) Ecb.AddComponent<DeadTag>(result.bodyIndexA, result.entityA);
            }
        }
    }
}