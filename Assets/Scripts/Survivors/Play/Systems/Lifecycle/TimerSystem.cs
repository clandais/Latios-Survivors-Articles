using Latios;
using Survivors.Play.Components;
using Survivors.Play.Scope.Commands;
using Unity.Entities;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Systems.Lifecycle
{
    public partial class TimerSystem : SubSystem
    {
        ICommandPublisher m_commandPublisher;

        EntityQuery m_entityQuery;
        EntityQuery m_gameOverQuery;

        [Inject]
        public void Construct(ICommandPublisher commandPublisher)
        {
            m_commandPublisher = commandPublisher;
        }


        protected override void OnCreate()
        {
            m_entityQuery = Fluent.With<PauseRequestedTag>()
                .Build();

            m_gameOverQuery = Fluent.With<GameOverScreenRequestedTag>()
                .Build();

            RequireForUpdate<GameTimerComponent>();
        }

        public override bool ShouldUpdateSystem() =>
            m_entityQuery.IsEmptyIgnoreFilter && m_gameOverQuery.IsEmptyIgnoreFilter;

        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            var gameTimer = sceneBlackboardEntity.GetComponentData<GameTimerComponent>();
            gameTimer.TimeRemaining -= deltaTime;


            if (gameTimer.TimeRemaining <= 0)
            {
                gameTimer.TimeRemaining = 0;
                sceneBlackboardEntity.AddComponent<GameOverScreenRequestedTag>();
                sceneBlackboardEntity.AddComponent<PauseRequestedTag>();

                var gameStats = sceneBlackboardEntity.GetComponentData<GameStatsComponent>();

                m_commandPublisher.PublishAsync(new TimerEndedCommand
                {
                    Kills = gameStats.EnemiesKilled
                });
            }
            else
            {
                sceneBlackboardEntity.SetComponentData(gameTimer);
                m_commandPublisher.PublishAsync(new UpdateTimeCommand
                {
                    TimeRemaining = gameTimer.TimeRemaining
                });
            }
        }
    }
}