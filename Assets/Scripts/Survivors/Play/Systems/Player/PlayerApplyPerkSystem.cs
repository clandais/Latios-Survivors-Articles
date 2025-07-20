using Latios;
using R3;
using Survivors.Play.Components;
using Survivors.Play.Scope;
using Survivors.Play.Scope.Perks;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Systems.Player
{
    public partial class PlayerApplyPerkSystem : SubSystem
    {
        ICommandSubscribable m_commandSubscribable;
        DisposableBag        m_disposableBag;

        [Inject]
        public void Construct(ICommandSubscribable commandSubscribable)
        {
            m_commandSubscribable = commandSubscribable;

            m_commandSubscribable.Subscribe<PerkSelectedCommand>(OnPerkSelected)
                .AddTo(ref m_disposableBag);
        }

        void OnPerkSelected(PerkSelectedCommand perkSelectedCommand, PublishContext _)
        {
            var perk = perkSelectedCommand.Perk;

            if (perk is HealthIncreasePerk healthIncreasePerk)
            {
                var playerHealth = sceneBlackboardEntity.GetComponentData<PlayerHealth>();
                playerHealth = healthIncreasePerk.Apply(
                    playerHealth
                );

                sceneBlackboardEntity.SetComponentData(playerHealth);
            }

            sceneBlackboardEntity.RemoveComponent<PlayerLevelUpScreenRequestedTag>();
        }

        protected override void OnUpdate() { }

        protected override void OnDestroy()
        {
            m_disposableBag.Dispose();
        }
    }
}