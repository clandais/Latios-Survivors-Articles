using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.SFX
{
    public class PlayerSfxAuhtoring : MonoBehaviour
    {
        [SerializeField] SfxListRef footStepListRef;

        class PlayerFootstepsAuthoringBaker : Baker<PlayerSfxAuhtoring>
        {
            public override void Bake(PlayerSfxAuhtoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                if (authoring.footStepListRef.oneShotSfxPrefabs == null) return;

                AddComponent(entity, new OneShotSfxSpawnerRef
                {
                    EventType = authoring.footStepListRef.eventType,
                    SfxPrefab = GetEntity(authoring.footStepListRef.oneShotSfxPrefabs, TransformUsageFlags.None)
                });
            }
        }
    }
}