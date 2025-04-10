using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Enemies
{
    [BurstCompile]
    public partial struct FollowPlayerSystem : ISystem
    {
        LatiosWorldUnmanaged m_world;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
            m_query = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<RigidBody>()
                .With<MovementSettings>()
                .With<PreviousVelocity>()
                .With<EnemyTag>()
                .Without<DeadTag>()
                .Build();

            // state.RequireForUpdate<PlayerPosition>();
            state.RequireForUpdate<FloorGridConstructedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var environmentLayer = m_world.sceneBlackboardEntity.GetCollectionComponent<EnvironmentCollisionLayer>(true).layer;
            var playerPosition = m_world.sceneBlackboardEntity.GetComponentData<PlayerPosition>();
            var grid = m_world.sceneBlackboardEntity.GetCollectionComponent<FloorGrid>();

            state.Dependency = new FollowPlayerJob
            {
                EnvironmentLayer = environmentLayer,
                Grid             = grid,
                DeltaTime        = SystemAPI.Time.DeltaTime,
                PlayerPosition   = playerPosition
            }.ScheduleParallel(m_query, state.Dependency);
        }


        [BurstCompile]
        partial struct FollowPlayerJob : IJobEntity
        {
            [ReadOnly] public CollisionLayer EnvironmentLayer;
            [ReadOnly] public FloorGrid      Grid;
            [ReadOnly] public float          DeltaTime;
            [ReadOnly] public PlayerPosition PlayerPosition;

            void Execute(TransformAspect transformAspect,
                in MovementSettings movementSettings,
                ref RigidBody rigidBody,
                ref PreviousVelocity previousVelocity)
            {
                var cellPos = Grid.WorldToCell(transformAspect.worldPosition.xz);
                var cellIdx = Grid.CellToIndex(cellPos);

                var targetDelta = float3.zero;
                var vecDelta = float2.zero;

                if (cellIdx >= 0 & cellIdx < Grid.CellCount) vecDelta = Grid.VectorField[cellIdx];

                var deltaToPlayer = math.normalizesafe(PlayerPosition.Position - transformAspect.worldPosition);

                var rayStart = transformAspect.worldPosition;
                var rayEnd = transformAspect.worldPosition + deltaToPlayer * 25f;

                // Check if the raycast hits the environment
                // If it does, we just follow the vector field
                if (!Latios.Psyshock.Physics.Raycast(rayStart, rayEnd, in EnvironmentLayer, out _, out _)) vecDelta += deltaToPlayer.xz;

                
                
                vecDelta = math.normalizesafe(vecDelta);


                targetDelta.xz = vecDelta;
                targetDelta.y  = transformAspect.worldPosition.y;


                var currentVelocity = rigidBody.velocity.linear;
                var desiredVelocity = math.normalize(targetDelta) * movementSettings.moveSpeed;

                desiredVelocity.y      = currentVelocity.y;
                previousVelocity.Value = currentVelocity;

                currentVelocity           = currentVelocity.MoveTowards(desiredVelocity, movementSettings.speedChangeRate);
                rigidBody.velocity.linear = currentVelocity;


                var lookDirection = math.length(currentVelocity) > math.EPSILON ? math.normalize(currentVelocity) : math.normalize(desiredVelocity);
                var lookRotation = quaternion.LookRotationSafe(lookDirection, math.up());
                transformAspect.worldRotation = transformAspect.worldRotation.RotateTowards(lookRotation, movementSettings.maxAngleDelta * DeltaTime);
            }
        }
    }
}