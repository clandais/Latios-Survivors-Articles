using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/*
 * https://www.reddit.com/r/Unity3D/comments/7gn7gd/handy_script_for_auto_loading_specific_scene_on/
 * Handy utility with DI
 */

namespace Survivors.Utilities
{
    internal static class SceneAutoLoader
    {
        [MenuItem("Scene Autoload/Select Master Scene...")]
        static void SelectStartupScene()
        {
            var masterScene = EditorUtility.OpenFilePanel("Select Master Scene", Application.dataPath, "unity");
            masterScene = masterScene.Replace(Application.dataPath, "Assets");
            if (!string.IsNullOrEmpty(masterScene)) EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(masterScene);
        }

        [MenuItem("Scene Autoload/Disable Scene Autoload")]
        static void DisableSceneAutoload()
        {
            EditorSceneManager.playModeStartScene = null;
        }
    }
}