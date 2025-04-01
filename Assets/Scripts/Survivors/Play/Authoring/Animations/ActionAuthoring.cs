using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Survivors.Play.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Animations
{
    public class ActionAuthoring : MonoBehaviour
    {
        [SerializeField] AnimationClipProperty actionClipProperty;

        [TemporaryBakingType]
        struct ActionClipSmartBakeItem : ISmartBakeItem<ActionAuthoring>
        {
            SmartBlobberHandle<SkeletonClipSetBlob> m_clipSetHandle;

            public bool Bake(ActionAuthoring authoring,
                IBaker baker)
            {
                if (!authoring.actionClipProperty.clip) return false;

                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                baker.AddComponent<ActionClipComponent>(entity);

                var clips = new NativeArray<SkeletonClipConfig>(1, Allocator.Temp);
                var clip = authoring.actionClipProperty.clip;

                clips[0] = new SkeletonClipConfig
                {
                    clip     = clip,
                    settings = SkeletonClipCompressionSettings.kDefaultSettings,
                    events   = clip.ExtractKinemationClipEvents(Allocator.Temp)
                };

                m_clipSetHandle = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), clips);

                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager,
                Entity entity)
            {
                entityManager.SetComponentData(entity, new ActionClipComponent
                {
                    ClipSet = m_clipSetHandle.Resolve(entityManager),
                });
            }
        }


        class ActionClipBaker : SmartBaker<ActionAuthoring, ActionClipSmartBakeItem> { }

        class ActionAuthoringBaker : Baker<ActionAuthoring>
        {
            public override void Bake(ActionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var clipEvents = authoring.actionClipProperty.clip.ExtractKinemationClipEvents(Allocator.Temp);
                var clipState = new ActionClipState
                {
                    ClipState = new ClipState
                    {
                        SpeedMultiplier = authoring.actionClipProperty.speedMultiplier,
                        EventHash = clipEvents.Length > 0
                            ? clipEvents[0].name.GetHashCode()
                            : -1
                    }
                };

                AddComponent(entity, clipState);
            }
        }
    }

    public struct ActionClipComponent : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> ClipSet;
    }

    public struct ActionClipState : IComponentData
    {
        public ClipState ClipState;
    }
}