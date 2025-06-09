using Latios;
using Survivors.Play.Components;
using Survivors.Play.Systems.Enemies;
using Survivors.Play.Systems.Enemies.Boids;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class EnemiesMotionSuperSystem : SuperSystem
    {
        EntityQuery m_query;

        protected override void CreateSystems()
        {
            m_query = Fluent.With<FloorGridConstructedTag>()
                .Build();


            GetOrCreateAndAddUnmanagedSystem<BoidsInitializeSystem>();
            GetOrCreateAndAddUnmanagedSystem<BoidsCenterSystem>();
            GetOrCreateAndAddUnmanagedSystem<BoidsAvoidanceSystem>();
            GetOrCreateAndAddUnmanagedSystem<BoidsAlignmentSystem>();
            GetOrCreateAndAddUnmanagedSystem<BoidsFollowSystem>();
            GetOrCreateAndAddUnmanagedSystem<FollowPlayerSystem>();
        }

        public override bool ShouldUpdateSystem() => !m_query.IsEmptyIgnoreFilter;
    }
}