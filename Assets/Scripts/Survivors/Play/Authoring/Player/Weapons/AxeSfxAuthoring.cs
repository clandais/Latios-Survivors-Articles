using Survivors.Play.Authoring.SFX;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.Weapons
{
    [AddComponentMenu("Survivors/Weapons/Axe SFX Authoring")]
    public class AxeSfxAuthoring : MonoBehaviour
    {
        [SerializeField] SfxListRefAuthoring sfxListRef;
        [SerializeField] SfxListRefAuthoring hitSfxListRef;

        class AxeSfxAuthoringBaker : Baker<AxeSfxAuthoring>
        {
            public override void Bake(AxeSfxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                if (authoring.sfxListRef == null) return;

                if (authoring.hitSfxListRef == null) return;

                AddComponent(entity, new AxeSfxRefs
                {
                    SwooshSpawnerEntity = GetEntity(authoring.sfxListRef, TransformUsageFlags.Dynamic),
                    HitSpawnerEntity    = GetEntity(authoring.hitSfxListRef, TransformUsageFlags.Dynamic)
                });
            }
        }
    }

    public struct AxeSfxRefs : IComponentData
    {
        public Entity SwooshSpawnerEntity;
        public Entity HitSpawnerEntity;
    }
}