#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Senses2026
{
    public static class EditorGizmoHelpers
    {
        private static SceneFactionManager m_factionManager;
        private static FactionSettings m_factionSettings;
        private static bool m_factionSettingsResolved;

        static EditorGizmoHelpers()
        {
            EditorApplication.hierarchyChanged += InvalidateFactionSettings;
            EditorApplication.projectChanged += InvalidateFactionSettings;
            Undo.undoRedoPerformed += InvalidateFactionSettings;
        }

        public static void InvalidateFactionSettings()
        {
            m_factionSettingsResolved = false;
            m_factionManager = null;
            m_factionSettings = null;
        }

        public static FactionSettings GetFactionSettings()
        {
            if (!m_factionSettingsResolved)
            {
                SceneFactionManager[] _managers = Resources.FindObjectsOfTypeAll<SceneFactionManager>();
                foreach (SceneFactionManager _manager in _managers)
                {
                    if (_manager.gameObject.scene.IsValid() && _manager.gameObject.scene.isLoaded && _manager.FactionSettings != null)
                    {
                        m_factionManager = _manager;
                        break;
                    }
                }
                m_factionSettings = Resources.Load<FactionSettings>("FactionSettings (SO)/DefaultFactionSettings");
                m_factionSettingsResolved = true;
            }
            return m_factionManager != null ? m_factionManager.FactionSettings : m_factionSettings;
        }

        private static GUIStyle m_factionLabelStyle;

        public static void DrawFactionLabel(Vector3 _position, FactionNames _faction)
        {
            FactionSettings _settings = GetFactionSettings();
            string _text = _settings != null ? _settings.GetFactionDisplayName((int)_faction) : _faction.ToString();
            if (m_factionLabelStyle == null)
            {
                m_factionLabelStyle = new GUIStyle(GUI.skin.label);
                m_factionLabelStyle.normal.textColor = Color.white;
                m_factionLabelStyle.normal.background = Texture2D.grayTexture;
                m_factionLabelStyle.fontStyle = FontStyle.Bold;
                m_factionLabelStyle.alignment = TextAnchor.MiddleCenter;
                m_factionLabelStyle.padding = new RectOffset(4, 4, 2, 2);
            }
            Handles.Label(_position, _text, m_factionLabelStyle);
        }
        public static float CalculateDetectionRadius(Component _owner, float _colliderScale)
        {
            if (!_owner.TryGetComponent(out TargetSenses _target))
            {
                return 0f;
            }

            if (_target.TargetType == TargetType.RigCollider)
            {
                if (!_owner.TryGetComponent(out RigCollider _rig) || _rig.Colliders == null)
                {
                    return 0f;
                }

                Bounds _bounds = new Bounds(_owner.transform.position, Vector3.zero);
                foreach (Collider _collider in _rig.Colliders)
                {
                    if (_collider != null)
                    {
                        _bounds.Encapsulate(_collider.bounds);
                    }
                }

                return _bounds.extents.x;
            }

            if (_colliderScale != 1f && _owner.TryGetComponent(out CharacterController _controller))
            {
                return _controller.radius * _colliderScale;
            }

            return _owner.TryGetComponent(out Collider _single) ? _single.bounds.extents.x * _colliderScale : 0f;
        }

        private static GUIStyle m_labelStyle;
        private static readonly GUIContent m_labelContent = new GUIContent();
        public static void DrawRayWithLabel(Senses _senses, TargetSenses _targetSenses, float _awarness, Color _color, float _gapStart = 0.4f, float _gapEnd = 0.6f, float _labelHeight = 0.1f)
        {
            // Avoid drawing in Game view
            // if (SceneView.currentDrawingSceneView == null)
            // return;
            if (_senses == null || _senses.Eyes == null || _targetSenses == null)
            {
                return;
            }

            Vector3 _start = _senses.Eyes.position;
            Vector3 _end = _targetSenses.transform.position;
            Vector3 _dir = _end - _start;
            float _length = _dir.magnitude;
            _dir.Normalize();
            // Segment positions
            Vector3 _firstEnd = _start + _dir * (_length * _gapStart);
            Vector3 _secondStart = _start + _dir * (_length * _gapEnd);
            Vector3 _labelPos = _start + _dir * (_length * 0.5f) + Vector3.up * _labelHeight;
            // Draw ray segments
            Debug.DrawLine(_start, _firstEnd, _color, 0.1f);
            Debug.DrawLine(_secondStart, _end, _color, 0.1f);
            // Draw awareness label
            m_labelContent.text = _awarness.ToString("F2");
            if (m_labelStyle == null)
            {
                m_labelStyle = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        textColor = Color.white,
                        background = Texture2D.grayTexture
                    },
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(4, 4, 2, 2)
                };
            }

            Handles.Label(_labelPos, m_labelContent, m_labelStyle);
        }
    }
}
#endif

