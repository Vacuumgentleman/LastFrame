using System.Collections.Generic;
using UnityEngine;
using Senses2026;
using UnityEngine.InputSystem; // El proyecto usa el Input System (Active Input Handling = Input System)

namespace SensesTestRoom
{
    /// <summary>
    /// Controla la test room del Senses Pack:
    ///  - detecta en qué estación está el jugador (por su X),
    ///  - muestra solo la UI de esa estación y aplica su iluminación (estaciones nocturnas),
    ///  - HUD con título/descripción y teclas para saltar entre estaciones o hacer un tour automático.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public class TestRoomManager : MonoBehaviour
    {
        public TestRoomStation[] Stations = new TestRoomStation[0];
        public Transform Player;
        public TestRoomCamera FollowCamera;
        public Light Sun;

        [Header("Iluminación por defecto (día)")]
        public float DefaultLightIntensity = 4f;
        public Color DefaultLightColor = new Color(1f, 0.957f, 0.839f);
        public Material DefaultSkybox;
        public float LightBlendSpeed = 3f;

        [Header("Tour automático")]
        public float AutoTourSecondsPerStation = 25f;

        int m_current = -1;
        bool m_showHelp = true;
        bool m_showList;
        bool m_autoTour;
        float m_autoTourTimer;
        Material m_appliedSkybox;
        readonly HashSet<TestRoomStation> m_visited = new HashSet<TestRoomStation>();

        CharacterController m_playerController;
        PlayerControler_MK2 m_playerInput;
        PlayerMover m_playerMover;

        GUIStyle m_titleStyle, m_bodyStyle, m_hintStyle, m_boxStyle, m_listStyle;

        void Start()
        {
            if (Player != null)
            {
                m_playerController = Player.GetComponent<CharacterController>();
                m_playerInput = Player.GetComponent<PlayerControler_MK2>();
                m_playerMover = Player.GetComponent<PlayerMover>();
            }

            foreach (TestRoomStation _station in Stations)
            {
                setUi(_station, false);
                setInsideBehaviours(_station, false);
            }

            m_appliedSkybox = RenderSettings.skybox;
            Teleport(0);
        }

        void Update()
        {
            handleInput();

            if (Player != null)
            {
                int _index = findStation(Player.position.x);
                if (_index != m_current && _index >= 0) enterStation(_index);
            }

            updateLighting();

            if (m_autoTour)
            {
                m_autoTourTimer -= Time.unscaledDeltaTime;
                if (m_autoTourTimer <= 0f) Teleport((m_current + 1) % Stations.Length);
            }
        }

        public void Teleport(int _index)
        {
            if (Stations.Length == 0 || Player == null) return;
            _index = (_index % Stations.Length + Stations.Length) % Stations.Length;

            TestRoomStation _station = Stations[_index];
            Vector3 _target = _station.SpawnPoint != null ? _station.SpawnPoint.position : _station.transform.position;

            if (m_playerController != null) m_playerController.enabled = false;
            Player.SetPositionAndRotation(_target, Quaternion.identity);
            if (m_playerController != null) m_playerController.enabled = true;

            enterStation(_index);
            if (FollowCamera != null) FollowCamera.Snap();
            m_autoTourTimer = AutoTourSecondsPerStation;
        }

        void enterStation(int _index)
        {
            if (m_current >= 0 && m_current < Stations.Length)
            {
                setUi(Stations[m_current], false);
                setInsideBehaviours(Stations[m_current], false);
            }

            m_current = _index;
            TestRoomStation _station = Stations[m_current];

            setUi(_station, true);
            setInsideBehaviours(_station, true);
            setPlayerFrozen(_station.FreezePlayer);
            if (FollowCamera != null) FollowCamera.Target = _station.CameraFocus != null ? _station.CameraFocus : Player;

            if (m_visited.Add(_station))
            {
                foreach (GameObject _go in _station.ActivateOnFirstEnter)
                {
                    if (_go != null) _go.SetActive(true);
                }
            }
        }

        void setUi(TestRoomStation _station, bool _state)
        {
            foreach (GameObject _go in _station.UiRoots)
            {
                if (_go != null) _go.SetActive(_state);
            }
        }

        void setInsideBehaviours(TestRoomStation _station, bool _state)
        {
            foreach (Behaviour _behaviour in _station.EnabledOnlyInside)
            {
                if (_behaviour != null) _behaviour.enabled = _state;
            }
        }

        void setPlayerFrozen(bool _frozen)
        {
            if (m_playerInput != null) m_playerInput.enabled = !_frozen;
            if (m_playerMover != null) m_playerMover.enabled = !_frozen;
        }

        int findStation(float _x)
        {
            for (int i = 0; i < Stations.Length; i++)
            {
                if (Stations[i].Contains(_x)) return i;
            }

            return -1;
        }

        void updateLighting()
        {
            if (Sun == null || m_current < 0) return;
            TestRoomStation _station = Stations[m_current];

            float _intensity = _station.OverrideLighting ? _station.LightIntensity : DefaultLightIntensity;
            Color _color = _station.OverrideLighting ? _station.LightColor : DefaultLightColor;
            Material _skybox = _station.OverrideLighting && _station.Skybox != null ? _station.Skybox : DefaultSkybox;

            float _t = 1f - Mathf.Exp(-LightBlendSpeed * Time.unscaledDeltaTime);
            Sun.intensity = Mathf.Lerp(Sun.intensity, _intensity, _t);
            Sun.color = Color.Lerp(Sun.color, _color, _t);

            if (_skybox != null && _skybox != m_appliedSkybox)
            {
                RenderSettings.skybox = _skybox;
                m_appliedSkybox = _skybox;
                DynamicGI.UpdateEnvironment();
            }
        }

        void handleInput()
        {
            if (keyDown(Key.N) || keyDown(Key.PageDown)) Teleport(m_current + 1);
            if (keyDown(Key.B) || keyDown(Key.PageUp)) Teleport(m_current - 1);
            if (keyDown(Key.R)) Teleport(m_current);
            if (keyDown(Key.H)) m_showHelp = !m_showHelp;
            if (keyDown(Key.L)) m_showList = !m_showList;
            if (keyDown(Key.T))
            {
                m_autoTour = !m_autoTour;
                m_autoTourTimer = AutoTourSecondsPerStation;
            }

            for (int i = 0; i < Mathf.Min(Stations.Length, 9); i++)
            {
                if (keyDown(Key.F1 + i)) Teleport(i);
            }
            if (Stations.Length > 9 && keyDown(Key.F10)) Teleport(9);
            if (Stations.Length > 10 && keyDown(Key.F11)) Teleport(10);
            if (Stations.Length > 11 && keyDown(Key.F12)) Teleport(11);
        }

        static bool keyDown(Key _key)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard _keyboard = Keyboard.current;
            return _keyboard != null && _keyboard[_key].wasPressedThisFrame;
#else
            return false;
#endif
        }

