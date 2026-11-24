#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Senses2026
{
    [InitializeOnLoad]
    public static class EditorFactionHelpers
    {
        private static readonly string[] m_enumNames = Enum.GetNames(typeof(FactionNames));
        private static readonly string[] m_fallbackNames = CreateFallbackNames();
        private static readonly int[] m_enumValues = CreateEnumValues();
        private static readonly string[] m_displayNames = new string[m_enumNames.Length];
        public static void Invalidate()
        {
            EditorGizmoHelpers.InvalidateFactionSettings();
        }

        private static string[] CreateFallbackNames()
        {
            string[] _names = new string[m_enumNames.Length];
            for (int _index = 0; _index < _names.Length; _index++)
            {
                _names[_index] = _index == 0 ? "None" : "Faction " + _index;
            }

            return _names;
        }

        private static int[] CreateEnumValues()
        {
            Array _values = Enum.GetValues(typeof(FactionNames));
            int[] _result = new int[_values.Length];
            for (int _index = 0; _index < _values.Length; _index++)
            {
                _result[_index] = (int)_values.GetValue(_index);
            }

            return _result;
        }

        public static string[] GetFactionNames()
        {
            FactionSettings _settings = EditorGizmoHelpers.GetFactionSettings();
            if (_settings == null)
            {
                return m_enumNames;
            }
            string[] _source = _settings.factionDisplayNames;
            for (int _index = 0; _index < m_displayNames.Length; _index++)
            {
                string _name = _source != null && _index < _source.Length ? _source[_index] : null;
                m_displayNames[_index] = _index == 0 || string.IsNullOrEmpty(_name) ? m_fallbackNames[_index] : _name;
            }

            return m_displayNames;
        }

        public static FactionNames FactionMaskField(FactionNames _currentMask, GUIContent _content)
        {
            int _displayMask = 0;
            for (int _index = 0; _index < m_enumValues.Length; _index++)
            {
                if (((int)_currentMask & m_enumValues[_index]) != 0)
                {
                    _displayMask |= 1 << _index;
                }
            }

            _displayMask = EditorGUILayout.MaskField(_content, _displayMask, m_enumNames);
            int _result = 0;
            for (int _index = 0; _index < m_enumValues.Length; _index++)
            {
                if ((_displayMask & (1 << _index)) != 0)
                {
                    _result |= m_enumValues[_index];
                }
            }

            return (FactionNames)_result;
        }
    }
}
#endif

