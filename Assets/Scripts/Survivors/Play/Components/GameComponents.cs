using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Survivors.Play.Components
{
    public struct PauseRequestedTag : IComponentData
    {
    }

    public struct DeadTag : IComponentData
    {
    }


    public struct HitInfos : IComponentData, IEnableableComponent
    {
        public float3 Position;
        public float3 Normal;
    }

    public partial struct SfxSpawnQueue : ICollectionComponent
    {
        public struct SfxSpawnData
        {
            public int EventHash;
            public EntityWith<Prefab> SfxPrefab;
            public float3 Position;
        }

        public NativeQueue<SfxSpawnData> SfxQueue;

        public JobHandle TryDispose(JobHandle inputDeps)
        {
            if (!SfxQueue.IsCreated)
            {
                return inputDeps;
            }

            return SfxQueue.Dispose(inputDeps);
        }
    }

    public partial struct VfxSpawnQueue : ICollectionComponent
    {
        public struct VfxSpawnData
        {
            public EntityWith<Prefab> VfxPrefab;
            public float3 Position;
        }

        public NativeQueue<VfxSpawnData> VfxQueue;

        public JobHandle TryDispose(JobHandle inputDeps)
        {
            if (!VfxQueue.IsCreated)
            {
                return inputDeps;
            }

            return VfxQueue.Dispose(inputDeps);
        }
    }

    public partial struct XpSpawnQueue : ICollectionComponent
    {
        public struct XpSpawnData
        {
            public EntityWith<Prefab> XpPrefab;
            public float3 Position;
        }

        public NativeQueue<XpSpawnData> XpQueue;

        public JobHandle TryDispose(JobHandle inputDeps)
        {
            if (!XpQueue.IsCreated)
            {
                return inputDeps;
            }

            return XpQueue.Dispose(inputDeps);
        }
    }

    public struct ShouldDestroyTag : IComponentData
    {
    }


    #region Steering
    public struct Velocity : IComponentData
    {
        public float3 Value;
    }

    public struct MaxSpeed : IComponentData
    {
        public float Value;
    }

    public struct MaxForce : IComponentData
    {
        public float Value;
    }

    public struct FollowRadius : IComponentData
    {
        public float Value;
    }
    #endregion
}
