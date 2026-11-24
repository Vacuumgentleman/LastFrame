using UnityEditor;
using UnityEngine;

#if (UNITY_EDITOR)
namespace Senses2026
{
    [CustomEditor(typeof(TargetSenses))]
    public class TargetSensesInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("View TargetSenses GitHub Documentation", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/00a%20TargetSenses%20Component.pdf"),
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        TargetSenses m_targetSenses;
        bool m_needTo_SeetEditorDirty;
        TargetType m_previousTargetType;
        FactionNames m_previousFactionName;
        GUIContent[] m_GUIConetent = new GUIContent[2];
        protected virtual void OnEnable()
        {
            m_targetSenses = (TargetSenses)target;
            m_previousFactionName = m_targetSenses.FactionName;
            m_previousTargetType = m_targetSenses.TargetType;
            m_GUIConetent[0] = new GUIContent("Target Type", "Chose 'Collider' (performance weiss solution) or 'RigCollider'.");
            m_GUIConetent[1] = new GUIContent("Faction", "If not needed, leave as 'None'. If need to add more Factions or change names edit FactionNames.cs file."); //TODO need to be updated
        }

        public override void OnInspectorGUI()
        {
            m_needTo_SeetEditorDirty = false;
            serializedObject.Update();
            DrawComponents();
            drawHelp();
            serializedObject.ApplyModifiedProperties();
            if (m_needTo_SeetEditorDirty)
            {
                EditorUtility.SetDirty(m_targetSenses);
            }
        }

        protected virtual void DrawComponents()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[0], GUILayout.Width(EditorGUIUtility.labelWidth));
            int _targetTypeValueIndex = (int)(TargetType)EditorGUILayout.EnumPopup((TargetType)m_targetSenses.TargetType, GUILayout.ExpandWidth(true));
            if ((TargetType)_targetTypeValueIndex != m_targetSenses.TargetType)
            {
                InspectorPerformanceUtility.RecordChange(m_targetSenses);
                m_targetSenses.Set_TargetType((TargetType)_targetTypeValueIndex);
                InspectorPerformanceUtility.CompleteChange(m_targetSenses);
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousTargetType != m_targetSenses.TargetType)
            {
                m_previousTargetType = m_targetSenses.TargetType;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.TargetType == TargetType.Collider)
            {
                //check did we had Collider
                if (!m_targetSenses.TryGetComponent<Collider>(out Collider _collider))
                {
                    EditorGUILayout.HelpBox("Chosen Target Type requires Collider component", MessageType.Warning);
                }
            }

            if (m_targetSenses.TargetType == TargetType.RigCollider)
            {
                //check did we had Collider
                if (!m_targetSenses.TryGetComponent<RigCollider>(out RigCollider _rigcollider))
                {
                    EditorGUILayout.HelpBox("Chosen Target Type requires RigCollider component", MessageType.Warning);
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[1], GUILayout.Width(EditorGUIUtility.labelWidth));
            // Load faction names from the manager (or fallback)
            string[] _factionNames = EditorFactionHelpers.GetFactionNames();
            int _currentIndex = (int)m_targetSenses.FactionName;
            int _newIndex = EditorGUILayout.Popup(_currentIndex, _factionNames, GUILayout.ExpandWidth(true));
            if (_newIndex != _currentIndex)
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_FactionName((FactionNames)_newIndex);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }

                m_previousFactionName = (FactionNames)_newIndex;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void drawHelp()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(LoadIconForEditorHelpWindow.LoadContent("IconHelp"), GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                InspectorPerformanceUtility.RecordChange(m_targetSenses);
                m_targetSenses.ToggleHelpPropertiesFoldout();
                InspectorPerformanceUtility.CompleteChange(m_targetSenses);
            }

            EditorGUILayout.EndHorizontal();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_targetSenses.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
        }
    }
}
#endif

