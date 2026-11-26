using Senses2026; // Senses Noise component
using UnityEngine; // MonoBehaviour, Time

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Emits Senses noise while Ollie runs. Walking and crouching are silent.
    /// </summary>
    [DisallowMultipleComponent] // Only one noise emitter per player
    [RequireComponent(typeof(PlayerMovement))] // Reads the running state
    [RequireComponent(typeof(Noise))] // Sends noise through Senses
    public class PlayerNoise : MonoBehaviour // Noise layer of the player
    {
        [Tooltip("Noise released while running, in dB (Senses). 80 dB is the package default. Tune by ear; the GDD target is about 8 m.")]
        [SerializeField] float m_runNoiseDb = 80f; // Noise strength while running
        [Tooltip("Seconds between noise pulses while running.")]
        [SerializeField] float m_runNoiseInterval = 0.4f; // Time between pulses

        PlayerMovement m_movement; // Cached movement script
        Noise m_noise; // Cached Senses noise component
        float m_timer; // Countdown to the next pulse

        // Caches the movement and noise components.
        void Awake()
        {
            m_movement = GetComponent<PlayerMovement>(); // Required component
            m_noise = GetComponent<Noise>(); // Required component
        }

        // Releases a noise pulse every interval while Ollie runs on the ground.
        void Update()
        {
            if (!m_movement.IsRunning || !m_movement.IsGrounded) // Not running or in the air
            {
                m_timer = 0f; // Next run starts with a pulse right away
                return; // No noise
            }

            m_timer -= Time.deltaTime; // Count down
            if (m_timer > 0f) return; // Not time yet

            m_noise.ReleaseNoise(m_runNoiseDb); // Tell nearby enemies through Senses
            m_timer = m_runNoiseInterval; // Restart the countdown
        }
    }
}
