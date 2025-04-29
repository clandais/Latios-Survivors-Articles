using System.Collections.Generic;
using Latios;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Enemies.SFX
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Survivors/Enemies/SFX/SkeletonHitSfx")]
    public class SkeletonHitSfxAuthoring : MonoBehaviour
    {
        [SerializeField] List<GameObject> sfxPrefabs;

        class SkeletonHitSfxAuthoringBaker : Baker<SkeletonHitSfxAuthoring>
        {
            public override void Bake(SkeletonHitSfxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<SkeletonHitSfxBufferElement>(entity);

                foreach (var sfxPrefab in authoring.sfxPrefabs)
                {
                    var prefab = GetEntity(sfxPrefab, TransformUsageFlags.Dynamic);
                    buffer.Add(new SkeletonHitSfxBufferElement
                    {
                        SfxPrefab = prefab
                    });
                }
            }
        }
    }


    [InternalBufferCapacity(8)]
    public struct SkeletonHitSfxBufferElement : IBufferElementData
    {
        public EntityWith<Prefab> SfxPrefab;
    }
}