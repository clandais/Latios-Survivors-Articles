﻿using Latios;
using Survivors.Play.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.Weapons
{
    public class AxeAuthoring : MonoBehaviour
    {
        [Header("Axe Config")] [SerializeField]
        float speed;

        [SerializeField] float  rotationSpeed;
        [SerializeField] float3 rotationAxis;

        [SerializeField] AxeSlashVfxAuthoring axeSlashVfxPrefab;

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
                    Prefab = GetEntity(authoring.axeSlashVfxPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }

    public struct ThrownWeaponComponent : IComponentData
    {
        public float  Speed;
        public float  RotationSpeed;
        public float3 RotationAxis;
        public float3 Direction;
    }

    public struct ThrownWeaponConfigComponent : IComponentData
    {
        public readonly float  Speed;
        public readonly float  RotationSpeed;
        public readonly float3 RotationAxis;

        public ThrownWeaponConfigComponent(float speed,
            float rotationSpeed,
            float3 rotationAxis)
        {
            Speed         = speed;
            RotationSpeed = rotationSpeed;
            RotationAxis  = rotationAxis;
        }
    }

    public struct ThrownWeaponHitVfx : IComponentData
    {
        public EntityWith<Prefab> Prefab;
    }
}