#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Senses2026
{
    [InitializeOnLoad]
    public static class InspectorPerformanceUtility
    {
        private static string[] m_layerNames;
        private static int[] m_layerNumbers;
        static InspectorPerformanceUtility()
        {
            EditorApplication.projectChanged += InvalidateLayers;
            Undo.undoRedoPerformed += InvalidateLayers;
        }

        public static void InvalidateLayers()
        {
            m_layerNames = null;
            m_layerNumbers = null;
        }

        private static void EnsureLayers()
        {
            if (m_layerNames != null)
            {
                return;
            }

            List<string> _names = new List<string>();
            List<int> _numbers = new List<int>();
            for (int _index = 0; _index < 32; _index++)
            {
                string _name = LayerMask.LayerToName(_index);
                if (string.IsNullOrEmpty(_name))
                {
                    continue;
                }

                _names.Add(_name);
                _numbers.Add(_index);
            }

            m_layerNames = _names.ToArray();
            m_layerNumbers = _numbers.ToArray();
        }

        public static LayerMask LayerMaskField(LayerMask _mask)
        {
            EnsureLayers();
            int _displayMask = ToDisplayMask(_mask.value, m_layerNumbers);
            EditorGUI.BeginChangeCheck();
            int _newMask = EditorGUILayout.MaskField(_displayMask, m_layerNames);
            if (!EditorGUI.EndChangeCheck())
            {
                return _mask;
            }

            return FromDisplayMask(_newMask, m_layerNumbers);
        }

        public static int ToDisplayMask(int _mask, int[] _layerNumbers)
        {
            if (_mask == -1)
            {
                return -1;
            }

            int _result = 0;
            for (int _index = 0; _index < _layerNumbers.Length; _index++)
            {
                if ((_mask & (1 << _layerNumbers[_index])) != 0)
                {
                    _result |= 1 << _index;
                }
            }

            return _result;
        }

        public static int FromDisplayMask(int _mask, int[] _layerNumbers)
        {
            if (_mask == -1)
            {
                return -1;
            }

            int _result = 0;
            for (int _index = 0; _index < _layerNumbers.Length; _index++)
            {
                if ((_mask & (1 << _index)) != 0)
                {
                    _result |= 1 << _layerNumbers[_index];
                }
            }

            return _result;
        }

        public static void RecordChange(Object _target)
        {
            Undo.RecordObject(_target, "Change Senses settings");
        }

        public static void CompleteChange(Object _target)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(_target))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(_target);
            }

            EditorUtility.SetDirty(_target);
        }

        public static void SynchronizeIndices(List<int> _destination, List<int> _source)
        {
            if (ReferenceEquals(_destination, _source))
            {
                return;
            }

            bool _same = _destination.Count == _source.Count;
            for (int _index = 0; _same && _index < _source.Count; _index++)
            {
                _same = _destination[_index] == _source[_index];
            }

            if (_same)
            {
                return;
            }

            _destination.Clear();
            _destination.AddRange(_source);
        }
    }
}
#endif

