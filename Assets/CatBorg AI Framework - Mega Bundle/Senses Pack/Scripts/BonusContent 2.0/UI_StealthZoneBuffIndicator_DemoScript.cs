using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

namespace Senses2026
{
    public class UI_StealthZoneBuffIndicator_DemoScript : MonoBehaviour
    {
        [SerializeField] Stealth m_stealth;
        [SerializeField] Image m_iconImage;
        [SerializeField] TextMeshProUGUI m_label;
        float m_previousValue;
        float m_currentValue;
        Color m_color;
        StringBuilder m_stringBuilder = new StringBuilder();

        void Awake()
        {
            m_color = m_iconImage.color;
            m_currentValue = m_stealth.ProvideZonerStealthAddon();
            m_previousValue = m_currentValue;
            m_color.a = m_currentValue;
            m_iconImage.color = m_color;
            m_stringBuilder.Clear();
            m_stringBuilder.Append((int)(m_currentValue * 100));
            m_stringBuilder.Append("%");
            m_label.SetText(m_stringBuilder);
            m_label.enabled = m_currentValue > .01f;
        }

        void Update()
        {
            m_currentValue = m_stealth.ProvideZonerStealthAddon();

            if (float.IsNaN(m_currentValue)) return;

            if (m_previousValue != m_currentValue)
            {
                m_previousValue = m_currentValue;
                m_color.a = m_currentValue;
                m_iconImage.color = m_color;
                m_stringBuilder.Clear();
                m_stringBuilder.Append((int)(m_currentValue * 100));
                m_stringBuilder.Append("%");
                m_label.SetText(m_stringBuilder);
                m_label.enabled = true;//m_currentValue > .01f;
            }
        }
    }

}
