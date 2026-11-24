using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if (UNITY_EDITOR)
namespace Senses2026
{
    [CustomEditor(typeof(Senses))]
    public class SensesInspector : Editor
    {
        private static readonly HelpLink[] m_helpGithub = new HelpLink[]
        {
            new HelpLink("", "https://github.com/AdamChochowski/SENSES/blob/main/02%20Documentation%20Senses2026/00%20Senses%20Component.pdf"), // Tooltip auto-filled
        };
        private static readonly HelpLink[] m_helpYoutube = new HelpLink[]
        {
        };
        Senses m_targetSenses;
        GUIContent[] m_GUIConetent = new GUIContent[42];
        bool m_needTo_SeetEditorDirty = false;
        Transform m_previousEyes;
        float m_previousCentralVision_Angle;
        float m_previousCentralVision_Radius;
        bool m_previousUsePeripheralFOV;
        float m_previousPeripheralVision_Angle;
        float m_previousPeripheralVision_Radius;
        bool m_previousDisplayFOV;
        bool m_previousIgnoreRotation_X;
        bool m_previousRayCastInFovOnly;
        bool m_previousDebugDrawRay;
        bool m_previousOverrrideRotation;
        Vector3 m_previousRequiredRotation;
        bool m_previousOverrridePosition;
        Vector3 m_previousRequiredPositionOffset;
        bool m_previousAbleToReset_Awarness;
        float m_previousForgetingRate;
        bool m_previous_WillUseSee;
        bool m_previous_WillUseHear;
        bool m_previous_WillUse_Smell;
        bool m_previousUse_IgnoreOwnFaction;
        bool m_previousPerfectLineOfViewCheck;
        float m_previousSmell_Radius;
        bool m_previousWillUseSmellCustomRefreshRate;
        float m_previousCooldown_Smell_TimeAmount;
        int m_previousScentLayer_intValue;
        bool m_previousDebug_Detected;
        float m_previousRequiredAwarness;
        SerializedProperty m_requiredFactionsProperty;
        int m_previous_RequiredFactions_intValue;
        List<int> m_selectedFactionNameIndicesCoppy;
        HearSensitivity m_previousHear_Sensitivity;
#region See
        private SerializedProperty m_ignoreLayersProperty;
        private SerializedProperty m_seeLayersSolutionProperty;
        private SerializedProperty m_seeTargetLayersProperty;
        private SerializedProperty m_seeBlockingLayersProperty;
        private SerializedProperty m_initialColliderBufferSizeProperty;
        private SerializedProperty m_scentLayerProperty;
#endregion
#region Hear
        private SerializedProperty m_hearSensitivityProperty;
#endregion
        GUIContent m_customIconContent;
        protected virtual void OnEnable()
        {
            m_targetSenses = (Senses)target;
            m_ignoreLayersProperty = serializedObject.FindProperty("IgnoreLayers");
            m_seeLayersSolutionProperty = serializedObject.FindProperty("m_seeLayersSolution");
            m_seeTargetLayersProperty = serializedObject.FindProperty("m_seeTargetLayers");
            m_seeBlockingLayersProperty = serializedObject.FindProperty("m_seeBlockingLayers");
            m_initialColliderBufferSizeProperty = serializedObject.FindProperty("m_initialColliderBufferSize");
            m_scentLayerProperty = serializedObject.FindProperty("ScentLayer");
            m_hearSensitivityProperty = serializedObject.FindProperty("Hear_Sensitivity");
            m_requiredFactionsProperty = serializedObject.FindProperty("RequiredFactions");
            m_previousEyes = m_targetSenses.Eyes;
            m_previousCentralVision_Angle = m_targetSenses.Central_VisionAngle;
            m_previousCentralVision_Radius = m_targetSenses.Central_VisionRadius;
            m_previousUsePeripheralFOV = m_targetSenses.Use_PeripheralFOV;
            m_previousPeripheralVision_Angle = m_targetSenses.Peripheral_VisionAngle;
            m_previousPeripheralVision_Radius = m_targetSenses.Peripheral_VisionRadius;
            m_previousDisplayFOV = m_targetSenses.Use_DisplayFOV;
            m_previousDebugDrawRay = m_targetSenses.Use_DebugDrawRay;
            m_previousIgnoreRotation_X = m_targetSenses.Use_IgnoreRotationX;
            m_previousRayCastInFovOnly = m_targetSenses.Use_RayCastInFovOnly;
            m_previousAbleToReset_Awarness = m_targetSenses.Use_Forgeting;
            m_previousForgetingRate = m_targetSenses.Forgeting_Rate;
            m_previousHear_Sensitivity = m_targetSenses.Hear_Sensitivity;
            m_previous_WillUseSee = m_targetSenses.Use_See;
            m_previous_WillUseHear = m_targetSenses.Use_Hear;
            m_previous_WillUse_Smell = m_targetSenses.Use_Smell;
            m_previousOverrrideRotation = m_targetSenses.Use_OffsetRotation;
            m_previousRequiredRotation = m_targetSenses.Offset_Rotation;
            m_previousOverrridePosition = m_targetSenses.Use_OffsetPosition;
            m_previousRequiredPositionOffset = m_targetSenses.Offset_Position;
            m_previousUse_IgnoreOwnFaction = m_targetSenses.Use_IgnoreOwnFaction;
            m_previousPerfectLineOfViewCheck = m_targetSenses.Use_PerfectLineOfViewCheck;
            m_previousSmell_Radius = m_targetSenses.Smell_Radius;
            m_previousWillUseSmellCustomRefreshRate = m_targetSenses.Use_CustomRefreshRateSmell;
            m_previousCooldown_Smell_TimeAmount = m_targetSenses.Cooldown_SmellTimeAmount;
            m_previousScentLayer_intValue = m_targetSenses.ScentLayer;
            m_previousDebug_Detected = m_targetSenses.Debug_Detected;
            m_previousRequiredAwarness = m_targetSenses.RequiredAwarness;
            m_previous_RequiredFactions_intValue = m_targetSenses.RequiredFactions;
            m_selectedFactionNameIndicesCoppy = new List<int>(m_targetSenses.SelectedFactionNameIndices);
            {
                m_GUIConetent[0] = new GUIContent("See Properties", "Click to show or hide all See Sensor settings.");
                m_GUIConetent[1] = new GUIContent("X", "Disable the See Sensor.");
                m_GUIConetent[2] = new GUIContent("Eyes Transform", "The transform used as the AI's eyes. Its position is used for vision checks and line-of-sight raycasts. " + "If left empty, the GameObject’s transform is used.");
                m_GUIConetent[3] = new GUIContent("Central Angle", "The main forward-facing vision cone angle (in degrees).");
                m_GUIConetent[4] = new GUIContent("Central Radius", "Maximum distance of the central vision zone, where the AI sees most clearly.");
                m_GUIConetent[5] = new GUIContent("Use Peripheral FOV", "Enables a wider secondary field of view for weaker edge vision.");
                m_GUIConetent[6] = new GUIContent("└─ Peripheral Angle", "Side vision angle for the peripheral zone.");
                m_GUIConetent[7] = new GUIContent("└─ Peripheral Radius", "Detection range of the peripheral vision.");
                m_GUIConetent[8] = new GUIContent("Display FOV", "Shows the vision cones in the Scene view for easy debugging.");
                m_GUIConetent[9] = new GUIContent("└─ Ignore Layers", "Legacy only: excluded layers are ignored both when collecting targets and when checking line of sight. Example: ignore Decorations to see through decorative colliders. Do not ignore Characters if you want to detect them, or Walls if walls should block sight. In Fixer33 Solution this mask is preserved but unused by See.");
                m_GUIConetent[10] = new GUIContent("Ignore Rotation X", "Keeps the vision parallel to the ground, ignoring vertical tilt. Useful when characters tilt up/down in animations.");
                m_GUIConetent[11] = new GUIContent("RayCast In FOV Only", "Restricts line-of-sight raycasts to targets physically inside the 2D field-of-view triangle. " + "Objects far above or below the view cone are ignored.");
                m_GUIConetent[12] = new GUIContent("Debug RayCasts", "Draws debug lines for line-of-sight checks. Green = no hit, Red = the ray hit something.");
                m_GUIConetent[13] = new GUIContent("Custom Refresh Rate", "Allows the See Sensor to update at a custom interval instead of every frame.");
                m_GUIConetent[14] = new GUIContent("Refresh Time [s]", "Time in seconds between See Sensor updates when using a custom refresh rate.");
                m_GUIConetent[15] = new GUIContent("Override Eyes Position", "Applies a positional offset to the eye transform (or the GameObject) when enabled.");
                m_GUIConetent[16] = new GUIContent("└─ Offset Position", "A Vector3 position offset applied when Override Eyes Position is enabled.");
                m_GUIConetent[17] = new GUIContent("Override Eyes Rotation", "Applies a rotation override to the eyes instead of using the GameObject's rotation.");
                m_GUIConetent[18] = new GUIContent("Hear Properties", "Click to show or hide all Hear Sensor settings.");
                m_GUIConetent[19] = new GUIContent("X", "Disable the Hear Sensor.");
                m_GUIConetent[20] = new GUIContent("Hear Sensitivity", "How loud a sound must be before the AI detects it. Higher sensitivity detects quieter noises.");
                m_GUIConetent[21] = new GUIContent("Able To Forget", "If enabled, awareness decreases over time when the target is no longer detected.");
                m_GUIConetent[22] = new GUIContent("Forget Cooldown [s]", "How often awareness decreases during forgetting.");
                m_GUIConetent[23] = new GUIContent("└─ Forget Rate", "How much awareness is reduced every cooldown tick.");
                m_GUIConetent[24] = new GUIContent("Add Next Sense", "Add another Sense (See, Hear, or Smell) to this component.");
                m_GUIConetent[25] = new GUIContent("Ignore Own Faction", "Prevents detection of actors with the same faction. Useful for teams and allies.");
                m_GUIConetent[26] = new GUIContent("Requires Perfect LOS", "If enabled, characters with TargetSenses can block each other’s view. If disabled, the AI can see through actors in a crowd so overlapping characters never hide what’s behind them. Obstacles still block or reduce LOS normally.");
                m_GUIConetent[27] = new GUIContent("Smell Properties", "Click to show or hide all Smell Sensor settings.");
                m_GUIConetent[28] = new GUIContent("Smell Radius", "Maximum distance at which scent nodes can be detected.");
                m_GUIConetent[29] = new GUIContent("Custom Refresh Rate", "Allows the Smell Sensor to update at a custom interval instead of every frame.");
                m_GUIConetent[30] = new GUIContent("Refresh Time [s]", "Time in seconds between Smell Sensor updates when custom refresh is enabled.");
                m_GUIConetent[31] = new GUIContent("Scent Layer", "Layers containing scent nodes the AI is allowed to detect.");
                m_GUIConetent[32] = new GUIContent("Debug", "Debug options for visualizing detected targets and sensor behavior.");
                m_GUIConetent[33] = new GUIContent("Debug Detected", "Highlights which targets are currently detected.");
                m_GUIConetent[34] = new GUIContent("└─ Required Awareness", "Minimum awareness needed before a target counts as detected in debug view.");
                m_GUIConetent[35] = new GUIContent("└─ Required Factions", "Only show debug detections for selected factions.");
                m_GUIConetent[36] = new GUIContent("Brain", "Settings related to awareness, memory, and forgetting behavior.");
                m_GUIConetent[37] = new GUIContent("└─ Offset Rotation", "Rotation offset applied to the eyes when Override Eyes Rotation is enabled.");
                m_GUIConetent[38] = new GUIContent("Layers Solution", "Legacy Layers Solution keeps the existing Ignore Layers behavior and is the default for existing scenes and prefabs. Fixer33 Solution collects candidates only from Targets Layers and checks sight against Targets Layers plus Blockers Layers. Example: Characters = targets, Walls = blockers, Decorations = neither. Narrow target layers reduce candidate processing; they do not limit the number of targets. Switching modes preserves both configurations. Fixer33 masks initially include Everything: configure them for your scene.");
                m_GUIConetent[39] = new GUIContent("└─ Targets Layers", "Fixer33 only: layers of colliders belonging to detectable TargetSenses. Example: put player and enemies on Characters and select Characters here; keep Walls out of this mask. For RigCollider characters, assign the relevant child colliders to the selected layers too. Selecting a layer does not turn an ordinary object into a TargetSenses. Nothing finds no new visual targets. Target layers also participate in sight raycasts so the target can be hit and Requires Perfect LOS can still make characters occlude one another.");
                m_GUIConetent[40] = new GUIContent("└─ Blockers Layers", "Fixer33 only: additional layers that can obstruct or reduce sight. Example: select Walls and Doors; omit Decorations if decorative colliders should be transparent to vision. An ordinary collider blocks; an Obstacle keeps its Range Decrease Factor behavior. Targets Layers are always included in sight raycasts, even when this mask is Nothing. Requires Perfect LOS still controls whether other TargetSenses block the view. These masks affect See only; hearing and Scent Layer are unchanged.");
                m_GUIConetent[41] = new GUIContent("Initial Collider Buffer", "Initial collider capacity allocated when Senses initializes in Play Mode, shared by See and Smell. Example: for about 80 nearby colliders, choose 128 to avoid early growth. Count colliders, not characters: one rig may use many slots. A full buffer doubles and repeats the query in the same update until all results fit, then keeps that capacity. This is not a detection limit; increasing it uses more memory per Senses. Changing this value after initialization applies on the next initialization.");
            }
        }

