using Survivors.GameScope;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using VitalRouter.VContainer;

namespace Survivors.MainMenu
{
    public class MainMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] MainMenuBehavior m_menuBehavior;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(m_menuBehavior);

            builder.UseEntryPoints(cfg =>
            {
                cfg.Add<MainMenuEntryPoint>();
                cfg.OnException(Debug.LogException);
            });

            builder.RegisterVitalRouter(routing =>
            {
                routing.Isolated = true;
                routing.Map<MainMenuRouter>();
            });

            builder.RegisterBuildCallback(container =>
            {
                var publisher = Parent.Container.Resolve<ICommandPublisher>();
                var mainMenuRouter = container.Resolve<MainMenuRouter>();
                mainMenuRouter.ParentPublisher = publisher;
            });
        }
    }
}