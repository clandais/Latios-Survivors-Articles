using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Survivors.Play.Authoring.Materials
{
    public class MaterialDissolveAuthoring : MonoBehaviour
    {
        [SerializeField] float dissolveSpeedMultiplier = .5f;

        class MaterialDissolveAuthoringBaker : Baker<MaterialDissolveAuthoring>
        {
            public override void Bake(MaterialDissolveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                var dissolveAmount = new MaterialDissolveAmount
                {
                    Value = 0.0f
                };

                AddComponent(entity, dissolveAmount);

                AddComponent(entity, new MaterialDissolveSpeed
                {
                    Value = authoring.dissolveSpeedMultiplier
                });
            }
        }
    }


    [MaterialProperty("_Dissolve_Amount")]
    public struct MaterialDissolveAmount : IComponentData
    {
        public float Value;
    }

    public struct MaterialDissolveSpeed : IComponentData
    {
        public float Value;
    }
}