using LastFrame.Core; // DebugRanges, DebugShapes
using Senses2026; // Senses
using UnityEngine; // MonoBehaviour, LineRenderer, Color

namespace LastFrame.Enemies // Enemy scripts of Last Frame
{
    /// <summary>
    /// Debug cone on the floor with the enemy's real vision (angle and radius from Senses), tinted by its state.
    /// The cone is clipped by walls and props, so it shows what the enemy can really see.
    /// Also a first version of the GDD vision cone drawn on the floor.
    /// </summary>
    [RequireComponent(typeof(Senses))] // Source of the vision values
    public class VisionConeDisplay : MonoBehaviour // Enemy vision debug view
    {
        [SerializeField] float m_lineWidth = 0.06f; // Cone outline thickness
        [SerializeField] Color m_calmColor = new Color(1f, 0.85f, 0.2f, 0.8f); // Patrol: yellow
        [SerializeField] Color m_suspiciousColor = new Color(1f, 0.6f, 0.1f, 0.9f); // Investigate: orange
        [SerializeField] Color m_alertColor = new Color(1f, 0.2f, 0.2f, 1f); // Chase: red
        [Tooltip("Height of the rays that clip the cone (about Ollie's body center).")]
        [SerializeField] float m_rayHeight = 1f; // Ray height above the floor
        [SerializeField] LayerMask m_blockers = ~0; // What stops the view
        [SerializeField] int m_rays = 30; // Rays across the cone (more = smoother edge)

        Senses m_senses; // Cached Senses
        EnemyAI m_ai; // Optional state source
        LineRenderer m_line; // Cone line

        // Caches components and creates the cone line.
        void Start()
        {
            m_senses = GetComponent<Senses>(); // Required component
            m_ai = GetComponent<EnemyAI>(); // Optional
            m_line = DebugRanges.CreateLine(transform, "VisionCone", m_lineWidth); // Cone child
        }

        // Redraws the cone every frame from the enemy's position and facing.
        void LateUpdate()
        {
            bool alive = !m_ai || m_ai.State != EnemyState.Dead; // Hide when erased
            bool show = DebugRanges.Visible && alive && m_senses.enabled && (!m_ai || m_ai.UseVision); // Debug on, sensing and not blind
            m_line.enabled = show; // Toggle line
            if (!show) return; // Nothing to draw

            Color c = !m_ai ? m_calmColor : m_ai.State == EnemyState.Chase ? m_alertColor : m_ai.State == EnemyState.Investigate ? m_suspiciousColor : m_calmColor; // Color by state
            m_line.startColor = m_line.endColor = c; // Apply color
            float radius = m_ai ? m_ai.EffectiveVisionRange : m_senses.Central_VisionRadius; // Shorter when Ollie crouches
            DrawClippedCone(m_senses.Central_VisionAngle, radius); // Real vision, stopped by obstacles
        }

        // Draws the cone outline with each ray shortened where it hits a wall or prop.
        void DrawClippedCone(float fullAngle, float radius)
        {
            Vector3 floor = transform.position + Vector3.up * 0.03f; // Cone tip on the floor
            Vector3 rayOrigin = transform.position + Vector3.up * m_rayHeight; // Where the rays start
            Vector3 forward = transform.forward; // Enemy facing
            forward.y = 0f; // Flat
            forward.Normalize(); // Unit direction
            m_line.loop = true; // Close back to the tip
            m_line.positionCount = m_rays + 2; // Tip + one point per ray
            m_line.SetPosition(0, floor); // Tip
            for (int i = 0; i <= m_rays; i++) // Every ray from left to right
            {
                float a = Mathf.Lerp(-fullAngle * 0.5f, fullAngle * 0.5f, i / (float)m_rays); // Ray angle
                Vector3 dir = Quaternion.Euler(0f, a, 0f) * forward; // Ray direction
                float distance = radius; // Full length by default
                foreach (var hit in Physics.RaycastAll(rayOrigin, dir, radius, m_blockers, QueryTriggerInteraction.Ignore)) // Everything on the ray
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue; // Skip the enemy itself
                    if (hit.collider is CharacterController) continue; // Ollie does not block the view
                    if (hit.distance < distance) distance = hit.distance; // Closest obstacle
                }
                m_line.SetPosition(i + 1, floor + dir * distance); // Point where the view ends
            }
        }
    }
}
