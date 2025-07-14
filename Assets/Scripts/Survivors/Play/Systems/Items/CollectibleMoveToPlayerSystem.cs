using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.Collectibles;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Items
{
    [RequireMatchingQueriesForUpdate]
    public partial struct CollectibleMoveToPlayerSystem : ISystem
    {
        LatiosWorldUnmanaged m_world;
        EntityQuery          m_xpQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
            m_xpQuery = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<CollectibleTag>()
                .With<Velocity>()
                .With<MaxSpeed>()
                .With<MaxForce>()
                .With<FollowRadius>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerPosition = m_world.sceneBlackboardEntity
                .GetComponentData<PlayerPosition>().Position;

            state.Dependency = new MoveToPlayerJob
            {
                PlayerPosition = playerPosition,
                DeltaTime      = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(m_xpQuery, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        partial struct MoveToPlayerJob : IJobEntity
        {
            [ReadOnly] public float3 PlayerPosition;
            [ReadOnly] public float  DeltaTime;

            void Execute(Entity entity,
                [EntityIndexInQuery] int index,
                TransformAspect transform,
                in FollowRadius followRadius,
                in MaxSpeed maxSpeed,
                in MaxForce maxForce,
                ref Velocity velocity)
            {
                var worldTransform = transform.worldTransform;

                worldTransform.rotation =
                    math.mul(worldTransform.rotation, Quat.RotateAroundAxis(math.up(), 90f * DeltaTime));

                if (math.distancesq(worldTransform.position, PlayerPosition) > followRadius.Value * followRadius.Value
                    && math.lengthsq(velocity.Value) < 0.01f)
                {
                    transform.worldTransform = worldTransform;
                    return;
                }

                var desiredVelocity = math.normalize(PlayerPosition + math.up() - worldTransform.position) *
                                      maxSpeed.Value;

                var force = desiredVelocity - velocity.Value;
                force *= maxForce.Value / maxSpeed.Value;

                velocity.Value += force * DeltaTime;

                worldTransform.position += velocity.Value * DeltaTime;

                // Update the transform
                transform.worldTransform = worldTransform;
            }
        }
    }
}