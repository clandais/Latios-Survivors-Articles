using Latios;
using Latios.Psyshock;
using Survivors.Play.Authoring.SceneBlackBoard;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Collider = Latios.Psyshock.Collider;

namespace Survivors.Play.Systems.Physics
{
    public partial struct DrawNavSystem : ISystem
    {
        EntityQuery m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_query = state.Fluent()
                .With<GridCollisionSettings>()
                .With<Collider>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new DrawNavJob().ScheduleParallel(m_query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        partial struct DrawNavJob : IJobEntity
        {
            void Execute(in Collider collider)
            {
                if (collider.type == ColliderType.TriMesh)
                {
                    TriMeshCollider triMeshCollider = collider;

                    ref var triangles =
                        ref triMeshCollider.triMeshColliderBlob.Value.triangles;

                    for (var i = 0; i < triangles.Length; i++)
                    {
                        var tri = triangles[i];

                        UnityEngine.Debug.DrawLine(tri.pointA, tri.pointB, Color.red);
                        UnityEngine.Debug.DrawLine(tri.pointB, tri.pointC, Color.red);
                        UnityEngine.Debug.DrawLine(tri.pointC, tri.pointA, Color.red);
                    }
                }
            }
        }
    }
}