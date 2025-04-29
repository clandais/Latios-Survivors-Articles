using System.Collections.Generic;
using Latios;
using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.Weapons
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Survivors/Player/Weapon/AxeSwoosh")]
    public class AxeSwooshAuthoring : MonoBehaviour
    {
        [SerializeField] List<GameObject> swooshPrefabs;
        [SerializeField] ESfxEventType    eventType;

        class AxeSwooshAuthoringBaker : Baker<AxeSwooshAuthoring>
        {
            public override void Bake(AxeSwooshAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<AxeSwooshBufferElement>(entity);

                foreach (var swooshPrefab in authoring.swooshPrefabs)
                {
                    var prefab = GetEntity(swooshPrefab, TransformUsageFlags.Dynamic);
                    buffer.Add(new AxeSwooshBufferElement
                    {
                        SwooshPrefab = prefab
                    });
                }
            }
        }
    }

    [InternalBufferCapacity(8)]
    public struct AxeSwooshBufferElement : IBufferElementData
    {
        public EntityWith<Prefab> SwooshPrefab;
    }
}