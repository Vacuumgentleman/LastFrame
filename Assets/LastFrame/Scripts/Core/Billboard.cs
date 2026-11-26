using UnityEngine; // MonoBehaviour, Camera, Transform

namespace LastFrame.Core // Shared scripts of Last Frame
{
    /// <summary>
    /// Keeps an object (sprite, label) always facing the camera.
    /// Reusable for Ollie, enemies and pickups while they are 2D drawings in a 3D world.
    /// </summary>
    public class Billboard : MonoBehaviour // Camera-facing helper
    {
        [Tooltip("Camera to face. Empty = Camera.main.")]
        [SerializeField] Transform m_cameraTransform; // Camera this object looks at
        [Tooltip("Only turn around the vertical axis (object stays upright).")]
        [SerializeField] bool m_lockVertical = false; // False = tilt with the camera angle (top-down view)

        // Finds the main camera if none was assigned.
        void Start()
        {
            if (!m_cameraTransform && Camera.main) m_cameraTransform = Camera.main.transform; // Default to main camera
        }

        // Rotates after all movement so the object never looks away from the camera.
        void LateUpdate()
        {
            if (!m_cameraTransform) return; // No camera to face
            if (m_lockVertical) // Upright mode
            {
                Vector3 forward = m_cameraTransform.forward; // Camera look direction
                forward.y = 0f; // Ignore camera tilt
                if (forward.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(forward); // Face camera horizontally
            }
            else
            {
                transform.rotation = m_cameraTransform.rotation; // Same rotation as the camera = flat to the screen
            }
        }
    }
}
