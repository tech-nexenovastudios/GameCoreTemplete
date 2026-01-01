using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eflatun.SceneReference;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Systems.SceneManagement {
    public class SceneGroupManager {
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnSceneUnloaded = delegate { };
        public event Action OnSceneGroupLoaded = delegate { };
        
        readonly AsyncOperationHandleGroup handleGroup = new AsyncOperationHandleGroup(10);
        readonly HashSet<string> persistentScenes = new();
        
        SceneGroup ActiveSceneGroup;
        
        public async Task LoadScenes(SceneGroup group, IProgress<float> progress, bool reloadDupScenes = false) { 
            
            if(UnityServices.State != ServicesInitializationState.Initialized) await UnityServices.InitializeAsync();
            
            ActiveSceneGroup = group;
            var loadedScenes = new List<string>();

            await UnloadScenes();

            // If the new group contains a Boot scene, we are likely returning to the main menu/start.
            // In this case, we MUST clear existing persistent scenes to ensure no "World" or other 
            // scenes survive the transition back to Boot.
            // Note: In the new architecture, Boot is one-time only, so this handles cold start.
            if (group.scenes.Any(s => s.sceneType == SceneType.Boot)) {
                persistentScenes.Clear();
                await UnloadScenes(); // Unload anything that was previously persistent
            }

            int sceneCount = SceneManager.sceneCount;
            
            for (var i = 0; i < sceneCount; i++) {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }

            var totalScenesToLoad = ActiveSceneGroup.scenes.Count;

            var operationGroup = new AsyncOperationGroup(totalScenesToLoad);

            for (var i = 0; i < totalScenesToLoad; i++) {
                var sceneData = group.scenes[i];
                if (reloadDupScenes == false && loadedScenes.Contains(sceneData.Name)) continue;
                
                if (sceneData.persistent) {
                    persistentScenes.Add(sceneData.Name);
                }

                if (sceneData.reference.State == SceneReferenceState.Regular)
                {
                    var operation = SceneManager.LoadSceneAsync(sceneData.reference.Path, LoadSceneMode.Additive);
                    operationGroup.Operations.Add(operation);
                }
                else if (sceneData.reference.State == SceneReferenceState.Addressable)
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.reference.Path, LoadSceneMode.Additive);
                    handleGroup.handles.Add(sceneHandle);
                }
                
                OnSceneLoaded.Invoke(sceneData.Name);
            }
            
            // Wait until all AsyncOperations in the group are done
            while (!operationGroup.IsDone || !handleGroup.IsDone) {
                progress?.Report((operationGroup.Progress + handleGroup.Progress) / 2);
                await Task.Delay(100);
            }

            Scene activeScene = SceneManager.GetSceneByName(ActiveSceneGroup.GetRuntimeActiveSceneName());

            if (activeScene.IsValid()) {
                SceneManager.SetActiveScene(activeScene);
            }

            OnSceneGroupLoaded.Invoke();
        }

        public async Task UnloadScene(string sceneName) {
            var operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation == null) return;
            
            while (!operation.isDone) {
                await Task.Delay(100);
            }
            OnSceneUnloaded.Invoke(sceneName);
        }

        public async Task UnloadScenes() {
            var scenes = new List<string>();
            var activeScene = SceneManager.GetActiveScene().name;
            
            int sceneCount = SceneManager.sceneCount;

            for (var i = sceneCount - 1; i > 0; i--) {
                var sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;
                
                var sceneName = sceneAt.name;
                if (sceneName == "Boot") continue;
                if (persistentScenes.Contains(sceneName)) continue;
                
                if (handleGroup.handles.Any(h => h.IsValid() && h.Result.Scene.name == sceneName)) continue;
                
                scenes.Add(sceneName);
            }
            
            // Create an AsyncOperationGroup
            var operationGroup = new AsyncOperationGroup(scenes.Count);
            
            foreach (var scene in scenes) { 
                var operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null) continue;
                
                operationGroup.Operations.Add(operation);

                OnSceneUnloaded.Invoke(scene);
            }
            
            foreach (var handle in handleGroup.handles) {
                if (handle.IsValid()) {
                    Addressables.UnloadSceneAsync(handle);
                }
            }
            handleGroup.handles.Clear();

            // Wait until all AsyncOperations in the group are done
            while (!operationGroup.IsDone) {
                await Task.Delay(100); // delay to avoid tight loop
            }
            
            // Optional: UnloadUnusedAssets - unloads all unused assets from memory
            await Resources.UnloadUnusedAssets();
        }
    }
    
    public readonly struct AsyncOperationGroup { 
        public readonly List<AsyncOperation> Operations;

        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);
        public bool IsDone => Operations.All(o => o.isDone);

        public AsyncOperationGroup(int initialCapacity) {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }

    public readonly struct AsyncOperationHandleGroup {
        public readonly List<AsyncOperationHandle<SceneInstance>> handles;
        
        public float Progress => handles.Count == 0 ? 0 : handles.Average(h => h.PercentComplete);
        public bool IsDone => handles.Count == 0 || handles.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity) {
            handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
}