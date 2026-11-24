using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace Senses2026
{
    [CustomEditor(typeof(Stealth))]
    public class StealthInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("View Stealth GitHub Documentation", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/01%20Stealth%20Component.pdf"),
            new HelpLink("View Lumen Stealth Addon GitHub Documentation", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/01a%20Lumen%20Stealth%20AddOn.pdf"),
            new HelpLink("View Zooner Stealth Addon GitHub Documentation", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/01b%20Zooner%20Stealth%20AddOn.pdf"),
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        Stealth m_stealth;
        bool m_prevUseLumen;
        bool m_prevUseZoner;
        float m_prevStealthBuff;
        bool m_prevUseCustomRadius;
        float m_prevRadius;
        bool m_prevUseDebugDraw;
        int m_prevColliderLimit;
        float m_prevOffsetY;
        bool m_prevLumenGod;
        bool m_prevZonerGod;
        LayerMask m_previousLayerMask;
        bool m_setDirty;
        GUIContent[] m_gui = new GUIContent[20];
        void OnEnable()
        {
            m_stealth = (Stealth)target;
            m_prevStealthBuff = m_stealth.StealthBuff;
            m_prevUseLumen = m_stealth.UseLumenStealthAddon;
            m_prevUseZoner = m_stealth.UseZonerStealthAddon;
            m_prevUseCustomRadius = m_stealth.UseCustomDetectionRadius;
            m_prevRadius = m_stealth.RadiusZonerStealthAddon;
            m_prevUseDebugDraw = m_stealth.UseDebugDrawRadius;
            m_prevColliderLimit = m_stealth.LimitCollidersDetectionZoner;
            m_prevOffsetY = m_stealth.OffsetAxisY;
            m_prevLumenGod = m_stealth.UseLumenGodMode;
            m_prevZonerGod = m_stealth.UseZonerGodMode;
            m_previousLayerMask = m_stealth.ZonerDetectionLayer;
            m_gui[0] = new GUIContent("Stealth Buff", "Base stealth multiplier.");
            m_gui[1] = new GUIContent("Add Lumen Stealth Addon", "Enables Lumen Stealth Addon.");
            m_gui[2] = new GUIContent("Add Zoner Stealth Addon", "Enables Zoner Stealth Addon.");
            m_gui[3] = new GUIContent("Zoner Stealth Addon Properties", "Show or hide Zoner Stealth Addon settings.");
            m_gui[4] = new GUIContent("X", "Disable Zoner Stealth Addon.");
            m_gui[7] = new GUIContent("Custom Radius", "Custom radius for detecting StealthZones.");
            m_gui[8] = new GUIContent("└─ New Radius", "Set radius for detecting StealthZones.");
            m_gui[9] = new GUIContent("Gizmo Radius", "Draw detection radius gizmo.");
            m_gui[10] = new GUIContent("Expected Colliders", "Optimal collider buffer size.");
            m_gui[11] = new GUIContent("Lumen Stealth Addon Properties", "Show or hide Lumen addon settings.");
            m_gui[12] = new GUIContent("X", "Disable Lumen addon.");
            m_gui[13] = new GUIContent("Lumen God Mode", "Max light gives max stealth buff.");
            m_gui[14] = new GUIContent("Current Value", "Runtime lumen value.");
            m_gui[15] = new GUIContent("Zoner God Mode", "Max Zoner value gives max stealth buff.");
            m_gui[16] = new GUIContent("Probe Offset Y", "Vertical offset for light measurement.");
            m_gui[17] = new GUIContent("Zoner Detection Layer", "Defines which layers contain Stealth Zones. Only colliders on these layers affect the stealth buff.");
        }

        public override void OnInspectorGUI()
        {
            m_setDirty = false;
            drawMain();
            drawHelp();
            if (m_setDirty)
            {
                EditorUtility.SetDirty(m_stealth);
            }
        }

        void drawMain()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Stealth Buff", GUILayout.Width(EditorGUIUtility.labelWidth + 10));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField(m_stealth.ProvideTotalStealthBuff(), GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[0], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Slider(m_stealth.StealthBuff, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetStealthBuff(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_prevStealthBuff != m_stealth.StealthBuff)
            {
                m_prevStealthBuff = m_stealth.StealthBuff;
                m_setDirty = true;
            }

            EditorGUILayout.Space();
            if (m_stealth.UseLumenStealthAddon)
            {
                drawLumenHeader();
                drawLumenDetails();
                EditorGUILayout.Space();
            }
            else if (GUILayout.Button(m_gui[1]))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetUseLumenStealthAddon(true);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }

                m_setDirty = true;
            }

            if (m_stealth.UseZonerStealthAddon)
            {
                drawZonerHeader();
                drawZonerDetails();
                EditorGUILayout.Space();
            }
            else if (GUILayout.Button(m_gui[2]))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetUseZonerStealthAddon(true);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }

                m_setDirty = true;
            }
        }

        void drawZonerHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconZoner");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(LoadIconForEditorHelpWindow.LoadContent("IconZoner"), GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button(m_gui[3], EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetLookForBuffZonesPropertiesFoldout(!m_stealth.LookForBuffZonesPropertiesFoldout);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }

                m_setDirty = true;
            }

            if (GUILayout.Button(m_gui[4], GUILayout.Width(20)))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetUseZonerStealthAddon(false);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }

                m_setDirty = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void drawZonerDetails()
        {
            if (!m_stealth.LookForBuffZonesPropertiesFoldout)
            {
                return;
            }

            EditorGUILayout.BeginVertical(CustomEditorStyles.NoMarginWindow);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stealth Zone Buff", GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField(m_stealth.ZonerStealthBuff);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[7], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_stealth.UseCustomDetectionRadius);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetUseCustomDetectionRadius(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_prevUseCustomRadius != m_stealth.UseCustomDetectionRadius)
            {
                m_prevUseCustomRadius = m_stealth.UseCustomDetectionRadius;
                m_setDirty = true;
            }

            if (m_stealth.UseCustomDetectionRadius)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_gui[8], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_stealth.RadiusZonerStealthAddon, 0.01f, 10f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_stealth);
                        m_stealth.SetRadiusZonerStealthAddon(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_stealth);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_prevRadius != m_stealth.RadiusZonerStealthAddon)
                {
                    m_prevRadius = m_stealth.RadiusZonerStealthAddon;
                    m_setDirty = true;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[9], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_stealth.UseDebugDrawRadius);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetUseDebugDrawRadius(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_prevUseDebugDraw != m_stealth.UseDebugDrawRadius)
            {
                m_prevUseDebugDraw = m_stealth.UseDebugDrawRadius;
                m_setDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[10], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.IntSlider(m_stealth.LimitCollidersDetectionZoner, 1, 200);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetLimitCollidersDetectionZoner(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_prevColliderLimit != m_stealth.LimitCollidersDetectionZoner)
            {
                m_prevColliderLimit = m_stealth.LimitCollidersDetectionZoner;
                m_setDirty = true;
            }

            // NEW — ZONER DETECTION LAYER
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[17], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            LayerMask _newMask = LayerMaskField(m_stealth.ZonerDetectionLayer);
            EditorGUILayout.EndHorizontal();
            if (_newMask != m_stealth.ZonerDetectionLayer)
            {
                m_previousLayerMask = _newMask;
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetZonerDetectionLayer(_newMask);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }

                m_setDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[15], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_stealth.UseZonerGodMode);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetZonerGodMode(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_prevZonerGod != m_stealth.UseZonerGodMode)
            {
                m_prevZonerGod = m_stealth.UseZonerGodMode;
                m_setDirty = true;
            }

            EditorGUILayout.EndVertical();
        }

        void drawLumenHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconLumenStealthAddon");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(LoadIconForEditorHelpWindow.LoadContent("IconLumenStealthAddon"), GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button(m_gui[11], EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetLumenPropertiesFoldout(!m_stealth.LumenPropertiesFoldout);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }

                m_setDirty = true;
            }

            if (GUILayout.Button(m_gui[12], GUILayout.Width(20)))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetUseLumenStealthAddon(false);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }

                m_setDirty = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        void drawLumenDetails()
        {
            if (!m_stealth.LumenPropertiesFoldout)
            {
                return;
            }

            EditorGUILayout.BeginVertical(CustomEditorStyles.NoMarginWindow);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[14], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField(m_stealth.LumenStealthBuff);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[16], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.FloatField(m_stealth.OffsetAxisY);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetOffsetAxisY(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_prevOffsetY != m_stealth.OffsetAxisY)
            {
                m_prevOffsetY = m_stealth.OffsetAxisY;
                m_setDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_gui[13], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_stealth.UseLumenGodMode);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_stealth);
                    m_stealth.SetLumenGodMode(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_stealth);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_prevLumenGod != m_stealth.UseLumenGodMode)
            {
                m_prevLumenGod = m_stealth.UseLumenGodMode;
                m_setDirty = true;
            }

            EditorGUILayout.EndVertical();
        }

        void drawHelp()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(LoadIconForEditorHelpWindow.LoadContent("IconHelp"), GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button("Need some help?", EditorStyles.boldLabel))
            {
                InspectorPerformanceUtility.RecordChange(m_stealth);
                m_stealth.ToggleHelpPropertiesFoldout();
                InspectorPerformanceUtility.CompleteChange(m_stealth);
            }

            EditorGUILayout.EndHorizontal();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_stealth.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
        }

        LayerMask LayerMaskField(LayerMask _layerMask)
        {
            return InspectorPerformanceUtility.LayerMaskField(_layerMask);
        }
    }
}
#endif

