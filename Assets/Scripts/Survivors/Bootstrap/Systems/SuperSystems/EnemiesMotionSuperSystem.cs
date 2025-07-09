using Latios;
using Survivors.Play.Systems.Enemies;
using Survivors.Play.Systems.NavMesh;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class EnemiesMotionSuperSystem : SuperSystem
    {
        EntityQuery m_query;

        protected override void CreateSystems()
        {
            // m_query = Fluent.With<FloorGridConstructedTag>()
            //     .Build();


            GetOrCreateAndAddUnmanagedSystem<NavMeshDebugSystem>();

            GetOrCreateAndAddUnmanagedSystem<EnemiesRequestPathToPlayerSystem>();
            GetOrCreateAndAddUnmanagedSystem<EnemiesPathDebugSystem>();

            GetOrCreateAndAddUnmanagedSystem<EnemyBoidCollisionAvoidanceSystem>();
            GetOrCreateAndAddUnmanagedSystem<SetBoidAgentsGoalSystem>();
            GetOrCreateAndAddUnmanagedSystem<FollowPlayerSystem>();
            // GetOrCreateAndAddUnmanagedSystem<CollideAndSlideSystem>();
        }

        // public override bool ShouldUpdateSystem() => !m_query.IsEmptyIgnoreFilter;
    }
}