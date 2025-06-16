using Latios;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.SFX
{
    public class DeathAuthoring : MonoBehaviour
    {
        [SerializeField] GameObject deathSfxPrefab;

        class DeathAuthoringBaker : Baker<DeathAuthoring>
        {
            public override void Bake(DeathAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DeathVoiceSfx
                {
                    DeathSfxPrefab = GetEntity(authoring.deathSfxPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }


    public struct DeathVoiceSfx : IComponentData
    {
        public EntityWith<Prefab> DeathSfxPrefab;
    }
}