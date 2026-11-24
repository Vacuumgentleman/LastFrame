using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Senses2026
{
    [CustomEditor(typeof(NoiseTrap))]
    public class NoiseTrapInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/05a%20Noise%20Trap.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        NoiseTrap m_trap;
        SerializedProperty m_noiseStrengthProp;
        GUIContent gc_NoiseStrength;
        GUIContent gc_Info;
        void OnEnable()
        {
            m_trap = (NoiseTrap)target;
            m_noiseStrengthProp = serializedObject.FindProperty("<NoiseStrength>k__BackingField");
            gc_NoiseStrength = new GUIContent("Noise Strength", "Defines how strong the trap's noise signal is when force is applied.\n" + "Higher values travel farther and alert more AI.");
            gc_Info = new GUIContent("Noise Trap", "Emits noise when receiving physics force through AddForceAtPosition.");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            // Header Box
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(gc_Info, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            // Noise Strength
            EditorGUILayout.PropertyField(m_noiseStrengthProp, gc_NoiseStrength);
            EditorGUILayout.Space(10);
            drawRequiredComponentsInfo();
            drawHelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_trap.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            serializedObject.ApplyModifiedProperties();
        }

        void drawRequiredComponentsInfo()
        {
            // Required Components Check
            checkComponent<Noise>("Noise");
            checkComponent<Rigidbody>("Rigidbody");
            checkComponent<Collider>("Collider");
            checkComponent<TargetSenses>("Target Senses");
        }

        void checkComponent<T>(string label)
            where T : Component
        {
            if (m_trap.GetComponent<T>() == null)
            {
                EditorGUILayout.HelpBox($"{label} component is missing.", MessageType.Error);
            }
        }

        void drawHelpFoldout()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            GUIContent _content = LoadIconForEditorHelpWindow.LoadContent("IconHelp", "Need some help?");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_content, GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_trap);
                    m_trap.ToggleHelpPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_trap);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

