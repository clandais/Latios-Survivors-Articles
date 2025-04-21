using System;
using Latios;
using Survivors.Play.Authoring.SFX;
using Unity.Entities;

namespace Survivors.Play.Components
{
    [Serializable]
    public struct SfxListRef
    {
        public OneShotSfxListAuthoring oneShotSfxPrefabs;
        public ESfxEventType           eventType;
    }

    public struct OneShotSfxSpawnerRef : IComponentData
    {
        public EntityWith<Prefab> SfxPrefab;
        public ESfxEventType      EventType;
    }

    public struct OneShotSfxSpawner : IComponentData
    {
        public int SfxCount;
    }

    public struct SfxTriggeredTag : IComponentData, IEnableableComponent { }


    [InternalBufferCapacity(8)]
    public struct OneShotSfxElement : IBufferElementData
    {
        public Entity Prefab;
    }
}