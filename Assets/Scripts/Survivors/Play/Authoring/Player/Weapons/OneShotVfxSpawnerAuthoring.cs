using Survivors.VfxTunnels;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.Weapons
{
    [AddComponentMenu("Survivors/Play/Authoring/VFX/OneShotVfxSpawner")]
    public class OneShotVfxSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] PositionGraphicsEventTunnel positionGraphicsEventTunnel;

        class OneShotVfxSpawnerAuthoringBaker : Baker<OneShotVfxSpawnerAuthoring>
        {
            public override void Bake(OneShotVfxSpawnerAuthoring spawnerAuthoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent(entity, new OneShotPositionEventSpawner
                {
                    PositionGraphicsEventTunnel = new UnityObjectRef<PositionGraphicsEventTunnel>
                    {
                        Value = spawnerAuthoring.positionGraphicsEventTunnel
                    }
                });
            }
        }
    }


    public struct OneShotPositionEventSpawner : IComponentData
    {
        public UnityObjectRef<PositionGraphicsEventTunnel> PositionGraphicsEventTunnel;
    }
}