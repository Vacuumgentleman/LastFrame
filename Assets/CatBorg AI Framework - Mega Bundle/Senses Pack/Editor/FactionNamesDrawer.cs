#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Senses2026
{
    [CustomPropertyDrawer(typeof(FactionNames))]
    public class FactionNamesDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EditorGUI.BeginProperty(_position, _label, _property);
            EditorGUI.BeginChangeCheck();
            int _newIndex = EditorGUI.Popup(_position, _label.text, _property.enumValueIndex, EditorFactionHelpers.GetFactionNames());
            if (EditorGUI.EndChangeCheck())
            {
                _property.enumValueIndex = _newIndex;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif

