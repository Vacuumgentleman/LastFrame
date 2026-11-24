using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Player/BC 5: Awarnes Indicator", 5)]
    public class AwarnesIndicator : MonoBehaviour
    {
        [SerializeField] MonoBehaviour m_sentry;
        ISentry m_iSentry;

        Animator m_awarnessIndicator_Animator;
        bool m_isDetected = false;
        bool m_previous_isDetected = false;
        [SerializeField] bool m_usePivotRelocation = false;
        [SerializeField] Transform[] m_Pivots;
        [SerializeField] Transform m_canvasTransform;
        Camera m_camera;

        void Awake()
        {
            m_iSentry = m_sentry.GetComponent<ISentry>();
            if (m_iSentry == null)
            {
                Debug.LogWarning("No ISentry detected!", gameObject);
                gameObject.SetActive(false);
            }
            m_awarnessIndicator_Animator = GetComponent<Animator>();
            m_camera = Camera.main;
        }

        void Update()
        {
            m_isDetected = m_iSentry.SomethingWasDetected();
            if (m_previous_isDetected != m_isDetected)
            {
                m_previous_isDetected = m_isDetected;
                m_awarnessIndicator_Animator.SetBool("isDetected", m_isDetected);

                if (m_isDetected && m_usePivotRelocation)
                {
                    int _chosenIndex = 0;
                    Vector2 _currentScreanPosition = Vector2.zero;
                    float _minX = float.MaxValue;

                    for (int i = 0; i < m_Pivots.Length; i++)
                    {
                        _currentScreanPosition = RectTransformUtility.WorldToScreenPoint(m_camera, m_Pivots[i].position);
                        if (_currentScreanPosition.x < _minX)
                        {
                            _minX = _currentScreanPosition.x;
                            _chosenIndex = i;
                        }
                    }

                    m_canvasTransform.position = m_Pivots[_chosenIndex].position;
                }
            }
        }

    }

}



