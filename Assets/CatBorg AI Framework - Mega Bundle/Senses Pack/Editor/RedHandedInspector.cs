using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)
namespace Senses2026
{
    [CustomEditor(typeof(RedHanded))]
    public class RedHandedInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/03%20RedHanded%20Component.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        RedHanded m_target;
        bool m_previous_UseCustomDetectionRadius;
        float m_previous_RadiusDetectionOfBuffZones;
        bool m_previous_UseDebugDrawRadius;
        int m_previous_AmountCollidersZonnerDetection;
        bool m_needTo_SeetEditorDirty;
        LayerMask m_previous_DistractorLayerMask;
        GUIContent[] m_GUIConetent = new GUIContent[7];
        protected virtual void OnEnable()
        {
            m_target = (RedHanded)target;
            // cache previous values
            m_previous_UseCustomDetectionRadius = m_target.Use_CustomDetectionRadius;
            m_previous_RadiusDetectionOfBuffZones = m_target.Radius_RedHanded;
            m_previous_UseDebugDrawRadius = m_target.Use_DebugDrawRadius;
            m_previous_AmountCollidersZonnerDetection = m_target.Limit_CollidersDetectionZoner;
            m_previous_DistractorLayerMask = m_target.RedHandedDetectionLayer;
            // initialize GUI content
            m_GUIConetent[0] = new GUIContent("Custom Radius", "When enabled, allows custom radius of detection for RedHanded zones and objects.");
            m_GUIConetent[1] = new GUIContent("└─ Detection Radius", "Radius for checking around colliders with RedHanded zones and objects.");
            m_GUIConetent[2] = new GUIContent("Gizmo Radius", "When enabled, draws gizmo radius for visual debugging.");
            m_GUIConetent[3] = new GUIContent("Expected Colliders", "Expected number of colliders in detection radius.");
            m_GUIConetent[4] = new GUIContent("Need some help?", "Click to open help section.");
            m_GUIConetent[5] = new GUIContent("RedHanded Properties", "Settings for RedHanded detection behavior.");
            m_GUIConetent[6] = new GUIContent("RedHanded Layers", "Which layers contain RedHanded Zones and Objects. Only colliders on these layers can affect RedHanded component.");
        }

        public override void OnInspectorGUI()
        {
            m_needTo_SeetEditorDirty = false;
            draw_TargetElements();
            EditorGUILayout.Space(8);
            check_ForRequiredComponents();
            draw_HelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_target.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
            if (m_needTo_SeetEditorDirty)
            {
                EditorUtility.SetDirty(m_target);
            }
        }

        void draw_TargetElements()
        {
            // --- Use Custom Radius ---
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[0], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_target.Use_CustomDetectionRadius, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_target);
                    m_target.Set_UseCustomDetectionRadius(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_target);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_UseCustomDetectionRadius != m_target.Use_CustomDetectionRadius)
            {
                m_previous_UseCustomDetectionRadius = m_target.Use_CustomDetectionRadius;
                m_needTo_SeetEditorDirty = true;
            }

            // --- Radius slider if custom enabled ---
            if (m_target.Use_CustomDetectionRadius)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[1], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_target.Radius_RedHanded, 0.001f, 5f, GUILayout.Width(EditorGUIUtility.labelWidth + 0), GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_target);
                        m_target.Set_RadiusRedHanded(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_target);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previous_RadiusDetectionOfBuffZones != m_target.Radius_RedHanded)
                {
                    m_previous_RadiusDetectionOfBuffZones = m_target.Radius_RedHanded;
                    m_needTo_SeetEditorDirty = true;
                }
            }

            // --- Debug Draw Radius ---
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[2], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_target.Use_DebugDrawRadius, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_target);
                    m_target.Set_UseDebugDrawRadius(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_target);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_UseDebugDrawRadius != m_target.Use_DebugDrawRadius)
            {
                m_previous_UseDebugDrawRadius = m_target.Use_DebugDrawRadius;
                m_needTo_SeetEditorDirty = true;
            }

            // --- Layer Mask ---
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[6], GUILayout.Width(EditorGUIUtility.labelWidth));
            LayerMask _newMask = LayerMaskField(m_target.RedHandedDetectionLayer);
            if (_newMask != m_target.RedHandedDetectionLayer)
            {
                m_previous_DistractorLayerMask = _newMask;
                {
                    InspectorPerformanceUtility.RecordChange(m_target);
                    m_target.Set_RedHandedDetectionLayer(_newMask);
                    InspectorPerformanceUtility.CompleteChange(m_target);
                }

                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.EndHorizontal();
            // --- Expected Colliders ---
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[3], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.IntSlider(m_target.Limit_CollidersDetectionZoner, 1, 200, GUILayout.Width(EditorGUIUtility.labelWidth + 0), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_target);
                    m_target.Set_Limit_CollidersDetectionZoner(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_target);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previous_AmountCollidersZonnerDetection != m_target.Limit_CollidersDetectionZoner)
            {
                m_previous_AmountCollidersZonnerDetection = m_target.Limit_CollidersDetectionZoner;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.Space(2);
        }

        void check_ForRequiredComponents()
        {
            if (m_target.GetComponent<TargetSenses>() == null)
            {
                EditorGUILayout.HelpBox("RedHanded component requires TargetSenses component to function.", MessageType.Warning);
            }
        }

        void draw_HelpFoldout()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHelp");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconHelp", "Need some help?");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            if (GUILayout.Button(m_GUIConetent[4], EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_target);
                    m_target.Toggle_HelpPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_target);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        LayerMask LayerMaskField(LayerMask _layerMask)
        {
            return InspectorPerformanceUtility.LayerMaskField(_layerMask);
        }
    }
}
#endif

