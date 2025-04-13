using Latios;
using Latios.Transforms;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;
using Collider = Latios.Psyshock.Collider;

namespace Survivors.Play.Authoring.Debug
{

        public class CaptureAuthoring : MonoBehaviour
        {
            public GameObject    colliderA;
            public GameObject    colliderB;
            public string        hexString;
            public bool          useHexString;
            public float         maxDistance   = 10.0f;
            public DrawOperation drawOperation = DrawOperation.UnityContactsBetweenClosest;
            public bool          rebakeToggle;


            private class CaptureBaker : Baker<CaptureAuthoring>
            {
                public override void Bake(CaptureAuthoring authoring)
                {
                    var    entity    = GetEntity(TransformUsageFlags.None);
                    Entity colliderA = default;
                    Entity colliderB = default;
                    if (!authoring.useHexString)
                    {
                        colliderA = GetEntity(authoring.colliderA, TransformUsageFlags.Renderable);
                        colliderB = GetEntity(authoring.colliderB, TransformUsageFlags.Renderable);
                    }
                    else
                    {
                        colliderA = CreateAdditionalEntity(TransformUsageFlags.ManualOverride);
                        colliderB = CreateAdditionalEntity(TransformUsageFlags.ManualOverride);
                        var reader = new PsyshockQueryDebugger.HexReader(authoring.hexString);
                        if (reader.ReadByte() != 1)
                        {
                            UnityEngine.Debug.Log("Reader failed to parse first byte.");
                            return;
                        }
                        var cA                                                      = reader.ReadCollider();
                        var transformA                                              = reader.ReadTransform();
                        var cB                                                      = reader.ReadCollider();
                        var transformB                                              = reader.ReadTransform();
                        AddComponent(colliderA, new WorldTransform { worldTransform = transformA });
                        AddComponent(colliderB, new WorldTransform { worldTransform = transformB });
                        AddComponent(colliderA, cA);
                        AddComponent(colliderB, cB);
                    }

                    AddComponent(entity, new CapturePairTarget
                    {
                        colliderA     = colliderA,
                        colliderB     = colliderB,
                        maxDistance   = authoring.maxDistance,
                        drawOperation = authoring.drawOperation
                    });
                }
            }
        }
        
    
    
    public enum DrawOperation
    {
        None,
        DistanceBetweenClosest,
        DistanceBetweenAll,
        UnityContactsBetweenClosest,
        UnityContactsBetweenAll,
    }
    
    struct CapturePairTarget : IComponentData
    {
        public EntityWith<Collider> colliderA;
        public EntityWith<Collider>                 colliderB;
        public float                                maxDistance;
        public DrawOperation                        drawOperation;
    }
}