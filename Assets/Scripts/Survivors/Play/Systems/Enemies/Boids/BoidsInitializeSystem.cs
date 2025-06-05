using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring.Enemies;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Enemies.Boids
{
    public partial struct BoidsInitializeSystem : ISystem
    {
        EntityQuery m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_query = state.Fluent()
                .WithAspect<TransformAspect>()
                .With<RigidBody>()
                .With<BoidTag>()
                .With<BoidNeighbor>()
                .With<BoidSettings>()
                .Without<DeadTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var bodies = new NativeArray<ColliderBody>(
                m_query.CalculateEntityCount(),
                Allocator.TempJob);

            state.Dependency = new ClearNeighborBufferJob
            {
                Bodies = bodies
            }.ScheduleParallel(m_query, state.Dependency);

            state.Dependency = Latios.Psyshock.Physics.BuildCollisionLayer(bodies)
                .ScheduleParallel(out var layer, Allocator.TempJob, state.Dependency);

            var findNeighbors = new FindNeighbors
            {
                EnemyTagLookup = SystemAPI.GetBufferLookup<BoidNeighbor>()
            };

            state.Dependency = Latios.Psyshock.Physics.FindPairs(layer, findNeighbors)
                .ScheduleParallel(state.Dependency);


            state.Dependency = bodies.Dispose(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }


        [BurstCompile]
        partial struct ClearNeighborBufferJob : IJobEntity
        {
            public NativeArray<ColliderBody> Bodies;

            void Execute(Entity entity, [EntityIndexInQuery] int idx,
                ref DynamicBuffer<BoidNeighbor> neighbors,
                in BoidSettings boidSettings,
                TransformAspect transform)
            {
                neighbors.Clear();

                var sphereCollider = new SphereCollider(float3.zero, boidSettings.neighborRadius / 2f);

                Bodies[idx] = new ColliderBody
                {
                    collider  = sphereCollider,
                    entity    = entity,
                    transform = transform.worldTransform
                };
            }
        }

        struct FindNeighbors : IFindPairsProcessor
        {
            public PhysicsBufferLookup<BoidNeighbor> EnemyTagLookup;

            public void Execute(in FindPairsResult result)
            {
                if (Latios.Psyshock.Physics.DistanceBetween(result.colliderA, result.transformA,
                        result.colliderB, result.transformB, 0f, out _))
                {
                    EnemyTagLookup[result.entityA].Add(new BoidNeighbor
                    {
                        Neighbor = new EntityWith<BoidTag>
                        {
                            entity = result.entityB
                        }
                    });


                    EnemyTagLookup[result.entityB].Add(new BoidNeighbor
                    {
                        Neighbor = new EntityWith<BoidTag>
                        {
                            entity = result.entityA
                        }
                    });
                }
            }
        }
    }
}