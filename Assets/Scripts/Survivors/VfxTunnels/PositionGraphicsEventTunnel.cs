using Latios.LifeFX;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.VfxTunnels
{
    [CreateAssetMenu(fileName = "New PositionGraphicsEventTunnel",
        menuName = "Survivors/VfxTunnels/PositionGraphicsEventTunnel")]
    public class PositionGraphicsEventTunnel : GraphicsEventTunnel<float3> { }
}