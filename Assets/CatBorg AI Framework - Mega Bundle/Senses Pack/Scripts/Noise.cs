using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 40: Noise", 40)]
    public class Noise : MonoBehaviour
    {
        [field: SerializeField] public bool UseNavMesh { get; private set; }
        public void SetUseNavMesh(bool _state) => UseNavMesh = _state;

        [field: SerializeField] public bool UseSpreadAccordingToDistance { get; private set; }
        public void SetSpreadAccordingToDistance(bool _state) => UseSpreadAccordingToDistance = _state;

        TargetSenses m_targetSenses;

        bool m_wasEnabled = false;

        public event System.Action<float, Vector3> OnAnyNoiseReleased;

        Collider[] m_collidersSphereCast = new Collider[1];

        int m_hitCount;

        [field: SerializeField] public bool UseSoundSurfacePropagation { get; private set; }
        public void SetSoundSurfacePropagation(bool _state) => UseSoundSurfacePropagation = _state;

        [field: SerializeField] public LayerMask IgnoreLayers;

        [field: SerializeField] public float GroundCheckRange { get; private set; } = 0.5f;
        public void SetGroundCheckRange(float _range) => GroundCheckRange = _range;

        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;
        [field: SerializeField] bool m_helpPropertiesFoldout;
        public void ToggleHelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;

        public bool SettingsPropertiesFoldout => m_settingsPropertiesFoldout;
        [field: SerializeField] bool m_settingsPropertiesFoldout;
        public void ToggleSettingsPropertiesFoldout() => m_settingsPropertiesFoldout = !m_settingsPropertiesFoldout;

        void enable()
        {
            m_targetSenses = GetComponent<TargetSenses>();
            m_wasEnabled = true;
        }

        public void ReleaseTestNoise180db() => ReleaseNoise(180f);
        public void ReleaseTestNoise120db() => ReleaseNoise(120f);
        public void ReleaseTestNoise80db() => ReleaseNoise(80f);
        public void ReleaseTestNoise30db() => ReleaseNoise(30f);

        public void ReleaseNoise(float _noiseValue)
        {
            if (!m_wasEnabled) enable();

            if (!enabled) return;

            float _highestFactor = 1f;

            if (UseSoundSurfacePropagation)
            {
                m_hitCount = overlapSphere(GroundCheckRange, ~IgnoreLayers);

                if (m_hitCount == m_collidersSphereCast.Length)
                {
                    if (m_hitCount <= 10)
                    {
                        m_hitCount *= 2;
                        m_collidersSphereCast = new Collider[m_hitCount];
                        return;
                    }
                }

                bool _anyTagDetected = false;
                float _highestY = float.MinValue;
                int _chosenIndex = 0;

                for (int _i = 0; _i < m_hitCount; _i++)
                {
                    if (SensesSettings.Instance.ProvidePropagationFactor(m_collidersSphereCast[_i], out float _factor))
                    {
                        float _currentY = m_collidersSphereCast[_i].transform.position.y;
                        if (_currentY > _highestY)
                        {
                            _highestY = _currentY;
                            _chosenIndex = _i;
                        }
                        _anyTagDetected = true;
                    }
                }

                if (_anyTagDetected)
                {
                    SensesSettings.Instance.ProvidePropagationFactor(m_collidersSphereCast[_chosenIndex], out float _factor);
                    _highestFactor = _factor;
                }
            }

            OnAnyNoiseReleased?.Invoke(_noiseValue * _highestFactor, transform.position);
            NoiseRecivers.Instance.SpreadNoise(_noiseValue * _highestFactor, transform.position, m_targetSenses, UseNavMesh, UseSpreadAccordingToDistance);
        }

        int overlapSphere(float _radius, LayerMask _layerMask)
        {
            return Physics.OverlapSphereNonAlloc(transform.position, _radius, m_collidersSphereCast, _layerMask, QueryTriggerInteraction.UseGlobal);
        }
    }
}