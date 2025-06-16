using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Survivors.Play.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Animations
{
    public class SingleDeathAnimationAuthoring : MonoBehaviour
    {
        [SerializeField] AnimationClipProperty properties;

        [TemporaryBakingType]
        struct AnimationClipSmartBakeItem : ISmartBakeItem<SingleDeathAnimationAuthoring>
        {
            SmartBlobberHandle<SkeletonClipSetBlob> m_clipHandle;

            public bool Bake(SingleDeathAnimationAuthoring authoring, IBaker baker)
            {
                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                baker.AddComponent<DeathClips>(entity);

                var clips = new NativeArray<SkeletonClipConfig>(1, Allocator.Temp);

                var clip = new SkeletonClipConfig
                {
                    clip     = authoring.properties.clip,
                    settings = SkeletonClipCompressionSettings.kDefaultSettings
                };

                clips[0] = clip;

                m_clipHandle = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);

                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
            {
                var clipset = m_clipHandle.Resolve(entityManager);
                entityManager.SetComponentData(entity, new DeathClips
                {
                    ClipSet = clipset
                });
            }
        }

        class ClipBaker : SmartBaker<SingleDeathAnimationAuthoring, AnimationClipSmartBakeItem> { }


        class SingleDeathAnimationAuthoringBaker : Baker<SingleDeathAnimationAuthoring>
        {
            public override void Bake(SingleDeathAnimationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DeathClipState
                {
                    State = new ClipState
                    {
                        Time            = 0f,
                        SpeedMultiplier = authoring.properties.speedMultiplier
                    }
                });
            }
        }
    }
}