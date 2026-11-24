#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Senses2026
{
    [CustomEditor(typeof(StealthZone))]
    public class StealthZoneInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/01c%20Stealth%20Zoone.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        StealthZone m_zone;
        SerializedProperty m_propStealthBuff;
        bool m_markDirty;
        GUIContent m_labelBuff;
        GUIContent m_disabledInfo;
        GUIContent m_colliderMissing;
        GUIContent m_colliderNotTrigger;
        void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            m_zone = (StealthZone)target;
            m_propStealthBuff = serializedObject.FindProperty("m_stealthBuff");
            m_labelBuff = new GUIContent("Stealth Buff", "Stealth bonus provided by this zone.\nIf the component is disabled, the buff behaves as 0.");
            m_disabledInfo = new GUIContent("Component is DISABLED:\n• Stealth Buff behaves as: 0\n\n" + "Runtime systems treat disabled zones as providing no stealth bonus.");
            m_colliderMissing = new GUIContent("Stealth Zone requires a Collider to function.\n" + "Add a Collider set to 'Is Trigger = true'.");
            m_colliderNotTrigger = new GUIContent("Collider should be set as a Trigger.\n" + "Otherwise objects cannot walk through the zone.");
        }

        public override void OnInspectorGUI()
        {
            m_markDirty = false;
            serializedObject.Update();
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            DrawDisabledInfo();
            DrawBuff();
            DrawColliderWarnings();
            draw_HelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_zone.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            if (serializedObject.ApplyModifiedProperties())
            {
                m_markDirty = true;
            }

            if (m_markDirty)
            {
                EditorUtility.SetDirty(m_zone);
            }
        }

        void DrawBuff()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_labelBuff, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.PropertyField(m_propStealthBuff, GUIContent.none, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
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

