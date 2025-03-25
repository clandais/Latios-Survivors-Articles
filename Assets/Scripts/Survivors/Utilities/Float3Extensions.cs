using Unity.Mathematics;

namespace Survivors.Utilities
{
    public static class Float3Extensions
    {
        public static float3 MoveTowards(this float3 current,
            float3 target,
            float maxDistanceDelta)
        {
            var a = target - current;
            var magnitude = math.length(a);

            if (magnitude <= maxDistanceDelta || magnitude < math.EPSILON) return target;

            return current + a / magnitude * maxDistanceDelta;
        }
    }
}