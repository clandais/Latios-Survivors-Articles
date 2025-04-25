using Survivors.Play.Authoring.SFX;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.SFX
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Survivors/Play/Player/SFX/Player SFX")]
    public class PlayerSfxAuthoring : MonoBehaviour
    {
        [SerializeField] SfxListRefAuthoring footStepListRef;

        class PlayerSfxAuthoringBaker : Baker<PlayerSfxAuthoring>
        {
            public override void Bake(PlayerSfxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                if (authoring.footStepListRef == null) return;

                AddComponent(entity, new PlayerSfxRefs
                {
                    FootStepSpawnerEntity = GetEntity(authoring.footStepListRef, TransformUsageFlags.Dynamic)
                });
            }
        }
    }


    public struct PlayerSfxRefs : IComponentData
    {
        public Entity FootStepSpawnerEntity;
    }
}