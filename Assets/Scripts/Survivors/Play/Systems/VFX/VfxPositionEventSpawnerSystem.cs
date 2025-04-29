using Latios;
using Latios.LifeFX;
using Latios.Transforms;
using Survivors.Play.Authoring.Player.Weapons;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.VFX
{
    public partial struct VfxPositionEventSpawnerSystem : ISystem
    {
        LatiosWorldUnmanaged m_worldUnmanaged;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mailBox = m_worldUnmanaged.worldBlackboardEntity.GetCollectionComponent<GraphicsEventPostal>(true);
            var dcb = m_worldUnmanaged.syncPoint.CreateDestroyCommandBuffer();

            state.Dependency = new VfxPositionEventSpawnerJob
            {
                Dcb     = dcb.AsParallelWriter(),
                MailBox = mailBox
            }.ScheduleParallel(state.Dependency);
        }
    }


    [BurstCompile]
    internal partial struct VfxPositionEventSpawnerJob : IJobEntity
    {
        public GraphicsEventPostal                 MailBox;
        public DestroyCommandBuffer.ParallelWriter Dcb;

        void Execute(Entity entity, [EntityIndexInQuery] int idx, in WorldTransform transform,
            ref OneShotPositionEventSpawner spawner)
        {
            var position = transform.worldTransform.position;
            MailBox.Send(position, spawner.PositionGraphicsEventTunnel);
            Dcb.Add(entity, idx);
        }
    }
}