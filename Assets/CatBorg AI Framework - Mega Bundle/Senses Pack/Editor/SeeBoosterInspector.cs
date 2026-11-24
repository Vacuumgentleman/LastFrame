#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Senses2026
{
    [CustomEditor(typeof(SeeBooster))]
    public class SeeBoosterInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/07%20SeeBoost%20Component.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        SeeBooster m_booster;
        SerializedProperty propAspect;
        SerializedProperty propOperation;
        SerializedProperty propValue;
        bool markDirty;
        GUIContent labelAspect;
        GUIContent labelOperation;
        GUIContent labelValue;
        GUIContent disabledInfo;
        void OnEnable()
        {
            m_booster = (SeeBooster)target;
            propAspect = serializedObject.FindProperty("<AffectedAspect>k__BackingField");
            propOperation = serializedObject.FindProperty("<OperationType>k__BackingField");
            propValue = serializedObject.FindProperty("<Value>k__BackingField");
            labelAspect = new GUIContent("Affected Aspect", "Choose which part of the See Sense is modified.");
            labelOperation = new GUIContent("Operation", "How the booster modifies the value.");
            labelValue = new GUIContent("Value", "Amount used by the selected operation.");
            disabledInfo = new GUIContent("Component is DISABLED:\n• Booster does not modify Delta See Awareness.\n• Multipliers and additions do not apply.");
        }

        public override void OnInspectorGUI()
        {
            markDirty = false;
            serializedObject.Update();
            EditorGUILayout.Space();
            DrawDisabledInfo();
            DrawMainProperties();
            draw_HelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_booster.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            if (serializedObject.ApplyModifiedProperties())
            {
                markDirty = true;
            }

            if (markDirty)
            {
                EditorUtility.SetDirty(m_booster);
            }
        }

        void DrawDisabledInfo()
        {
            if (!m_booster.enabled)
            {
                EditorGUILayout.HelpBox(disabledInfo.text, MessageType.Info);
                EditorGUILayout.Space();
            }
        }

        void DrawMainProperties()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(propAspect, labelAspect);
            EditorGUILayout.PropertyField(propOperation, labelOperation);
            EditorGUILayout.PropertyField(propValue, labelValue);
            EditorGUILayout.EndVertical();
        }

        void draw_HelpFoldout()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconHelp");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                InspectorPerformanceUtility.RecordChange(m_booster);
                m_booster.Toggle_HelpPropertiesFoldout();
                InspectorPerformanceUtility.CompleteChange(m_booster);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

