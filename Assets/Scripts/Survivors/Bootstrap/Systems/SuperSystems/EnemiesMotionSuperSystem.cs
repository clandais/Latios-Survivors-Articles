using Latios;
using Survivors.Play.Components;
using Survivors.Play.Systems.Enemies;
using Survivors.Play.Systems.NavMesh;
using Unity.Entities;

namespace Survivors.Bootstrap.Systems.SuperSystems
{
    public partial class EnemiesMotionSuperSystem : SuperSystem
    {
        EntityQuery m_query;

        protected override void CreateSystems()
        {
            m_query = Fluent.With<PlayerTag>()
                .With<DeadTag>()
                .Build();

            GetOrCreateAndAddUnmanagedSystem<NavMeshDebugSystem>();

            GetOrCreateAndAddUnmanagedSystem<EnemiesRequestPathToPlayerSystem>();

            GetOrCreateAndAddUnmanagedSystem<EnemyBoidCollisionAvoidanceSystem>();
            GetOrCreateAndAddUnmanagedSystem<SetBoidAgentsGoalSystem>();
            GetOrCreateAndAddUnmanagedSystem<FollowPlayerSystem>();
        }

        public override bool ShouldUpdateSystem() => m_query.IsEmptyIgnoreFilter;
    }
}