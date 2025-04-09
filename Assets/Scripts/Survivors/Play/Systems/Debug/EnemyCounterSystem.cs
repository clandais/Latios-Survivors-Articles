using Survivors.Play.Authoring;
using Survivors.Play.Components;
using Survivors.Play.Scope;
using Unity.Entities;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Systems.Debug
{
    public partial class EnemyCounterSystem : SystemBase
    {
        ICommandPublisher m_publisher;

        [Inject]
        public void Construct(ICommandPublisher publisher)
        {
            m_publisher = publisher;
        }
        
        protected override void OnCreate()
        {
            RequireForUpdate<PlayerTag>();
        }
        
        protected override void OnUpdate()
        {
            string message = "";
            
            var aliveEnemyCount = SystemAPI.QueryBuilder().WithAll<EnemyTag>().WithNone<DeadTag>().Build().CalculateEntityCount();
            message += $"Alive Enemies: {aliveEnemyCount}\n";
            
            var deadEnemyCount = SystemAPI.QueryBuilder().WithAll<EnemyTag>().WithAll<DeadTag>().Build().CalculateEntityCount();
            message += $"Dead Enemies: {deadEnemyCount}\n";

            m_publisher.PublishAsync(new DebugTextCommand
            {
                Text = message,
            });
        }
    }
}