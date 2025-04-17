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

                var clipsForKinemation = new NativeArray<SkeletonClipConfig>(5, Allocator.Temp);

                var authoringClips = new AnimationClip[5];

                authoringClips[(int)EDirections.Center] = authoring.animations.center.clip;
                authoringClips[(int)EDirections.Down]   = authoring.animations.down.clip;
                authoringClips[(int)EDirections.Up]     = authoring.animations.up.clip;
                authoringClips[(int)EDirections.Left]   = authoring.animations.left.clip;
                authoringClips[(int)EDirections.Right]  = authoring.animations.right.clip;


                for (var i = 0; i < 5; i++)
                {
                    var clip = authoringClips[i];
                    if (clip == null) continue;


                    clipsForKinemation[i] = new SkeletonClipConfig
                    {
                        clip     = clip,
                        settings = SkeletonClipCompressionSettings.kDefaultSettings,
                        events   = clip.ExtractKinemationClipEvents(Allocator.Temp)
                    };
                }


                m_clipSetHandle = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clipsForKinemation);

                clipsForKinemation.Dispose();

                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager,
                Entity entity)
            {
                entityManager.SetComponentData(entity, new Clips { ClipSet = m_clipSetHandle.Resolve(entityManager) });
            }
        }


        class ClipBaker : SmartBaker<FourDirectionsAnimationsAuthoring, AnimationClipsSmartBakeItem>
        {
        }

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
            return center.clip == null || down.clip == null || up.clip == null || left.clip == null ||
                   right.clip == null;
        }
    }

    #endregion
}