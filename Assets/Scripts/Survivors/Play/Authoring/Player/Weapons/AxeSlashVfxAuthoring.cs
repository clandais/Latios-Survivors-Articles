using Survivors.VfxTunnels;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.Player.Weapons
{
    [AddComponentMenu("Survivors/Play/Authoring/Player/Weapons/AxeSlashVfx")]
    public class AxeSlashVfxAuthoring : MonoBehaviour
    {
        [SerializeField] PositionGraphicsEventTunnel positionGraphicsEventTunnel;

        class AxeSlashVfxAuthoringBaker : Baker<AxeSlashVfxAuthoring>
        {
            public override void Bake(AxeSlashVfxAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent(entity, new OneShotPositionEventSpawner
                {
                    PositionGraphicsEventTunnel = new UnityObjectRef<PositionGraphicsEventTunnel>
                    {
                        Value = authoring.positionGraphicsEventTunnel
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