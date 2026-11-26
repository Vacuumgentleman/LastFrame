using System; // Action delegate for events
using LastFrame.Player; // PlayerInputReader
using UnityEngine; // MonoBehaviour

namespace LastFrame.Weapons // Weapon scripts of Last Frame
{
    /// <summary>
    /// Switches between the ink pen and the corrector with Q and sends left click to the active weapon.
    /// </summary>
    [DisallowMultipleComponent] // One switcher per player
    [RequireComponent(typeof(PlayerInputReader))] // Reads attack and switch input
    public class WeaponSwitcher : MonoBehaviour // Weapon selection
    {
        [Tooltip("Weapons in switch order. Element 0 is equipped at start (GDD: pen first).")]
        [SerializeField] Weapon[] m_weapons; // Pen and corrector

        PlayerInputReader m_input; // Cached input reader
        int m_index = -1; // Active weapon index

        public Weapon Current => m_index >= 0 ? m_weapons[m_index] : null; // Active weapon or null
        public event Action<Weapon> OnWeaponChanged; // For HUD and placeholder label

        // Caches input and finds weapons if the list is empty.
        void Awake()
        {
            m_input = GetComponent<PlayerInputReader>(); // Required component
            if (m_weapons == null || m_weapons.Length == 0) m_weapons = GetComponentsInChildren<Weapon>(); // Auto-fill list
        }

        // Equips the first weapon.
        void Start()
        {
            for (int i = 0; i < m_weapons.Length; i++) m_weapons[i].SetEquipped(false); // Start with all off
            if (m_weapons.Length > 0) Equip(0); // Equip the pen
        }

        // Handles switch and attack input every frame.
        void Update()
        {
            if (m_weapons.Length == 0) return; // Nothing to use
            if (m_input.SwitchWeaponPressedThisFrame) Equip((m_index + 1) % m_weapons.Length); // Q = next weapon
            if (m_input.AttackPressedThisFrame) Current?.TryAttack(); // Left click = attack
        }

        // Activates the weapon at an index and deactivates the previous one.
        public void Equip(int index)
        {
            if (index == m_index || index < 0 || index >= m_weapons.Length) return; // Invalid or same weapon
            if (Current) Current.SetEquipped(false); // Put away old weapon
            m_index = index; // Store new index
            Current.SetEquipped(true); // Take out new weapon
            OnWeaponChanged?.Invoke(Current); // Notify HUD / label
        }
    }
}
