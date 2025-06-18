using Latios;
using LatiosNavigation.Authoring;
using LatiosNavigation.Utils;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.NavMesh
{
    [RequireMatchingQueriesForUpdate]
    public partial struct NavMeshDebugSystem : ISystem
    {
        EntityQuery m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_query = state.Fluent()
                .With<NavMeshSurfaceBlobReference>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new DebugJob().Schedule(m_query, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }


        [BurstCompile]
        partial struct DebugJob : IJobEntity
        {
            void Execute(in NavMeshSurfaceBlobReference blob)
            {
                ref var blobAsset = ref blob.NavMeshSurfaceBlob.Value;
                NavUtils.Debug(ref blobAsset);
            }
        }
    }
}