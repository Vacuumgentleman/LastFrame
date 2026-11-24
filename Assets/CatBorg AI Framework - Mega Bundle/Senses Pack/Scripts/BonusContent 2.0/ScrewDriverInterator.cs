using UnityEngine;
using System.Collections;

namespace Senses2026BonusContent
{
    public class ScrewDriverInterator : MonoBehaviour
    {
        [SerializeField] GameObject[] m_particleSystems;
        [SerializeField] GameObject[] m_lightSources;
        [SerializeField] float m_flickerDuration = 1.5f;
        [SerializeField] float m_flickerIntervalMin = 0.05f;
        [SerializeField] float m_flickerIntervalMax = 0.2f;

        public void ActionWasFinished()
        {
            StartCoroutine(FlickerLightsBeforeShutdown());
        }

        IEnumerator FlickerLightsBeforeShutdown()
        {
            float _elapsed = 0f;

            while (_elapsed < m_flickerDuration)
            {
                bool _lightsOn = Random.value > 0.5f;
                for (int i = 0; i < m_lightSources.Length; i++)
                {
                    m_lightSources[i].SetActive(_lightsOn);
                }

                float _waitTime = Random.Range(m_flickerIntervalMin, m_flickerIntervalMax);
                yield return new WaitForSeconds(_waitTime);
                _elapsed += _waitTime;
            }

            for (int i = 0; i < m_particleSystems.Length; i++)
            {
                m_particleSystems[i].SetActive(true);
            }

            for (int i = 0; i < m_lightSources.Length; i++)
            {
                m_lightSources[i].SetActive(false);
            }

            Destroy(this);
        }
    }
}
