using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)
namespace Senses2026
{
    [CustomEditor(typeof(Obstacle))]
    public class ObstacleInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/30810ac7a18a7d5587c012fba1b801d5b25a4be0/02%20Documentation%20Senses2026/06%20Obstacle%20Component.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        Obstacle m_obstacle;
        float m_previous_RangeDecreaseFactor;
        bool m_needTo_SeetEditorDirty;
        GUIContent[] m_GUIConetent = new GUIContent[1];
        protected virtual void OnEnable()
        {
            m_obstacle = (Obstacle)target;
            m_previous_RangeDecreaseFactor = m_obstacle.Range_DecreaseFactor;
            m_GUIConetent[0] = new GUIContent("Range Decrease Factor", "Float value that slice the remaining vision range of a character which looking through obstacle)");
        }

        public override void OnInspectorGUI()
        {
            m_needTo_SeetEditorDirty = false;
            DrawComponents();
            CheckCollider();
            EditorGUILayout.Space();
            draw_HelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_obstacle.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            if (m_needTo_SeetEditorDirty)
            {
                EditorUtility.SetDirty(m_obstacle);
            }
        }

        protected virtual void DrawComponents()
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[0], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Slider(m_obstacle.Range_DecreaseFactor, 0f, 1f, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_obstacle);
                    m_obstacle.Set_RangeDecreaseFactor(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_obstacle);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_RangeDecreaseFactor != m_obstacle.Range_DecreaseFactor)
            {
                m_previous_RangeDecreaseFactor = m_obstacle.Range_DecreaseFactor;
                m_needTo_SeetEditorDirty = true;
            }
        }

        protected virtual void CheckCollider()
        {
            if (m_obstacle.GetComponent<Collider>() == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Obstacle component requires collider", MessageType.Warning);
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
                InspectorPerformanceUtility.RecordChange(m_obstacle);
                m_obstacle.Toggle_HelpPropertiesFoldout();
                InspectorPerformanceUtility.CompleteChange(m_obstacle);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

