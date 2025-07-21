using UnityEngine;

namespace Survivors.Play.Scope.Perks
{
    public abstract class Perk : ScriptableObject
    {
        public string description;
        public Sprite icon;
    }
}