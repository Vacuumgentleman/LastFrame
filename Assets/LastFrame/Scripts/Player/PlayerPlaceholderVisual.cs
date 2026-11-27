using LastFrame.Weapons; // WeaponSwitcher
using TMPro; // TextMeshPro world text
using UnityEngine; // MonoBehaviour, SpriteRenderer, Color

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Temporary 2D look for Ollie until the art team delivers sprites and animations.
    /// Tints the sprite and writes the current movement state above it.
    /// </summary>
    [DisallowMultipleComponent] // One placeholder visual per player
    public class PlayerPlaceholderVisual : MonoBehaviour // Visual feedback of the player state
    {
        enum MoveState { Idle, Walk, Run, Crouch } // States shown by the placeholder

        [Header("References")] // Inspector group title
        [SerializeField] PlayerMovement m_movement; // Source of the movement state
        [SerializeField] SpriteRenderer m_sprite; // Sprite to tint (later: Ollie's sprite)
        [SerializeField] TextMeshPro m_label; // Text above the sprite
        [Tooltip("Transform scaled when crouching. Its pivot should be at Ollie's feet. Empty = the sprite transform.")]
        [SerializeField] Transform m_scaleRoot; // Squashed toward the feet when crouching
        [Tooltip("Optional: shows the equipped weapon on a second line.")]
        [SerializeField] WeaponSwitcher m_weapons; // Source of the equipped weapon
        [Tooltip("Optional: shows the ink left next to the weapon.")]
        [SerializeField] InkAmmo m_ammo; // Source of the ink amount

        [Header("Colors")] // Inspector group title
        [SerializeField] Color m_idleColor = new Color(0.85f, 0.85f, 0.85f); // Light gray when standing still
        [SerializeField] Color m_walkColor = new Color(0.30f, 0.80f, 0.35f); // Green when walking
        [SerializeField] Color m_runColor = new Color(0.95f, 0.45f, 0.15f); // Orange when running (noisy)
        [SerializeField] Color m_crouchColor = new Color(0.30f, 0.45f, 0.95f); // Blue when crouching (stealth)

        [Header("Labels")] // Inspector group title
        [SerializeField] string m_idleText = "IDLE"; // Text when standing still
        [SerializeField] string m_walkText = "WALK"; // Text when walking
        [SerializeField] string m_runText = "RUN"; // Text when running
        [SerializeField] string m_crouchText = "CROUCH"; // Text when crouching

        [Header("Crouch squash")] // Inspector group title
        [Tooltip("Sprite height multiplier while crouching, so the change is visible.")]
        [SerializeField] float m_crouchScaleY = 0.6f; // 60% height when crouching
        [SerializeField] float m_scaleSpeed = 12f; // How fast the sprite squashes

        MoveState m_state = (MoveState)(-1); // Invalid value so the first frame always refreshes
        Vector3 m_baseScale; // Sprite scale when standing
        string m_lastLabel; // Last text written, to avoid rebuilding it every frame

        // Finds missing references and saves the standing scale.
        void Awake()
        {
            if (!m_movement) m_movement = GetComponentInParent<PlayerMovement>(); // Movement is on the Player root
            if (!m_sprite) m_sprite = GetComponentInChildren<SpriteRenderer>(); // Sprite under this object
            if (!m_label) m_label = GetComponentInChildren<TextMeshPro>(); // Label under this object
            if (!m_weapons) m_weapons = GetComponentInParent<WeaponSwitcher>(); // Weapons are on the Player root
            if (!m_ammo) m_ammo = GetComponentInParent<InkAmmo>(); // Ink is on the Player root
            if (!m_scaleRoot && m_sprite) m_scaleRoot = m_sprite.transform; // Default squash target
            if (m_scaleRoot) m_baseScale = m_scaleRoot.localScale; // Remember standing size
        }

        // Updates color, text and squash from the current movement state.
        void Update()
        {
            if (!m_movement) return; // Nothing to read
            MoveState next = GetState(); // State this frame
            if (next != m_state) ApplyState(next); // Only repaint when it changes
            UpdateLabel(); // State, weapon and ink text

            if (m_scaleRoot) // Smooth squash while crouching
            {
                Vector3 target = m_baseScale; // Standing size
                if (m_state == MoveState.Crouch) target.y *= m_crouchScaleY; // Shorter when crouching
                m_scaleRoot.localScale = Vector3.Lerp(m_scaleRoot.localScale, target, m_scaleSpeed * Time.deltaTime); // Ease to target
            }
        }

        // Reads PlayerMovement and returns the state to show.
        MoveState GetState()
        {
            if (m_movement.IsCrouching) return MoveState.Crouch; // Crouch wins over moving
            if (m_movement.IsRunning) return MoveState.Run; // Running
            if (m_movement.IsMoving) return MoveState.Walk; // Walking
            return MoveState.Idle; // Standing still
        }

        // Sets the color and text for a state.
        void ApplyState(MoveState state)
        {
            m_state = state; // Store current state
            Color color = state == MoveState.Run ? m_runColor : state == MoveState.Walk ? m_walkColor : state == MoveState.Crouch ? m_crouchColor : m_idleColor; // Pick color
            if (m_sprite) m_sprite.color = color; // Tint sprite
            if (m_label) m_label.color = color; // Same color as the sprite
        }

        // Writes "STATE" plus "WEAPON · ink" on a second line, only when something changed.
        void UpdateLabel()
        {
            if (!m_label) return; // No label to write
            string text = m_state == MoveState.Run ? m_runText : m_state == MoveState.Walk ? m_walkText : m_state == MoveState.Crouch ? m_crouchText : m_idleText; // State name
            if (m_weapons && m_weapons.Current) // A weapon is equipped
            {
                text += "\n<size=70%>" + m_weapons.Current.DisplayName; // Weapon name, smaller
                if (m_ammo) text += " · " + m_ammo.Current; // Ink left
                text += "</size>"; // Close size tag
            }
            if (text == m_lastLabel) return; // Nothing new
            m_lastLabel = text; // Remember it
            m_label.text = text; // Write it
        }
    }
}
