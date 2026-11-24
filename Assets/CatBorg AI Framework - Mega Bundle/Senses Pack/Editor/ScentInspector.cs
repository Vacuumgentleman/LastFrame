#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Senses2026
{
    [CustomEditor(typeof(Scent))]
    public class ScentInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/04%20Scent%20Component.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        Scent m_scent;
        bool m_needTo_SeetEditorDirty = false;
        GUIContent[] m_GUIConetent = new GUIContent[30];
        float m_previous_ScentStrength;
        bool m_previous_UseVisualisation;
        float m_previous_NodeSeparation;
        int m_previousScentLayer_intValue;
        Gradient m_previous_ScentColorGradient;
        private SerializedProperty m_gradientProperty;
        bool m_previous_UseCustomRefreshRate;
        float m_previous_CooldownRefreshRate;
        bool m_previous_UseCustomDetectionRadius;
        float m_previous_RadiusZonerScentAddon;
        bool m_previous_UseDebugDrawRadius;
        int m_previous_LimitCollidersDetectionZoner;
        float m_previous_VisualisationOffsetY;
        LayerMask m_previous_DistractorLayerMask;
        protected virtual void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            m_scent = (Scent)target;
            m_gradientProperty = serializedObject.FindProperty("<ScentColorGradient>k__BackingField");
            // Cache previous values
            m_previous_ScentStrength = m_scent.ScentStrength;
            m_previous_UseVisualisation = m_scent.UseVisualisation;
            m_previous_NodeSeparation = m_scent.NodeSeparation;
            m_previousScentLayer_intValue = m_scent.ScentLayer;
            m_previous_ScentColorGradient = m_scent.ScentColorGradient;
            m_previous_UseCustomRefreshRate = m_scent.Use_CustomRefreshRate;
            m_previous_CooldownRefreshRate = m_scent.CooldownRefreshRate;
            m_previous_UseCustomDetectionRadius = m_scent.Use_CustomDetectionRadius;
            m_previous_RadiusZonerScentAddon = m_scent.Radius_ZonerScentAddon;
            m_previous_UseDebugDrawRadius = m_scent.Use_DebugDrawRadius;
            m_previous_LimitCollidersDetectionZoner = m_scent.Limit_CollidersDetectionZoner;
            m_previous_VisualisationOffsetY = m_scent.VisualisationOffsetY;
            m_previous_DistractorLayerMask = m_scent.DistractorDetectionLayer;
            // GUI labels
            m_GUIConetent[0] = new GUIContent("Scent Strength", "Sets the base scent intensity.");
            m_GUIConetent[1] = new GUIContent("Use Visualization", "Enables or disables visual scent trail.");
            m_GUIConetent[2] = new GUIContent("Node Separation", "Minimum spacing between Scent Nodes.");
            m_GUIConetent[3] = new GUIContent("Scent Layer", "Unity layer used for scent detection.");
            m_GUIConetent[4] = new GUIContent("└─ Scent Gradient", "Gradient for visualization.");
            m_GUIConetent[5] = new GUIContent("Zoner Scent Add-On Properties", "Add-on settings.");
            m_GUIConetent[6] = new GUIContent("X", "Disable Add-On.");
            m_GUIConetent[7] = new GUIContent("Add Zoner Scent Add-On", "Enable Add-On.");
            m_GUIConetent[8] = new GUIContent("Custom Refresh Rate", "Check distractors less often.");
            m_GUIConetent[9] = new GUIContent("└─ Refresh Time [s]", "Update interval.");
            m_GUIConetent[10] = new GUIContent("Custom Radius", "Override auto-calculated radius.");
            m_GUIConetent[11] = new GUIContent("└─ Detection Radius", "Custom radius.");
            m_GUIConetent[12] = new GUIContent("Gizmo Radius", "Draws the detection radius in the Scene View (requires Gizmos enabled).");
            m_GUIConetent[13] = new GUIContent("Expected Colliders", "Expected Amount of Colliders in Radius - Small Value could be used when is not many colliders in radius area, High Value with multiple colliders in radius area.");
            m_GUIConetent[14] = new GUIContent("└─ Offset Y", "Vertical offset for visualization.");
            m_GUIConetent[16] = new GUIContent("Detection Layer", "Which layers contain ScentZoneDistractor objects. Only colliders on these layers can modify or block scent.");
        }

        public override void OnInspectorGUI()
        {
            m_needTo_SeetEditorDirty = false;
            if (serializedObject != null && serializedObject.targetObject != null)
            {
                serializedObject.Update();
            }

            draw_TargetElements();
            check_ForRequiredComponents();
            draw_HelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_scent.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            if (serializedObject != null && serializedObject.targetObject != null)
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (m_needTo_SeetEditorDirty)
            {
                EditorUtility.SetDirty(m_scent);
            }
        }

        void draw_TargetElements()
        {
            //==============================
            // Scent strength
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[0], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Slider(m_scent.ScentStrength, 1f, 10f);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_ScentStrength(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_ScentStrength != m_scent.ScentStrength)
            {
                m_previous_ScentStrength = m_scent.ScentStrength;
                m_needTo_SeetEditorDirty = true;
            }

            //==============================
            // Visualization toggle
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[1], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_scent.UseVisualisation);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_UseVisualisation(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_UseVisualisation != m_scent.UseVisualisation)
            {
                m_previous_UseVisualisation = m_scent.UseVisualisation;
                m_needTo_SeetEditorDirty = true;
            }

            // Visualization details
            if (m_scent.UseVisualisation)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[4], GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_gradientProperty, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    Gradient _gradient = m_gradientProperty.gradientValue;
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_ScentColorGradient(_gradient);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }

                EditorGUILayout.EndHorizontal();
                if (m_previous_ScentColorGradient != m_scent.ScentColorGradient)
                {
                    m_previous_ScentColorGradient = m_scent.ScentColorGradient;
                    m_needTo_SeetEditorDirty = true;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[14], GUILayout.Width(EditorGUIUtility.labelWidth));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_scent.VisualisationOffsetY, -2f, 2f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_scent);
                        m_scent.Set_VisualisationOffsetY(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_scent);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previous_VisualisationOffsetY != m_scent.VisualisationOffsetY)
                {
                    m_previous_VisualisationOffsetY = m_scent.VisualisationOffsetY;
                    m_needTo_SeetEditorDirty = true;
                }
            }

            //==============================
            // Node separation
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[2], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Slider(m_scent.NodeSeparation, 1f, 5f);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_NodeSeparation(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_NodeSeparation != m_scent.NodeSeparation)
            {
                m_previous_NodeSeparation = m_scent.NodeSeparation;
                m_needTo_SeetEditorDirty = true;
            }

            //==============================
            // Scent layer
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[3], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.LayerField(m_scent.ScentLayer);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_ScentLayer(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousScentLayer_intValue != m_scent.ScentLayer)
            {
                m_previousScentLayer_intValue = m_scent.ScentLayer;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            //==============================
            // Zoner Add-On
            //==============================
            if (m_scent.Use_Zoner_ScentAddon)
            {
                EditorGUILayout.BeginHorizontal();
                draw_ZonerScentAddon_propertiesHeader();
                EditorGUILayout.EndHorizontal();
                draw_ZonerScentAddon_details();
                EditorGUILayout.Space();
            }
            else
            {
                if (GUILayout.Button(m_GUIConetent[7]))
                {
                    {
                        InspectorPerformanceUtility.RecordChange(m_scent);
                        m_scent.Set_UseZonerScentAddon(true);
                        InspectorPerformanceUtility.CompleteChange(m_scent);
                    }

                    m_needTo_SeetEditorDirty = true;
                }
            }
        }

        //===========================================
        // Zoner Addon Header
        //===========================================
        void draw_ZonerScentAddon_propertiesHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconZonerScent");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconZonerScent");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button(m_GUIConetent[5], EditorStyles.boldLabel))
            {
                InspectorPerformanceUtility.RecordChange(m_scent);
                m_scent.Toggle_ZonerScentAddonFoldout();
                InspectorPerformanceUtility.CompleteChange(m_scent);
            }

            if (GUILayout.Button(m_GUIConetent[6], GUILayout.Width(20)))
            {
                InspectorPerformanceUtility.RecordChange(m_scent);
                m_scent.Set_UseZonerScentAddon(false);
                InspectorPerformanceUtility.CompleteChange(m_scent);
            }

            EditorGUILayout.EndHorizontal();
        }

        //===========================================
        // Zoner Addon Details
        //===========================================
        void draw_ZonerScentAddon_details()
        {
            if (!m_scent.ZonerScentAddonFoldout)
            {
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            // Show live info
            if (m_scent.ScentZonner_BlockScentEmmision)
            {
                EditorGUILayout.HelpBox("Emission BLOCKED by Scent Zone Distractor.", MessageType.Info);
            }
            else if (m_scent.ScentZoneModificationValue != 0)
            {
                EditorGUILayout.LabelField("Modification Value", m_scent.ScentZoneModificationValue.ToString());
            }

            //==============================
            // Custom Refresh
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[8], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_scent.Use_CustomRefreshRate);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_UseCustomRefreshRate(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_UseCustomRefreshRate != m_scent.Use_CustomRefreshRate)
            {
                m_previous_UseCustomRefreshRate = m_scent.Use_CustomRefreshRate;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_scent.Use_CustomRefreshRate)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[9], GUILayout.Width(EditorGUIUtility.labelWidth));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_scent.CooldownRefreshRate, 0.016f, 1f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_scent);
                        m_scent.Set_CooldownRefreshRate(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_scent);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previous_CooldownRefreshRate != m_scent.CooldownRefreshRate)
                {
                    m_previous_CooldownRefreshRate = m_scent.CooldownRefreshRate;
                    m_needTo_SeetEditorDirty = true;
                }
            }

            //==============================
            // Custom Radius
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[10], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_scent.Use_CustomDetectionRadius);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_UseCustomDetectionRadius(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_UseCustomDetectionRadius != m_scent.Use_CustomDetectionRadius)
            {
                m_previous_UseCustomDetectionRadius = m_scent.Use_CustomDetectionRadius;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_scent.Use_CustomDetectionRadius)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[11], GUILayout.Width(EditorGUIUtility.labelWidth));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_scent.Radius_ZonerScentAddon, 0.001f, 5f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_scent);
                        m_scent.Set_Radius_ZonerScentAddon(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_scent);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previous_RadiusZonerScentAddon != m_scent.Radius_ZonerScentAddon)
                {
                    m_previous_RadiusZonerScentAddon = m_scent.Radius_ZonerScentAddon;
                    m_needTo_SeetEditorDirty = true;
                }
            }

            //==============================
            // Debug Draw
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[12], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_scent.Use_DebugDrawRadius);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_UseDebugDrawRadius(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_UseDebugDrawRadius != m_scent.Use_DebugDrawRadius)
            {
                m_previous_UseDebugDrawRadius = m_scent.Use_DebugDrawRadius;
                m_needTo_SeetEditorDirty = true;
            }

            //==============================
            // LayerMask: Distractor Detection Layer
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[16], GUILayout.Width(EditorGUIUtility.labelWidth));
            LayerMask _newMask = LayerMaskField(m_scent.DistractorDetectionLayer);
            if (_newMask != m_scent.DistractorDetectionLayer)
            {
                m_previous_DistractorLayerMask = _newMask;
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_DistractorDetectionLayer(_newMask);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }

                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.EndHorizontal();
            //==============================
            // Max colliders to check
            //==============================
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[13], GUILayout.Width(EditorGUIUtility.labelWidth));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.IntSlider(m_scent.Limit_CollidersDetectionZoner, 10, 200);
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_scent);
                    m_scent.Set_Limit_CollidersDetectionZoner(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_scent);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_LimitCollidersDetectionZoner != m_scent.Limit_CollidersDetectionZoner)
            {
                m_previous_LimitCollidersDetectionZoner = m_scent.Limit_CollidersDetectionZoner;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.EndVertical();
        }

        // LayerMask dropdown (multi-select)
        LayerMask LayerMaskField(LayerMask _layerMask)
        {
            return InspectorPerformanceUtility.LayerMaskField(_layerMask);
        }

        void check_ForRequiredComponents()
        {
            if (m_scent.GetComponent<TargetSenses>() == null)
            {
                EditorGUILayout.HelpBox("Scent requires TargetSenses component.", MessageType.Warning);
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
                InspectorPerformanceUtility.RecordChange(m_scent);
                m_scent.Toggle_HelpPropertiesFoldout();
                InspectorPerformanceUtility.CompleteChange(m_scent);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

