using System.Collections.Generic;
using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.SFX
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Survivors/Play/SFX/One Shot SFX List")]
    public class OneShotSfxListAuthoring : MonoBehaviour
    {
        [SerializeField] List<GameObject> oneShotSfxPrefabs;

        class OneShotSfxListAuthoringBaker : Baker<OneShotSfxListAuthoring>
        {
            public override void Bake(OneShotSfxListAuthoring authoring)
            {
                if (authoring.oneShotSfxPrefabs.Count == 0) return;


                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new OneShotSfxSpawner
                {
                    SfxCount = authoring.oneShotSfxPrefabs.Count
                });

                var buffer = AddBuffer<OneShotSfxElement>(entity);
                foreach (var prefab in authoring.oneShotSfxPrefabs)
                    buffer.Add(new OneShotSfxElement
                    {
                        Prefab = GetEntity(prefab, TransformUsageFlags.Dynamic)
                    });
            }
        }
    }
}