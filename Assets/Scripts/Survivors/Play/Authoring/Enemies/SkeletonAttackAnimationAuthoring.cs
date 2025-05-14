using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Survivors.Play.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Enemies
{
    public class SkeletonAttackAnimationAuthoring : MonoBehaviour
    {
        [SerializeField]        AnimationClipProperty attackClipProperty;
        [SerializeField] public float                 minDistanceToTarget = 1f;

        [TemporaryBakingType]
        struct AttackClipSmartBakeItem : ISmartBakeItem<SkeletonAttackAnimationAuthoring>
        {
            SmartBlobberHandle<SkeletonClipSetBlob> m_clipSetHandle;

            public bool Bake(SkeletonAttackAnimationAuthoring authoring, IBaker baker)
            {
                if (!authoring.attackClipProperty.clip) return false;

                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                baker.AddComponent<SkeletonMinionAttackAnimation>(entity);

                var clips = new NativeArray<SkeletonClipConfig>(1, Allocator.Temp);
                var clip = authoring.attackClipProperty.clip;

                clips[0] = new SkeletonClipConfig
                {
                    clip     = clip,
                    settings = SkeletonClipCompressionSettings.kDefaultSettings,
                    events   = clip.ExtractKinemationClipEvents(Allocator.Temp)
                };

                m_clipSetHandle = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);

                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
            {
                entityManager.SetComponentData(entity, new SkeletonMinionAttackAnimation
                {
                    ClipSet = m_clipSetHandle.Resolve(entityManager)
                });
            }
        }

        class AttackClipBaker : SmartBaker<SkeletonAttackAnimationAuthoring, AttackClipSmartBakeItem> { }

        class SkeletonAttackAnimationAuthoringBaker : Baker<SkeletonAttackAnimationAuthoring>
        {
            public override void Bake(SkeletonAttackAnimationAuthoring animationAuthoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var clipState = new ClipState
                {
                    SpeedMultiplier = animationAuthoring.attackClipProperty.speedMultiplier,
                    CurrentWeight   = 1f
                };

                AddComponent(entity, new SkeletonMinionAttackAnimationState
                {
                    State            = clipState,
                    DistanceToTarget = animationAuthoring.minDistanceToTarget
                });

                AddComponent(entity, new SkeletonMinionAttackAnimationTag());
                SetComponentEnabled<SkeletonMinionAttackAnimationTag>(entity, false);
            }
        }
    }


    public struct SkeletonMinionAttackAnimationTag : IComponentData, IEnableableComponent { }

    public struct SkeletonMinionAttackAnimation : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> ClipSet;
    }

    public struct SkeletonMinionAttackAnimationState : IComponentData
    {
        public ClipState State;
        public float     DistanceToTarget;
    }
}