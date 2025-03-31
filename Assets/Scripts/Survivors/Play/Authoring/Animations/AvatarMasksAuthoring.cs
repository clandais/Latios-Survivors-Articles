using System.Collections.Generic;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Survivors.Play.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Animations
{
    public class AvatarMasksAuthoring : MonoBehaviour
    {
        [SerializeField] List<AvatarMask> avatarMasks;


        [TemporaryBakingType]
        struct AvatarMasksSmartBakeItem : ISmartBakeItem<AvatarMasksAuthoring>
        {
            SmartBlobberHandle<SkeletonBoneMaskSetBlob> m_blobberHandle;

            public bool Bake(AvatarMasksAuthoring authoring,
                IBaker baker)
            {
                if (authoring.avatarMasks.Count == 0)
                    return false;

                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                baker.AddComponent<AvatarMasks>(entity);

                var masks = new NativeArray<UnityObjectRef<AvatarMask>>(authoring.avatarMasks.Count, Allocator.Temp);
                for (var i = 0; i < authoring.avatarMasks.Count; i++) masks[i] = authoring.avatarMasks[i];

                m_blobberHandle = baker.RequestCreateBlobAsset(baker.GetComponent<Animator>(), masks);
                masks.Dispose();

                return true;
            }

            public void PostProcessBlobRequests(EntityManager entityManager,
                Entity entity)
            {
                entityManager.SetComponentData(entity, new AvatarMasks
                {
                    Blob = m_blobberHandle.Resolve(entityManager)
                });
            }
        }

        class AvatarMasksBaker : SmartBaker<AvatarMasksAuthoring, AvatarMasksSmartBakeItem> { }
    }
}