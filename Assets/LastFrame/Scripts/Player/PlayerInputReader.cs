using System; // Action delegate for events
using UnityEngine; // MonoBehaviour, PlayerPrefs, Vector2
using UnityEngine.InputSystem; // New Input System API

namespace LastFrame.Player // Player scripts of Last Frame
{
    public enum CrouchMode { Hold, Toggle } // Hold = crouch while key held, Toggle = press to switch

    /// <summary>
    /// Reads player input with the Input System and exposes it to the other player scripts.
    /// Based on the Senses demo PlayerControler_MK2, without jump.
    /// </summary>
    [DisallowMultipleComponent] // Only one input reader per player
    public class PlayerInputReader : MonoBehaviour // Input layer of the player
    {
        const string CrouchModePrefKey = "LastFrame.CrouchMode"; // PlayerPrefs key used by the options menu

        [Header("Crouch")] // Inspector group title
        [Tooltip("Hold: crouch while Ctrl is held. Toggle: press Ctrl to crouch or stand. The options menu can change it with SetCrouchMode().")]
        [SerializeField] CrouchMode m_crouchMode = CrouchMode.Hold; // Current crouch mode, editable in the Inspector
        [Tooltip("Load the crouch mode saved by the options menu (PlayerPrefs) on start.")]
        [SerializeField] bool m_loadSavedCrouchMode = true; // If true, the saved option overrides the Inspector value

        InputAction m_move; // WASD / arrows / left stick
        InputAction m_sprint; // Shift / left stick press
        InputAction m_crouch; // Ctrl / gamepad east button
        InputAction m_attack; // Left click / gamepad west button
        InputAction m_switchWeapon; // Q / gamepad right shoulder
        InputAction m_pointer; // Mouse position on screen

        public Vector2 Move { get; private set; } // Movement input, magnitude 0 to 1
        public bool SprintHeld { get; private set; } // True while the run key is held
        public bool CrouchHeld { get; private set; } // True while the crouch key is held
        public bool CrouchPressedThisFrame { get; private set; } // True only on the frame the crouch key goes down
        public bool AttackPressedThisFrame { get; private set; } // True only on the frame left click goes down
        public bool SwitchWeaponPressedThisFrame { get; private set; } // True only on the frame Q goes down
        public Vector2 PointerPosition { get; private set; } // Mouse position in screen pixels (for aiming)
        public CrouchMode CrouchMode => m_crouchMode; // Read-only access to the crouch mode

        public event Action<CrouchMode> OnCrouchModeChanged; // Raised when the options menu changes the mode

        // Loads the saved crouch mode and creates the input actions with their bindings.
        void Awake()
        {
            if (m_loadSavedCrouchMode && PlayerPrefs.HasKey(CrouchModePrefKey)) // A saved option exists
                m_crouchMode = (CrouchMode)PlayerPrefs.GetInt(CrouchModePrefKey); // Use the saved option

            m_move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2"); // 2D movement action
            m_move.AddCompositeBinding("2DVector") // WASD composite
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s") // W forward, S back
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d"); // A left, D right
            m_move.AddCompositeBinding("2DVector") // Arrow keys composite
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow") // Up and down arrows
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow"); // Left and right arrows
            m_move.AddBinding("<Gamepad>/leftStick"); // Gamepad left stick

            m_sprint = new InputAction("Sprint", InputActionType.Button); // Run action
            m_sprint.AddBinding("<Keyboard>/leftShift"); // Left Shift
            m_sprint.AddBinding("<Keyboard>/rightShift"); // Right Shift
            m_sprint.AddBinding("<Gamepad>/leftStickPress"); // Gamepad stick click

            m_crouch = new InputAction("Crouch", InputActionType.Button); // Crouch action
            m_crouch.AddBinding("<Keyboard>/leftCtrl"); // Left Ctrl (GDD)
            m_crouch.AddBinding("<Keyboard>/rightCtrl"); // Right Ctrl
            m_crouch.AddBinding("<Gamepad>/buttonEast"); // Gamepad B / Circle

            m_attack = new InputAction("Attack", InputActionType.Button); // Attack action
            m_attack.AddBinding("<Mouse>/leftButton"); // Left click (GDD)
            m_attack.AddBinding("<Gamepad>/buttonWest"); // Gamepad X / Square

            m_switchWeapon = new InputAction("SwitchWeapon", InputActionType.Button); // Weapon switch action
            m_switchWeapon.AddBinding("<Keyboard>/q"); // Q (GDD)
            m_switchWeapon.AddBinding("<Gamepad>/rightShoulder"); // Gamepad RB / R1

            m_pointer = new InputAction("Pointer", InputActionType.PassThrough, "<Pointer>/position"); // Mouse or touch position
        }

        // Turns the input actions on when the component is enabled.
        void OnEnable()
        {
            m_move.Enable(); // Start reading movement
            m_sprint.Enable(); // Start reading run
            m_crouch.Enable(); // Start reading crouch
            m_attack.Enable(); // Start reading attack
            m_switchWeapon.Enable(); // Start reading weapon switch
            m_pointer.Enable(); // Start reading mouse position
        }

        // Turns the input actions off and clears the values so the player stops.
        void OnDisable()
        {
            m_move.Disable(); // Stop reading movement
            m_sprint.Disable(); // Stop reading run
            m_crouch.Disable(); // Stop reading crouch
            m_attack.Disable(); // Stop reading attack
            m_switchWeapon.Disable(); // Stop reading weapon switch
            m_pointer.Disable(); // Stop reading mouse position
            Move = Vector2.zero; // No movement while disabled
            SprintHeld = CrouchHeld = CrouchPressedThisFrame = false; // No buttons while disabled
            AttackPressedThisFrame = SwitchWeaponPressedThisFrame = false; // No attack or switch while disabled
        }

        // Frees the input actions when the object is destroyed.
        void OnDestroy()
        {
            m_move?.Dispose(); // Release movement action
            m_sprint?.Dispose(); // Release run action
            m_crouch?.Dispose(); // Release crouch action
            m_attack?.Dispose(); // Release attack action
            m_switchWeapon?.Dispose(); // Release weapon switch action
            m_pointer?.Dispose(); // Release pointer action
        }

        // Reads the current input once per frame.
        void Update()
        {
            Move = Vector2.ClampMagnitude(m_move.ReadValue<Vector2>(), 1f); // Diagonals are not faster
            SprintHeld = m_sprint.IsPressed(); // Run key state
            CrouchHeld = m_crouch.IsPressed(); // Crouch key state
            CrouchPressedThisFrame = m_crouch.WasPressedThisFrame(); // Crouch key just pressed
            AttackPressedThisFrame = m_attack.WasPressedThisFrame(); // Left click just pressed
            SwitchWeaponPressedThisFrame = m_switchWeapon.WasPressedThisFrame(); // Q just pressed
            PointerPosition = m_pointer.ReadValue<Vector2>(); // Current mouse position
        }

        // Changes the crouch mode (called by the options menu) and optionally saves it.
        public void SetCrouchMode(CrouchMode mode, bool save = true)
        {
            if (m_crouchMode == mode) return; // Nothing to change
            m_crouchMode = mode; // Apply the new mode
            if (save) // Persist the choice between sessions
            {
                PlayerPrefs.SetInt(CrouchModePrefKey, (int)mode); // Store mode as int
                PlayerPrefs.Save(); // Write to disk
            }
            OnCrouchModeChanged?.Invoke(mode); // Notify listeners (UI, movement)
        }
    }
}
