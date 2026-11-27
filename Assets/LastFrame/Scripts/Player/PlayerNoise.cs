using LastFrame.Core; // NoiseEvents
using UnityEngine; // MonoBehaviour, Time

namespace LastFrame.Player // Player scripts of Last Frame
{
    /// <summary>
    /// Makes noise while Ollie runs. Walking and crouching are silent.
    /// Enemies closer than the radius hear the exact point where she was (NoiseEvents), nothing farther.
    /// </summary>
    [DisallowMultipleComponent] // Only one noise emitter per player
    [RequireComponent(typeof(PlayerMovement))] // Reads the running state
    public class PlayerNoise : MonoBehaviour // Noise layer of the player
    {
        [Tooltip("How far enemies hear Ollie running, in meters.")]
        [SerializeField] float m_runNoiseRadius = 5f; // Run noise range
        [Tooltip("Seconds between noise pulses while running.")]
        [SerializeField] float m_runNoiseInterval = 0.4f; // Time between pulses
        [Tooltip("Debug: write a console line every time Ollie makes noise.")]
        [SerializeField] bool m_logNoise = true; // Console feedback while testing

        PlayerMovement m_movement; // Cached movement script
        float m_timer; // Countdown to the next pulse

        public float RunNoiseRadius => m_runNoiseRadius; // For the enemy hearing debug circle
        public float CurrentNoiseRadius => m_movement && m_movement.IsRunning ? m_runNoiseRadius : 0f; // For the debug ring and HUD

        // Caches the movement component.
        void Awake()
        {
            m_movement = GetComponent<PlayerMovement>(); // Required component
        }

        // Emits a noise pulse every interval while Ollie runs on the ground.
        void Update()
        {
            if (!m_movement.IsRunning || !m_movement.IsGrounded) // Not running or in the air
            {
                m_timer = 0f; // Next run starts with a pulse right away
                return; // No noise
            }

            m_timer -= Time.deltaTime; // Count down
            if (m_timer > 0f) return; // Not time yet

            NoiseEvents.Emit(transform.position, m_runNoiseRadius, NoiseSource.Footsteps); // Enemies within the radius hear this point
            if (m_logNoise) Debug.Log($"[PlayerNoise] Run noise {m_runNoiseRadius} m at {transform.position} (running={m_movement.IsRunning}, crouching={m_movement.IsCrouching}, speed={m_movement.CurrentSpeed:0.0})", this); // Debug
            m_timer = m_runNoiseInterval; // Restart the countdown
        }
    }
}
