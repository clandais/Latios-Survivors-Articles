using System;
using Survivors.Play.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.Enemies
{
    public class BoidAuthoring : MonoBehaviour
    {
        [SerializeField] BoidSettings boidSettings;

        class BoidAuthoringBaker : Baker<BoidAuthoring>
        {
            public override void Bake(BoidAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<BoidTag>(entity);
                AddBuffer<BoidNeighbor>(entity);
                AddComponent(entity, authoring.boidSettings);
                AddComponent(entity, new BoidForces
                {
                    AlignmentForce = float3.zero,
                    AvoidanceForce = float3.zero,
                    CenteringForce = float3.zero
                });
            }
        }
    }

    [Serializable]
    public struct BoidSettings : IComponentData
    {
        public float neighborRadius;
        public float centeringStrength;

        public float avoidanceRadius;
        public float avoidanceStrength;

        public float alignmentRadius;
        public float alignmentStrength;

        public float followStrength;
    }

    public struct BoidForces : IComponentData
    {
        public float3 AvoidanceForce;
        public float3 AlignmentForce;
        public float3 CenteringForce;
    }
}