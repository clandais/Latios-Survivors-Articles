using Latios;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Enemies
{
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct EnemyKilledSystem : ISystem, ISystemNewScene
    {
        EntityQuery          _query;
        LatiosWorldUnmanaged _world;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _query = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<EnemyTag>()
                .With<DeadTag>()
                .With<Collider>()
                .With<XpDropPrefab>()
                .Build();

            _world = state.GetLatiosWorldUnmanaged();
        }

        public void OnNewScene(ref SystemState state)
        {
            state.InitSystemRng("DisableDeadCollidersSystem");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var rcb = _world.syncPoint.CreateEntityCommandBuffer();

            var xpSpawnQueue = _world.sceneBlackboardEntity.GetCollectionComponent<XpSpawnQueue>()
                .XpQueue;

            state.Dependency = new RemoveCollidersJob
            {
                CommandBuffer = rcb.AsParallelWriter(),
                XpSpawnQueue  = xpSpawnQueue.AsParallelWriter()
            }.ScheduleParallel(_query, state.Dependency);
        }

        [BurstCompile]
        partial struct RemoveCollidersJob : IJobEntity
        {
            public NativeQueue<XpSpawnQueue.XpSpawnData>.ParallelWriter XpSpawnQueue;
            public EntityCommandBuffer.ParallelWriter                   CommandBuffer;

            void Execute(Entity entity,
                [EntityIndexInQuery] int index,
                TransformAspect transform,
                in XpDropPrefab dropPrefab)
            {
                CommandBuffer.RemoveComponent<Collider>(index, entity);

                XpSpawnQueue.Enqueue(new XpSpawnQueue.XpSpawnData
                {
                    Position = transform.worldPosition,
                    XpPrefab = dropPrefab.Prefab
                });
            }
        }
    }
}