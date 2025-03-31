using Eflatun.SceneReference;
using UnityEngine;

namespace Survivors.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameSceneReferences", menuName = "Survivors/GameSceneReferences")]
    public class GameScenesReferences : ScriptableObject
    {
        [SerializeField] public SceneReference mainMenuScene;
        [SerializeField] public SceneReference playScene;
    }
}


