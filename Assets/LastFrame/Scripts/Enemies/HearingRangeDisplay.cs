using LastFrame.Core; // DebugRanges, DebugShapes, NoiseMath
using LastFrame.Player; // PlayerNoise
using Senses2026; // Senses, HearSensitivity
using UnityEngine; // MonoBehaviour, LineRenderer, Color

namespace LastFrame.Enemies // Enemy scripts of Last Frame
{
    /// <summary>
    /// Debug circle around the enemy: if Ollie runs inside it, this enemy hears her.
    /// Radius is Ollie's run noise radius (PlayerNoise), the same value the enemy uses to hear her.
    /// </summary>
    [RequireComponent(typeof(Senses))] // Source of the hearing sensitivity
    public class HearingRangeDisplay : MonoBehaviour // Enemy hearing debug view
    {
        [SerializeField] Color m_color = new Color(0.6f, 0.8f, 1f, 0.6f); // Light blue
        [SerializeField] float m_lineWidth = 0.04f; // Circle thickness
        [Tooltip("Player noise used as reference. Empty = the first PlayerNoise in the scene.")]
        [SerializeField] PlayerNoise m_playerNoise; // Ollie's noise settings

        Senses m_senses; // Cached Senses
        EnemyAI m_ai; // Optional, hides the circle when dead
        LineRenderer m_line; // Circle line

        // Caches components and creates the circle line.
        void Start()
        {
            m_senses = GetComponent<Senses>(); // Required component
            m_ai = GetComponent<EnemyAI>(); // Optional
            if (!m_playerNoise) m_playerNoise = FindAnyObjectByType<PlayerNoise>(); // Find Ollie
            m_line = DebugRanges.CreateLine(transform, "HearingRange", m_lineWidth); // Circle child
            m_line.startColor = m_line.endColor = m_color; // Circle color
        }

        // Redraws the hearing circle around the enemy.
        void LateUpdate()
        {
            bool alive = !m_ai || m_ai.State != EnemyState.Dead; // Hide when erased
            bool show = DebugRanges.Visible && alive && m_playerNoise && m_senses.enabled; // Debug on and hearing
            m_line.enabled = show; // Toggle line
            if (!show) return; // Nothing to draw
            float radius = m_playerNoise.RunNoiseRadius; // How far Ollie's run is heard
            DebugShapes.Circle(m_line, transform.position + Vector3.up * 0.03f, radius); // Draw on the floor
        }

        // Minimum dB this enemy hears (same values as Senses).
        static float ThresholdDb(HearSensitivity sensitivity)
        {
            switch (sensitivity) // Senses table
            {
                case HearSensitivity.Low: return 40f; // Hard of hearing
                case HearSensitivity.Medium: return 20f; // Normal
                case HearSensitivity.High: return 5f; // Our enemies
                default: return 0f; // Perfect
            }
        }
    }
}
