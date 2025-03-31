using Survivors.Play.Authoring;
using UnityEngine;

namespace Survivors.ScriptableObjects
{
    [CreateAssetMenu(fileName = "New Player Data", menuName = "Survivors/Player Data")]
    public class PlayerData : ScriptableObject
    {
        public MovementSettings movementSettings;
    }
}