using TMPro;
using UnityEngine;

namespace Senses2026
{
    public class WarningTable_DemoScript : MonoBehaviour
    {
        [SerializeField] string m_message;
        [SerializeField] GameObject m_popup;
        [SerializeField] TextMeshProUGUI m_label;
        [SerializeField] RectTransform m_rectTransform;
        public string Message => m_message;

        public void DisplayText()
        {
            m_label.text = m_message;
            m_popup.SetActive(true);
            m_popup.GetComponent<RectTransform>().anchoredPosition = m_rectTransform.anchoredPosition;
        }
    }
}

