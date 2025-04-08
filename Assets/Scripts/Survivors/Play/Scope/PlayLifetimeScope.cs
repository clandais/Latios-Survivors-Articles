using Survivors.Play.Scope.MonoBehaviours;
using Survivors.Play.Systems.Camera;
using Survivors.Play.Systems.Input;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using VitalRouter.VContainer;

namespace Survivors.Play.Scope
{
    public class PlayLifetimeScope : LifetimeScope
    {
        [SerializeField] PlayStateMenu playStateMenu;
        [SerializeField] Image         crosshair;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playStateMenu);
            builder.RegisterInstance(crosshair);

            builder.UseEntryPoints(cfg =>
            {
                cfg.Add<PlayStateController>();
                cfg.OnException(Debug.LogException);
            });

            builder.RegisterVitalRouter(routing =>
            {
                routing.Isolated = true;
                routing.Map<PlayStateRouter>();
            });


            builder.RegisterSystemFromDefaultWorld<EscapeKeySystem>();
            builder.RegisterSystemFromDefaultWorld<CinemachineTargetUpdater>();
            builder.RegisterSystemFromDefaultWorld<PlayerInputSystem>();

            builder.RegisterBuildCallback(container =>
            {
                var publisher = Parent.Container.Resolve<ICommandPublisher>();
                var playStateRouter = container.Resolve<PlayStateRouter>();
                playStateRouter.ParentPublisher = publisher;
            });
        }
    }
}