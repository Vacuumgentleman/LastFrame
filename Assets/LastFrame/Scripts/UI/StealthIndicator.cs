using TMPro;
using UnityEngine;
using Senses2026;

namespace LastFrame.UI
{
    /// <summary>
    /// World-space eye icon above the player that shows how exposed she is:
    /// Hidden (crouching, Senses Stealth active), Visible, or Detected (a Senses sentry has spotted her).
    /// Place it on a World Space canvas parented to the player. Empty sprite slots fall back to color + label.
    /// </summary>
    public class StealthIndicator : MonoBehaviour
    {
        public enum Exposure { Hidden, Visible, Detected }

        [SerializeField] Stealth m_stealth;
        [SerializeField] UnityEngine.UI.Image m_icon;
        [SerializeField] TMP_Text m_label;
        [Tooltip("Index 0 = hidden, 1 = visible, 2 = detected.")]
        [SerializeField] Sprite[] m_sprites = new Sprite[3];
        [SerializeField] Color[] m_fallbackColors =
        {
            new Color32(0x66, 0x73, 0x6A, 0xFF), // hidden: verde humo
            new Color32(0xE2, 0xBD, 0x73, 0xFF), // visible: latón claro
            new Color32(0xD2, 0x63, 0x61, 0xFF), // detected: rojo apagado
        };
        [SerializeField] string[] m_fallbackTexts = { "OCULTA", "VISIBLE", "¡TE VEN!" };
        [Tooltip("Seconds between sentry checks (Detected state).")]
        [SerializeField] float m_detectionCheckInterval = 0.2f;

        public Exposure Current { get; private set; } = (Exposure)(-1);

        Camera m_camera;
        float m_nextCheck;
        bool m_detected;

        void Awake()
        {
            if (m_stealth == null) m_stealth = GetComponentInParent<Stealth>();
        }

        void LateUpdate()
        {
            if (m_camera == null) m_camera = Camera.main;
            if (m_camera != null) transform.rotation = m_camera.transform.rotation; // billboard

            if (Time.time >= m_nextCheck)
            {
                m_nextCheck = Time.time + m_detectionCheckInterval;
                m_detected = AnySentryDetects();
            }

            Exposure _state = m_detected ? Exposure.Detected : (m_stealth != null && m_stealth.enabled ? Exposure.Hidden : Exposure.Visible);
            if (_state != Current) Apply(_state);
        }

        void Apply(Exposure _state)
        {
            Current = _state;
            int _i = (int)_state;
            Sprite _sprite = m_sprites != null && _i < m_sprites.Length ? m_sprites[_i] : null;
            if (m_icon != null)
            {
                m_icon.sprite = _sprite;
                m_icon.color = _sprite != null ? Color.white : m_fallbackColors[_i];
            }
            if (m_label != null)
            {
                m_label.gameObject.SetActive(_sprite == null);
                m_label.text = m_fallbackTexts[_i];
            }
        }

        static bool AnySentryDetects()
        {
            foreach (MonoBehaviour _behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (_behaviour is ISentry _sentry && _behaviour.isActiveAndEnabled && _sentry.SomethingWasDetected()) return true;
            }
            return false;
        }
    }
}
