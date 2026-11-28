using System; // Action delegates for events
using LastFrame.Core; // IDamageable, DamageInfo, CameraShake
using LastFrame.UI; // HUDController (optional)
using Senses2026; // Scent (kit ink trail)
using UnityEngine; // MonoBehaviour, ParticleSystem

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Ollie's hearts and ink bleeding (issue #8).
    /// Each hit removes a heart, shakes the camera and splashes ink. While she bleeds (~6 s) she moves slower and the kit's
    /// Scent drops trail points that last 6 s each; enemies follow them with DidSmell. Hiding does not erase the points.
    /// At 0 hearts she is erased: controls are disabled and OnDied is raised (game over lives in #9).
    /// </summary>
    [DisallowMultipleComponent] // One health per player
    public class PlayerHealth : MonoBehaviour, IDamageable // Hit receiver for the enemy eraser
    {
        [Header("Hearts")]
        [SerializeField, Min(1)] int m_maxHearts = 3; // GDD: 3 hearts

        [Header("Ink bleeding")]
        [SerializeField] float m_bleedDuration = 6f; // GDD: bleeds for ~6 s after a hit
        [Tooltip("Speed while bleeding. The GDD only says 'slower': value pending team agreement.")]
        [SerializeField, Range(0.1f, 1f)] float m_bleedSpeedMultiplier = 0.75f; // Placeholder slow down
        [SerializeField] Scent m_scent; // Kit scent emitter (the ink trail). Empty = the one on this object
        [Tooltip("Seconds each trail point lasts (kit: Scent strength = lifetime).")]
        [SerializeField] float m_trailLifetime = 6f; // GDD: each point disappears after 6 s
        [Tooltip("Distance between trail points. The kit drops points by distance, not by time (GDD: every 0.3 s ≈ 0.7 m at bleeding walk speed).")]
        [SerializeField] float m_trailSpacing = 0.7f; // Meters between points
        [SerializeField] Gradient m_trailColor = DefaultInkGradient(); // Black ink fading out
        [Tooltip("Off = the scent is always on (old smell demo stations).")]
        [SerializeField] bool m_scentOnlyWhileBleeding = true; // GDD: no trail unless bleeding

        [Header("Hit feedback")]
        [SerializeField] CameraShake m_cameraShake; // Empty = found or added on the main camera
        [SerializeField] float m_shakeDuration = 0.25f; // Seconds of shake per hit
        [SerializeField] float m_shakeStrength = 0.2f; // Meters of shake per hit
        [SerializeField] ParticleSystem m_inkSplash; // Optional ink burst on hit
        [SerializeField] HUDController m_hud; // Optional HUD (hearts icon)

        [Header("Death")]
        [Tooltip("Components turned off when Ollie is erased (movement, weapons...). Empty = PlayerMovement and WeaponSwitcher.")]
        [SerializeField] Behaviour[] m_disableOnDeath; // Controls to stop on death

        PlayerMovement m_movement; // Gets the bleeding slow down
        const float k_noTrailSpacing = 100000f; // Spacing that never drops a point

        public int Hearts { get; private set; } // Current hearts
        public int MaxHearts => m_maxHearts; // Max hearts
        public float BleedTimeLeft { get; private set; } // Seconds of bleeding left
        public bool IsBleeding => BleedTimeLeft > 0f; // True while the ink trail is on
        public bool IsDead => Hearts <= 0; // True after the last heart

        public event Action<int> OnHeartsChanged; // New heart count (HUD)
        public event Action<DamageInfo> OnHit; // A hit landed (sound, VFX)
        public event Action<bool> OnBleedingChanged; // Bleeding started (true) or stopped (false)
        public event Action OnDied; // 0 hearts: game over (#9)

        // Caches components and prepares the kit scent before its Start runs.
        void Awake()
        {
            m_movement = GetComponent<PlayerMovement>(); // Speed control
            if (!m_scent) m_scent = GetComponent<Scent>(); // Kit trail on Ollie
            if (m_disableOnDeath == null || m_disableOnDeath.Length == 0) m_disableOnDeath = new Behaviour[] { m_movement, GetComponent<Weapons.WeaponSwitcher>() }; // Default controls
            Hearts = m_maxHearts; // Full health
            if (m_scent) // Configure the kit trail
            {
                m_scent.Set_ScentStrength(m_trailLifetime); // Point lifetime in seconds
                m_scent.Set_ScentColorGradient(m_trailColor); // Ink color
                if (m_scentOnlyWhileBleeding) SetTrail(false); // No trail until hit
                else m_scent.Set_NodeSeparation(m_trailSpacing); // Always on
            }
        }

        // Finds the camera shake and tells the HUD the starting hearts.
        void Start()
        {
            if (!m_cameraShake && Camera.main) m_cameraShake = Camera.main.GetComponent<CameraShake>(); // Reuse the camera's shake
            if (!m_cameraShake && Camera.main) m_cameraShake = Camera.main.gameObject.AddComponent<CameraShake>(); // Or add one (no ?? with Unity objects)
            NotifyHearts(); // HUD shows 3 hearts
        }

        // Counts the bleeding time down.
        void Update()
        {
            if (!IsBleeding) return; // Nothing to count
            BleedTimeLeft -= Time.deltaTime; // Tick
            if (BleedTimeLeft <= 0f) StopBleeding(); // Wound closed
        }

        /// <summary>A hit from an enemy (the eraser): one heart, shake, splash and bleeding.</summary>
        public void TakeDamage(DamageInfo info)
        {
            if (IsDead) return; // Already erased
            Hearts = Mathf.Max(0, Hearts - Mathf.Max(1, info.Amount)); // At least one heart per hit
            if (m_cameraShake) m_cameraShake.Shake(m_shakeDuration, m_shakeStrength); // Screen shake
            if (m_inkSplash) m_inkSplash.Play(); // Ink burst
            OnHit?.Invoke(info); // Sound / VFX listeners
            NotifyHearts(); // HUD broken heart
            if (IsDead) Die(); // Last heart
            else StartBleeding(); // Ink trail and slow down
        }

        /// <summary>Gives hearts back (heart pickup, #4). Does not revive.</summary>
        public void Heal(int amount = 1)
        {
            if (IsDead || amount <= 0) return; // Dead or nothing to add
            Hearts = Mathf.Min(m_maxHearts, Hearts + amount); // Cap at max
            NotifyHearts(); // HUD
        }

        // Starts (or restarts) the ~6 s of bleeding.
        void StartBleeding()
        {
            bool wasBleeding = IsBleeding; // To raise the event once
            BleedTimeLeft = m_bleedDuration; // Restart the timer on every hit
            if (m_movement) m_movement.SpeedMultiplier = m_bleedSpeedMultiplier; // Slower
            if (m_scentOnlyWhileBleeding) SetTrail(true); // Drop ink points
            if (!wasBleeding) OnBleedingChanged?.Invoke(true); // Bleeding started
        }

        // Ends the bleeding: normal speed, no new points (old points stay until they expire).
        void StopBleeding()
        {
            BleedTimeLeft = 0f; // Done
            if (m_movement) m_movement.SpeedMultiplier = 1f; // Normal speed
            if (m_scentOnlyWhileBleeding) SetTrail(false); // Stop dropping points
            OnBleedingChanged?.Invoke(false); // Bleeding stopped
        }

        // Turns the kit trail on or off without disabling the Scent component (re-enabling it breaks its Zoner buffer).
        void SetTrail(bool on)
        {
            if (!m_scent) return; // No kit trail
            m_scent.Set_NodeSeparation(on ? m_trailSpacing : k_noTrailSpacing); // Points by distance, or never
            m_scent.Set_UseVisualisation(on); // Trail renderer emits only while bleeding (old segments fade out)
        }

        // 0 hearts: Ollie is erased.
        void Die()
        {
            if (IsBleeding) StopBleeding(); // No trail after death
            foreach (var b in m_disableOnDeath) if (b) b.enabled = false; // Stop controls
            OnDied?.Invoke(); // Game over (#9)
            Debug.Log("[PlayerHealth] Ollie was erased (0 hearts).", this); // Until #9 exists
        }

        // Sends the heart count to listeners and the HUD.
        void NotifyHearts()
        {
            if (m_hud) m_hud.SetHealth(Hearts); // HUD icon
            OnHeartsChanged?.Invoke(Hearts); // Listeners
        }

        // Black ink that fades out along the trail.
        static Gradient DefaultInkGradient()
        {
            var g = new Gradient(); // New gradient
            g.SetKeys(new[] { new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 0f), new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 1f) }, // Ink blue-black
                      new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }); // Fades at the old end
            return g; // Ready
        }
    }
}
