using UnityEngine; // MonoBehaviour, Camera, Random
using UnityEngine.Rendering; // RenderPipelineManager (URP camera callbacks)

namespace LastFrame.Core // Shared scripts of Last Frame
{
    /// <summary>
    /// Short screen shake (Ollie's hits, issue #8).
    /// The offset is applied only while the camera renders and removed right after, so it never fights the follow script
    /// (the kit TestRoomCamera, or our own camera later) that moves the camera in LateUpdate.
    /// </summary>
    [RequireComponent(typeof(Camera))] // Lives on the camera it shakes
    public class CameraShake : MonoBehaviour // Render-time camera offset
    {
        [SerializeField] float m_frequency = 25f; // Shake changes per second
        [SerializeField] bool m_scaleWithTime = true; // Off = shake even when the game is paused (Time.timeScale 0)

        Camera m_camera; // Camera that gets the offset
        float m_timeLeft; // Seconds of shake left
        float m_duration; // Total duration of the current shake (for the fade)
        float m_strength; // Max offset in meters
        float m_seed; // Random start for the noise
        Vector3 m_applied; // Offset currently added to the camera

        // Caches the camera and a random seed.
        void Awake()
        {
            m_camera = GetComponent<Camera>(); // Required component
            m_seed = Random.value * 100f; // Different noise per camera
        }

        // Hooks into the render callbacks.
        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera; // Add the offset before rendering
            RenderPipelineManager.endCameraRendering += OnEndCamera; // Remove it after rendering
        }

        // Unhooks and removes any offset left.
        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera; // Unhook
            RenderPipelineManager.endCameraRendering -= OnEndCamera; // Unhook
            RemoveOffset(); // Leave the camera clean
        }

        /// <summary>Starts a shake. A stronger or longer shake replaces a weaker one.</summary>
        public void Shake(float duration, float strength)
        {
            if (duration <= 0f || strength <= 0f) return; // Nothing to do
            if (strength >= m_strength * (m_duration > 0f ? m_timeLeft / m_duration : 0f)) m_strength = strength; // Keep the stronger one
            m_timeLeft = Mathf.Max(m_timeLeft, duration); // Keep the longer one
            m_duration = m_timeLeft; // Fade over the whole time left
        }

        // Counts the shake down.
        void Update()
        {
            if (m_timeLeft <= 0f) return; // Not shaking
            m_timeLeft -= m_scaleWithTime ? Time.deltaTime : Time.unscaledDeltaTime; // Tick
            if (m_timeLeft <= 0f) { m_timeLeft = 0f; m_strength = 0f; } // Done
        }

        // Adds a fading noise offset right before this camera renders.
        void OnBeginCamera(ScriptableRenderContext context, Camera cam)
        {
            if (cam != m_camera || m_timeLeft <= 0f) return; // Other camera or not shaking
            float fade = m_duration > 0f ? m_timeLeft / m_duration : 0f; // 1 -> 0 over the shake
            float t = (m_scaleWithTime ? Time.time : Time.unscaledTime) * m_frequency; // Noise time
            Vector3 local = new Vector3(Mathf.PerlinNoise(m_seed, t) - 0.5f, Mathf.PerlinNoise(m_seed + 10f, t) - 0.5f, 0f) * 2f; // -1..1 on screen axes
            m_applied = transform.TransformVector(local) * (m_strength * fade); // World offset
            transform.position += m_applied; // Shift for this render only
        }

        // Removes the offset right after this camera rendered.
        void OnEndCamera(ScriptableRenderContext context, Camera cam)
        {
            if (cam != m_camera) return; // Other camera
            RemoveOffset(); // Back to the follow position
        }

        // Takes the applied offset back off the camera.
        void RemoveOffset()
        {
            if (m_applied == Vector3.zero) return; // Nothing applied
            transform.position -= m_applied; // Undo
            m_applied = Vector3.zero; // Clean
        }
    }
}
