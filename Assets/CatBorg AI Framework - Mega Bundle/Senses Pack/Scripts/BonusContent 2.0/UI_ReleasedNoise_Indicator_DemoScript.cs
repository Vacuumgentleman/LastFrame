using UnityEngine;
using UnityEngine.UI;

namespace Senses2026
{
    public class UI_ReleasedNoise_Indicator_DemoScript : MonoBehaviour
    {
        [SerializeField] Slider m_slider;

        float m_tempValue;
        float m_timer = 0f;

        [SerializeField] private float m_coolDownTime = 1f;

        void Awake()
        {
            m_slider.maxValue = 160;
            m_slider.minValue = 0f;
            m_slider.value = 0f;
        }

        void Update()
        {
            if (m_tempValue > 0f)
            {
                m_timer += Time.deltaTime;
                m_tempValue = Mathf.Lerp(m_tempValue, 0f, m_timer / m_coolDownTime);
                m_slider.value = m_tempValue;
            }
        }

        public void SetTempValue(float _tempValue)
        {
            if (m_tempValue < _tempValue)
            {
                m_tempValue = _tempValue;
                m_slider.value = m_tempValue;
                m_timer = 0;
            }
        }
    }
}
