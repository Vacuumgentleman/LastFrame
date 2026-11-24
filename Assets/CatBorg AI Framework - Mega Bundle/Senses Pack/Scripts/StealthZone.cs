using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 51: Stealth Zone", 51)]
    public class StealthZone : MonoBehaviour
    {
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;
        [field: SerializeField] bool m_helpPropertiesFoldout;
        public void Toggle_HelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;

        [SerializeField, Range(0f, 1f)]
        float m_stealthBuff = 0f;

        // Ensures we only randomize the value once.
        [SerializeField, HideInInspector]
        bool m_initialized = false;

        public float StealthBuff
        {
            get
            {
                if (!enabled)
                    return 0f;

                return m_stealthBuff;
            }
            private set => m_stealthBuff = value;
        }

        public void Set_StealthBuff(float value)
            => m_stealthBuff = value;

        private void Awake()
        {
            // Ensure ColliderReference exists
            if (!TryGetComponent(out ColliderReference _))
                gameObject.AddComponent<ColliderReference>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Only randomize ONCE, only in editor, only before play mode
            if (!Application.isPlaying && !m_initialized)
            {
                m_stealthBuff = RandomMedian01();
                m_initialized = true;
            }
        }

        float RandomMedian01()
        {
            return (Random.value + Random.value) * 0.5f;
        }

        float YourCustomRandom()
        {
            float _from = .3f;
            float _to = .7f;
            return Random.Range(_from, _to);
        }
#endif
    }
}
