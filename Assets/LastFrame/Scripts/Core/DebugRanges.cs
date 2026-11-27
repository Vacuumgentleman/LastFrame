using UnityEngine; // MonoBehaviour, Material
using UnityEngine.InputSystem; // Keyboard

namespace LastFrame.Core // Shared scripts of Last Frame
{
    /// <summary>
    /// Global switch for the in-game debug ranges (noise rings, vision cones). F3 toggles them in play mode.
    /// Also holds the shared line material so every debug drawing looks the same.
    /// </summary>
    public class DebugRanges : MonoBehaviour // Debug view settings
    {
        [SerializeField] bool m_showOnStart = true; // Visible when the scene starts
        [SerializeField] Material m_lineMaterial; // Material for every debug line (Sprites-Default)

        public static bool Visible { get; private set; } = true; // Read by every debug drawer
        public static Material LineMaterial { get; private set; } // Shared material

        // Applies the start settings.
        void Awake()
        {
            Visible = m_showOnStart; // Initial visibility
            LineMaterial = m_lineMaterial; // Share the material
        }

        // F3 shows or hides all debug ranges.
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame) Visible = !Visible; // Toggle
        }

        // Creates a LineRenderer child set up for floor drawings.
        public static LineRenderer CreateLine(Transform parent, string name, float width)
        {
            var go = new GameObject(name); // Holder object
            go.transform.SetParent(parent, false); // Keep it with its owner
            var line = go.AddComponent<LineRenderer>(); // The line
            line.useWorldSpace = true; // Points in world space
            line.widthMultiplier = width; // Thickness
            line.numCornerVertices = 2; // Softer corners
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // No shadows
            line.receiveShadows = false; // No shadows
            line.alignment = LineAlignment.TransformZ; // Lie flat if the holder is rotated
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Face up so the line is flat on the floor
            if (LineMaterial) line.sharedMaterial = LineMaterial; // Shared look
            return line; // Ready to fill
        }
    }
}
