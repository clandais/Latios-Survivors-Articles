using Latios.Anna.Systems;
using Latios.Transforms.Systems;
using Survivors.Play.Components;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSuperSystem))]
    public partial class AnnaRootSystem : AnnaSuperSystem
    {
        EntityQuery m_shouldUpdateQuery;


        protected override void CreateSystems()
        {
            m_shouldUpdateQuery = Fluent.With<PauseRequestedTag>()
                .Build();

            base.CreateSystems();
            //
            // EnableSystemSorting = false;
            //
            // GetOrCreateAndAddUnmanagedSystem<BuildEnvironmentCollisionLayerSystem>();
            // GetOrCreateAndAddUnmanagedSystem<BuildKinematicCollisionLayerSystem>();
            // GetOrCreateAndAddUnmanagedSystem<BuildRigidBodyCollisionLayerSystem>();
            // GetOrCreateAndAddUnmanagedSystem<RigidBodyVsRigidBodySystem>();
            // GetOrCreateAndAddUnmanagedSystem<RigidBodyVsEnvironmentSystem>();
            // GetOrCreateAndAddUnmanagedSystem<RigidBodyVsKinematicSystem>();
            // GetOrCreateAndAddUnmanagedSystem<SolveSystem>();
            // GetOrCreateAndAddUnmanagedSystem<IntegrateRigidBodiesSystem>();
        }

        public override bool ShouldUpdateSystem() => m_shouldUpdateQuery.IsEmptyIgnoreFilter;
    }
}