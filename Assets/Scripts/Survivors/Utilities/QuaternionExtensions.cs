using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Survivors.Utilities
{
    public static class QuaternionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateTowards(this quaternion from,
            quaternion to,
            float maxDegreesDelta) => math.slerp(from, to, math.radians(maxDegreesDelta));
    }

    public static class Quat
    {
        public static quaternion RotateAroundAxis(float3 axis,
            float angle)
        {
            axis = math.normalize(axis);
            math.sincos(0.5f * angle, out var sina, out var cosa);
            return math.quaternion(
                axis.x * sina,
                axis.y * sina,
                axis.z * sina,
                cosa);
        }
    }
}