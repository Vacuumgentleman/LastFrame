using UnityEngine;

namespace LastFrame.Visual
{
    /// <summary>
    /// A failing light in the abandoned studio: it stays on for a while, then goes out (sometimes with a short
    /// stutter) and comes back. The light really turns off, so Senses' light detection (Lumen) changes with it:
    /// the dark moments are windows for the player to cross. Its floor pool, cone and glowing fixture go with it.
    /// </summary>
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField] Light m_light;
        [Tooltip("Renderers that show the light (floor pool, cone, glowing tube). Hidden while the light is off.")]
        [SerializeField] Renderer[] m_visuals = new Renderer[0];
        [SerializeField] Vector2 m_onSeconds = new Vector2(2.5f, 4.5f);
        [SerializeField] Vector2 m_offSeconds = new Vector2(1.2f, 2.2f);
        [Tooltip("Chance of a few quick blinks before going out.")]
        [Range(0f, 1f)] [SerializeField] float m_stutterChance = 0.6f;

        float m_timer;
        bool m_on = true;
        int m_stutters;

        void OnEnable()
        {
            if (m_light == null) m_light = GetComponentInChildren<Light>();
            m_timer = Random.Range(m_onSeconds.x, m_onSeconds.y);
            Apply(true);
        }

        void Update()
        {
            m_timer -= Time.deltaTime;
            if (m_timer > 0f) return;

            if (m_stutters > 0)
            {
                // quick blinks; after the last one the light goes out for a real off period
                m_stutters--;
                Apply(!m_on);
                m_timer = 0.07f;
                if (m_stutters == 0) { Apply(false); m_timer = Random.Range(m_offSeconds.x, m_offSeconds.y); }
                return;
            }

            if (m_on)
            {
                if (Random.value < m_stutterChance)
                {
                    m_stutters = 2 * Random.Range(1, 3);
                    Apply(false);
                    m_timer = 0.07f;
                    return;
                }
                Apply(false);
                m_timer = Random.Range(m_offSeconds.x, m_offSeconds.y);
            }
            else
            {
                Apply(true);
                m_timer = Random.Range(m_onSeconds.x, m_onSeconds.y);
            }
        }

        void Apply(bool _on)
        {
            m_on = _on;
            if (m_light != null) m_light.enabled = _on;
            foreach (Renderer _r in m_visuals) if (_r != null) _r.enabled = _on;
        }
    }
}
