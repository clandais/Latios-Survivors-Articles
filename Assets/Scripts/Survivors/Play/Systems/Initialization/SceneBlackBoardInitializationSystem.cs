using Latios;
using Survivors.Play.Authoring.SceneBlackBoard;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Initialization
{
    [BurstCompile]
    public partial struct SceneBlackBoardInitializationSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged m_worldUnmanaged;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        public void OnNewScene(ref SystemState state)
        {
            m_worldUnmanaged.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new WeaponSpawnQueue
            {
                WeaponQueue = new NativeQueue<WeaponSpawnQueue.WeaponSpawnData>(Allocator.Persistent)
            });

            m_worldUnmanaged.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new SfxSpawnQueue
            {
                SfxQueue = new NativeQueue<SfxSpawnQueue.SfxSpawnData>(Allocator.Persistent)
            });

            m_worldUnmanaged.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld(new VfxSpawnQueue
            {
                VfxQueue = new NativeQueue<VfxSpawnQueue.VfxSpawnData>(Allocator.Persistent)
            });
        }
    }
}