        void buildStyles()
        {
            if (m_titleStyle != null) return;

            Texture2D _bg = new Texture2D(1, 1);
            _bg.SetPixel(0, 0, new Color(0.05f, 0.07f, 0.1f, 0.82f));
            _bg.Apply();

            m_boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(12, 12, 6, 8) };
            m_boxStyle.normal.background = _bg;

            m_titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold, wordWrap = true, alignment = TextAnchor.UpperCenter };
            m_titleStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);

            m_bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true, alignment = TextAnchor.UpperCenter, richText = true };
            m_bodyStyle.normal.textColor = Color.white;

            m_hintStyle = new GUIStyle(m_bodyStyle) { fontSize = 11 };
            m_hintStyle.normal.textColor = new Color(0.6f, 0.9f, 1f);

            m_listStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };
            m_listStyle.normal.textColor = Color.white;
        }

        void OnGUI()
        {
            if (Stations.Length == 0 || m_current < 0) return;
            buildStyles();

            TestRoomStation _station = Stations[m_current];
            float _width = Mathf.Min(520f, Screen.width * 0.6f);
            float _x = (Screen.width - _width) * 0.5f;

            // Arriba: estación actual (título siempre; descripción con H).
            GUILayout.BeginArea(new Rect(_x, 8f, _width, Screen.height * 0.4f));
            GUILayout.BeginVertical(m_boxStyle);
            GUILayout.Label($"{m_current + 1}/{Stations.Length} · {_station.Title}", m_titleStyle);
            if (m_showHelp)
            {
                if (!string.IsNullOrEmpty(_station.Description)) GUILayout.Label(_station.Description, m_bodyStyle);
                if (!string.IsNullOrEmpty(_station.ControlsHint)) GUILayout.Label(_station.ControlsHint, m_hintStyle);
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            // Abajo: controles de la test room.
            if (m_showHelp)
            {
                string _keys = "N/B estación · F1–F12 ir a · R reiniciar · T tour " + (m_autoTour ? "<b>ON</b>" : "off") +
                               " · L lista · H ocultar · Rueda zoom";
                GUI.Label(new Rect(220f, Screen.height - 24f, Screen.width - 440f, 22f), _keys, m_hintStyle);
            }

            if (m_showList)
            {
                float _listHeight = 30f + Stations.Length * 20f;
                GUILayout.BeginArea(new Rect(Screen.width - 330f, Screen.height - _listHeight - 10f, 320f, _listHeight));
                GUILayout.BeginVertical(m_boxStyle);
                for (int i = 0; i < Stations.Length; i++)
                {
                    string _line = $"F{i + 1}  {Stations[i].Title}";
                    GUILayout.Label(i == m_current ? $"<color=#FFD95A><b>▶ {_line}</b></color>" : _line, m_listStyle);
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }
    }
}
