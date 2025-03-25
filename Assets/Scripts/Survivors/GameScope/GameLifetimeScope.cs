using Survivors.GameScope.Commands;
using Survivors.GameScope.MonoBehaviours;
using Survivors.ScriptableObjects;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using VitalRouter.VContainer;

namespace Survivors.GameScope
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] GameScenesReferences gameScenesReferences;
        [SerializeField] CinemachineBehaviour cinemachineBehaviour;
        [SerializeField] CurtainBehaviour curtainBehaviour;
        
#if UNITY_EDITOR

        protected override void Awake()
        {
            base.Awake();

            // Dispose MANUALLY the world when exiting play mode
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode) World.DefaultGameObjectInjectionWorld?.Dispose();
            };
        }
#endif

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameScenesReferences);
            builder.RegisterInstance(cinemachineBehaviour);
            builder.RegisterInstance(curtainBehaviour);

            builder.RegisterVitalRouter(routingBuilder => { routingBuilder.Map<GlobalRouter>(); });

            builder.RegisterBuildCallback(container =>
            {
                // Once the container built, we want to start the game in the main menu
                var publisher = container.Resolve<ICommandPublisher>();
                publisher.PublishAsync(new MainMenuStateCommand());
            });
        }
    }
}