using UnityEngine;
using UnityEngine.UI;

namespace Senses2026
{
    public class UI_LumenBuff_Indicator_DemoScript : MonoBehaviour
    {
        [SerializeField] Stealth m_stealth;
        [SerializeField] Slider m_slider;
        [SerializeField] Image m_image;

        float m_previous_StealthValue = -1;
        float m_current_StealthValue;
        Color m_color;

        void Awake()
        {
            m_slider.maxValue = 1f;
            m_slider.minValue = 0f;
            m_current_StealthValue = 0f;
            m_slider.value = 0f;
        }
        void Update()
        {
            m_color = m_image.color;
            m_color.a = m_stealth.UseLumenStealthAddon ? 1 : .3f;
            m_image.color = m_color;

            m_current_StealthValue = m_stealth.LumenStealthBuff;

            if (float.IsNaN(m_current_StealthValue)) return;

            if (m_previous_StealthValue != m_current_StealthValue)
            {
                m_slider.value = m_current_StealthValue;
                m_previous_StealthValue = m_current_StealthValue;
            }
        }
    }

}
