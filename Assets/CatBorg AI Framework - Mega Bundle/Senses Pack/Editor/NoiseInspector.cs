using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Senses2026
{
    [CustomEditor(typeof(Noise))]
    public class NoiseInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/05%20Noise%20Component.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        Noise m_noise;
        bool m_previousUseNavMesh;
        bool m_previousSpreadAccordingToDistance;
        bool m_needToSetEditorDirty = false;
        float m_previousGroundCheckRange;
        GUIContent[] m_guiContent = new GUIContent[10];
        bool m_previousUseSoundSurfacePropagation;
        SoundPropagationSettings m_soundSettings;
        SerializedProperty m_ignoreLayers;
        int m_previousIgnoreLayersValue;
        protected virtual void OnEnable()
        {
            m_noise = (Noise)target;
            m_soundSettings = Resources.Load<SoundPropagationSettings>("SoundPropagationSettings");
            m_previousUseNavMesh = m_noise.UseNavMesh;
            m_previousSpreadAccordingToDistance = m_noise.UseSpreadAccordingToDistance;
            m_previousGroundCheckRange = m_noise.GroundCheckRange;
            m_previousUseSoundSurfacePropagation = m_noise.UseSoundSurfacePropagation;
            m_ignoreLayers = serializedObject.FindProperty("IgnoreLayers");
            m_previousIgnoreLayersValue = m_noise.IgnoreLayers;
            m_guiContent[0] = new GUIContent("Use NavMesh", "When enabled, the distance to a Senses Component with a Hear Sensor will be calculated using the NavMesh. If no valid path is found, the distance will be calculated in a straight line.");
            m_guiContent[1] = new GUIContent("Distance Distribution", "When enabled, all Senses Components will receive emitted noise in order of distance (physically accurate solution). When disabled, all Senses Components will receive emitted noise in sphere cast order (performance-efficient solution).");
            m_guiContent[2] = new GUIContent("Surface Propagation", "When enabled, the noise value will be multiplied based on the type of surface below the agent.");
            m_guiContent[3] = new GUIContent("Check Range", "Allows configuring the collider check range that affects sound propagation. If multiple elements are detected, sound propagation will be influenced by the element with the highest Y position.");
            m_guiContent[4] = new GUIContent("Ignore Layers", "Allows setting up ignored layers.");
            m_guiContent[5] = new GUIContent("Settings", "Allows setting up sound propagation settings, TAGs and Factors.");
            m_guiContent[6] = new GUIContent("Release Test Noise: 180 dB", "Extremely loud — similar to a rocket launch at close range.\nUseful for testing extreme propagation.\n(Available only in Play Mode)");
            m_guiContent[7] = new GUIContent("Release Test Noise: 120 dB", "Very loud — comparable to a thunderclap or a jet engine at 100 meters.\nUseful for long-distance tests.\n(Available only in Play Mode)");
            m_guiContent[8] = new GUIContent("Release Test Noise: 80 dB", "Moderately loud — like heavy traffic, a vacuum cleaner, or loud conversation.\nTypical gameplay-level noise.\n(Available only in Play Mode)");
            m_guiContent[9] = new GUIContent("Release Test Noise: 30 dB", "Quiet — similar to whispering or leaves rustling.\nGood for subtle stealth noise tests.\n(Available only in Play Mode)");
        }

        public override void OnInspectorGUI()
        {
            m_needToSetEditorDirty = false;
            serializedObject.Update();
            drawTargetElements();
            checkForRequiredComponents();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            drawSettingsHeader();
            drawNoiseButtons();
            drawHelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_noise.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            serializedObject.ApplyModifiedProperties();
            if (m_needToSetEditorDirty)
            {
                EditorUtility.SetDirty(m_noise);
            }
        }

        void drawTargetElements()
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_guiContent[0], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_noise.UseNavMesh, GUILayout.Width(EditorGUIUtility.labelWidth + 16));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_noise);
                    m_noise.SetUseNavMesh(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_noise);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousUseNavMesh != m_noise.UseNavMesh)
            {
                m_previousUseNavMesh = m_noise.UseNavMesh;
                m_needToSetEditorDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_guiContent[1], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_noise.UseSpreadAccordingToDistance, GUILayout.Width(EditorGUIUtility.labelWidth + 16));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_noise);
                    m_noise.SetSpreadAccordingToDistance(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_noise);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousSpreadAccordingToDistance != m_noise.UseSpreadAccordingToDistance)
            {
                m_previousSpreadAccordingToDistance = m_noise.UseSpreadAccordingToDistance;
                m_needToSetEditorDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_guiContent[2], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_noise.UseSoundSurfacePropagation, GUILayout.Width(EditorGUIUtility.labelWidth + 16));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_noise);
                    m_noise.SetSoundSurfacePropagation(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_noise);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousUseSoundSurfacePropagation != m_noise.UseSoundSurfacePropagation)
            {
                m_previousUseSoundSurfacePropagation = m_noise.UseSoundSurfacePropagation;
                m_needToSetEditorDirty = true;
            }

            if (m_noise.UseSoundSurfacePropagation)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_guiContent[3], GUILayout.Width(EditorGUIUtility.labelWidth));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_noise.GroundCheckRange, 0.01f, 2f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_noise);
                        m_noise.SetGroundCheckRange(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_noise);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previousGroundCheckRange != m_noise.GroundCheckRange)
                {
                    m_previousGroundCheckRange = m_noise.GroundCheckRange;
                    m_needToSetEditorDirty = true;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_guiContent[4], GUILayout.Width(EditorGUIUtility.labelWidth));
                m_ignoreLayers.intValue = layerMaskField(m_ignoreLayers.intValue);
                EditorGUILayout.EndHorizontal();
                if (m_previousIgnoreLayersValue != m_ignoreLayers.intValue)
                {
                    m_previousIgnoreLayersValue = m_ignoreLayers.intValue;
                    m_needToSetEditorDirty = true;
                }

                drawSoundPropagationSettings();
            }
        }

        void checkForRequiredComponents()
        {
            if (m_noise.GetComponent<TargetSenses>() == null)
            {
                EditorGUILayout.HelpBox("Noise component requires Target Senses Component", MessageType.Warning);
            }
        }

        void drawSettingsHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("Settings");
            GUIContent _content = LoadIconForEditorHelpWindow.LoadContent("Settings", "Open test noise panel");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            if (_icon != null)
            {
                GUILayout.Label(_content, GUILayout.Width(20), GUILayout.Height(20));
            }

            if (GUILayout.Button("Noise Test Panel", EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_noise);
                    m_noise.ToggleSettingsPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_noise);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void drawNoiseButtons()
        {
            if (!m_noise.SettingsPropertiesFoldout)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(CustomEditorStyles.NoMarginWindow);
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            if (GUILayout.Button(m_guiContent[6]))
            {
                m_noise.ReleaseTestNoise180db();
            }

            if (GUILayout.Button(m_guiContent[7]))
            {
                m_noise.ReleaseTestNoise120db();
            }

            if (GUILayout.Button(m_guiContent[8]))
            {
                m_noise.ReleaseTestNoise80db();
            }

            if (GUILayout.Button(m_guiContent[9]))
            {
                m_noise.ReleaseTestNoise30db();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        void drawHelpFoldout()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            GUIContent _content = LoadIconForEditorHelpWindow.LoadContent("IconHelp", "Need some help?");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            if (_icon != null)
            {
                GUILayout.Label(_content, GUILayout.Width(20), GUILayout.Height(20));
            }

            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_noise);
                    m_noise.ToggleHelpPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_noise);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void drawSoundPropagationSettings()
        {
            if (m_soundSettings == null)
            {
                EditorGUILayout.HelpBox("SoundPropagationSettings asset not found in Resources!", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_guiContent[5], GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(m_soundSettings, typeof(SoundPropagationSettings), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        static LayerMask layerMaskField(LayerMask _layerMask)
        {
            return InspectorPerformanceUtility.LayerMaskField(_layerMask);
        }
    }
}
#endif

