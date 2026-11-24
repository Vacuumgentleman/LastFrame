using UnityEditor;
using UnityEngine;

namespace Senses2026
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true); // Disable field
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup(); // Re-enable GUI
        }
    }
#endif
}
