#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Senses2026
{
    public static class InspectorHelperDrawHelpWindow
    {
        public static void Draw_HelpWindow(bool _state, HelpLink[] _gitHubLinks, HelpLink[] _youtubeLinks, string _discordLink = "https://discord.gg/EV3eZYsdNW")
        {
            EditorGUILayout.Space();
            if (_state)
            {
                EditorGUILayout.BeginHorizontal();
                DrawLinks("Github", _gitHubLinks, "View GitHub Documentation");
                DrawLinks("Youtube", _youtubeLinks, "Watch YouTube Tutorials");
                drawIconLink("Discord", _discordLink, "Join the Discord Community");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }

        private static void DrawLinks(string _iconName, HelpLink[] _links, string _fallbackTooltip)
        {
            if (_links == null)
            {
                return;
            }

            foreach (HelpLink _link in _links)
            {
                drawIconLink(_iconName, _link.url, string.IsNullOrEmpty(_link.tooltip) ? _fallbackTooltip : _link.tooltip);
                GUILayout.Space(2);
            }
        }

        public static void drawIconLink(string _iconName, string _url, string _tooltip)
        {
            GUIContent _content = LoadIconForEditorHelpWindow.LoadContent(_iconName, _tooltip);
            Texture _icon = _content.image;
            if (_icon == null)
            {
                return;
            }

            Rect _rect = GUILayoutUtility.GetRect(_icon.width, _icon.height, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(_rect, _icon, ScaleMode.ScaleToFit, true);
            if (GUI.Button(_rect, _content, GUIStyle.none))
            {
                Application.OpenURL(_url);
            }

            EditorGUIUtility.AddCursorRect(_rect, MouseCursor.Link);
        }
    }

    [InitializeOnLoad]
    public static class LoadIconForEditorHelpWindow
    {
        private static readonly Dictionary<string, Texture2D> m_iconCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<(string, string), GUIContent> m_contentCache = new Dictionary<(string, string), GUIContent>();
        private static readonly HashSet<string> m_reportedMissing = new HashSet<string>();
        private static string m_iconFolder;
        static LoadIconForEditorHelpWindow()
        {
            EditorApplication.projectChanged += Invalidate;
        }

        public static void Invalidate()
        {
            m_iconCache.Clear();
            m_contentCache.Clear();
            m_iconFolder = null;
        }

        public static GUIContent LoadContent(string _iconName, string _tooltip = "")
        {
            (string, string) _key = (_iconName, _tooltip);
            if (!m_contentCache.TryGetValue(_key, out GUIContent _content))
            {
                _content = new GUIContent(LoadIcon(_iconName), _tooltip);
                m_contentCache.Add(_key, _content);
            }

            return _content;
        }

        public static Texture2D LoadIcon(string _iconName)
        {
            if (m_iconCache.TryGetValue(_iconName, out Texture2D _icon))
            {
                return _icon;
            }

            if (m_iconFolder == null)
            {
                m_iconFolder = "Assets/CatBorg AI Framework - Mega Bundle/Senses Pack/Editor/Icons";
                if (!AssetDatabase.IsValidFolder(m_iconFolder))
                {
                    string[] _scripts = AssetDatabase.FindAssets("InspectorHelperDrawHelpWindow t:MonoScript");
                    foreach (string _guid in _scripts)
                    {
                        string _scriptPath = AssetDatabase.GUIDToAssetPath(_guid);
                        if (!string.Equals(Path.GetFileName(_scriptPath), "InspectorHelperDrawHelpWindow.cs", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        string _folder = Path.GetDirectoryName(_scriptPath).Replace('\\', '/') + "/Icons";
                        if (AssetDatabase.IsValidFolder(_folder))
                        {
                            m_iconFolder = _folder;
                            break;
                        }
                    }
                }
            }

            _icon = AssetDatabase.LoadAssetAtPath<Texture2D>(m_iconFolder + "/" + _iconName + ".png");
            if (_icon == null)
            {
                string[] _guids = AssetDatabase.FindAssets(_iconName + " t:Texture2D");
                foreach (string _guid in _guids)
                {
                    string _path = AssetDatabase.GUIDToAssetPath(_guid);
                    if (string.Equals(Path.GetFileName(_path), _iconName + ".png", StringComparison.OrdinalIgnoreCase))
                    {
                        _icon = AssetDatabase.LoadAssetAtPath<Texture2D>(_path);
                        break;
                    }
                }
            }

            m_iconCache[_iconName] = _icon;
            if (_icon == null && m_reportedMissing.Add(_iconName))
            {
                Debug.LogWarning("Senses editor icon not found: " + _iconName + ".png");
            }

            return _icon;
        }
    }

    [System.Serializable]
    public class HelpLink
    {
        public string tooltip;
        public string url;
        public HelpLink(string tooltip, string url)
        {
            this.tooltip = string.IsNullOrEmpty(tooltip) ? "View GitHub Documentation" : tooltip;
            this.url = url;
        }
    }
}
#endif

