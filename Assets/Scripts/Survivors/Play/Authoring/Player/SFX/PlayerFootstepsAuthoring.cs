using System.Collections.Generic;
using Latios;
using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.SFX
{
    public class PlayerFootstepsAuthoring : MonoBehaviour
    {
        [SerializeField] List<GameObject> footstepPrefabs;
        [SerializeField] ESfxEventType eventType;

        class PlayerFootstepsAuthoringBaker : Baker<PlayerFootstepsAuthoring>
        {
            public override void Bake(PlayerFootstepsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<FootstepBufferElement>(entity);

                foreach (var footstepPrefab in authoring.footstepPrefabs)
                {
                    var prefab = GetEntity(footstepPrefab, TransformUsageFlags.Dynamic);
                    buffer.Add(new FootstepBufferElement
                    {
                        FootstepPrefab = prefab
                    });
                }
            }
        }
    }


    [InternalBufferCapacity(8)]
    public struct FootstepBufferElement : IBufferElementData
    {
        public EntityWith<Prefab> FootstepPrefab;
    }
}