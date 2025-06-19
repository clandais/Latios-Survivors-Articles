using Latios;
using Latios.Psyshock;
using Survivors.Play.Authoring.Environment;
using UnityEngine;

namespace Survivors.Utilities
{
    public static class CoreExtensions
    {
        public static PhysicsSettings GetPhysicsSettings(this LatiosWorldUnmanaged latiosWorld)
        {
            if (latiosWorld.sceneBlackboardEntity.HasComponent<PhysicsSettings>())
                return latiosWorld.sceneBlackboardEntity.GetComponentData<PhysicsSettings>();

            if (latiosWorld.worldBlackboardEntity.HasComponent<PhysicsSettings>())
                return latiosWorld.worldBlackboardEntity.GetComponentData<PhysicsSettings>();

            Debug.LogWarning("PhysicsSettings not found in either scene or world blackboard. Using default settings.");

            return new PhysicsSettings
            {
                CollisionLayerSettings = CollisionLayerSettings.kDefault
            };
        }
    }
}