using UnityEngine; // MonoBehaviour, Physics, CharacterController

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Keeps Ollie's camera-facing sprite out of walls (test of "idea 1", 27/11).
    /// The sprite leans back with the camera pitch, so its top can sink into a wall on the far side of the screen.
    /// Each frame a thin box with the sprite's tilt checks for walls; if it touches one, only this frame's movement
    /// toward the wall (away from the camera) is undone. Sideways movement is kept, so Ollie can slide along walls.
    /// </summary>
    [RequireComponent(typeof(CharacterController))] // Moves Ollie back with the same collision rules
    public class SpriteWallGuard : MonoBehaviour // Sprite vs wall guard
    {
        [Tooltip("Ollie's drawing. Empty = the first SpriteRenderer in the children.")]
        [SerializeField] SpriteRenderer m_sprite; // Sprite whose area must stay out of walls
        [Tooltip("Layers that count as walls. Ignore Raycast is excluded so it does not mix with the 'idea 2' blockers.")]
        [SerializeField] LayerMask m_wallLayers = ~(1 << 2); // Everything except Ignore Raycast
        [Tooltip("Box width as a fraction of the capsule diameter. Below 1 so walls at Ollie's sides never trigger the guard.")]
        [SerializeField, Range(0.1f, 1f)] float m_widthFactor = 0.8f; // Narrow box: only walls in front count
        [SerializeField] float m_thickness = 0.05f; // Half depth of the check box (meters)
        [SerializeField] float m_fallbackStep = 0.02f; // Gentle push per frame if she is already inside (meters)
        [SerializeField] float m_teleportDistance = 1f; // A jump bigger than this per frame is a teleport, not movement
        [SerializeField] bool m_drawGizmo = true; // Shows the check box in the Scene view

        CharacterController m_controller; // Ollie's capsule
        Camera m_camera; // Game camera (gives the push direction)
        readonly Collider[] m_hits = new Collider[16]; // Reused overlap buffer (no garbage)
        Vector3 m_lastPosition; // Position at the end of the previous frame
        bool m_hasLastPosition; // False until the first frame

        /// <summary>True when the guard corrected Ollie this frame (debug).</summary>
        public bool BlockedThisFrame { get; private set; } // Debug flag

        // Caches components.
        void Awake()
        {
            m_controller = GetComponent<CharacterController>(); // Required component
            if (!m_sprite) m_sprite = GetComponentInChildren<SpriteRenderer>(); // Default sprite
        }

        // Forgets the last position so a respawn is not undone.
        void OnEnable() => m_hasLastPosition = false; // Fresh start

        // Runs after movement and after the billboard turned the sprite: undoes the forward part of the movement if the sprite touches a wall.
        void LateUpdate()
        {
            BlockedThisFrame = false; // Reset debug flag
            if (!m_sprite || !m_controller.enabled) { m_hasLastPosition = false; return; } // Nothing to check
            if (!m_camera) m_camera = Camera.main; // Find the camera once
            if (!m_camera) return; // No camera yet
            Vector3 back = -m_camera.transform.forward; // Toward the camera
            back.y = 0f; // Only on the floor plane
            if (back.sqrMagnitude < 0.0001f) return; // Camera looking straight down: no lean, no clipping
            back.Normalize(); // Unit direction

            Vector3 delta = transform.position - m_lastPosition; // Movement this frame
            if (!m_hasLastPosition || delta.sqrMagnitude > m_teleportDistance * m_teleportDistance) // First frame or teleport
            {
                m_lastPosition = transform.position; // Start tracking from here
                m_hasLastPosition = true; // Tracking
                return; // Do not judge a teleport
            }

            if (TouchesWall()) // The sprite is in a wall
            {
                float forward = -Vector3.Dot(delta, back); // How much she moved away from the camera this frame
                if (forward > 0f) m_controller.Move(back * forward); // Undo only that part; sideways movement stays
                if (TouchesWall()) m_controller.Move(back * m_fallbackStep); // Still inside (wall moved, spawn): ease out slowly
                BlockedThisFrame = true; // Remember for debug
            }
            m_lastPosition = transform.position; // Next frame compares against this
        }

        // True if the sprite's area overlaps a wall (static, solid, not Ollie, not the floor).
        bool TouchesWall()
        {
            GetBox(out Vector3 center, out Vector3 half, out Quaternion rotation); // Sprite area as a thin box
            int count = Physics.OverlapBoxNonAlloc(center, half, m_hits, rotation, m_wallLayers, QueryTriggerInteraction.Ignore); // Solid colliders only
            for (int i = 0; i < count; i++) // Check each hit
            {
                Collider c = m_hits[i]; // Current collider
                if (c.transform.IsChildOf(transform)) continue; // Ollie herself
                if (c.attachedRigidbody && !c.attachedRigidbody.isKinematic) continue; // Loose objects (cans) are pushed, not walls
                if (c.bounds.max.y <= transform.position.y + 0.05f) continue; // The floor
                if (c.GetComponentInParent<UnityEngine.AI.NavMeshAgent>()) continue; // Enemies are not walls
                return true; // A wall
            }
            return false; // Free
        }

        // Builds a thin box with the sprite's height and tilt, centered on Ollie, as wide as a fraction of her capsule.
        void GetBox(out Vector3 center, out Vector3 half, out Quaternion rotation)
        {
            Transform t = m_sprite.transform; // Sprite transform (tilted by the billboard)
            Vector3 size = m_sprite.sprite ? m_sprite.sprite.bounds.size : Vector3.one; // Local sprite size
            Vector3 scale = t.lossyScale; // World scale
            center = t.TransformPoint(m_sprite.sprite ? m_sprite.sprite.bounds.center : Vector3.zero); // World center of the drawing
            Vector3 right = t.right; // Sprite's horizontal axis
            Vector3 offset = center - transform.position; // Center relative to Ollie
            center -= right * Vector3.Dot(offset, right); // Same left-right position as the capsule
            float radius = m_controller ? m_controller.radius * Mathf.Abs(transform.lossyScale.x) : 0.3f; // Capsule radius (edit mode safe)
            half = new Vector3(radius * m_widthFactor, size.y * Mathf.Abs(scale.y) * 0.5f, m_thickness); // Half extents
            rotation = t.rotation; // Same tilt as the sprite
        }

        // Draws the check box (red when it corrected Ollie).
        void OnDrawGizmos()
        {
            if (!m_drawGizmo || !m_sprite) return; // Off or no sprite
            if (!m_controller) m_controller = GetComponent<CharacterController>(); // Edit mode
            GetBox(out Vector3 center, out Vector3 half, out Quaternion rotation); // Current box
            Gizmos.color = BlockedThisFrame ? Color.red : new Color(0.3f, 1f, 0.3f, 0.6f); // Red = blocked
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one); // Box space
            Gizmos.DrawWireCube(Vector3.zero, half * 2f); // Box outline
        }
    }
}
