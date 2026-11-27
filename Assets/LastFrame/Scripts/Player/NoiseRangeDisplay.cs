using LastFrame.Core; // DebugRanges, DebugShapes
using UnityEngine; // MonoBehaviour, LineRenderer, Color

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Debug ring on the floor that shows how far enemies can hear Ollie right now (running = 8 m, walking and crouching = silent).
    /// </summary>
    [RequireComponent(typeof(PlayerNoise))] // Reads the current noise radius
    public class NoiseRangeDisplay : MonoBehaviour // Player noise debug view
    {
        [SerializeField] Color m_color = new Color(1f, 0.5f, 0.15f, 0.9f); // Orange like the RUN label
        [SerializeField] float m_lineWidth = 0.06f; // Ring thickness
        [SerializeField] float m_growSpeed = 10f; // How fast the ring follows the state

        PlayerNoise m_noise; // Cached noise script
        LineRenderer m_line; // Ring line
        float m_radius; // Current drawn radius

        // Caches the noise script and creates the ring.
        void Start()
        {
            m_noise = GetComponent<PlayerNoise>(); // Required component
            m_line = DebugRanges.CreateLine(transform, "NoiseRange", m_lineWidth); // Ring child
            m_line.startColor = m_line.endColor = m_color; // Ring color
        }

        // Grows or shrinks the ring to the current noise radius.
        void LateUpdate()
        {
            m_radius = Mathf.Lerp(m_radius, m_noise.CurrentNoiseRadius, m_growSpeed * Time.deltaTime); // Smooth size
            bool show = DebugRanges.Visible && m_radius > 0.05f; // Hide when silent or debug off
            m_line.enabled = show; // Toggle line
            if (show) DebugShapes.Circle(m_line, transform.position + Vector3.up * 0.03f, m_radius); // Draw around the feet
        }
    }
}
