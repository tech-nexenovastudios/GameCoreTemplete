#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace Systems.SceneManagement.Editor
{
    [CustomPropertyDrawer(typeof(SceneType))]
    public class SceneTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null) return;

            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string[] names;
            int[] values;
            try
            {
                names = Enum.GetNames(typeof(SceneType))
                    .Where(name => name != nameof(SceneType.ActiveScene))
                    .ToArray();

                values = names
                    .Select(name => (int)(SceneType)Enum.Parse(typeof(SceneType), name))
                    .ToArray();
            }
            catch (Exception)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            int currentIndex = Array.IndexOf(values, property.intValue);
            if (currentIndex == -1) currentIndex = 0;

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, names);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = values[newIndex];
            }
            EditorGUI.EndProperty();
        }
    }
}
#endif