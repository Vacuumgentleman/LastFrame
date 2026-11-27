using TMPro; // TextMeshPro world text
using UnityEngine; // MonoBehaviour, SpriteRenderer, Color

namespace LastFrame.Enemies // Enemy scripts of Last Frame
{
    /// <summary>
    /// Temporary feedback for the AI cartoon until the final art arrives.
    /// GDD conveyance: nothing while patrolling, "?" when investigating, "!" when chasing.
    /// </summary>
    [DisallowMultipleComponent] // One visual per enemy
    public class EnemyStateVisual : MonoBehaviour // Enemy state feedback
    {
        [SerializeField] EnemyAI m_ai; // Source of the state
        [SerializeField] SpriteRenderer m_sprite; // Drawing to tint
        [SerializeField] SpriteRenderer m_facingMarker; // Floor line that shows where it looks
        [SerializeField] TextMeshPro m_label; // "?" / "!" above the head

        [SerializeField] Color m_patrolColor = new Color(1f, 0.85f, 0.2f); // Bright AI yellow
        [SerializeField] Color m_investigateColor = new Color(1f, 0.6f, 0.1f); // Orange when suspicious
        [SerializeField] Color m_chaseColor = new Color(1f, 0.2f, 0.2f); // Red when chasing
        [SerializeField] Color m_deadColor = new Color(0.15f, 0.15f, 0.45f); // Ink blue when erased

        // Finds the AI and subscribes to its state changes.
        void Awake()
        {
            if (!m_ai) m_ai = GetComponentInParent<EnemyAI>(); // AI is on the enemy root
        }

        // Starts listening to state changes.
        void OnEnable()
        {
            if (m_ai) m_ai.OnStateChanged += Apply; // Subscribe
        }

        // Stops listening to state changes.
        void OnDisable()
        {
            if (m_ai) m_ai.OnStateChanged -= Apply; // Unsubscribe
        }

        [Tooltip("Debug: shows the sight awareness (0-100) under the symbol.")]
        [SerializeField] bool m_showAwareness = true; // Evidence of how Senses perceives Ollie

        // Keeps the label up to date (reason and awareness can change without a state change).
        void LateUpdate()
        {
            if (!m_ai || !m_label || m_ai.State == EnemyState.Dead) return; // Nothing to show
            string symbol = m_ai.State == EnemyState.Chase ? "!" : m_ai.State == EnemyState.Investigate ? "?" + ReasonText(m_ai.LastStimulus) : ""; // GDD symbols
            string awareness = m_showAwareness && m_ai.SeeAwareness > 0f ? $"\n<size=35%>{m_ai.SeeAwareness:0}/{m_ai.AlertAwareness}</size>" : ""; // e.g. "34/50"
            m_label.text = symbol + awareness; // Final label
        }

        // Short debug text for the reason the enemy is investigating.
        static string ReasonText(EnemyStimulus stimulus)
        {
            switch (stimulus) // Map reason to text
            {
                case EnemyStimulus.Heard: return "\n<size=40%>(heard)</size>"; // Heard Ollie running
                case EnemyStimulus.InkImpact: return "\n<size=40%>(ink)</size>"; // Ink drop impact
                case EnemyStimulus.Seen: return "\n<size=40%>(last seen)</size>"; // Lost sight, checking last seen spot
                case EnemyStimulus.Glimpsed: return "\n<size=40%>(glimpse)</size>"; // Saw something, not sure yet
                case EnemyStimulus.Smelled: return "\n<size=40%>(smell)</size>"; // Following a scent
                default: return ""; // No reason
            }
        }

        // Paints the current state on the first frame.
        void Start()
        {
            if (m_ai) Apply(m_ai.State); // Initial look
        }

        // Updates color and symbol for a state.
        void Apply(EnemyState state)
        {
            Color color = state == EnemyState.Chase ? m_chaseColor : state == EnemyState.Investigate ? m_investigateColor : state == EnemyState.Dead ? m_deadColor : m_patrolColor; // Pick color
            string symbol = state == EnemyState.Chase ? "!" : state == EnemyState.Investigate ? "?" : ""; // GDD symbols
            if (m_sprite) m_sprite.color = color; // Tint drawing
            if (m_facingMarker) m_facingMarker.color = color; // Tint floor marker
            if (m_label) { m_label.text = symbol; m_label.color = color; } // Symbol above the head
        }
    }
}
