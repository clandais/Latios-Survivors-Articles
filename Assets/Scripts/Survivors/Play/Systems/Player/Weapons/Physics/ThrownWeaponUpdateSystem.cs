using Latios;
using Latios.Anna;
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
    public partial struct ThrownWeaponUpdateSystem : ISystem
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
            var dcb = m_latiosWorldUnmanaged.syncPoint.CreateDestroyCommandBuffer();
            var collisionLayer = m_latiosWorldUnmanaged.sceneBlackboardEntity
                .GetCollectionComponent<EnvironmentCollisionLayer>().layer;


            state.Dependency = new ThrownWeaponUpdateJob
            {
                DeltaTime            = SystemAPI.Time.DeltaTime,
                EnvironmentLayer     = collisionLayer,
                DestroyCommandBuffer = dcb.AsParallelWriter()
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }


    [BurstCompile]
    internal partial struct ThrownWeaponUpdateJob : IJobEntity
    {
        public            DestroyCommandBuffer.ParallelWriter DestroyCommandBuffer;
        [ReadOnly] public CollisionLayer                      EnvironmentLayer;

        [ReadOnly] public float DeltaTime;

        void Execute(
            Entity entity,
            [EntityIndexInQuery] int entityIndexInQuery,
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
                var collision = Latios.Psyshock.Physics.ColliderCast(
                    in collider,
                    in transformQvs,
                    transformQvs.position + thrownWeapon.Direction * thrownWeapon.Speed * DeltaTime,
                    in EnvironmentLayer,
                    out _,
                    out _);

                if (collision)
                {
                    DestroyCommandBuffer.Add(entity, entityIndexInQuery);

                    return;
                }


                transformQvs.position += thrownWeapon.Direction * steppedSpeed * DeltaTime;
                transformQvs.rotation = math.mul(transformQvs.rotation, Quat.RotateAroundAxis(
                    thrownWeapon.RotationAxis, steppedRotation * DeltaTime));
            }


            transform.worldTransform = transformQvs;
        }
    }
}