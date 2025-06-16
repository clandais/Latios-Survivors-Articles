using System.Collections.Generic;
using Latios;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.SFX
{
    public class PlayerHitSfxAuthoring : MonoBehaviour
    {
        [SerializeField] List<GameObject> HitSfxPrefabs;

        class PlayerHitSfxAuthoringBaker : Baker<PlayerHitSfxAuthoring>
        {
            public override void Bake(PlayerHitSfxAuthoring authoring)
            {
                if (authoring.HitSfxPrefabs.Count == 0)
                    return;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<PlayerHitSfxBufferElement>(entity);

                foreach (var hitSfxPrefab in authoring.HitSfxPrefabs)
                {
                    var prefab = GetEntity(hitSfxPrefab, TransformUsageFlags.Dynamic);
                    buffer.Add(new PlayerHitSfxBufferElement
                    {
                        HitSfxPrefab = prefab
                    });
                }
            }
        }
    }


    [InternalBufferCapacity(8)]
    public struct PlayerHitSfxBufferElement : IBufferElementData
    {
        public EntityWith<Prefab> HitSfxPrefab;
    }
}