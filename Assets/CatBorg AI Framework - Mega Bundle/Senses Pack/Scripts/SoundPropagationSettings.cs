using UnityEngine;

namespace Senses2026
{
    [CreateAssetMenu(fileName = "SoundPropagationSettings", menuName = "Settings/Sound Propagation Settings")]
    public class SoundPropagationSettings : ScriptableObject
    {
        [HideInInspector][SerializeField] private SoundPropagationData[] m_soundData;

        public SoundPropagationData[] SoundData => m_soundData;
    }

    [System.Serializable]
    public class SoundPropagationData
    {
        [SerializeField] private float m_factor = 1f;
        public float Factor => m_factor;

        [SerializeField] private string m_TAG;
        public string TAG => m_TAG;
    }
}

