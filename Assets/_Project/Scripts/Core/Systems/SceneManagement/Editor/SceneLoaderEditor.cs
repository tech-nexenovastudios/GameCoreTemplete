#if UNITY_EDITOR
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Systems.SceneManagement.Editor
{
    [CustomEditor(typeof(SceneLoader))]
    [CanEditMultipleObjects]
    public sealed class SceneLoaderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (target == null) return;

            serializedObject.Update();
            
            try
            {
                DrawDefaultInspector();

                if (serializedObject.isEditingMultipleObjects)
                {
                    serializedObject.ApplyModifiedProperties();
                    return;
                }

                var sceneLoader = (SceneLoader)target;
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Scene Group Controls", EditorStyles.boldLabel);

                using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                {
                    if (GUILayout.Button("Load Core Scene Group"))
                        LoadSceneGroup(sceneLoader, "Core");

                    if (GUILayout.Button("Load Gameplay Scene Group"))
                        LoadSceneGroup(sceneLoader, "Gameplay");

                    if (GUILayout.Button("Load Overlays Scene Group"))
                        LoadSceneGroup(sceneLoader, "Overlays");
                }
            }
            finally
            {
                if (serializedObject != null && serializedObject.targetObject != null)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void LoadSceneGroup(SceneLoader sceneLoader, string groupName)
        {
            if (sceneLoader == null) return;
            // Use fire-and-forget but catch exceptions
            _ = sceneLoader.LoadSceneGroup(groupName);
        }
    }
}
#endif
