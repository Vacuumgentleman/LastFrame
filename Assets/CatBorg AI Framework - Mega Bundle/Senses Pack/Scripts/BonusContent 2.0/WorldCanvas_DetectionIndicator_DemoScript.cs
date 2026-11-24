using UnityEngine;
using UnityEngine.UI;

namespace Senses2026
{
    public class WorldCanvas_DetectionIndicator_DemoScript : MonoBehaviour
    {
        [SerializeField] TargetSenses m_targetSenses;
        public TargetSenses TargetSenses => m_targetSenses;
        [SerializeField] Image m_image;
        [SerializeField] Color m_notDetectedColor;
        [SerializeField] Color m_detectedColor;
        [SerializeField] Color m_focusedColor;

        public void SetColor(int _state) => setCollorAccordingToState(_state);
        void setCollorAccordingToState(int _state)
        {
            switch (_state)
            {
                case 0:
                    {
                        m_image.color = m_notDetectedColor;
                        return;
                    }
                case 1:
                    {
                        m_image.color = m_detectedColor;
                        return;
                    }
                case 2:
                    {
                        m_image.color = m_focusedColor;
                        return;
                    }
            }
        }
    }
}

