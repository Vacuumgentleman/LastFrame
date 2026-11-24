using System.Collections;
using UnityEngine;


namespace Senses2026
{
    public class LightDetector : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The camera that scans for light.")]
        public Camera m_camLightScan;
        [Tooltip("Time between light value updates (default = 0.1f).")]
        public float m_fUpdateTime = 0.1f;
        [SerializeField] float s_fLightValue;
        const int c_iTextureSize = 16;
        Texture2D m_texLight;
        RenderTexture m_texTemp;
        Rect m_rectLight;
        Color m_LightPixel;
        Stealth m_current_StealthComponent;
        bool m_corutineCoolingDown = false;
        [SerializeField] GameObject m_darknesBox;
        [SerializeField] float m_initialMin_BrightnessValue = float.MaxValue;
        [SerializeField] float m_initialTop_BrightnessValue = float.MinValue;
        Transform m_transform;

        void Start()
        {
            m_transform = this.transform;
            m_initialMin_BrightnessValue = float.MaxValue;
            m_initialTop_BrightnessValue = float.MinValue;
            initializeTextures();
            StartCoroutine(InitialLightDetection_MK2());
        }

        void Update()
        {
            if (m_corutineCoolingDown) return;
            m_corutineCoolingDown = true;
            m_current_StealthComponent = LumenHandler.Instance.GetNext_Stealth();
            if (m_current_StealthComponent == null)
            {
                StartCoroutine(LackOfJobDelay());
                return;
            }
            m_transform.position = m_current_StealthComponent.PositionLumenMeasurement;
            StartCoroutine(LightDetectionUpdate());
        }

        void initializeTextures()
        {
            m_texLight = new Texture2D(c_iTextureSize, c_iTextureSize, TextureFormat.RGB24, false);
            m_texTemp = new RenderTexture(c_iTextureSize, c_iTextureSize, 24);
            m_rectLight = new Rect(0f, 0f, c_iTextureSize, c_iTextureSize);
        }

        IEnumerator LightDetectionUpdate()
        {
            m_corutineCoolingDown = true;
            m_camLightScan.targetTexture = m_texTemp;
            m_camLightScan.Render();
            RenderTexture.active = m_texTemp;
            m_texLight.ReadPixels(m_rectLight, 0, 0);
            RenderTexture.active = null;
            m_camLightScan.targetTexture = null;
            s_fLightValue = 0f;
            for (int _x = 0; _x < c_iTextureSize; _x++)
            {
                for (int _y = 0; _y < c_iTextureSize; _y++)
                {
                    m_LightPixel = m_texLight.GetPixel(_x, _y);
                    s_fLightValue += m_LightPixel.r + m_LightPixel.g + m_LightPixel.b;
                }
            }
            s_fLightValue /= (3 * c_iTextureSize * c_iTextureSize);

            if (s_fLightValue < m_initialMin_BrightnessValue) m_initialMin_BrightnessValue = s_fLightValue; //change 2.0
            if (s_fLightValue > m_initialTop_BrightnessValue) m_initialTop_BrightnessValue = s_fLightValue; //change 2.0

            s_fLightValue = Mathf.Clamp(s_fLightValue, m_initialMin_BrightnessValue, m_initialTop_BrightnessValue);

            s_fLightValue = (m_initialTop_BrightnessValue - s_fLightValue) / (m_initialTop_BrightnessValue - m_initialMin_BrightnessValue);// )//(1 - s_fLightValue) / (1 - m_initialMin_BrightnessValue) * (1 - 0);
            s_fLightValue = Mathf.Sqrt(s_fLightValue);
            if (s_fLightValue != float.NaN) m_current_StealthComponent.SetLumenStealthValue(s_fLightValue);
            yield return new WaitForSeconds(m_fUpdateTime);
            m_corutineCoolingDown = false;
        }
        IEnumerator InitialLightDetection_MK2()
        {
            m_corutineCoolingDown = true;

            LumenProbes[] _foundLumenProbes;

#if UNITY_6000_5_OR_NEWER

    // New Unity 6 API - no FindObjectsSortMode
    _foundLumenProbes = FindObjectsByType<LumenProbes>();

#elif UNITY_2022_2_OR_NEWER

            // Unity 2022.2 - Unity 6.5
            _foundLumenProbes = FindObjectsByType<LumenProbes>(
                FindObjectsSortMode.None
            );

#else

    // Older Unity versions
    _foundLumenProbes = FindObjectsOfType<LumenProbes>();

#endif

            if (_foundLumenProbes.Length > 0)
            {
                foreach (LumenProbes _lumenProbes in _foundLumenProbes)
                {
                    transform.position = _lumenProbes.transform.position;

                    m_texTemp = new RenderTexture(
                        c_iTextureSize,
                        c_iTextureSize,
                        24
                    );

                    m_camLightScan.targetTexture = m_texTemp;
                    m_camLightScan.Render();

                    RenderTexture.active = m_texTemp;
                    m_texLight.ReadPixels(m_rectLight, 0, 0);
                    RenderTexture.active = null;

                    m_camLightScan.targetTexture = null;

                    s_fLightValue = 0f;

                    for (int _x = 0; _x < c_iTextureSize; _x++)
                    {
                        for (int _y = 0; _y < c_iTextureSize; _y++)
                        {
                            m_LightPixel = m_texLight.GetPixel(_x, _y);

                            s_fLightValue +=
                                m_LightPixel.r +
                                m_LightPixel.g +
                                m_LightPixel.b;
                        }
                    }

                    s_fLightValue /=
                        (3f * c_iTextureSize * c_iTextureSize);

                    if (s_fLightValue < m_initialMin_BrightnessValue)
                        m_initialMin_BrightnessValue = s_fLightValue;

                    if (s_fLightValue > m_initialTop_BrightnessValue)
                        m_initialTop_BrightnessValue = s_fLightValue;

                    yield return new WaitForSeconds(m_fUpdateTime);

                    _lumenProbes.gameObject.SetActive(false);
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    "Warning: Light detector did not find any LumenProbes. " +
                    "No worries—it will self-calibrate during usage. " +
                    "However, for better accuracy, consider placing LumenProbes in the scene. " +
                    "You can find the LumenProbes prefab in the Resources folder. " +
                    "Place a few in areas with varying light levels (e.g., very bright and very dark). " +
                    "Check the documentation for more details, or feel free to reach out for assistance!"
                );
#endif
            }

            m_corutineCoolingDown = false;
        }
        IEnumerator LackOfJobDelay()
        {
            m_corutineCoolingDown = true;
            yield return new WaitForSeconds(m_fUpdateTime);
            m_corutineCoolingDown = false;
        }

    }

}
