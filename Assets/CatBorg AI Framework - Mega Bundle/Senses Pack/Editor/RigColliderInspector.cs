using UnityEditor;
using UnityEngine;
using System.Text;

#if (UNITY_EDITOR)
namespace Senses2026
{
    [CustomEditor(typeof(RigCollider))]
    public class RigColliderInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/02%20RigCollider.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        private RigCollider m_rigCollider;
        bool m_needTo_SeetEditorDirty = false;
        int m_previousLength;
        Transform[] m_previousTransformsForColliders;
        Collider[] m_previousColliders;
        StringBuilder m_stringBuilder;
        GUIContent[] m_GUIConetent = new GUIContent[6];
        protected virtual void OnEnable()
        {
            m_rigCollider = (RigCollider)target;
            m_previousLength = m_rigCollider.Length;
            m_previousTransformsForColliders = new Transform[m_rigCollider.Length];
            m_previousColliders = new Collider[m_rigCollider.Length];
            m_stringBuilder = new StringBuilder();
            for (int _i = 0; _i < m_rigCollider.Length; _i++)
            {
                m_previousTransformsForColliders[_i] = m_rigCollider.m_transformsForColliders[_i];
                m_previousColliders[_i] = m_rigCollider.Colliders[_i];
            }

            m_GUIConetent[0] = new GUIContent("Bone [] Length", "Allows setting up length of Bone Array.");
            m_GUIConetent[1] = new GUIContent("- Collider", "Click to remove collider from bone.");
            m_GUIConetent[2] = new GUIContent("+ Collider", "Click to generate collider for bone.");
            m_GUIConetent[3] = new GUIContent("Add Colliders To Bones[]", "Click to generate all missing Colliders to Bone Array");
            m_GUIConetent[4] = new GUIContent("Remove Colliders From Bones[]", "Click to destroys all Colliders atached to Bone Array");
            m_GUIConetent[5] = new GUIContent("Find all Colliders", "Click to find all Colliders already atached to Bone Array");
            m_needTo_SeetEditorDirty = false;
        }

        public override void OnInspectorGUI()
        {
            m_needTo_SeetEditorDirty = false;
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            serializedObject.Update();
            DrawComponents();
            EditorGUILayout.Space();
            draw_HelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_rigCollider.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            serializedObject.ApplyModifiedProperties();
            if (m_needTo_SeetEditorDirty)
            {
                EditorUtility.SetDirty(m_rigCollider);
            }
        }

        protected virtual void DrawComponents()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[0], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.IntField(m_rigCollider.Length, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_rigCollider);
                    m_rigCollider.Set_Length(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_rigCollider);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousLength != m_rigCollider.Length)
            {
                m_previousLength = m_rigCollider.Length;
                m_previousTransformsForColliders = new Transform[m_rigCollider.Length];
                m_previousColliders = new Collider[m_rigCollider.Length];
                m_needTo_SeetEditorDirty = true;
            }

            for (int _i = 0; _i < m_rigCollider.Length; _i++)
            {
                EditorGUILayout.BeginHorizontal();
                m_stringBuilder.Clear();
                m_stringBuilder.Append("Bone ");
                m_stringBuilder.Append(_i);
                EditorGUILayout.LabelField(m_stringBuilder.ToString(), GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = (Transform)EditorGUILayout.ObjectField(m_rigCollider.m_transformsForColliders[_i], typeof(Transform), true, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_rigCollider);
                        m_rigCollider.Set_Transform(_i, _newValue);
                        InspectorPerformanceUtility.CompleteChange(m_rigCollider);
                    }
                }

                {
                    if (m_rigCollider.Colliders[_i] != null)
                    {
                        if (GUILayout.Button(m_GUIConetent[1]))
                        {
                            m_rigCollider.RemoveCollider(_i);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(m_GUIConetent[2]))
                        {
                            m_rigCollider.AddCollider(_i);
                        }
                    }

                    if (m_previousColliders[_i] != m_rigCollider.Colliders[_i])
                    {
                        m_previousColliders[_i] = m_rigCollider.Colliders[_i];
                        m_needTo_SeetEditorDirty = true;
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previousTransformsForColliders[_i] != m_rigCollider.m_transformsForColliders[_i])
                {
                    m_previousTransformsForColliders[_i] = m_rigCollider.m_transformsForColliders[_i];
                    m_needTo_SeetEditorDirty = true;
                }
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            if (GUILayout.Button(m_GUIConetent[3]))
            {
                m_rigCollider.AddAllColliders();
            }

            if (GUILayout.Button(m_GUIConetent[4]))
            {
                m_rigCollider.RemoveAllColliders();
            }

            if (GUILayout.Button(m_GUIConetent[5]))
            {
                m_rigCollider.FindAllCollidersAndGrabReferencs();
            }
        }

        protected virtual void CheckCollider()
        {
            if (m_rigCollider.GetComponent<Collider>() == null)
            {
                EditorGUILayout.HelpBox("Stealth component requires collider", MessageType.Warning);
            }
        }

        void draw_HelpFoldout()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconHelp", "Need some help?");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_rigCollider);
                    m_rigCollider.ToggleHelpPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_rigCollider);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

