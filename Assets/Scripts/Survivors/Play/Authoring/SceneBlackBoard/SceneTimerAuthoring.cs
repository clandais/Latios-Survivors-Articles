using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.SceneBlackBoard
{
    public class SceneTimerAuthoring : MonoBehaviour
    {
        [SerializeField] int gameDurationInSeconds = 300;

        class SceneTimerAuthoringBaker : Baker<SceneTimerAuthoring>
        {
            public override void Bake(SceneTimerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new GameTimerComponent
                {
                    GameDurationInSeconds = authoring.gameDurationInSeconds,
                    TimeRemaining         = authoring.gameDurationInSeconds
                });
            }
        }
    }
}