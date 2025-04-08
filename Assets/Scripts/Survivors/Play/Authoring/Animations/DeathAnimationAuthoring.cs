using System;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Survivors.Play.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Animations
{
    public class DeathAnimationAuthoring : MonoBehaviour
    {
        [SerializeField] DeathClipsProperties deathAnimation;

        [TemporaryBakingType]
        struct AnimationClipSmartBakeItem : ISmartBakeItem<DeathAnimationAuthoring>
        {
            SmartBlobberHandle<SkeletonClipSetBlob> m_clipHandle;

            public bool Bake(DeathAnimationAuthoring authoring,
                IBaker baker)
            {
                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                baker.AddComponent<DeathClips>(entity);

                var clips = new NativeArray<SkeletonClipConfig>(3, Allocator.Temp);

                var clipA = new SkeletonClipConfig
                {
                    clip     = authoring.deathAnimation.DeathA.clip,
                    settings = SkeletonClipCompressionSettings.kDefaultSettings
                };

                var clipB = new SkeletonClipConfig
                {
                    clip     = authoring.deathAnimation.DeathB.clip,
                    settings = SkeletonClipCompressionSettings.kDefaultSettings
                };

                var clipC = new SkeletonClipConfig
                {
                    clip     = authoring.deathAnimation.DeathC.clip,
                    settings = SkeletonClipCompressionSettings.kDefaultSettings
                };

                clips[0] = clipA;
                clips[1] = clipB;
                clips[2] = clipC;

                m_clipHandle = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);

                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager,
                Entity entity)
            {
                var clipset = m_clipHandle.Resolve(entityManager);
                entityManager.SetComponentData(entity, new DeathClips
                {
                    ClipSet = clipset
                });
            }
        }

        class ClipBaker : SmartBaker<DeathAnimationAuthoring, AnimationClipSmartBakeItem> { }

        class DeathAnimationBaker : Baker<DeathAnimationAuthoring>
        {
            public override void Bake(DeathAnimationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DeathClipsStates
                {
                    StateA = new ClipState
                    {
                        Time            = 0f,
                        SpeedMultiplier = authoring.deathAnimation.DeathA.speedMultiplier
                    },
                    StateB = new ClipState
                    {
                        Time            = 0f,
                        SpeedMultiplier = authoring.deathAnimation.DeathB.speedMultiplier
                    },
                    StateC = new ClipState
                    {
                        Time            = 0f,
                        SpeedMultiplier = authoring.deathAnimation.DeathC.speedMultiplier
                    },
                    ChosenState = -1
                });
            }
        }
    }

    #region Skeletion Authoring Structs

    [Serializable]
    public struct DeathClipsProperties
    {
        public AnimationClipProperty DeathA;
        public AnimationClipProperty DeathB;
        public AnimationClipProperty DeathC;
    }

    #endregion
}