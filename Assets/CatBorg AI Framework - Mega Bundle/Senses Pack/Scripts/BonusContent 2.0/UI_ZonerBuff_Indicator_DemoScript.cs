using UnityEngine;
using UnityEngine.UI;

namespace Senses2026
{
    public class UI_ZonerBuff_Indicator_DemoScript : MonoBehaviour
    {
        [SerializeField] Stealth m_stealth;
        [SerializeField] Slider m_slider;
        [SerializeField] Image m_image;
        Color m_color = Color.white;
        bool m_previous_StealthEnabled;
        float m_previousZonerStealthBuff;
        float m_currentZonerStealthBuff;

        void Awake()
        {
            m_slider.maxValue = 1f;
            m_slider.minValue = 0f;
            m_currentZonerStealthBuff = m_stealth.ZonerStealthBuff;
            m_slider.value = m_currentZonerStealthBuff;
            m_previousZonerStealthBuff = m_currentZonerStealthBuff;
        }
        void Update()
        {
            m_currentZonerStealthBuff = m_stealth.ZonerStealthBuff;


            if (m_previousZonerStealthBuff != m_currentZonerStealthBuff)
            {
                m_slider.value = m_currentZonerStealthBuff;
                m_previousZonerStealthBuff = m_currentZonerStealthBuff;
            }
        }
    }

}
