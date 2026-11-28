using System; // Action delegate for events
using LastFrame.UI; // HUDController (optional)
using UnityEngine; // MonoBehaviour, Mathf

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Ink ammo for the pen. Starts full at 3 (team decision 28/11, matches the HUD ink pot) and is refilled by ink pots.
    /// Keeping it between sets will be handled by the GameManager (#5).
    /// </summary>
    [DisallowMultipleComponent] // One ammo pool per player
    public class InkAmmo : MonoBehaviour // Ink resource
    {
        [SerializeField] int m_startInk = 3; // Team decision 28/11: start full
        [SerializeField] int m_maxInk = 3; // Team decision 28/11: 3 drops (HUD shows 3/3)
        [SerializeField] HUDController m_hud; // Optional HUD (ink pot)

        public int Current { get; private set; } // Ink drops available
        public int Max => m_maxInk; // Max ink for the HUD ink pot
        public event Action<int, int> OnInkChanged; // (current, max) for HUD and GameManager

        // Fills the ammo with the starting value.
        void Awake()
        {
            Current = Mathf.Clamp(m_startInk, 0, m_maxInk); // Start value inside the limits
        }

        // Notifies listeners once everything has subscribed.
        void Start()
        {
            Notify(); // First HUD update
        }

        // Uses one drop. Returns false if there is no ink.
        public bool TryConsume(int amount = 1)
        {
            if (Current < amount) return false; // Not enough ink
            Current -= amount; // Spend ink
            Notify(); // Update HUD
            return true; // Shot allowed
        }

        // Adds ink (ink pot pickup). Returns how much was really added.
        public int Add(int amount)
        {
            int before = Current; // Value before the pickup
            Current = Mathf.Clamp(Current + amount, 0, m_maxInk); // Add without passing the max
            if (Current != before) Notify(); // Update HUD only if it changed
            return Current - before; // Real amount added
        }

        // Sends the ink to the HUD and listeners.
        void Notify()
        {
            if (m_hud) m_hud.SetAmmo(Current); // HUD ink pot
            OnInkChanged?.Invoke(Current, m_maxInk); // Listeners
        }

        // Sets the ink directly (GameManager restores it when a new set loads).
        public void SetInk(int value)
        {
            Current = Mathf.Clamp(value, 0, m_maxInk); // Keep inside limits
            Notify(); // Update HUD
        }
    }
}
