using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Settings
{
    public class FlowFieldSettingsAuthoring : MonoBehaviour
    {
        [SerializeField] int cellSize = 2;

        class FlowFieldSettingsBaker : Baker<FlowFieldSettingsAuthoring>
        {
            public override void Bake(FlowFieldSettingsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FlowFieldSettings
                {
                    CellSize = authoring.cellSize
                });
            }
        }
    }
}