        public override void OnInspectorGUI()
        {
            m_needTo_SeetEditorDirty = false;
            EditorGUILayout.Space(7);
            serializedObject.Update();
            InspectorPerformanceUtility.SynchronizeIndices(m_selectedFactionNameIndicesCoppy, m_targetSenses.SelectedFactionNameIndices);
            DrawComponents();
            if (serializedObject != null && serializedObject.targetObject != null)
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (m_needTo_SeetEditorDirty)
            {
                EditorUtility.SetDirty(m_targetSenses);
            }
        }

        protected virtual void DrawComponents()
        {
            //EditorGUI.indentLevel++;
            if (m_targetSenses.Use_See)
            {
                EditorGUILayout.BeginHorizontal();
                draw_see_propertiesHeader();
                EditorGUILayout.EndHorizontal();
                draw_see_details();
                EditorGUILayout.Space(1);
            }

            if (m_targetSenses.Use_Hear)
            {
                EditorGUILayout.BeginHorizontal();
                draw_hear_propertiesHeader();
                EditorGUILayout.EndHorizontal();
                draw_hear_details();
                EditorGUILayout.Space(1);
            }

            if (m_targetSenses.Use_Smell)
            {
                EditorGUILayout.BeginHorizontal();
                draw_smell_propertiesHeader();
                EditorGUILayout.EndHorizontal();
                draw_smell_details();
                EditorGUILayout.Space(1);
            }

            if (!m_targetSenses.Use_See || !m_targetSenses.Use_Hear || !m_targetSenses.Use_Smell)
            {
                if (GUILayout.Button(m_GUIConetent[24]))
                {
                    // Create and display a popup menu
                    GenericMenu _menu = new GenericMenu();
                    if (!m_targetSenses.Use_See)
                    {
                        _menu.AddItem(new GUIContent("SEE"), false, enable_see);
                    }

                    if (!m_targetSenses.Use_Hear)
                    {
                        _menu.AddItem(new GUIContent("HEAR"), false, enable_hear);
                    }

                    if (!m_targetSenses.Use_Smell)
                    {
                        _menu.AddItem(new GUIContent("SMELL"), false, enable_smell);
                    }

                    _menu.ShowAsContext();
                }

                EditorGUILayout.Space(1);
            }

            if (m_targetSenses.Use_See || m_targetSenses.Use_Hear || m_targetSenses.Use_Smell)
            {
                EditorGUILayout.BeginHorizontal();
                draw_brain_propertiesHeader();
                EditorGUILayout.EndHorizontal();
                draw_brain_details();
                EditorGUILayout.Space(1);
            }

            if (m_previous_WillUseSee != m_targetSenses.Use_See)
            {
                m_previous_WillUseSee = m_targetSenses.Use_See;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_previous_WillUseHear != m_targetSenses.Use_Hear)
            {
                m_previous_WillUseHear = m_targetSenses.Use_Hear;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_previous_WillUse_Smell != m_targetSenses.Use_Smell)
            {
                m_previous_WillUse_Smell = m_targetSenses.Use_Smell;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            draw_settings_propertiesHeader();
            EditorGUILayout.EndHorizontal();
            draw_settings_details();
            EditorGUILayout.Space(1);
            draw_HelpFoldout();
            InspectorHelperDrawHelpWindow.Draw_HelpWindow(m_targetSenses.HelpPropertiesFoldout, m_helpGithub, m_helpYoutube);
        }

        void enable_see() => set_See_State(true);
        void set_See_State(bool _state)
        {
            if (m_targetSenses.Use_See == _state)
            {
                return;
            }

            InspectorPerformanceUtility.RecordChange(m_targetSenses);
            m_targetSenses.Set_UseSee(_state);
            InspectorPerformanceUtility.CompleteChange(m_targetSenses);
        }

        void enable_hear() => set_Hear_State(true);
        void set_Hear_State(bool _state)
        {
            if (m_targetSenses.Use_Hear == _state)
            {
                return;
            }

            InspectorPerformanceUtility.RecordChange(m_targetSenses);
            m_targetSenses.Set_UseHear(_state);
            InspectorPerformanceUtility.CompleteChange(m_targetSenses);
        }

        void enable_smell() => set_Smell_State(true);
        void set_Smell_State(bool _state)
        {
            if (m_targetSenses.Use_Smell == _state)
            {
                return;
            }

            InspectorPerformanceUtility.RecordChange(m_targetSenses);
            m_targetSenses.Set_UseSmell(_state);
            InspectorPerformanceUtility.CompleteChange(m_targetSenses);
        }

        void draw_see_propertiesHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconSee 1");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconSee 1", "See Properties");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            // Check if the header is clicked
            if (GUILayout.Button(m_GUIConetent[0], EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Toggle_SeePropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            if (GUILayout.Button(m_GUIConetent[1], GUILayout.Width(20)))
            {
                set_See_State(false);
            }

            EditorGUILayout.EndHorizontal();
        }

        void draw_see_details()
        {
            if (!m_targetSenses.SeePropertiesFoldout)
            {
                return;
            }

            //EditorGUILayout.Space();
            //EditorGUI.indentLevel++;
            // Start a vertical layout group with a box around it
            EditorGUILayout.BeginVertical(CustomEditorStyles.NoMarginWindow);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[2], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = (Transform)EditorGUILayout.ObjectField(m_targetSenses.Eyes, typeof(Transform), true, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_Eyes(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousEyes != m_targetSenses.Eyes)
            {
                m_previousEyes = m_targetSenses.Eyes;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.Eyes == null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                EditorGUILayout.HelpBox("No Eye Transform selectd, so will use GameObject.transform instead", MessageType.Warning, true);
                EditorGUILayout.EndHorizontal();
            }

            // Angle Central Vision
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[3], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.FloatField(m_targetSenses.Central_VisionAngle, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_CentralVisionAngle(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousCentralVision_Angle != m_targetSenses.Central_VisionAngle)
            {
                m_previousCentralVision_Angle = m_targetSenses.Central_VisionAngle;
                m_needTo_SeetEditorDirty = true;
            }

            // Radius Central Vision
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[4], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.FloatField(m_targetSenses.Central_VisionRadius, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_CentralVisionRadius(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousCentralVision_Radius != m_targetSenses.Central_VisionRadius)
            {
                m_previousCentralVision_Radius = m_targetSenses.Central_VisionRadius;
                m_needTo_SeetEditorDirty = true;
            }

            // Use Peripheral FOV
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[5], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_PeripheralFOV, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UsePeripheralFOV(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousUsePeripheralFOV != m_targetSenses.Use_PeripheralFOV)
            {
                m_previousUsePeripheralFOV = m_targetSenses.Use_PeripheralFOV;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.Use_PeripheralFOV)
            {
                //EditorGUI.indentLevel++;
                // Angle Peripheral Vision
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[6], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.FloatField(m_targetSenses.Peripheral_VisionAngle, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_targetSenses);
                        m_targetSenses.Set_PeripheralVisionAngle(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previousPeripheralVision_Angle != m_targetSenses.Peripheral_VisionAngle)
                {
                    m_previousPeripheralVision_Angle = m_targetSenses.Peripheral_VisionAngle;
                    m_needTo_SeetEditorDirty = true;
                }

                // Radius Peripheral Vision
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[7], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.FloatField(m_targetSenses.Peripheral_VisionRadius, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_targetSenses);
                        m_targetSenses.Set_PeripheralVision_Radius(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previousPeripheralVision_Radius != m_targetSenses.Peripheral_VisionRadius)
                {
                    m_previousPeripheralVision_Radius = m_targetSenses.Peripheral_VisionRadius;
                    m_needTo_SeetEditorDirty = true;
                }
            //EditorGUI.indentLevel--;
            }

            // Display FOV
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[8], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_DisplayFOV, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseDisplayFOV(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousDisplayFOV != m_targetSenses.Use_DisplayFOV)
            {
                m_previousDisplayFOV = m_targetSenses.Use_DisplayFOV;
                m_needTo_SeetEditorDirty = true;
            }

            draw_see_layerSettings();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[10], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_IgnoreRotationX, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseIgnoreRotationX(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousIgnoreRotation_X != m_targetSenses.Use_IgnoreRotationX)
            {
                m_previousIgnoreRotation_X = m_targetSenses.Use_IgnoreRotationX;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[11], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_RayCastInFovOnly, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseRayCastInFovOnly(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousRayCastInFovOnly != m_targetSenses.Use_RayCastInFovOnly)
            {
                m_previousRayCastInFovOnly = m_targetSenses.Use_RayCastInFovOnly;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[12], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_DebugDrawRay, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_DebugDrawRay(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousDebugDrawRay != m_targetSenses.Use_DebugDrawRay)
            {
                m_previousDebugDrawRay = m_targetSenses.Use_DebugDrawRay;
                m_needTo_SeetEditorDirty = true;
            }

            /*
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[13], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_CustomRefreshRateSee, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseCustomRefreshRateSee(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed
            EditorGUILayout.EndHorizontal();
            if (m_previousWillUseCustomRefreshRate != m_targetSenses.Use_CustomRefreshRateSee)
            {
                m_previousWillUseCustomRefreshRate = m_targetSenses.Use_CustomRefreshRateSee;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.Use_CustomRefreshRateSee)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[14], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_targetSenses.Cooldown_SeeTimeAmount, .016f, 1f, GUILayout.Width(EditorGUIUtility.labelWidth + 0), GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_targetSenses);
                        m_targetSenses.Set_CooldownSeeTimeAmount(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                if (m_previousCooldown_See_TimeAmount != m_targetSenses.Cooldown_SeeTimeAmount)
                {
                    m_previousCooldown_See_TimeAmount = m_targetSenses.Cooldown_SeeTimeAmount;
                    m_needTo_SeetEditorDirty = true;
                }
            }
            */
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[15], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_OffsetPosition, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseOffsetPosition(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousOverrridePosition != m_targetSenses.Use_OffsetPosition)
            {
                m_previousOverrridePosition = m_targetSenses.Use_OffsetPosition;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.Use_OffsetPosition)
            {
                //EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Vector3Field(m_GUIConetent[16], m_targetSenses.Offset_Position, GUILayout.Width(EditorGUIUtility.labelWidth + 0), GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_targetSenses);
                        m_targetSenses.Set_OffsetPosition(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    }
                }

                EditorGUILayout.EndHorizontal();
                //EditorGUI.indentLevel--;
                if (m_previousRequiredPositionOffset != m_targetSenses.Offset_Position)
                {
                    m_previousRequiredPositionOffset = m_targetSenses.Offset_Position;
                    m_needTo_SeetEditorDirty = true;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[17], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_OffsetRotation, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseOffsetRotation(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousOverrrideRotation != m_targetSenses.Use_OffsetRotation)
            {
                m_previousOverrrideRotation = m_targetSenses.Use_OffsetRotation;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.Use_OffsetRotation)
            {
                //EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Vector3Field(m_GUIConetent[37], m_targetSenses.Offset_Rotation, GUILayout.Width(EditorGUIUtility.labelWidth + 0), GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_targetSenses);
                        m_targetSenses.Set_OffsetRotation(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    }
                }

                EditorGUILayout.EndHorizontal();
                //EditorGUI.indentLevel--;
                if (m_previousRequiredRotation != m_targetSenses.Offset_Rotation)
                {
                    m_previousRequiredRotation = m_targetSenses.Offset_Rotation;
                    m_needTo_SeetEditorDirty = true;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[26], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_PerfectLineOfViewCheck, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UsePerfectLineOfViewCheck(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousPerfectLineOfViewCheck != m_targetSenses.Use_PerfectLineOfViewCheck)
            {
                m_previousPerfectLineOfViewCheck = m_targetSenses.Use_PerfectLineOfViewCheck;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.EndVertical();
            //EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        void draw_hear_propertiesHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconHear3");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconHear3", "Hear Properties");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            // Check if the header is clicked
            if (GUILayout.Button(m_GUIConetent[18], EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Toggle_HearPropertiesFoldou();
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            if (GUILayout.Button(m_GUIConetent[19], GUILayout.Width(20)))
            {
                set_Hear_State(false);
            }

            EditorGUILayout.EndHorizontal();
        }

        void draw_hear_details()
        {
            if (!m_targetSenses.HearPropertiesFoldout)
            {
                return; // Don't draw the properties if the foldout is closed
            }

            // EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(CustomEditorStyles.NoMarginWindow); // Start a vertical layout group with a box around it
            // Display the enum dropdown for Hear_Sensitivity with the same width settings
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[20], GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.PropertyField(m_hearSensitivityProperty, GUIContent.none, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            if (m_previousHear_Sensitivity != m_targetSenses.Hear_Sensitivity)
            {
                m_previousHear_Sensitivity = m_targetSenses.Hear_Sensitivity;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.EndVertical(); // End the vertical layout group with a box around it
            //EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        void draw_smell_propertiesHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconSmell");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconSmell", "Smell Properties");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            // Check if the header is clicked
            if (GUILayout.Button(m_GUIConetent[27], EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Toggle_SmellPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            if (GUILayout.Button(m_GUIConetent[19], GUILayout.Width(20)))
            {
                set_Smell_State(false);
            }

            EditorGUILayout.EndHorizontal();
        }

        void draw_smell_details()
        {
            if (!m_targetSenses.SmellPropertiesFoldout)
            {
                return; // Don't draw the properties if the foldout is closed
            }

            //EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(CustomEditorStyles.NoMarginWindow); // Start a vertical layout group with a box around it
            // Display the enum dropdown for Hear_Sensitivity with the same width settings
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[28], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Slider(m_targetSenses.Smell_Radius, 0f, 5f, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_SmellRadius(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            EditorGUILayout.EndHorizontal();
            if (m_previousSmell_Radius != m_targetSenses.Smell_Radius)
            {
                m_previousSmell_Radius = m_targetSenses.Smell_Radius;
                m_needTo_SeetEditorDirty = true;
            }

            /*
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[29], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_CustomRefreshRateSmell, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseCustomRefreshRateSmell(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed
            EditorGUILayout.EndHorizontal();
            if (m_previousWillUseSmellCustomRefreshRate != m_targetSenses.Use_CustomRefreshRateSmell)
            {
                m_previousWillUseSmellCustomRefreshRate = m_targetSenses.Use_CustomRefreshRateSmell;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.Use_CustomRefreshRateSmell)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[30], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_targetSenses.Cooldown_SmellTimeAmount, .016f, 1f, GUILayout.Width(EditorGUIUtility.labelWidth + 0), GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_targetSenses);
                        m_targetSenses.Set_CooldownSmellTimeAmount(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                if (m_previousCooldown_Smell_TimeAmount != m_targetSenses.Cooldown_SmellTimeAmount)
                {
                    m_previousCooldown_Smell_TimeAmount = m_targetSenses.Cooldown_SmellTimeAmount;
                    m_needTo_SeetEditorDirty = true;
                }
            }
            */
            // Uses dedicated layer for scents
            // Scent Layer Mask
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[31], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            m_scentLayerProperty.intValue = LayerMaskField(m_scentLayerProperty.intValue);
            EditorGUILayout.EndHorizontal();
            if (m_previousScentLayer_intValue != m_scentLayerProperty.intValue)
            {
                m_previousScentLayer_intValue = m_scentLayerProperty.intValue;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.EndVertical(); // End the vertical layout group with a box around it
            //EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        void draw_brain_propertiesHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("IconBrain");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("IconBrain", "Brain");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            // Check if the header is clicked
            if (GUILayout.Button(m_GUIConetent[36], EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Toggle_BrainPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void draw_brain_details()
        {
            if (!m_targetSenses.BrainPropertiesFoldout)
            {
                return;
            }

            EditorGUILayout.Space();
            // Start a vertical layout group with a box around it
            EditorGUILayout.BeginVertical(CustomEditorStyles.NoMarginWindow);
            if (m_targetSenses.Use_See || m_targetSenses.Use_Smell)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[41], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                EditorGUILayout.PropertyField(m_initialColliderBufferSizeProperty, GUIContent.none, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[21], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_Forgeting, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseForgeting(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousAbleToReset_Awarness != m_targetSenses.Use_Forgeting)
            {
                m_previousAbleToReset_Awarness = m_targetSenses.Use_Forgeting;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.Use_Forgeting)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[23], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_targetSenses.Forgeting_Rate, .01f, 10f, GUILayout.Width(EditorGUIUtility.labelWidth + 0), GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_targetSenses);
                        m_targetSenses.Set_ForgetingRate(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previousForgetingRate != m_targetSenses.Forgeting_Rate)
                {
                    m_previousForgetingRate = m_targetSenses.Forgeting_Rate;
                    m_needTo_SeetEditorDirty = true;
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[25], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Use_IgnoreOwnFaction, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseIgnoreOwnFaction(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousUse_IgnoreOwnFaction != m_targetSenses.Use_IgnoreOwnFaction)
            {
                m_previousUse_IgnoreOwnFaction = m_targetSenses.Use_IgnoreOwnFaction;
                m_needTo_SeetEditorDirty = true;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        void draw_settings_propertiesHeader()
        {
            Texture2D _icon = LoadIconForEditorHelpWindow.LoadIcon("Settings");
            GUIContent _customIconContent = LoadIconForEditorHelpWindow.LoadContent("Settings", "Debug");
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(_customIconContent, GUILayout.Width(20), GUILayout.Height(20));
            // Check if the header is clicked
            if (GUILayout.Button(m_GUIConetent[32], EditorStyles.boldLabel))
            {
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Toggle_SettingsPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void draw_settings_details()
        {
            if (!m_targetSenses.SettingsPropertiesFoldout)
            {
                return;
            }

            EditorGUILayout.Space();
            //EditorGUI.indentLevel++;
            // Start a vertical layout group with a box around it
            EditorGUILayout.BeginVertical(CustomEditorStyles.NoMarginWindow);
            // Use Debug Detected Target Senses
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[33], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            {
                EditorGUI.BeginChangeCheck();
                var _newValue = EditorGUILayout.Toggle(m_targetSenses.Debug_Detected, GUILayout.Width(EditorGUIUtility.labelWidth + 16), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_UseDebug_Detected(_newValue);
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            } // Adjust the width as needed

            EditorGUILayout.EndHorizontal();
            if (m_previousDebug_Detected != m_targetSenses.Debug_Detected)
            {
                m_previousDebug_Detected = m_targetSenses.Debug_Detected;
                m_needTo_SeetEditorDirty = true;
            }

            if (m_targetSenses.Debug_Detected)
            {
                // Required Awarness
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[34], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                {
                    EditorGUI.BeginChangeCheck();
                    var _newValue = EditorGUILayout.Slider(m_targetSenses.RequiredAwarness, 0.01f, 100f, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        InspectorPerformanceUtility.RecordChange(m_targetSenses);
                        m_targetSenses.Set_RequiredAwarness(_newValue);
                        InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (m_previousRequiredAwarness != m_targetSenses.RequiredAwarness)
                {
                    m_previousRequiredAwarness = m_targetSenses.RequiredAwarness;
                    m_needTo_SeetEditorDirty = true;
                }

                // Required Factions
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_GUIConetent[35], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                string[] _factionNames = EditorFactionHelpers.GetFactionNames();
                int _currentMask = 0;
                // Convert stored list of selected indices to bitmask
                foreach (int _index in m_selectedFactionNameIndicesCoppy)
                {
                    _currentMask |= (1 << _index);
                }

                int _newMask = EditorGUILayout.MaskField(_currentMask, _factionNames, GUILayout.MaxWidth(float.MaxValue), GUILayout.ExpandWidth(true));
                if (_newMask != _currentMask)
                {
                    m_selectedFactionNameIndicesCoppy.Clear();
                    // Convert bitmask back to list of indices
                    for (int _i = 0; _i < _factionNames.Length; _i++)
                    {
                        if ((_newMask & (1 << _i)) != 0)
                        {
                            m_selectedFactionNameIndicesCoppy.Add(_i);
                        }
                    }

                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Set_SelectedFactionNames(new List<int>(m_selectedFactionNameIndicesCoppy));
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                    m_needTo_SeetEditorDirty = true;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
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
                    InspectorPerformanceUtility.RecordChange(m_targetSenses);
                    m_targetSenses.Toggle_HelpPropertiesFoldout();
                    InspectorPerformanceUtility.CompleteChange(m_targetSenses);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void draw_see_layerSettings()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_GUIConetent[38], GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            EditorGUILayout.PropertyField(m_seeLayersSolutionProperty, GUIContent.none, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            if (m_seeLayersSolutionProperty.intValue == (int)SeeLayersSolution.Fixer33)
            {
                draw_see_layerMask(m_seeTargetLayersProperty, m_GUIConetent[39]);
                draw_see_layerMask(m_seeBlockingLayersProperty, m_GUIConetent[40]);
            }
            else
            {
                draw_see_layerMask(m_ignoreLayersProperty, m_GUIConetent[9]);
            }
        }

        void draw_see_layerMask(SerializedProperty _property, GUIContent _label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_label, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            EditorGUILayout.PropertyField(_property, GUIContent.none, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }

        static LayerMask LayerMaskField(LayerMask _layerMask)
        {
            return InspectorPerformanceUtility.LayerMaskField(_layerMask);
        }

        protected virtual void OnDisable()
        {
            // Apply modified properties back to the target object when disabling the inspector
            if (serializedObject != null && serializedObject.targetObject != null)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif

