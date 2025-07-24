using Survivors.Play.Components;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Authoring.SceneBlackBoard
{
    public class GameStatsAuthoring : MonoBehaviour
    {
        class GameStatsAuthoringBaker : Baker<GameStatsAuthoring>
        {
            public override void Bake(GameStatsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GameStatsComponent
                {
                    EnemiesKilled = 0
                });
            }
        }
    }
}