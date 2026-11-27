using LastFrame.Core; // DebugRanges, DebugShapes
using UnityEngine; // MonoBehaviour, LineRenderer, Color

namespace LastFrame.Enemies // Enemy scripts of Last Frame
{
    /// <summary>
    /// Debug circle around the enemy: if Ollie makes a noise of this strength inside it, this enemy hears her.
    /// Radius comes from EnemyAI's hearing threshold (Senses received dB), so it matches what the AI really does.
    /// </summary>
    [RequireComponent(typeof(EnemyAI))] // Source of the hearing threshold
    public class HearingRangeDisplay : MonoBehaviour // Enemy hearing debug view
    {
        [SerializeField] Color m_color = new Color(0.6f, 0.8f, 1f, 0.6f); // Light blue
        [SerializeField] float m_lineWidth = 0.04f; // Circle thickness
        [Tooltip("Noise used as reference for the circle. 60 dB = Ollie running (demo run clips).")]
        [SerializeField] float m_referenceNoiseDb = 60f; // Running footsteps

        EnemyAI m_ai; // Hearing threshold and state
        LineRenderer m_line; // Circle line

        // Caches the AI and creates the circle line.
        void Start()
        {
            m_ai = GetComponent<EnemyAI>(); // Required component
            m_line = DebugRanges.CreateLine(transform, "HearingRange", m_lineWidth); // Circle child
            m_line.startColor = m_line.endColor = m_color; // Circle color
        }

        // Redraws the hearing circle around the enemy.
        void LateUpdate()
        {
            bool show = DebugRanges.Visible && m_ai.State != EnemyState.Dead; // Debug on and alive
            m_line.enabled = show; // Toggle line
            if (!show) return; // Nothing to draw
            float radius = m_ai.HearingRadiusFor(m_referenceNoiseDb); // How far a run is heard (~8.9 m with threshold 41)
            DebugShapes.Circle(m_line, transform.position + Vector3.up * 0.03f, radius); // Draw on the floor
        }
    }
}
