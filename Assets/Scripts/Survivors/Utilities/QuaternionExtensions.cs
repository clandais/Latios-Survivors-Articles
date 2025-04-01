using System.Runtime.CompilerServices;
using Latios.Psyshock;
using Latios.Transforms;
using Unity.Mathematics;

namespace Survivors.Utilities
{
    public static class QuaternionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateTowards(this quaternion from,
            quaternion to,
            float maxDegreesDelta)
        {
            return math.slerp(from, to, math.radians(maxDegreesDelta));
        }
    }

    public static class Quat
    {
        public static quaternion RotateAroundAxis(float3 axis,
            float angle)
        {
            float sina, cosa;
            
            axis = math.normalize(axis);
            
            math.sincos(0.5f * angle, out sina, out cosa);

            return math.quaternion(
                axis.x * sina,
                axis.y * sina,
                axis.z * sina,
                cosa);
        }
    }
    
}