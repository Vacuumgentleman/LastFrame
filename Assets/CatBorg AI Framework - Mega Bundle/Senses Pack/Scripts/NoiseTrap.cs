using UnityEngine;

namespace Senses2026
{
    [RequireComponent(typeof(Noise))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(TargetSenses))]
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 41: Noise Trap", 41)]
    public class NoiseTrap : MonoBehaviour
    {
        [field: SerializeField] public float NoiseStrength { get; private set; } = 80f;
        public void SetNoiseStrength(float _noiseStrength) => NoiseStrength = _noiseStrength;
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;
        [field: SerializeField] bool m_helpPropertiesFoldout;
        public void ToggleHelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;

        Noise m_noise;
        Rigidbody m_rigidbody;
        float m_lastNoiseTime;
        float m_noiseCooldown = 0.2f;

        bool m_wasEnabled = false;

        void Start()
        {
            if (!m_wasEnabled)
                enable();
        }

        void enable()
        {
            // Noise required
            if (!TryGetComponent(out m_noise))
            {
                Debug.LogError("[NoiseTrap] Missing required component <Noise>. Add component manually.", this);
                return;
            }

            // Rigidbody required
            if (!TryGetComponent(out m_rigidbody))
            {
                Debug.LogError("[NoiseTrap] Missing required Rigidbody component. Add Rigidbody to this object.", this);
                return;
            }

            // ColliderReference is not required but must exist
            if (!TryGetComponent(out ColliderReference _colliderReference))
            {
                _colliderReference = gameObject.AddComponent<ColliderReference>();
                Debug.Log("[NoiseTrap] Missing ColliderReference. Auto-created hidden maintenance component.", this);
            }

            m_wasEnabled = true;
        }

        public void AddForceAtPosition(Vector3 _forceVector, Vector3 _forcePosition, ForceMode _forceMode)
        {
            if (!m_wasEnabled)
            {
                Debug.LogWarning("[NoiseTrap] Cannot apply force: NoiseTrap failed during initialization.", this);
                return;
            }

            if (Time.time - m_lastNoiseTime < m_noiseCooldown)
                return;

            m_lastNoiseTime = Time.time;

            m_rigidbody.AddForceAtPosition(_forceVector, _forcePosition, _forceMode);
            m_noise.ReleaseNoise(NoiseStrength);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!m_wasEnabled)
                return;

            if (Time.time - m_lastNoiseTime < m_noiseCooldown)
                return;

            m_lastNoiseTime = Time.time;

            // Ignore tiny contacts
            float impact = collision.relativeVelocity.magnitude;
            if (impact < 0.1f)
                return;

            if (collision.contactCount > 0)
            {
                Vector3 point = collision.GetContact(0).point;
                Vector3 force = collision.relativeVelocity * .5f;
                m_rigidbody.AddForceAtPosition(force, point, ForceMode.Impulse);
            }

            // Calculate dynamic noise
            float dynamicNoise = NoiseStrength * Mathf.Clamp01(impact / 5f);

            ColliderReference _colliderReference = GlobalColliderReferenceRegistry.Get(collision.collider);
            m_noise.ReleaseNoise(dynamicNoise);
        }
    }
}
