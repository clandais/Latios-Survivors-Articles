using Survivors.Play.Components;
using UnityEngine;

namespace Survivors.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MovementSettingsData", menuName = "ScriptableObjects/MovementSettingsData")]
    public class MovementSettingsData : ScriptableObject
    {
        public MovementSettings movementSettings;
    }
}