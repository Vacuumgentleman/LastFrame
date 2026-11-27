using System; // Action delegate for events
using Senses2026; // Senses Stealth component
using UnityEngine; // MonoBehaviour, CharacterController, Physics

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Moves Ollie with a CharacterController: walk, run and crouch (no jump).
    /// Movement is relative to the camera yaw. Based on the Senses demo PlayerMover_MK2.
    /// While crouching it enables the Senses Stealth component, which lowers detection.
    /// </summary>
    [DisallowMultipleComponent] // Only one movement script per player
    [RequireComponent(typeof(CharacterController))] // Needs a CharacterController to move
    [RequireComponent(typeof(PlayerInputReader))] // Needs the input reader
    public class PlayerMovement : MonoBehaviour // Movement layer of the player
    {
        [Header("Speeds (m/s)")] // Inspector group title
        [SerializeField] float m_walkSpeed = 3f; // Walk speed (GDD: 3)
        [SerializeField] float m_runSpeed = 6f; // Run speed (GDD: 6)
        [SerializeField] float m_crouchSpeed = 1.5f; // Crouch speed (GDD: 1.5)
        [SerializeField] float m_speedChangeRate = 10f; // How fast speed reaches its target
        [SerializeField] float m_rotationSmoothTime = 0.1f; // Turn smoothing time in seconds

        [Header("Gravity")] // Inspector group title
        [SerializeField] float m_gravity = -15f; // Gravity applied every frame

        [Header("Crouch")] // Inspector group title
        [SerializeField] float m_crouchHeight = 1.2f; // Capsule height while crouching
        [SerializeField] Vector3 m_crouchCenter = new Vector3(0f, 0.6f, 0f); // Capsule center while crouching
        [SerializeField] float m_crouchTransitionSpeed = 7f; // How fast the capsule changes size
        [Tooltip("Layers that block standing up when something is above Ollie.")]
        [SerializeField] LayerMask m_ceilingLayers = ~0; // Everything by default

        [Header("References")] // Inspector group title
        [Tooltip("Camera used to orient movement. Empty = Camera.main.")]
        [SerializeField] Transform m_cameraTransform; // Camera that defines "forward"
        [Tooltip("Senses Stealth component, enabled while crouching. Empty = the one on this object.")]
        [SerializeField] Stealth m_stealth; // Lowers detection while crouching

        [Header("Animator (optional)")] // Inspector group title
        [SerializeField] Animator m_animator; // Character animator
        [SerializeField] string m_speedParam = "Velocity_Z"; // Float parameter for the blend tree
        [SerializeField] string m_crouchParam = "isStealth"; // Bool parameter for crouch

        CharacterController m_controller; // Cached CharacterController
        PlayerInputReader m_input; // Cached input reader
        Vector3 m_standCenter; // Capsule center when standing
        float m_standHeight; // Capsule height when standing
        float m_speed; // Current horizontal speed
        float m_verticalVelocity; // Current vertical speed (gravity)
        float m_rotationVelocity; // Helper value for SmoothDampAngle
        int m_speedHash, m_crouchHash; // Animator parameter hashes
        bool m_hasSpeedParam, m_hasCrouchParam; // True if the animator has those parameters

        public bool IsCrouching { get; private set; } // True while crouching
        public bool IsRunning { get; private set; } // True while running and moving
        public bool IsMoving { get; private set; } // True when there is movement input
        public bool IsGrounded => m_controller.isGrounded; // True when touching the ground
        public float CurrentSpeed => m_speed; // Current horizontal speed for other scripts

        public event Action<bool> OnCrouchChanged; // Raised when Ollie crouches or stands up

        // Caches components and the standing capsule size.
        void Awake()
        {
            m_controller = GetComponent<CharacterController>(); // Required component
            m_input = GetComponent<PlayerInputReader>(); // Required component
            if (!m_stealth) TryGetComponent(out m_stealth); // Find Stealth if not assigned
            if (!m_animator) TryGetComponent(out m_animator); // Find Animator if not assigned

            m_standCenter = m_controller.center; // Save standing center
            m_standHeight = m_controller.height; // Save standing height
            CacheAnimatorParams(); // Check which animator parameters exist
        }

        // Finds the camera and starts with stealth off (standing).
        void Start()
        {
            if (!m_cameraTransform && Camera.main) m_cameraTransform = Camera.main.transform; // Default to main camera
            if (m_stealth) m_stealth.enabled = false; // Standing = no stealth bonus
        }

        // Runs the movement steps every frame.
        void Update()
        {
            UpdateCrouchState(); // Decide crouch or stand
            UpdateCollider(); // Resize the capsule
            Move(); // Move and rotate Ollie
            UpdateAnimator(); // Send values to the animator
        }

        // Decides whether Ollie should be crouching this frame.
        void UpdateCrouchState()
        {
            bool wantsCrouch = IsCrouching; // Start from the current state

            if (m_input.CrouchMode == CrouchMode.Hold) // Hold mode
                wantsCrouch = m_input.CrouchHeld; // Crouch only while the key is held
            else if (m_input.CrouchPressedThisFrame) // Toggle mode and key just pressed
                wantsCrouch = !IsCrouching; // Switch state

            if (m_input.SprintHeld && m_input.Move != Vector2.zero) wantsCrouch = false; // Running stands Ollie up (same as the demo)

            if (!wantsCrouch && IsCrouching && IsCeilingBlocked()) wantsCrouch = true; // Stay crouched under a low obstacle

            SetCrouching(wantsCrouch); // Apply the result
        }

        // Applies the crouch state, toggles Stealth and raises the event.
        void SetCrouching(bool crouching)
        {
            if (crouching == IsCrouching) return; // No change
            IsCrouching = crouching; // Store new state
            if (m_stealth) m_stealth.enabled = crouching; // Stealth bonus only while crouching
            OnCrouchChanged?.Invoke(crouching); // Notify listeners (HUD, audio)
        }

        // Returns true if something above Ollie stops her from standing up.
        bool IsCeilingBlocked()
        {
            float radius = m_controller.radius * 0.9f; // Slightly thinner than the capsule
            Vector3 bottom = transform.position + Vector3.up * (radius + m_controller.skinWidth); // Lower sphere center
            Vector3 top = transform.position + Vector3.up * (m_standHeight - radius); // Upper sphere center at standing height
            foreach (var hit in Physics.OverlapCapsule(bottom, top, radius, m_ceilingLayers, QueryTriggerInteraction.Ignore)) // Colliders in the standing space
                if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return true; // Ignore Ollie's own colliders
            return false; // Nothing above
        }

        // Smoothly changes the capsule between standing and crouching size.
        void UpdateCollider()
        {
            float t = m_crouchTransitionSpeed * Time.deltaTime; // Lerp amount this frame
            m_controller.height = Mathf.Lerp(m_controller.height, IsCrouching ? m_crouchHeight : m_standHeight, t); // Height
            m_controller.center = Vector3.Lerp(m_controller.center, IsCrouching ? m_crouchCenter : m_standCenter, t); // Center
        }

        // Moves and rotates Ollie relative to the camera and applies gravity.
        void Move()
        {
            Vector2 input = m_input.Move; // Movement input this frame
            IsMoving = input.sqrMagnitude > 0.01f; // Ignore tiny stick drift
            IsRunning = IsMoving && m_input.SprintHeld && !IsCrouching; // Run only when moving and standing

            float targetSpeed = !IsMoving ? 0f : IsRunning ? m_runSpeed : IsCrouching ? m_crouchSpeed : m_walkSpeed; // Pick speed for the state
            m_speed = Mathf.MoveTowards(m_speed, targetSpeed, m_speedChangeRate * Time.deltaTime * Mathf.Max(1f, m_runSpeed)); // Accelerate or slow down

            Vector3 moveDir = Vector3.zero; // Horizontal direction this frame
            if (IsMoving) // There is input
            {
                float camYaw = m_cameraTransform ? m_cameraTransform.eulerAngles.y : 0f; // Camera horizontal angle
                float targetYaw = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + camYaw; // Input angle relative to camera
                float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref m_rotationVelocity, m_rotationSmoothTime); // Smooth turn
                transform.rotation = Quaternion.Euler(0f, yaw, 0f); // Face the movement direction
                moveDir = Quaternion.Euler(0f, targetYaw, 0f) * Vector3.forward; // Move toward the input direction
            }
            else if (m_speed > 0f) // No input but still slowing down
            {
                moveDir = transform.forward; // Keep sliding forward until stopped
            }

            if (m_controller.isGrounded && m_verticalVelocity < 0f) m_verticalVelocity = -2f; // Stick to the ground
            m_verticalVelocity += m_gravity * Time.deltaTime; // Apply gravity

            m_controller.Move(moveDir * (m_speed * Time.deltaTime) + Vector3.up * (m_verticalVelocity * Time.deltaTime)); // Final move
        }

        // Checks which animator parameters exist so missing ones do not cause errors.
        void CacheAnimatorParams()
        {
            if (!m_animator || !m_animator.runtimeAnimatorController) return; // No animator to check
            foreach (var p in m_animator.parameters) // Look through all parameters
            {
                if (p.name == m_speedParam) m_hasSpeedParam = true; // Speed parameter found
                if (p.name == m_crouchParam) m_hasCrouchParam = true; // Crouch parameter found
            }
            m_speedHash = Animator.StringToHash(m_speedParam); // Cache hash for speed
            m_crouchHash = Animator.StringToHash(m_crouchParam); // Cache hash for crouch
        }

        // Sends speed and crouch state to the animator (same normalization as the demo).
        void UpdateAnimator()
        {
            if (!m_animator) return; // No animator assigned
            if (m_hasSpeedParam) // Speed parameter exists
                m_animator.SetFloat(m_speedHash, IsCrouching ? m_speed / m_crouchSpeed : m_speed / m_runSpeed); // 0 to 1 blend value
            if (m_hasCrouchParam) // Crouch parameter exists
                m_animator.SetBool(m_crouchHash, IsCrouching); // Crouch animation on or off
        }
    }
}
