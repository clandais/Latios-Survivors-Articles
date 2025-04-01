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
    public class FourDirectionsAnimationsAuthoring : MonoBehaviour
    {
        public FourDirAnimations animations;
        [SerializeField] float intertialBlendDuration = 0.15f;
        [SerializeField] float velocityChangeThreshold = 0.1f;

        [TemporaryBakingType]
        struct AnimationClipsSmartBakeItem : ISmartBakeItem<FourDirectionsAnimationsAuthoring>
        {
            SmartBlobberHandle<SkeletonClipSetBlob> m_clipSetHandle;

            public bool Bake(FourDirectionsAnimationsAuthoring authoring,
                IBaker baker)
            {
                if (authoring.animations.IsMissingAnimations()) return false;

                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                baker.AddComponent<Clips>(entity);

                var clips = new NativeArray<SkeletonClipConfig>(5, Allocator.Temp);

                for (var i = 0; i < 5; i++)
                {
                    AnimationClip clip = null;

                    switch (i)
                    {
                        case (int)EDirections.Center:
                            clip = authoring.animations.center.clip;

                            break;
                        case (int)EDirections.Down:
                            clip = authoring.animations.down.clip;

                            break;
                        case (int)EDirections.Up:
                            clip = authoring.animations.up.clip;

                            break;
                        case (int)EDirections.Left:
                            clip = authoring.animations.left.clip;

                            break;
                        case (int)EDirections.Right:
                            clip = authoring.animations.right.clip;

                            break;
                    }


                    clips[i] = new SkeletonClipConfig
                    {
                        clip     = clip,
                        settings = SkeletonClipCompressionSettings.kDefaultSettings
                    };
                }


                m_clipSetHandle = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);

                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager,
                Entity entity)
            {
                entityManager.SetComponentData(entity, new Clips { ClipSet = m_clipSetHandle.Resolve(entityManager) });
            }
        }


        class ClipBaker : SmartBaker<FourDirectionsAnimationsAuthoring, AnimationClipsSmartBakeItem> { }

        class FourDirectionsAnimationsAuthoringBaker : Baker<FourDirectionsAnimationsAuthoring>
        {
            public override void Bake(FourDirectionsAnimationsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var clipStates = new FourDirectionClipStates
                {
                    Center = new ClipState { SpeedMultiplier = authoring.animations.center.speedMultiplier },
                    Down   = new ClipState { SpeedMultiplier = authoring.animations.down.speedMultiplier },
                    Up     = new ClipState { SpeedMultiplier = authoring.animations.up.speedMultiplier },
                    Left   = new ClipState { SpeedMultiplier = authoring.animations.left.speedMultiplier },
                    Right  = new ClipState { SpeedMultiplier = authoring.animations.right.speedMultiplier }
                };

                AddComponent(entity, clipStates);

                AddComponent(entity,
                    new InertialBlendState(authoring.intertialBlendDuration, authoring.velocityChangeThreshold));
            }
        }
    }


    #region Animation Authoring Structs

    [Serializable]
    public struct FourDirAnimations
    {
        public AnimationClipProperty center;
        public AnimationClipProperty down;
        public AnimationClipProperty up;
        public AnimationClipProperty left;
        public AnimationClipProperty right;

        public bool IsMissingAnimations()
        {
            return center.clip == null || down.clip == null || up.clip == null || left.clip == null || right.clip == null;
        }
    }

    #endregion
}