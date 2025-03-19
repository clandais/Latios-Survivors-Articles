using System;
using Cysharp.Threading.Tasks;
using Survivors.Bootstrap;
using Survivors.GameScope.Commands;
using Survivors.ScriptableObjects;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VitalRouter;
using SceneReference = Eflatun.SceneReference.SceneReference;

namespace Survivors.GameScope
{
    [Routes]
    public partial class GlobalRouter
    {
        [Inject] GameScenesReferences m_gameScenesReferences;


        async UniTask DisposeScene(SceneReference sceneRef)
        {
            var activeScenes = SceneManager.sceneCount;
            for (var i = 0; i < activeScenes; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.buildIndex == sceneRef.BuildIndex) await SceneManager.UnloadSceneAsync(scene);
            }

            await UniTask.CompletedTask;
        }

        [Route]
        async UniTask On(MainMenuStateCommand _)
        {
            await DisposeScene(m_gameScenesReferences.playScene);

            World.DefaultGameObjectInjectionWorld?.Dispose();

            await SceneManager.LoadSceneAsync(m_gameScenesReferences.mainMenuScene.BuildIndex, LoadSceneMode.Additive);
        }

        [Route]
        async UniTask On(PlayStateCommand _)
        {
            await DisposeScene(m_gameScenesReferences.mainMenuScene);

            if (new LatiosBootstrap().Initialize("Latios Survivors World"))
            {
                Debug.Log("Latios initialized");
            }
            else
            {
                Debug.LogException(new Exception("Latios failed to initialize :'("));
                return;
            }

            await SceneManager.LoadSceneAsync(m_gameScenesReferences.playScene.BuildIndex, LoadSceneMode.Additive);
        }
    }
}