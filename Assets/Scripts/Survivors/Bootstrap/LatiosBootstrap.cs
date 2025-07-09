using Boids;
using Latios;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Latios.LifeFX;
using Latios.Myri;
using Latios.Navigator;
using Latios.Psyshock.Authoring;
using Latios.Transforms;
using Latios.Transforms.Authoring;
using Unity.Entities;
using UnityEngine.Scripting;

namespace Survivors.Bootstrap
{
    [Preserve]
    public class LatiosBakingBootstrap : ICustomBakingBootstrap
    {
        public void InitializeBakingForAllWorlds(ref CustomBakingBootstrapContext context)
        {
            //Latios.Authoring.CoreBakingBootstrap.ForceRemoveLinkedEntityGroupsOfLength1(ref context);
            TransformsBakingBootstrap.InstallLatiosTransformsBakers(ref context);
            PsyshockBakingBootstrap.InstallUnityColliderBakers(ref context);
            KinemationBakingBootstrap.InstallKinemation(ref context);
            // UnikaBakingBootstrap.InstallUnikaEntitySerialization(ref context);
            NavigatorBakingBootstrap.InstallNavigatorBakers(ref context);
        }
    }

    [Preserve]
    public class LatiosEditorBootstrap : ICustomEditorBootstrap
    {
        public World Initialize(string defaultEditorWorldName)
        {
            var world = new LatiosWorld(defaultEditorWorldName, WorldFlags.Editor);
            world.useExplicitSystemOrdering = true;

            var systems = DefaultWorldInitialization.GetAllSystemTypeIndices(WorldSystemFilterFlags.Default, true);
            BootstrapTools.InjectUnitySystems(systems, world, world.simulationSystemGroup);

            TransformsBootstrap.InstallTransforms(world, world.simulationSystemGroup);
            KinemationBootstrap.InstallKinemation(world);
            // CalligraphicsBootstrap.InstallCalligraphics(world);

            BootstrapTools.InjectRootSuperSystems(systems, world, world.simulationSystemGroup);

            return world;
        }
    }

    [Preserve]
    public class LatiosBootstrap : ICustomBootstrap
    {
        public bool Initialize(string defaultWorldName)
        {
            var world = new LatiosWorld(defaultWorldName);
            World.DefaultGameObjectInjectionWorld = world;
            world.useExplicitSystemOrdering       = true;

            var systems = DefaultWorldInitialization.GetAllSystemTypeIndices(WorldSystemFilterFlags.Default);

            BootstrapTools.InjectUnitySystems(systems, world, world.simulationSystemGroup);

            //Latios.CoreBootstrap.InstallSceneManager(world);
            TransformsBootstrap.InstallTransforms(world, world.simulationSystemGroup);
            MyriBootstrap.InstallMyri(world);
            KinemationBootstrap.InstallKinemation(world);
            // CalligraphicsBootstrap.InstallCalligraphics(world);
            // CalligraphicsBootstrap.InstallCalligraphicsAnimations(world);
            // UnikaBootstrap.InstallUnikaEntitySerialization(world);
            LifeFXBootstrap.InstallLifeFX(world);

            NavigatorBootstrap.InstallNavigator(world);
            StupidBoidsBootstrap.InstallBoids(world);

            // AnnaBootstrap.InstallAnna(world);

            BootstrapTools.InjectRootSuperSystems(systems, world, world.simulationSystemGroup);

            world.initializationSystemGroup.SortSystems();
            world.simulationSystemGroup.SortSystems();
            world.presentationSystemGroup.SortSystems();

            BootstrapTools.AddWorldToCurrentPlayerLoopWithDelayedSimulation(world);

            return true;
        }
    }
}