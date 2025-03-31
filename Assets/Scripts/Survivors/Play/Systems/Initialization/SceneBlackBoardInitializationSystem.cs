using Latios;
using Survivors.Play.Authoring.SceneBlackBoard;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Survivors.Play.Systems.Initialization
{
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
        }
    }
}