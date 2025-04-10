using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring
{
    public class LevelTagAuthoring : MonoBehaviour
    {
        class LevelTagAuthoringBaker : Baker<LevelTagAuthoring>
        {
            public override void Bake(LevelTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<LevelTag>(entity);
            }
        }
        
    }
    
    public struct LevelTag : IComponentData { }
}