#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Senses2026
{
    [CustomEditor(typeof(SoundPropagationSettings))]
    public class SoundPropagationSettingsEditor : Editor
    {
        private SerializedProperty m_soundData;
        private readonly HashSet<string> m_tags = new HashSet<string>();
        private bool m_tagsDirty = true;
        private void OnEnable()
        {
            m_soundData = serializedObject.FindProperty("m_soundData");
            EditorApplication.projectChanged += InvalidateTags;
            Undo.undoRedoPerformed += InvalidateTags;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= InvalidateTags;
            Undo.undoRedoPerformed -= InvalidateTags;
        }

        private void InvalidateTags()
        {
            m_tagsDirty = true;
        }

        private void EnsureTags()
        {
            if (!m_tagsDirty)
            {
                return;
            }

            m_tags.Clear();
            foreach (string _tag in InternalEditorUtility.tags)
            {
                m_tags.Add(_tag);
            }

            m_tagsDirty = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureTags();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                int _index = m_soundData.arraySize;
                m_soundData.arraySize++;
                SerializedProperty _newElement = m_soundData.GetArrayElementAtIndex(_index);
                _newElement.FindPropertyRelative("m_TAG").stringValue = "NewTag";
                _newElement.FindPropertyRelative("m_factor").floatValue = 1f;
            }

            EditorGUILayout.EndHorizontal();
            int _removeIndex = -1;
            GUIContent _validIcon = LoadIconForEditorHelpWindow.LoadContent("Success", "TAG exists!");
            GUIContent _invalidIcon = LoadIconForEditorHelpWindow.LoadContent("Failure", "TAG does not exist!");
            for (int _index = 0; _index < m_soundData.arraySize; _index++)
            {
                SerializedProperty _element = m_soundData.GetArrayElementAtIndex(_index);
                SerializedProperty _tag = _element.FindPropertyRelative("m_TAG");
                SerializedProperty _factor = _element.FindPropertyRelative("m_factor");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(m_tags.Contains(_tag.stringValue) ? _validIcon : _invalidIcon, GUILayout.Width(20), GUILayout.Height(20));
                EditorGUILayout.LabelField("Tag:", GUILayout.Width(40));
                EditorGUILayout.PropertyField(_tag, GUIContent.none, GUILayout.Width(70));
                EditorGUILayout.LabelField("Factor:", GUILayout.Width(50));
                _factor.floatValue = EditorGUILayout.Slider(_factor.floatValue, 0f, 2f);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _removeIndex = _index;
                }

                EditorGUILayout.Space(20);
                EditorGUILayout.EndHorizontal();
            }

            if (_removeIndex >= 0)
            {
                m_soundData.DeleteArrayElementAtIndex(_removeIndex);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

