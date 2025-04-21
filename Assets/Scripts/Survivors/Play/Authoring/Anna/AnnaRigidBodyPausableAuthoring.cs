using Latios.Anna.Authoring;
using Latios.Psyshock;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Authoring.Anna
{
    [AddComponentMenu("Survivors/Anna/RigidBodyPausable")]
    [RequireComponent(typeof(AnnaRigidBodyAuthoring))]
    public class AnnaRigidBodyPausableAuthoring : MonoBehaviour
    {
        class AnnaRigidBodyPausableBaker : Baker<AnnaRigidBodyPausableAuthoring>
        {
            public override void Bake(AnnaRigidBodyPausableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SavedRigidBodyState>(entity);
            }
        }
    }

    /// <summary>
    ///     Saved state of a rigid body.
    /// </summary>
    public struct SavedRigidBodyState : IComponentData
    {
        public UnitySim.Velocity Velocity;
        public float InverseMass;
        public half CoefficientOfFriction;
        public half CoefficientOfRestitution;
    }
}