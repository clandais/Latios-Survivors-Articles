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


            var addComponentsCommandBuffer =
                m_latiosWorldUnmanaged.syncPoint.CreateAddComponentsCommandBuffer<HitInfos>(
                    AddComponentsDestroyedEntityResolution.DropData);

            addComponentsCommandBuffer.AddComponentTag<DeadTag>();

            var ecb = m_latiosWorldUnmanaged.syncPoint.CreateEntityCommandBuffer();

            state.Dependency = new ThrownWeaponCollisionJob
            {
                AddComponentsCommandBuffer = addComponentsCommandBuffer.AsParallelWriter(),
                SfxTriggeredTags           = SystemAPI.GetComponentLookup<SfxTriggeredTag>(),
                EnemyCollisionLayer        = enemyCollisionLayer,
                DeltaTime                  = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }


        [BurstCompile]
        partial struct ThrownWeaponCollisionJob : IJobEntity
        {
            [ReadOnly] public CollisionLayer EnemyCollisionLayer;
            [ReadOnly] public float DeltaTime;
            public AddComponentsCommandBuffer<HitInfos>.ParallelWriter AddComponentsCommandBuffer;
            [NativeDisableParallelForRestriction] public ComponentLookup<SfxTriggeredTag> SfxTriggeredTags;

            void Execute(
                ref WorldTransform transform,
                in ThrownWeaponComponent thrownWeapon,
                in Collider collider
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
                        AddComponentsCommandBuffer.Add(bodyInfos.entity, new HitInfos
                        {
                            Position = hitInfos.hitpoint,
                            Normal   = hitInfos.normalOnTarget * thrownWeapon.Speed
                        }, bodyInfos.bodyIndex);


                        SfxTriggeredTags.SetComponentEnabled(bodyInfos.entity, true);
                    }


                    transformQvs.position += thrownWeapon.Direction * steppedSpeed * DeltaTime;
                    transformQvs.rotation = math.mul(transformQvs.rotation, Quat.RotateAroundAxis(
                        thrownWeapon.RotationAxis, steppedRotation * DeltaTime));
                }
            }
        }
    }
}