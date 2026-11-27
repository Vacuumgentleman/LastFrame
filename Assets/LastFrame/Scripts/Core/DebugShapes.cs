using UnityEngine; // LineRenderer, Vector3, Mathf

namespace LastFrame.Core // Shared scripts of Last Frame
{
    /// <summary>
    /// Fills LineRenderers with simple floor shapes (circle, cone) for in-game debug views.
    /// </summary>
    public static class DebugShapes // Debug drawing helper
    {
        // Draws a flat circle on the floor around a center point.
        public static void Circle(LineRenderer line, Vector3 center, float radius, int segments = 48)
        {
            line.loop = true; // Close the circle
            line.positionCount = segments; // One point per segment
            for (int i = 0; i < segments; i++) // Every point
            {
                float a = i * Mathf.PI * 2f / segments; // Angle in radians
                line.SetPosition(i, center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius); // Point on the circle
            }
        }

        // Draws a flat cone (pie slice) on the floor from an origin toward a direction.
        public static void Cone(LineRenderer line, Vector3 origin, Vector3 forward, float fullAngle, float radius, int arcSegments = 24)
        {
            forward.y = 0f; // Keep it flat
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward; // Safe default
            forward.Normalize(); // Unit direction
            line.loop = true; // Close back to the origin
            line.positionCount = arcSegments + 2; // Origin + arc points
            line.SetPosition(0, origin); // Tip of the cone
            float half = fullAngle * 0.5f; // Half angle on each side
            for (int i = 0; i <= arcSegments; i++) // Arc points from left to right
            {
                float a = Mathf.Lerp(-half, half, i / (float)arcSegments); // Angle of this point
                line.SetPosition(i + 1, origin + Quaternion.Euler(0f, a, 0f) * forward * radius); // Point on the arc
            }
        }
    }
}
