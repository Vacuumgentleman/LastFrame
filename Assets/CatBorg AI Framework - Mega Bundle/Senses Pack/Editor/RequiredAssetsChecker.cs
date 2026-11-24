using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Senses2026BonusContent
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class RequiredAssetsChecker : EditorWindow
    {
        const string m_sceneNameKeyword = "Demo 14 - RedHanded (PineBased)";
        const string m_prefsKey = "Senses_RequiredAssetsCheck";

        // Required scripts / types and their corresponding Asset Store URLs
        static readonly Dictionary<string, string> m_requiredTypes = new()
        {
            { "StarterAssets.ThirdPersonController", "https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526" },
            { "PinePack.Executor", "https://assetstore.unity.com/packages/tools/behavior-ai/pine-pack-behavior-trees-296606" }
        };

        static string[] m_missingTypes;
        static bool m_shownThisSession;

        // GUI Styles & Icons
        GUIStyle m_titleStyle;
        GUIStyle m_headerStyle;
        GUIStyle m_footerStyle;
        GUIStyle m_linkStyle;
        Texture2D m_iconSuccess;
        Texture2D m_iconWarning;

        static RequiredAssetsChecker()
        {
            // Hook into scene opened event
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        static void OnSceneOpened(Scene _scene, OpenSceneMode _mode)
        {
            if (m_shownThisSession) return;
            if (!_scene.name.Contains(m_sceneNameKeyword)) return;

            m_missingTypes = FindMissingAssets();
            ShowWindow();
            m_shownThisSession = true;
        }

        public static void ShowWindow()
        {
            var _window = GetWindow<RequiredAssetsChecker>(true, "Required Assets Check", true);
            _window.minSize = new Vector2(460, 320);
            _window.maxSize = new Vector2(460, 320);
            _window.Show();
        }

        void OnEnable()
        {
            // Load built-in icons
            m_iconSuccess = EditorGUIUtility.IconContent("TestPassed").image as Texture2D;
            m_iconWarning = EditorGUIUtility.IconContent("console.warnicon").image as Texture2D;

            // Styles
            m_titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
            m_headerStyle = new GUIStyle(EditorStyles.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            m_footerStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 10 };
            m_linkStyle = new GUIStyle(EditorStyles.linkLabel) { alignment = TextAnchor.MiddleLeft, wordWrap = true };
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Senses + PinePack", m_titleStyle);
            GUILayout.Label("Demo Scene Requirements", m_headerStyle);
            GUILayout.Space(15);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Verifying required project assets and scripts...", EditorStyles.miniBoldLabel);
            GUILayout.Space(10);

            if (m_missingTypes == null || m_missingTypes.Length == 0)
            {
                DrawResultBox("✅ All required assets and scripts are present.\nYour demo scene is ready to use!", m_iconSuccess, new Color(0.55f, 0.85f, 0.55f));
            }
            else
            {
                DrawMissingBox();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Re-check", GUILayout.Width(100), GUILayout.Height(24)))
            {
                m_missingTypes = FindMissingAssets();
                Repaint();
            }

            if (GUILayout.Button("Close", GUILayout.Width(100), GUILayout.Height(24)))
            {
                Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Senses2026 Bonus Content", m_footerStyle);
            GUILayout.Space(5);
        }

        void DrawResultBox(string _message, Texture2D _icon, Color _tint)
        {
            Color _old = GUI.color;
            GUI.color = _tint;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = _old;

            GUILayout.BeginHorizontal();
            GUILayout.Label(_icon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Space(5);
            EditorGUILayout.LabelField(_message, EditorStyles.wordWrappedLabel);
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        void DrawMissingBox()
        {
            Color _old = GUI.color;
            GUI.color = new Color(1f, 0.8f, 0.4f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = _old;

            GUILayout.BeginHorizontal();
            GUILayout.Label(m_iconWarning, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Space(5);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("⚠ Missing required assets or scripts:", EditorStyles.boldLabel);

            foreach (var _type in m_missingTypes)
            {
                GUILayout.Space(3);
                EditorGUILayout.LabelField($"• {_type}", EditorStyles.wordWrappedLabel);

                if (m_requiredTypes.TryGetValue(_type, out string _url))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("Open Asset Store Page →", m_linkStyle))
                    {
                        Application.OpenURL(_url);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        static string[] FindMissingAssets()
        {
            var _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var _found = m_requiredTypes.Keys.Where(_typeName => _assemblies.Any(_a => _a.GetType(_typeName, false) != null)).ToArray();
            return m_requiredTypes.Keys.Except(_found).ToArray();
        }
    }
#endif
}
