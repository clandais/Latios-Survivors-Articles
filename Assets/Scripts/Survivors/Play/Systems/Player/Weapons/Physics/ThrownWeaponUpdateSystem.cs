using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring.Player.Weapons;
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
        public DestroyCommandBuffer.ParallelWriter DestroyCommandBuffer;
        [ReadOnly] public CollisionLayer EnvironmentLayer;
        [ReadOnly] public float DeltaTime;

        public void Execute(
            Entity entity,
            [EntityIndexInQuery] int entityIndexInQuery,
            ref WorldTransform transform,
            in ThrownWeaponComponent thrownWeapon,
            in Collider collider
        )
        {
            var transformQvs = transform.worldTransform;

            float3 startPosition = transformQvs.position;
            float3 endPosition = startPosition + thrownWeapon.Direction * thrownWeapon.Speed * DeltaTime;
            float stepSize = math.length(endPosition - startPosition) / 4; // 4: why not.

            for (float i = 0; i <= 1; i+= stepSize)
            {
                float3 currentPosition = math.lerp(startPosition, endPosition, i);
                
                if (Latios.Psyshock.Physics.ColliderCast(
                        in collider,
                        in transformQvs,
                        currentPosition,
                        in EnvironmentLayer,
                        out var result,
                        out _))
                {
                    switch (collider.type)
                    {
                        case ColliderType.Capsule:
                        {
                            CapsuleCollider capsuleCollider = collider;

                            if (result.distance <= capsuleCollider.radius * 2f)
                            {
                                DestroyCommandBuffer.Add(entity, entityIndexInQuery);

                                return;
                            }
                            
                            break;
                        }
                        default:
                            // Don't care
                            break;
                    }
                }
                
            }
            transform.worldTransform.position = endPosition;


            transform.worldTransform.rotation = math.mul(transformQvs.rotation, Quat.RotateAroundAxis(
                thrownWeapon.RotationAxis,
                thrownWeapon.RotationSpeed * DeltaTime));
        }
    }
}