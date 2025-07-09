using Latios;
using Survivors.Play.Components;
using Survivors.Play.Systems.Animations;
using Survivors.Play.Systems.Player;
using Survivors.Play.Systems.Player.Weapons.Initialization;
using Unity.Entities;

namespace Survivors.Bootstrap.RootSystems.SuperSystems
{
    public partial class AnimationSuperSystem : SuperSystem
    {
        protected override void CreateSystems()
        {
            GetOrCreateAndAddUnmanagedSystem<FourDirectionsAnimationSystem>();
            GetOrCreateAndAddUnmanagedSystem<FourDirectionsAnimationEventsSystem>();


            GetOrCreateAndAddUnmanagedSystem<SkeletonAttackAnimationSystem>();
            GetOrCreateAndAddUnmanagedSystem<SkeletonDeathSystem>();
            GetOrCreateAndAddUnmanagedSystem<PlayerDeathSystem>();
        }
    }

    public partial class PlayerAnimationSuperSystem : SuperSystem
    {
        EntityQuery m_entityQuery;

        protected override void CreateSystems()
        {
            m_entityQuery = Fluent.With<PlayerTag>().With<DeadTag>().Build();

            GetOrCreateAndAddUnmanagedSystem<PlayerActionSystem>();
            GetOrCreateAndAddUnmanagedSystem<WeaponThrowTriggerSystem>();
        }


        public override bool ShouldUpdateSystem() => m_entityQuery.IsEmptyIgnoreFilter;
    }
}