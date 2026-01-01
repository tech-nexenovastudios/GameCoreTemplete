using System;
using System.Collections.Generic;
using System.Linq;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.SceneManagement 
{
    [Serializable]
    [CreateAssetMenu(fileName = "SceneGroup_", menuName = "Scriptable Objects/Scene Management/Scene Group")]
    public class SceneGroup : ScriptableObject
    {
        public string groupName = "New Scene Group";
        public List<SceneData> scenes = new();

        public string FindSceneNameByType(SceneType sceneType)
        {
            return scenes
                .FirstOrDefault(scene => scene.sceneType == sceneType)
                ?.Name;
        }

        public string GetRuntimeActiveSceneName()
        {
            // Priority: World -> GameplayBase -> Boot
            var scene = scenes.FirstOrDefault(s => s.sceneType == SceneType.World) ??
                        scenes.FirstOrDefault(s => s.sceneType == SceneType.GameplayBase) ??
                        scenes.FirstOrDefault(s => s.sceneType == SceneType.Boot);

            return scene?.Name;
        }

        public Scene GetActiveWorldScene()
        {
            var worldSceneData = scenes
                .Where(scene => scene.sceneType == SceneType.World)
                .ToList();

            if (worldSceneData.Count == 0) return default;

            var loadedWorldScenes = worldSceneData.Select(sceneData => SceneManager.GetSceneByName(sceneData.Name))
                .Where(scene => scene.IsValid() && scene.isLoaded);
            foreach (var scene in loadedWorldScenes) return scene;

            return default;
        }

        public void OnValidate() {
            if (scenes == null) return;
            foreach (var scene in scenes) {
                if (scene != null) scene.OnValidate();
            }

            ValidateSceneCounts();
        }

        private void ValidateSceneCounts() 
        {
            if (scenes == null) return;
            int bootCount = scenes.Count(s => s != null && s.sceneType == SceneType.Boot);
            int persistentSystemsCount = scenes.Count(s => s != null && s.sceneType == SceneType.PersistentSystems);
            int uiShellCount = scenes.Count(s => s != null && s.sceneType == SceneType.AppShell);
            int gameplayBaseCount = scenes.Count(s => s != null && s.sceneType == SceneType.GameplayBase);
            int worldCount = scenes.Count(s => s != null && s.sceneType == SceneType.World);

            if (bootCount > 0 || persistentSystemsCount > 0 || uiShellCount > 0) {
                if (bootCount != 1) UnityEngine.Debug.LogWarning($"SceneGroup {groupName} should have exactly 1 Boot scene if it's a Core group.");
                if (persistentSystemsCount != 1) UnityEngine.Debug.LogWarning($"SceneGroup {groupName} should have exactly 1 PersistentSystems scene if it's a Core group.");
                if (uiShellCount != 1) UnityEngine.Debug.LogWarning($"SceneGroup {groupName} should have exactly 1 AppShell scene if it's a Core group.");
            }

            if (gameplayBaseCount > 0 || worldCount > 0) {
                if (gameplayBaseCount != 1) UnityEngine.Debug.LogWarning($"SceneGroup {groupName} should have exactly 1 GameplayBase scene if it's a Gameplay group.");
                if (worldCount != 1) UnityEngine.Debug.LogWarning($"SceneGroup {groupName} should have exactly 1 World scene if it's a Gameplay group.");
            }
        }
    }
    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        public SceneType sceneType;

        public bool loadAdditive = true;
        public bool persistent = false;

        public string Name => reference.Name;

        public void OnValidate() {
            if (reference == null) return;
            if (sceneType == SceneType.ActiveScene) {
                UnityEngine.Debug.LogError($"SceneType.ActiveScene is forbidden in the editor. Resetting to World.");
                sceneType = SceneType.World;
            }

            if (sceneType == SceneType.Overlay || sceneType == SceneType.Cinematic || 
                sceneType == SceneType.World || sceneType == SceneType.GameplayBase || 
                sceneType == SceneType.Boot) {
                if (persistent) {
                    UnityEngine.Debug.LogWarning($"Scene {Name} of type {sceneType} should not be persistent. Resetting.");
                    persistent = false;
                }
            }

            if (sceneType == SceneType.AppShell || sceneType == SceneType.PersistentSystems) {
                if (!persistent) {
                    UnityEngine.Debug.LogWarning($"Scene {Name} of type {sceneType} should typically be persistent. Setting to true.");
                    persistent = true;
                }
            }
        }
    }
    
    public enum SceneType
    {
        // Runtime-resolved (NOT stored in SceneData)
        ActiveScene,

        Boot,
        PersistentSystems,
        AppShell,

        GameplayBase,
        World,

        Overlay,
        Cinematic
    }
}