using LastFrame.Core; // DebugRanges, DebugShapes
using LastFrame.Enemies; // EnemyAI (hearing threshold)
using UnityEngine; // MonoBehaviour, LineRenderer, Color

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Debug ring on the floor that shows how far enemies hear Ollie right now.
    /// Running = the radius an enemy hears 60 dB at (Senses received dB vs EnemyAI threshold); walking and crouching = silent (GDD).
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))] // Reads the running state
    public class NoiseRangeDisplay : MonoBehaviour // Player noise debug view
    {
        [SerializeField] Color m_color = new Color(1f, 0.5f, 0.15f, 0.9f); // Orange like the RUN label
        [SerializeField] float m_lineWidth = 0.06f; // Ring thickness
        [SerializeField] float m_growSpeed = 10f; // How fast the ring follows the state
        [Tooltip("dB of Ollie's running steps (released by the demo run animation clips).")]
        [SerializeField] float m_runNoiseDb = 60f; // Running footsteps
        [Tooltip("Enemy whose hearing threshold sizes the ring. Empty = the first EnemyAI in the scene.")]
        [SerializeField] EnemyAI m_referenceEnemy; // Hearing reference

        PlayerMovement m_movement; // Running state
        LineRenderer m_line; // Ring line
        float m_radius; // Current drawn radius

        // Caches the movement and creates the ring.
        void Start()
        {
            m_movement = GetComponent<PlayerMovement>(); // Required component
            if (!m_referenceEnemy) m_referenceEnemy = FindAnyObjectByType<EnemyAI>(); // Any enemy (all share the same threshold for now)
            m_line = DebugRanges.CreateLine(transform, "NoiseRange", m_lineWidth); // Ring child
            m_line.startColor = m_line.endColor = m_color; // Ring color
        }

        // Grows or shrinks the ring: hearing radius while running, nothing otherwise.
        void LateUpdate()
        {
            float target = m_movement.IsRunning && m_referenceEnemy ? m_referenceEnemy.HearingRadiusFor(m_runNoiseDb) : 0f; // Heard radius
            m_radius = Mathf.Lerp(m_radius, target, m_growSpeed * Time.deltaTime); // Smooth size
            bool show = DebugRanges.Visible && m_radius > 0.05f; // Hide when silent or debug off
            m_line.enabled = show; // Toggle line
            if (show) DebugShapes.Circle(m_line, transform.position + Vector3.up * 0.03f, m_radius); // Draw around the feet
        }
    }
}
