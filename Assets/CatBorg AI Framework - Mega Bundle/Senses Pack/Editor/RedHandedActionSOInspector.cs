using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Senses2026
{
    [CustomEditor(typeof(RedHandedActionSO))]
    public class RedHandedActionSOInspector : Editor
    {
        RedHandedActionSO m_actionSO;
        bool m_needToSetDirty;
        List<int> m_selectedFactionNameIndicesCopy;
        GUIContent[] m_guiContent = new GUIContent[4];
        protected virtual void OnEnable()
        {
            m_actionSO = (RedHandedActionSO)target;
            m_selectedFactionNameIndicesCopy = new List<int>(m_actionSO.SelectedFactionNameIndices);
            m_guiContent[0] = new GUIContent("Required Factions", "Objects belonging to these factions will be affected by this Red Handed Action.");
            m_guiContent[1] = new GUIContent("Result Faction", "The faction that affected objects will switch to after this action triggers.");
            m_guiContent[2] = new GUIContent("Apply If Got Caught", "If enabled, this action will only apply when a Red Handed component is caught.");
            m_guiContent[3] = new GUIContent("Apply Globally", "If enabled, the faction change will apply to all matching factions in the scene.");
        }

        public override void OnInspectorGUI()
        {
            m_needToSetDirty = false;
            serializedObject.Update();
            InspectorPerformanceUtility.SynchronizeIndices(m_selectedFactionNameIndicesCopy, m_actionSO.SelectedFactionNameIndices);
            DrawComponents();
            if (serializedObject != null && serializedObject.targetObject != null)
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (m_needToSetDirty)
            {
                EditorUtility.SetDirty(m_actionSO);
            }
        }

        protected virtual void DrawComponents()
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            // --- Required Factions ---
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_guiContent[0], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            string[] _factionNames = EditorFactionHelpers.GetFactionNames();
            int _currentMask = 0;
            foreach (int _index in m_selectedFactionNameIndicesCopy)
            {
                _currentMask |= (1 << _index);
            }

            int _newMask = EditorGUILayout.MaskField(_currentMask, _factionNames, GUILayout.ExpandWidth(true));
            if (_newMask != _currentMask)
            {
                m_selectedFactionNameIndicesCopy.Clear();
                for (int _i = 0; _i < _factionNames.Length; _i++)
                {
                    if ((_newMask & (1 << _i)) != 0)
                    {
                        m_selectedFactionNameIndicesCopy.Add(_i);
                    }
                }

                InspectorPerformanceUtility.RecordChange(m_actionSO);
                m_actionSO.Set_SelectedFactionNames(new List<int>(m_selectedFactionNameIndicesCopy));
                InspectorPerformanceUtility.CompleteChange(m_actionSO);
                m_needToSetDirty = true;
            }

            EditorGUILayout.EndHorizontal();
            // --- Result Faction ---
            DrawFactionPopup(m_guiContent[1], m_actionSO.ResultFactionName, m_actionSO, (value) =>
            {
                InspectorPerformanceUtility.RecordChange(m_actionSO);
                m_actionSO.ResultFactionName = value;
                InspectorPerformanceUtility.CompleteChange(m_actionSO);
                m_needToSetDirty = true;
            });
            // --- Apply If Got Caught ---
            bool _newApplyIfCaught = EditorGUILayout.Toggle(m_guiContent[2], m_actionSO.ApplyIsGotCaught);
            if (_newApplyIfCaught != m_actionSO.ApplyIsGotCaught)
            {
                InspectorPerformanceUtility.RecordChange(m_actionSO);
                m_actionSO.ApplyIsGotCaught = _newApplyIfCaught;
                InspectorPerformanceUtility.CompleteChange(m_actionSO);
                m_needToSetDirty = true;
            }

            // --- Apply Globally ---
            bool _newApplyGlobal = EditorGUILayout.Toggle(m_guiContent[3], m_actionSO.ApplyGlobaly);
            if (_newApplyGlobal != m_actionSO.ApplyGlobaly)
            {
                InspectorPerformanceUtility.RecordChange(m_actionSO);
                m_actionSO.ApplyGlobaly = _newApplyGlobal;
                InspectorPerformanceUtility.CompleteChange(m_actionSO);
                m_needToSetDirty = true;
            }

            // --- Optional Warning ---
            if (m_actionSO.SelectedFactionNameIndices.Contains((int)m_actionSO.ResultFactionName))
            {
                EditorGUILayout.HelpBox("One or more Required Factions match the Result Faction. This may cause unintended behavior.", MessageType.Warning);
            }
        }

        void DrawFactionPopup(GUIContent label, FactionNames previousValue, RedHandedActionSO so, System.Action<FactionNames> setValue)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            string[] _factionNames = EditorFactionHelpers.GetFactionNames();
            int _currentIndex = (int)previousValue;
            int _newIndex = EditorGUILayout.Popup(_currentIndex, _factionNames, GUILayout.ExpandWidth(true));
            if (_newIndex != _currentIndex)
            {
                setValue((FactionNames)_newIndex);
                m_needToSetDirty = true;
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

