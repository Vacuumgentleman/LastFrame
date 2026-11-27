using UnityEngine; // MonoBehaviour, Time

namespace LastFrame.Weapons // Weapon scripts of Last Frame
{
    /// <summary>
    /// Base class for Ollie's weapons. Handles the cooldown; each weapon implements its own attack.
    /// </summary>
    public abstract class Weapon : MonoBehaviour // Shared weapon behaviour (OOP: inheritance)
    {
        [SerializeField] string m_displayName = "Weapon"; // Name shown in the HUD / label
        [SerializeField] float m_cooldown = 0.5f; // Seconds between attacks

        float m_nextAttackTime; // Time when the weapon can attack again

        public string DisplayName => m_displayName; // Read-only name
        public bool IsEquipped { get; private set; } // True while this is the active weapon
        public bool IsReady => Time.time >= m_nextAttackTime; // True when the cooldown is over

        // Tries to attack. Returns true if the attack happened.
        public bool TryAttack()
        {
            if (!IsEquipped || !IsReady) return false; // Not active or still cooling down
            if (!PerformAttack()) return false; // Weapon could not attack (e.g. no ink)
            m_nextAttackTime = Time.time + m_cooldown; // Start cooldown
            return true; // Attack done
        }

        // Called by the WeaponSwitcher when this weapon becomes active or inactive.
        public void SetEquipped(bool equipped)
        {
            IsEquipped = equipped; // Store state
            OnEquippedChanged(equipped); // Let the child react (show model, etc.)
        }

        // Each weapon implements its attack here. Return false if it could not attack.
        protected abstract bool PerformAttack();

        // Optional hook for children when equipped or unequipped.
        protected virtual void OnEquippedChanged(bool equipped) { }
    }
}
