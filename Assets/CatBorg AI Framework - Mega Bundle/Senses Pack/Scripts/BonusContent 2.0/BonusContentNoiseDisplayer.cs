using UnityEngine;

namespace Senses2026
{
    [RequireComponent(typeof(Noise))]
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Player/BC 4: Noise Displayer", 4)]
    public class BonusContentNoiseDisplayer : MonoBehaviour
    {
        bool m_wasEnabled = false;
        Noise m_noise;
        bool m_detectedNoiseCompoennt;

        UI_ReleasedNoise_Indicator_DemoScript m_UI_ReleasedNoise_Indicator_DemoScript;
        bool m_detected_UI_ReleasedNoise_Indicator_DemoScript;

        void enable()
        {
            m_detectedNoiseCompoennt = TryGetComponent<Noise>(out m_noise);

#if UNITY_6000_5_OR_NEWER
            m_UI_ReleasedNoise_Indicator_DemoScript =
            FindAnyObjectByType<UI_ReleasedNoise_Indicator_DemoScript>();
#else
            m_UI_ReleasedNoise_Indicator_DemoScript =
            FindFirstObjectByType<UI_ReleasedNoise_Indicator_DemoScript>();
#endif

            m_detected_UI_ReleasedNoise_Indicator_DemoScript =
                m_UI_ReleasedNoise_Indicator_DemoScript != null;

            m_wasEnabled = true;
        }


        void OnEnable()
        {
            if (!m_wasEnabled) enable();
            if (m_detectedNoiseCompoennt) m_noise.OnAnyNoiseReleased += HandleNoiseReleased;
        }
        void OnDisable()
        {
            if (!m_wasEnabled) enable();
            if (m_detectedNoiseCompoennt) m_noise.OnAnyNoiseReleased -= HandleNoiseReleased;
        }

        void HandleNoiseReleased(float noiseValue, Vector3 noisePosition)
        {
            if (!m_wasEnabled) enable();
            if (m_detected_UI_ReleasedNoise_Indicator_DemoScript) m_UI_ReleasedNoise_Indicator_DemoScript.SetTempValue(noiseValue);
        }


    }
}

