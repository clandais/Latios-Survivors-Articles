using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using ICollectionComponent = Latios.ICollectionComponent;

namespace Survivors.Play.Authoring.SceneBlackBoard
{
    public class SceneWeapons : MonoBehaviour
    {
        [SerializeField] public GameObject AxePrefab;
        [SerializeField]        float      AxeSize = 1f;

        class SceneWeaponsPrefabsBaker : Baker<SceneWeapons>
        {
            public override void Bake(SceneWeapons authoring)
            {
                if (!authoring.AxePrefab) return;

                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new WeaponPerks
                {
                    Size = authoring.AxeSize
                });

                var buffer = AddBuffer<PrefabBufferElement>(entity);
                var prefab = GetEntity(authoring.AxePrefab, TransformUsageFlags.Dynamic);
                buffer.Add(new PrefabBufferElement
                {
                    Prefab = prefab
                });
            }
        }
    }


    public struct PrefabBufferElement : IBufferElementData
    {
        public EntityWith<Prefab> Prefab;
    }

    public struct WeaponPerks : IComponentData
    {
        public float Size;
    }

    public partial struct WeaponSpawnQueue : ICollectionComponent
    {
        public struct WeaponSpawnData
        {
            public EntityWith<Prefab> WeaponPrefab;
            public float3             Direction;
            public float3             Position;
        }

        public NativeQueue<WeaponSpawnData> WeaponQueue;

        public JobHandle TryDispose(JobHandle inputDeps)
        {
            if (!WeaponQueue.IsCreated)
                return inputDeps;

            return WeaponQueue.Dispose(inputDeps);
        }
    }
}