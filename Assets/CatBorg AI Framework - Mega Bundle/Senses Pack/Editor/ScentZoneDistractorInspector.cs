#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Senses2026
{
    [CustomEditor(typeof(ScentZoneDistractor))]
    public class ScentZoneDistractorInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/04a%20ScentZoneDistractor.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        ScentZoneDistractor m_zone;
        SerializedProperty m_propModification;
        SerializedProperty m_propStrengthChange;
        GUIContent m_labelModification;
        GUIContent m_labelStrength;
        GUIContent m_disabledInfo;
        GUIContent m_colliderMissing;
        GUIContent m_colliderNotTrigger;
        void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            m_zone = (ScentZoneDistractor)target;
            m_propModification = serializedObject.FindProperty("m_scentModification");
            m_propStrengthChange = serializedObject.FindProperty("m_scentStrengthChange");
            m_labelModification = new GUIContent("Scent Modification", "Choose:\n• None → No effect\n• Change Scent Strength → Adds or subtracts scent\n• Block Scent Emission → Completely blocks scent");
            m_labelStrength = new GUIContent("└─ Change Strength", "Amount by which this zone modifies scent strength.");
            m_disabledInfo = new GUIContent("Component is DISABLED:\n" + "• Modification behaves as: None\n" + "• Strength Change behaves as: 0\n\n" + "Runtime will ignore stored values while the component is disabled.");
            m_colliderMissing = new GUIContent("Scent Zone Distractor requires a Collider.\n" + "Add a Collider set to 'Is Trigger = true'.");
            m_colliderNotTrigger = new GUIContent("Collider should be set as a Trigger.\n" + "Otherwise objects cannot pass through the zone.");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            DrawDisabledInfo();
            DrawProperties();
            DrawColliderWarnings();
            draw_HelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_zone.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            serializedObject.ApplyModifiedProperties();
        }

        void DrawProperties()
        {
            EditorGUILayout.PropertyField(m_propModification, m_labelModification);
            if ((ScentModification)m_propModification.enumValueIndex == ScentModification.ChangeScentStrength)
            {
                EditorGUILayout.PropertyField(m_propStrengthChange, m_labelStrength);
            }
        }

        void DrawDisabledInfo()
        {
            if (!m_zone.enabled)
            {
                EditorGUILayout.HelpBox(m_disabledInfo.text, MessageType.Info);
                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            }
        }

        void DrawColliderWarnings()
        {
            Collider _col = m_zone.GetComponent<Collider>();
            if (_col == null)
            {
                EditorGUILayout.HelpBox(m_colliderMissing.text, MessageType.Warning);
                return;
            }

            if (!_col.isTrigger)
            {
                EditorGUILayout.HelpBox(m_colliderNotTrigger.text, MessageType.Warning);
            }
        }

        void draw_HelpFoldout()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconHelp");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                InspectorPerformanceUtility.RecordChange(m_zone);
                m_zone.Toggle_HelpPropertiesFoldout();
                InspectorPerformanceUtility.CompleteChange(m_zone);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

