using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Survivors.Utilities
{
    public static class Float3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 MoveTowards(this float3 current,
            float3 target,
            float maxDistanceDelta)
        {
            var a = target - current;
            var magnitude = math.length(a);

            if (magnitude <= maxDistanceDelta || magnitude < math.EPSILON) return target;

            return current + a / magnitude * maxDistanceDelta;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngle(this float3 from, float3 to, float3 axis)
        {
            float num1 = from.Angle(to);
            float num2 = (float) ((double) from.y * (double) to.z - (double) from.z * (double) to.y);
            float num3 = (float) ((double) from.z * (double) to.x - (double) from.x * (double) to.z);
            float num4 = (float) ((double) from.x * (double) to.y - (double) from.y * (double) to.x);
            float num5 = math.sign((float) ((double) axis.x * (double) num2 + (double) axis.y * (double) num3 + (double) axis.z * (double) num4));
            return num1 * num5;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this float3 from, float3 to)
        {
            float num = (float) math.sqrt((double)  math.lengthsq( from) * math.lengthsq(to));
            return num < 1.0000000036274937E-15 ? 0.0f : (float) math.acos((double) math.clamp(math.dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }
    }
}