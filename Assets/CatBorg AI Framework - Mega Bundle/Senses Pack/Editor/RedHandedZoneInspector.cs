#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Senses2026
{
    [CustomEditor(typeof(RedHandedZone))]
    public class RedHandedZoneInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/03a%20RedHanded%20Zones.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        RedHandedZone m_zone;
        bool m_markDirty;
        ZoneMode m_prevMode;
        List<int> m_factionIndicesCopy;
        GUIContent m_labelMode;
        GUIContent m_labelRequired;
        GUIContent m_labelResult;
        GUIContent m_infoDisabled;
        GUIContent m_warningNoCollider;
        GUIContent m_warningNotTrigger;
        void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            m_zone = (RedHandedZone)target;
            m_prevMode = m_zone.ZoneMode;
            m_factionIndicesCopy = new List<int>(m_zone.SelectedFactionNameIndices);
            m_labelMode = new GUIContent("Zone Mode", "Defines how this zone affects RedHanded entities.");
            m_labelRequired = new GUIContent("Required Factions", "Only these factions will trigger the zone’s effect.");
            m_labelResult = new GUIContent("Result Faction", "Faction assigned when this zone triggers.");
            m_infoDisabled = new GUIContent("Component is DISABLED:\n" + "• Zone will NOT affect any RedHanded entities\n" + "• FactionNameWasChanged() always returns FALSE");
            m_warningNoCollider = new GUIContent("RedHanded Zone requires a Collider.\n" + "Add a Collider set to 'Is Trigger = true'.");
            m_warningNotTrigger = new GUIContent("Collider should be set as a Trigger.\n" + "Otherwise RedHanded checks will not function.");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            InspectorPerformanceUtility.SynchronizeIndices(m_factionIndicesCopy, m_zone.SelectedFactionNameIndices);
            m_markDirty = false;
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            DrawDisabledInfo();
            DrawModeSelector();
            DrawRequiredFactionMask();
            DrawResultFactionPopup();
            DrawColliderWarnings();
            DrawHelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_zone.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            serializedObject.ApplyModifiedProperties();
            if (m_markDirty)
            {
                EditorUtility.SetDirty(m_zone);
            }
        }

        void DrawDisabledInfo()
        {
            if (!m_zone.enabled)
            {
                EditorGUILayout.HelpBox(m_infoDisabled.text, MessageType.Info);
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            }
        }

        void DrawColliderWarnings()
        {
            Collider _col = m_zone.GetComponent<Collider>();
            if (_col == null)
            {
                EditorGUILayout.HelpBox(m_warningNoCollider.text, MessageType.Warning);
                return;
            }

            if (!_col.isTrigger)
            {
                EditorGUILayout.HelpBox(m_warningNotTrigger.text, MessageType.Warning);
            }
        }

        void DrawModeSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_labelMode, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            ZoneMode _newMode = (ZoneMode)EditorGUILayout.EnumPopup(m_zone.ZoneMode, GUILayout.ExpandWidth(true));
            if (_newMode != m_zone.ZoneMode)
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_zone);
                    m_zone.Set_ZoneMode(_newMode);
                    InspectorPerformanceUtility.CompleteChange(m_zone);
                }

                m_prevMode = _newMode;
                m_markDirty = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawRequiredFactionMask()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_labelRequired, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            string[] _names = GetFactionNames();
            int _mask = 0;
            foreach (int _idx in m_factionIndicesCopy)
            {
                _mask |= 1 << _idx;
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

                InspectorPerformanceUtility.RecordChange(m_zone);
                m_zone.Set_SelectedFactionNames(new List<int>(m_factionIndicesCopy));
                InspectorPerformanceUtility.CompleteChange(m_zone);
                m_markDirty = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawResultFactionPopup()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_labelResult, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            string[] _names = GetFactionNames();
            int _current = (int)m_zone.ResultFactionName;
            int _newIndex = EditorGUILayout.Popup(_current, _names, GUILayout.ExpandWidth(true));
            if (_newIndex != _current)
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_zone);
                    m_zone.Set_ResultFactionName((FactionNames)_newIndex);
                    InspectorPerformanceUtility.CompleteChange(m_zone);
                }

                m_markDirty = true;
            }

            EditorGUILayout.EndHorizontal();
            if (m_zone.ZoneMode != ZoneMode.None && m_zone.SelectedFactionNameIndices.Contains((int)m_zone.ResultFactionName))
            {
                EditorGUILayout.HelpBox("One or more Required Factions match Result Faction.\nThis may prevent the zone from functioning correctly.", MessageType.Warning);
            }
        }

        void DrawHelpFoldout()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            GUIContent _content = LoadIconForEditorHelpWindow.LoadContent("IconHelp", "Help");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_content, GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                InspectorPerformanceUtility.RecordChange(m_zone);
                m_zone.Toggle_HelpPropertiesFoldout();
                InspectorPerformanceUtility.CompleteChange(m_zone);
            }

            EditorGUILayout.EndHorizontal();
        }

        string[] GetFactionNames()
        {
            return EditorFactionHelpers.GetFactionNames();
        }
    }
}
#endif

