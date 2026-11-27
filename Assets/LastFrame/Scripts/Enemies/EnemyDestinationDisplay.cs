using LastFrame.Core; // DebugRanges, DebugShapes
using UnityEngine; // MonoBehaviour, LineRenderer, Color

namespace LastFrame.Enemies // Enemy scripts of Last Frame
{
    /// <summary>
    /// Debug marker of where the enemy believes Ollie is: a line from the enemy to the last known position and a small ring there.
    /// Orange while investigating ("?"), red while chasing ("!"). Hidden while patrolling.
    /// </summary>
    [RequireComponent(typeof(EnemyAI))] // Source of the last known position
    public class EnemyDestinationDisplay : MonoBehaviour // Enemy target debug view
    {
        [SerializeField] Color m_investigateColor = new Color(1f, 0.6f, 0.1f, 0.9f); // Orange
        [SerializeField] Color m_chaseColor = new Color(1f, 0.2f, 0.2f, 1f); // Red
        [SerializeField] float m_lineWidth = 0.05f; // Line thickness
        [SerializeField] float m_markerRadius = 0.4f; // Ring size at the last known position

        EnemyAI m_ai; // Cached AI
        LineRenderer m_path; // Enemy -> target line
        LineRenderer m_marker; // Ring at the target

        // Caches the AI and creates both lines.
        void Start()
        {
            m_ai = GetComponent<EnemyAI>(); // Required component
            m_path = DebugRanges.CreateLine(transform, "DestinationLine", m_lineWidth); // Line child
            m_marker = DebugRanges.CreateLine(transform, "DestinationMarker", m_lineWidth); // Ring child
        }

        // Draws the line and ring when the enemy is investigating or chasing.
        void LateUpdate()
        {
            bool show = DebugRanges.Visible && m_ai.HasLastKnownPosition; // Only with a target
            m_path.enabled = m_marker.enabled = show; // Toggle both
            if (!show) return; // Nothing to draw

            Color c = m_ai.State == EnemyState.Chase ? m_chaseColor : m_investigateColor; // Color by state
            m_path.startColor = m_path.endColor = m_marker.startColor = m_marker.endColor = c; // Apply color
            Vector3 from = transform.position + Vector3.up * 0.05f; // Enemy feet
            Vector3 to = m_ai.LastKnownPosition; // Last known position
            to.y = from.y; // Same height on the floor
            m_path.loop = false; // Open line
            m_path.positionCount = 2; // Two points
            m_path.SetPosition(0, from); // Start at the enemy
            m_path.SetPosition(1, to); // End at the target
            DebugShapes.Circle(m_marker, to, m_markerRadius, 20); // Ring at the target
        }
    }
}
