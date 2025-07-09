using Latios;
using Latios.Psyshock;
using Survivors.Play.Authoring.Environment;

namespace Survivors.Utilities
{
    public static class CoreExtensions
    {
        public static bool GetPhysicsSettings(this LatiosWorldUnmanaged latiosWorld,
            out PhysicsSettings physicsSettings)
        {
            if (latiosWorld.sceneBlackboardEntity.HasComponent<PhysicsSettings>())
            {
                physicsSettings = latiosWorld.sceneBlackboardEntity.GetComponentData<PhysicsSettings>();
                return true;
            }

            if (latiosWorld.worldBlackboardEntity.HasComponent<PhysicsSettings>())
            {
                physicsSettings = latiosWorld.worldBlackboardEntity.GetComponentData<PhysicsSettings>();
                return true;
            }

            physicsSettings = new PhysicsSettings
            {
                CollisionLayerSettings = CollisionLayerSettings.kDefault
            };

            return false;
        }
    }
}