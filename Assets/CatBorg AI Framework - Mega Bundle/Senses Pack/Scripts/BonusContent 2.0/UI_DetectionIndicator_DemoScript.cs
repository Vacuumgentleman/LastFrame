using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Senses2026
{
    public class UI_DetectionIndicator_DemoScript : MonoBehaviour
    {
        [SerializeField] Image m_image;
        [SerializeField] Sprite m_detectedSprite;
        [SerializeField] Sprite m_evasionSprite;

        [SerializeField] Text m_label;

        [SerializeField] bool m_isDetected = false;
        bool m_previous_isDetected;

        [SerializeField] MonoBehaviour[] m_iSentry;
        ISentry[] m_sentryArray;
        bool m_checkedStatus;

        void Awake()
        {
            m_previous_isDetected = m_isDetected;
            m_image.sprite = m_isDetected ? m_detectedSprite : m_evasionSprite;

            m_sentryArray = new ISentry[m_iSentry.Length];
            for (int i = 0; i < m_sentryArray.Length; i++)
            {
                m_sentryArray[i] = m_iSentry[i].GetComponent<ISentry>();
            }
        }

        void Update()
        {
            m_checkedStatus = false;
            for (int i = 0; i < m_sentryArray.Length; i++)
            {
                if (m_sentryArray[i].CurrentAwarnes() > m_sentryArray[i].RequiredAwarness()) m_checkedStatus = true;
            }

            m_isDetected = m_checkedStatus;

            if (m_previous_isDetected == m_isDetected) return;
            StopAllCoroutines();

            m_previous_isDetected = m_isDetected;
            m_image.sprite = m_isDetected ? m_detectedSprite : m_evasionSprite;

            if (!m_isDetected)
            {
                StartCoroutine(Coorutine_Evasion());
            }
            else
            {
                m_label.text = ("99.99");

            }
        }

        IEnumerator Coorutine_Evasion()
        {
            float _timeTillEnd = 99.99f;
            while (_timeTillEnd >= 0.1f)
            {
                _timeTillEnd -= .1f;
                yield return null;

                m_label.text = (_timeTillEnd.ToString("00.00"));
            }
            m_label.text = ("00.00");

        }
    }

}
