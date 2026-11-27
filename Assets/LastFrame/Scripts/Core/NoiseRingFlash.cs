using UnityEngine; // MonoBehaviour, LineRenderer, Color

namespace LastFrame.Core // Shared scripts of Last Frame
{
    /// <summary>
    /// Short expanding ring on the floor that shows a one-time noise (ink impact). Debug only.
    /// </summary>
    public class NoiseRingFlash : MonoBehaviour // One-shot noise ring
    {
        LineRenderer m_line; // Ring line
        Vector3 m_center; // Ring center on the floor
        float m_radius; // Final radius
        float m_time; // Time since spawn
        const float Duration = 0.6f; // Seconds visible

        // Creates a ring at a point if debug ranges are visible.
        public static void Spawn(Vector3 point, float radius)
        {
            if (!DebugRanges.Visible) return; // Debug view off
            var go = new GameObject("NoiseRingFlash"); // Holder
            var flash = go.AddComponent<NoiseRingFlash>(); // Behaviour
            flash.m_center = new Vector3(point.x, point.y + 0.03f, point.z); // Slightly above the floor
            flash.m_radius = radius; // Target size
            flash.m_line = DebugRanges.CreateLine(go.transform, "Ring", 0.06f); // Line
        }

        // Grows the ring and fades it out, then removes it.
        void Update()
        {
            m_time += Time.deltaTime; // Advance time
            float t = Mathf.Clamp01(m_time / Duration); // 0 to 1
            DebugShapes.Circle(m_line, m_center, Mathf.Lerp(m_radius * 0.3f, m_radius, t)); // Grow
            Color c = new Color(0.4f, 0.6f, 1f, 1f - t); // Blue ink, fading
            m_line.startColor = m_line.endColor = c; // Apply color
            if (t >= 1f) Destroy(gameObject); // Done
        }
    }
}
