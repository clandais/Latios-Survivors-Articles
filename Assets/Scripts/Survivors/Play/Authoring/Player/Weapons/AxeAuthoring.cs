using Survivors.Play.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Survivors.Play.Authoring.Player.Weapons
{
    public class AxeAuthoring : MonoBehaviour
    {
        [Header("Axe Config")] [SerializeField]
        float speed;

        [SerializeField] float rotationSpeed;
        [SerializeField] float3 rotationAxis;

        [FormerlySerializedAs("axeSlashVfxPrefab")] [SerializeField]
        OneShotVfxSpawnerAuthoring oneShotVfxSpawnerPrefab;

        class AxeAuthoringBaker : Baker<AxeAuthoring>
        {
            public override void Bake(AxeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<ThrownWeaponComponent>(entity);
                AddComponent(entity, new ThrownWeaponConfigComponent
                (
                    authoring.speed,
                    authoring.rotationSpeed,
                    authoring.rotationAxis
                ));

                AddComponent<WeaponTag>(entity);

                AddComponent(entity, new ThrownWeaponHitVfx
                {
                    Prefab = GetEntity(authoring.oneShotVfxSpawnerPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}
