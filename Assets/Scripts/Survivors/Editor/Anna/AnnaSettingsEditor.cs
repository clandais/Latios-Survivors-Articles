using Latios.Anna;
using UnityEditor;
using UnityEngine;

namespace Survivors.Editor.Anna
{
    [CustomEditor(typeof(AnnaSettings))]
    public class AnnaSettingsEditor : UnityEditor.Editor
    {
        void OnSceneGUI()
        {
            var t = target as AnnaSettings;

            if (!t)
                return;

            var bounds = t.worldBounds;

            Handles.color = Color.green;
            Handles.DrawWireCube(bounds.center, bounds.size);
        }
    }
}