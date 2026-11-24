using UnityEngine;
using UnityEngine.UI;

namespace Senses2026
{
    public class UI_TotalBuff_Indicator_DemoScript : MonoBehaviour
    {
        [SerializeField] Stealth m_stealth;
        [SerializeField] Slider m_slider;
        [SerializeField] Image m_image;
        Color m_color = Color.white;
        bool m_previous_StealthEnabled;
        float m_previous_StealthValue;
        float m_current_StealthValue;

        void Awake()
        {
            m_slider.maxValue = 1f;
            m_slider.minValue = 0f;
            m_current_StealthValue = m_stealth.ProvideTotalStealthBuff();
            m_slider.value = m_current_StealthValue;
            m_previous_StealthValue = m_current_StealthValue;
            m_previous_StealthEnabled = m_stealth.enabled;
            m_color.a = !m_previous_StealthEnabled ? .5f : 1f;
            m_image.color = m_color;
        }
        void Update()
        {
            if (m_stealth.enabled != m_previous_StealthEnabled)
            {
                m_previous_StealthEnabled = !m_previous_StealthEnabled;
                m_color.a = !m_previous_StealthEnabled ? .3f : 1f;
                m_image.color = m_color;
            }

            m_current_StealthValue = m_stealth.ProvideTotalStealthBuff();

            if (float.IsNaN(m_current_StealthValue)) return;

            if (m_previous_StealthValue != m_current_StealthValue)
            {
                m_slider.value = m_current_StealthValue;
                m_previous_StealthValue = m_current_StealthValue;
            }
        }
    }

}
