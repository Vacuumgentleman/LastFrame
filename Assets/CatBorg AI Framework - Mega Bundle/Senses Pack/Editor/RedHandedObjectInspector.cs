#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Senses2026
{
    [CustomEditor(typeof(RedHandedObject))]
    public class RedHandedObjectInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/03b%20RedHanded%20Objects.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        RedHandedObject m_object;
        bool m_markDirty;
        ObjectMode m_prevMode;
        List<int> m_factionIndicesCopy;
        GUIContent[] m_gui = new GUIContent[4];
        void OnEnable()
        {
            m_object = (RedHandedObject)target;
            m_prevMode = m_object.ObjectsMode;
            m_factionIndicesCopy = new List<int>(m_object.SelectedFactionNameIndices);
            m_gui[0] = new GUIContent("Object Mode");
            m_gui[1] = new GUIContent("Required Factions");
            m_gui[2] = new GUIContent("Result Faction");
            m_gui[3] = new GUIContent("Component is DISABLED:\n• Object Mode is inactive\n• Repercussions will not be applied\n");
        }

        public override void OnInspectorGUI()
        {
            m_markDirty = false;
            serializedObject.Update();
            InspectorPerformanceUtility.SynchronizeIndices(m_factionIndicesCopy, m_object.SelectedFactionNameIndices);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            DrawDisabledInfo();
            DrawMode();
            DrawFactionMask();
            DrawResultFaction();
            DrawColliderWarnings();
            DrawHelpFoldout();
            serializedObject.ApplyModifiedProperties();
            if (m_markDirty)
            {
                EditorUtility.SetDirty(m_object);
            }
        }

        void DrawMode()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[0], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            ObjectMode _new = (ObjectMode)EditorGUILayout.EnumPopup(m_object.ObjectsMode, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            if (_new != m_object.ObjectsMode)
            {
                m_prevMode = _new;
                {
                    InspectorPerformanceUtility.RecordChange(m_object);
                    m_object.Set_ZoneMode(_new);
                    InspectorPerformanceUtility.CompleteChange(m_object);
                }

                m_markDirty = true;
            }
        }

        void DrawFactionMask()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[1], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            string[] _names = GetFactionNames();
            int _mask = 0;
            for (int _i = 0; _i < m_factionIndicesCopy.Count; _i++)
            {
                _mask |= (1 << m_factionIndicesCopy[_i]);
            }

            int _newMask = EditorGUILayout.MaskField(_mask, _names, GUILayout.ExpandWidth(true));
            if (_newMask != _mask)
            {
                m_factionIndicesCopy.Clear();
                for (int _i = 0; _i < _names.Length; _i++)
                {
                    if ((_newMask & (1 << _i)) != 0)
                    {
                        m_factionIndicesCopy.Add(_i);
                    }
                }

                InspectorPerformanceUtility.RecordChange(m_object);
                m_object.Set_SelectedFactionNames(new List<int>(m_factionIndicesCopy));
                InspectorPerformanceUtility.CompleteChange(m_object);
                m_markDirty = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawResultFaction()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[2], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            string[] _names = GetFactionNames();
            int _current = (int)m_object.ResultFactionName;
            int _new = EditorGUILayout.Popup(_current, _names, GUILayout.ExpandWidth(true));
            if (_current != _new)
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_object);
                    m_object.Set_ResultFactionName((FactionNames)_new);
                    InspectorPerformanceUtility.CompleteChange(m_object);
                }

                m_markDirty = true;
            }

            EditorGUILayout.EndHorizontal();
            if (m_object.ObjectsMode != ObjectMode.None && m_object.SelectedFactionNameIndices.Contains((int)m_object.ResultFactionName))
            {
                EditorGUILayout.HelpBox("One or more Required Factions match Result Faction. This may make Object Mode ineffective.", MessageType.Warning);
            }
        }

        void DrawDisabledInfo()
        {
            if (!m_object.enabled)
            {
                EditorGUILayout.HelpBox(m_gui[3].text, MessageType.Info);
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            }
        }

        void DrawColliderWarnings()
        {
            Collider _col = m_object.GetComponent<Collider>();
            if (!_col)
            {
                EditorGUILayout.HelpBox("RedHandedObject requires a Collider.", MessageType.Warning);
                return;
            }

            if (_col.isTrigger)
            {
                EditorGUILayout.HelpBox("Collider should NOT be a Trigger.\nObjects must collide with the RedHandedObject.", MessageType.Warning);
            }
        }

        void DrawHelpFoldout()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            GUILayout.Label(LoadIconForEditorHelpWindow.LoadContent("IconHelp"), GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                InspectorPerformanceUtility.RecordChange(m_object);
                m_object.Toggle_HelpPropertiesFoldout();
                InspectorPerformanceUtility.CompleteChange(m_object);
            }

            EditorGUILayout.EndHorizontal();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_object.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
        }

        string[] GetFactionNames()
        {
            return EditorFactionHelpers.GetFactionNames();
        }
    }
}
#endif

