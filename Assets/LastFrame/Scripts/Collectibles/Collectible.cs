using System; // Action delegates for events
using LastFrame.Player; // PlayerInputReader (E key)
using TMPro; // TextMeshPro prompt
using UnityEngine; // MonoBehaviour, AudioSource

namespace LastFrame.Collectibles // Pickups of Last Frame
{
    /// <summary>
    /// Base class of every pickup (issue #4). Ollie picks it up with E when she is close.
    /// Floats, glows with a pencil outline and shows the "E" prompt; plays a sound and raises events for the player and the GameManager (#5).
    /// A new pickup is a new class that overrides OnCollect (and CanCollect if it can be refused), without changing this base.
    /// </summary>
    public abstract class Collectible : MonoBehaviour // Abstract pickup
    {
        [Header("Pick up")]
        [SerializeField] float m_radius = 1.2f; // Distance on the floor to pick it up (meters)
        [SerializeField] string m_promptText = "E"; // Shown when Ollie can pick it up

        [Header("Look (placeholder)")]
        [Tooltip("Child that floats. Empty = child named \"Visual\".")]
        [SerializeField] Transform m_visual; // Floating part
        [SerializeField] float m_bobHeight = 0.15f; // Float amplitude (meters)
        [SerializeField] float m_bobSpeed = 2f; // Float speed
        [Tooltip("Pencil outline that glows. Empty = child named \"Outline\".")]
        [SerializeField] SpriteRenderer m_outline; // Glowing outline
        [SerializeField] float m_glowSpeed = 3f; // Glow pulse speed
        [SerializeField] Vector2 m_glowAlpha = new Vector2(0.35f, 1f); // Min and max outline alpha
        [Tooltip("Prompt above the pickup. Empty = child named \"Prompt\".")]
        [SerializeField] TextMeshPro m_prompt; // "E" or the reason it cannot be picked up

        [Header("Sound")]
        [SerializeField] AudioClip m_sound; // Inkwell glug-glug, heart bell, tape click...
        [SerializeField, Range(0f, 1f)] float m_volume = 1f; // Sound volume

        static PlayerInputReader s_player; // Ollie (cached for every pickup)
        Vector3 m_visualStart; // Visual local position at rest
        float m_phase; // Random float phase so pickups do not move in sync

        public bool IsCollected { get; private set; } // True once picked up

        /// <summary>Any pickup was collected (pickup, player). For the GameManager (#5), sound or stats.</summary>
        public static event Action<Collectible, GameObject> OnAnyCollected; // Global notice
        /// <summary>This pickup was collected (player).</summary>
        public event Action<GameObject> OnCollected; // Local notice

        // Finds the optional children and remembers the rest position.
        protected virtual void Awake()
        {
            if (!m_visual) m_visual = transform.Find("Visual"); // Default floating part
            if (!m_outline && m_visual) { var o = m_visual.Find("Outline"); if (o) m_outline = o.GetComponent<SpriteRenderer>(); } // Default outline
            if (!m_prompt && m_visual) { var pr = m_visual.Find("Prompt"); if (pr) m_prompt = pr.GetComponent<TextMeshPro>(); } // Default prompt
            if (m_visual) m_visualStart = m_visual.localPosition; // Rest position
            m_phase = UnityEngine.Random.value * 10f; // Desync
            if (m_prompt) m_prompt.gameObject.SetActive(false); // Hidden until Ollie is near
        }

        // Floats, glows, shows the prompt and listens for E while Ollie is close.
        protected virtual void Update()
        {
            if (IsCollected) return; // Already taken
            Animate(); // Float and glow
            PlayerInputReader player = FindPlayer(); // Ollie
            bool near = player && IsNear(player.transform.position); // In range?
            bool can = near && CanCollect(player.gameObject); // Allowed right now?
            if (m_prompt) // Prompt above the pickup
            {
                m_prompt.gameObject.SetActive(near); // Only when near
                if (near) m_prompt.text = can ? m_promptText : CannotCollectText; // "E" or the reason
            }
            if (can && player.InteractPressedThisFrame) Collect(player.gameObject); // Picked up with E
        }

        /// <summary>Picks it up without the E key (tests, scripted events). Returns false if it was refused.</summary>
        public bool TryCollect(GameObject player)
        {
            if (IsCollected || !player || !CanCollect(player)) return false; // Not possible
            Collect(player); // Take it
            return true; // Done
        }

        /// <summary>True if the player can take it now (e.g. not with full hearts).</summary>
        protected virtual bool CanCollect(GameObject player) => true; // Default: always

        /// <summary>Prompt text when CanCollect is false.</summary>
        protected virtual string CannotCollectText => ""; // Default: nothing

        /// <summary>What the pickup does to the player.</summary>
        protected abstract void OnCollect(GameObject player);

        // Applies the effect, plays the sound, raises the events and hides the pickup.
        void Collect(GameObject player)
        {
            IsCollected = true; // Only once
            OnCollect(player); // Effect of the derived class
            if (m_sound) AudioSource.PlayClipAtPoint(m_sound, transform.position, m_volume); // Pickup sound
            OnCollected?.Invoke(player); // Local listeners
            OnAnyCollected?.Invoke(this, player); // GameManager and others
            gameObject.SetActive(false); // Gone from the set
        }

        // True when Ollie is within the radius on the floor and roughly at the same height.
        bool IsNear(Vector3 playerPos)
        {
            Vector3 d = playerPos - transform.position; // Offset
            if (Mathf.Abs(d.y) > 2f) return false; // Different floor
            d.y = 0f; // Flat distance
            return d.sqrMagnitude <= m_radius * m_radius; // Inside the radius
        }

        // Bobs the visual and pulses the outline alpha.
        void Animate()
        {
            float t = Time.time + m_phase; // Local time
            if (m_visual) m_visual.localPosition = m_visualStart + Vector3.up * (Mathf.Sin(t * m_bobSpeed) * m_bobHeight); // Float
            if (m_outline) // Glow
            {
                Color c = m_outline.color; // Current color
                c.a = Mathf.Lerp(m_glowAlpha.x, m_glowAlpha.y, (Mathf.Sin(t * m_glowSpeed) + 1f) * 0.5f); // Pulse
                m_outline.color = c; // Apply
            }
        }

        // Finds Ollie once and keeps the reference.
        static PlayerInputReader FindPlayer()
        {
            if (!s_player) s_player = FindFirstObjectByType<PlayerInputReader>(); // Unity null check (no ??)
            return s_player; // Ollie or null
        }

        // Shows the pick up radius in the Scene view.
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f); // Gold
            Gizmos.DrawWireSphere(transform.position, m_radius); // Radius
        }
    }
}
