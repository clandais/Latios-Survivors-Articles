using Survivors.Play.Components;
using Survivors.ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.SceneBlackBoard
{
    public class HeroSceneBlackboardAuthoring : MonoBehaviour
    {
        [SerializeField] int                  playerStartingHealth = 100;
        [SerializeField] float                damageDelay          = 0.5f;
        [SerializeField] MovementSettingsData movementSettingsData;

        class HeroSceneBlackboardAuthoringBaker : Baker<HeroSceneBlackboardAuthoring>
        {
            public override void Bake(HeroSceneBlackboardAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new PlayerHealth
                {
                    CurrentHealth  = authoring.playerStartingHealth,
                    MaxHealth      = authoring.playerStartingHealth,
                    DamageDelay    = authoring.damageDelay,
                    LastDamageTime = 0
                });

                AddComponent(entity, new PlayerExperience
                {
                    CurrentExperience = 0,
                    CurrentLevel      = 1
                });

                var movementSettings = authoring.movementSettingsData.movementSettings;
                AddComponent(entity, movementSettings);
            }
        }
    }
}