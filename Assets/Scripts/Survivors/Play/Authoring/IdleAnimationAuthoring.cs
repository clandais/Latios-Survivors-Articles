using System;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring
{
    [RequireComponent(typeof(Animator))]
    public class IdleAnimationAuthoring : MonoBehaviour
    {
        [SerializeField] AnimationClipProperty idleAnimation;

        [TemporaryBakingType]
        struct AnimationClipSmartBakeItem : ISmartBakeItem<IdleAnimationAuthoring>
        {
            SmartBlobberHandle<SkeletonClipSetBlob> m_handle;

            public bool Bake(IdleAnimationAuthoring authoring, IBaker baker)
            {
                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                baker.AddComponent<IdleClip>(entity);

                var clips = new NativeArray<SkeletonClipConfig>(1, Allocator.Temp);
                clips[0] = new SkeletonClipConfig
                {
                    clip     = authoring.idleAnimation.clip,
                    settings = SkeletonClipCompressionSettings.kDefaultSettings
                };

                m_handle = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);
                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
            {
                var clipSet = m_handle.Resolve(entityManager);
                entityManager.SetComponentData(entity, new IdleClip { ClipSet = clipSet });
            }
        }

        class IdleClipBaker : SmartBaker<IdleAnimationAuthoring, AnimationClipSmartBakeItem>
        {
        }

        class IdleAnimationAuthoringBaker : Baker<IdleAnimationAuthoring>
        {
            public override void Bake(IdleAnimationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new IdleClipState
                {
                    State = new ClipState
                    {
                        Time            = 0f,
                        PreviousTime    = 0f,
                        SpeedMultiplier = authoring.idleAnimation.speedMultiplier
                    }
                });
            }
        }
    }


    /// <summary>
    ///     Only relevant for the authoring time.
    /// </summary>
    [Serializable]
    public struct AnimationClipProperty
    {
        public AnimationClip clip;
        public float speedMultiplier;
    }

    /// <summary>
    ///   The idle clip to be played.
    /// </summary>
    public struct IdleClip : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> ClipSet;
    }


    /// <summary>
    ///    The state of the idle clip.
    /// </summary>
    public struct IdleClipState : IComponentData
    {
        public ClipState State;
    }

    
    /// <summary>
    ///  The state of a clip.
    /// </summary>
    public struct ClipState
    {
        public float PreviousTime;
        public float Time;
        public float SpeedMultiplier;
        public int EventHash;

        public void Update(float deltaTime)
        {
            PreviousTime =  Time;
            Time         += deltaTime;
        }
    }
}