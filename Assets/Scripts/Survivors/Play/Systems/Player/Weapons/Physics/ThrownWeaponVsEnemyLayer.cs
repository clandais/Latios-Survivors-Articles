using Latios;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring.Player.Weapons;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player.Weapons.Physics
{
    public partial struct ThrownWeaponVsEnemyLayer : ISystem
    {
        LatiosWorldUnmanaged m_latiosWorldUnmanaged;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var enemyCollisionLayer = m_latiosWorldUnmanaged.sceneBlackboardEntity
                .GetCollectionComponent<EnemyCollisionLayer>().Layer;


            var icb = m_latiosWorldUnmanaged.syncPoint.CreateEntityCommandBuffer();

            state.Dependency = new ThrownWeaponCollisionJob
            {
                EnemyCollisionLayer = enemyCollisionLayer,
                DeltaTime           = SystemAPI.Time.DeltaTime,
                HitInfosLookup      = SystemAPI.GetComponentLookup<HitInfos>(),
                Icb                 = icb.AsParallelWriter(),
                VfxQueue = m_latiosWorldUnmanaged.sceneBlackboardEntity
                    .GetCollectionComponent<VfxSpawnQueue>().VfxQueue.AsParallelWriter()
            }.ScheduleParallel(state.Dependency);
        }


        [BurstCompile]
        partial struct ThrownWeaponCollisionJob : IJobEntity
        {
            [ReadOnly]                            public CollisionLayer                     EnemyCollisionLayer;
            [ReadOnly]                            public float                              DeltaTime;
            [NativeDisableParallelForRestriction] public ComponentLookup<HitInfos>          HitInfosLookup;
            public                                       EntityCommandBuffer.ParallelWriter Icb;

            [NativeDisableParallelForRestriction]
            public NativeQueue<VfxSpawnQueue.VfxSpawnData>.ParallelWriter VfxQueue;

            void Execute(
                Entity _,
                [EntityIndexInQuery] int entityIndexInQuery,
                ref WorldTransform transform,
                in ThrownWeaponComponent thrownWeapon,
                in Collider collider,
                in ThrownWeaponHitVfx thrownWeaponHitVfx
            )
            {
                var transformQvs = transform.worldTransform;
                var stepCount = 4;

                var steppedSpeed = thrownWeapon.Speed / stepCount;
                var steppedRotation = thrownWeapon.RotationSpeed / stepCount;

                for (float i = 0; i < stepCount; i++)
                {
                    if (Latios.Psyshock.Physics.ColliderCast(in collider, in transformQvs,
                            transformQvs.position + thrownWeapon.Direction * thrownWeapon.Speed * DeltaTime,
                            in EnemyCollisionLayer,
                            out var hitInfos,
                            out var bodyInfos))
                    {
                        var e = bodyInfos.entity;
                        if (HitInfosLookup.IsComponentEnabled(e)) continue;

                        Icb.AddComponent<DeadTag>(bodyInfos.bodyIndex, e);

                        HitInfosLookup.SetComponentEnabled(e, true);
                        var hitInfosComponent = HitInfosLookup[e];
                        hitInfosComponent.Position = hitInfos.hitpoint;
                        hitInfosComponent.Normal   = hitInfos.normalOnTarget * thrownWeapon.Speed;
                        HitInfosLookup[e]          = hitInfosComponent;


                        VfxQueue.Enqueue(new VfxSpawnQueue.VfxSpawnData
                        {
                            VfxPrefab = thrownWeaponHitVfx.Prefab,
                            Position  = hitInfos.hitpoint
                        });
                    }

                    transformQvs.position += thrownWeapon.Direction * steppedSpeed * DeltaTime;
                    transformQvs.rotation = math.mul(transformQvs.rotation, Quat.RotateAroundAxis(
                        thrownWeapon.RotationAxis, steppedRotation * DeltaTime));
                }
            }
        }
    }
}