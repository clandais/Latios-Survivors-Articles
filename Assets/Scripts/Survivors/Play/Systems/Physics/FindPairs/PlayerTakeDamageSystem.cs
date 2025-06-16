using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring.Enemies;
using Survivors.Play.Authoring.Player.SFX;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Survivors.Play.Systems.Physics.FindPairs
{
    [BurstCompile]
    public partial struct PlayerTakeDamageSystem : ISystem, ISystemNewScene
    {
        BuildCollisionLayerTypeHandles m_handles;
        LatiosWorldUnmanaged           m_world;
        EntityQuery                    m_playerQuery;
        EntityQuery                    m_enemyAttackingQuery;
        Rng                            m_rng;

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
                PlayerHealthLookup  = SystemAPI.GetComponentLookup<PlayerHealth>(),
                DeathVoiceSfxLookup = SystemAPI.GetComponentLookup<DeathVoiceSfx>(),
                PlayerHitSfxLookup  = SystemAPI.GetBufferLookup<PlayerHitSfxBufferElement>(),
                Time                = (float)SystemAPI.Time.ElapsedTime,
                Ecb                 = ecb.AsParallelWriter(),
                Rng                 = m_rng.Shuffle()
            };

            state.Dependency = Latios.Psyshock.Physics.FindPairs(playerLayer, attackingEnemyLayer, findPairProcessor)
                .ScheduleParallelByA(JobHandle.CombineDependencies(playerLayerJh, enemyLayerJh));
        }


        struct DamagePlayerProcessor : IFindPairsProcessor
        {
            public PhysicsComponentLookup<PlayerHealth>           PlayerHealthLookup;
            public PhysicsComponentLookup<DeathVoiceSfx>          DeathVoiceSfxLookup;
            public PhysicsBufferLookup<PlayerHitSfxBufferElement> PlayerHitSfxLookup;
            public float                                          Time;
            public EntityCommandBuffer.ParallelWriter             Ecb;
            public Rng                                            Rng;

            public void Execute(in FindPairsResult result)
            {
                var random = Rng.GetSequence(result.jobIndex);

                ref var playerHealth = ref PlayerHealthLookup.GetRW(result.entityA).ValueRW;

                if (playerHealth.LastDamageTime + playerHealth.DamageDelay < Time)
                {
                    playerHealth.CurrentHealth  -= 1;
                    playerHealth.LastDamageTime =  Time;

                    var playerHitSfx = PlayerHitSfxLookup[result.entityA];
                    var randInt = random.NextInt(0, playerHitSfx.Length);

                    var playerHitSfxPrefab = playerHitSfx[randInt].HitSfxPrefab;
                    var playerHitSfxInstance = Ecb.Instantiate(result.bodyIndexA, playerHitSfxPrefab);
                    var transform = new WorldTransform
                    {
                        worldTransform = result.transformA
                    };

                    Ecb.SetComponent(result.bodyIndexA, playerHitSfxInstance, transform);
                }


                if (playerHealth.CurrentHealth <= 0)
                {
                    Ecb.AddComponent<DeadTag>(result.bodyIndexA, result.entityA);
                    var deathSfx = DeathVoiceSfxLookup.GetRW(result.entityA).ValueRO.DeathSfxPrefab;
                    var deathSfxInstance = Ecb.Instantiate(result.bodyIndexA, deathSfx);
                    var transform = new WorldTransform
                    {
                        worldTransform = result.transformA
                    };

                    Ecb.SetComponent(result.bodyIndexA, deathSfxInstance, transform);
                    Ecb.RemoveComponent<RigidBody>(result.bodyIndexA, result.entityA);
                }
            }
        }

        public void OnNewScene(ref SystemState state)
        {
            m_rng = new Rng("PlayerTakeDamageSystem");
        }
    }
}