﻿using Survivors.Play.Scope.MonoBehaviours;
using Survivors.Play.Systems.Camera;
using Survivors.Play.Systems.Debug;
using Survivors.Play.Systems.Input;
using Survivors.Play.Systems.SFX;
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
        [SerializeField] DebugCanvas   debugCanvas;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playStateMenu);
            builder.RegisterInstance(crosshair);
            builder.RegisterInstance(debugCanvas);

            builder.UseEntryPoints(cfg =>
            {
                cfg.Add<PlayStateController>();
                cfg.OnException(Debug.LogException);
            });

            builder.RegisterVitalRouter(routing =>
            {
                routing.Isolated = true;
                routing.Map<PlayStateRouter>();
                routing.Map<DebugRouter>();
            });


            builder.RegisterSystemFromDefaultWorld<EscapeKeySystem>();
            builder.RegisterSystemFromDefaultWorld<CinemachineTargetUpdater>();
            builder.RegisterSystemFromDefaultWorld<PlayerInputSystem>();
            builder.RegisterSystemFromDefaultWorld<MainAudioListenerUpdateSystem>();
            builder.RegisterSystemFromDefaultWorld<EnemyCounterSystem>();

            builder.RegisterBuildCallback(container =>
            {
                var publisher = Parent.Container.Resolve<ICommandPublisher>();
                var playStateRouter = container.Resolve<PlayStateRouter>();
                playStateRouter.ParentPublisher = publisher;
            });
        }
    }
}