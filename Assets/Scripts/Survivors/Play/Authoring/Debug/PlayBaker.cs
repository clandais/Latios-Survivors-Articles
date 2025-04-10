using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Survivors.Play.Authoring.Debug
{
    [Serializable]
    public struct SerializedColliderDistanceResult
    {
        public bool   hit;
        public float3 hitpointA;
        public float3 hitpointB;
        public float3 normalA;
        public float3 normalB;
        public float  distance;
        public int    subColliderIndexA;
        public int    subColliderIndexB;
    }

    [Serializable]
    public struct SerializedUnityContactsResult
    {
        public bool             hit;
        public float3           contactNormal;
        public List<Float3Pair> contactPointPairs;
    }

    [Serializable]
    public struct Float3Pair
    {
        public float3 a;
        public float3 b;
    }
}