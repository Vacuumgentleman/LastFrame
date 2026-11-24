using System.Collections.Generic;
using UnityEngine;

namespace Senses2026
{
    public class SentryHeadTwist_DemoScript : MonoBehaviour
    {
        [SerializeField] Transform m_faceBone;
        [SerializeField] Senses m_senses;
        bool m_playerWasDetected;
        [SerializeField] TargetSenses m_targetSenses;
        [SerializeField] SeeSenseSearchType m_lookFor;
        [SerializeField][Range(0.01f, 100f)] float m_detectionSensitive;
        public float DetectionSensitive => m_detectionSensitive;
        List<TargetSenses> m_detectedTargetSense;
        [SerializeField] WorldCanvas_DetectionIndicator_DemoScript[] m_worldCanvas_DetectionIndicator_DemoScripts;
        bool m_wasDeected;

        [SerializeField] float m_distanceSQR;
        [SerializeField] float m_angle;

        void Update()
        {
            m_playerWasDetected = m_senses.WasAnythingSeen((int)m_detectionSensitive, out m_targetSenses, out float _cuurentAwarness, out m_distanceSQR, out m_angle, new FactionNames[1] { FactionNames.Faction1 }, m_lookFor, true);
            if (m_targetSenses != default)
            {
                Quaternion _lookRotation = Quaternion.LookRotation(m_targetSenses.transform.position - m_senses.Eyes.position, Vector3.up);
                _lookRotation.eulerAngles = new Vector3(m_faceBone.rotation.eulerAngles.x, _lookRotation.eulerAngles.y, m_faceBone.rotation.eulerAngles.z);
                m_faceBone.rotation = _lookRotation;
            }

            m_detectedTargetSense = m_senses.GetDetectedTargetList();
            m_wasDeected = false;
            foreach (var _worldCanvas_DetectionIndicator_DemoScripts in m_worldCanvas_DetectionIndicator_DemoScripts)
            {
                if (_worldCanvas_DetectionIndicator_DemoScripts.TargetSenses == m_targetSenses)
                {
                    _worldCanvas_DetectionIndicator_DemoScripts.SetColor(2);
                }
                else
                {
                    for (int i = 0; i < m_detectedTargetSense.Count; i++)
                    {
                        if (m_detectedTargetSense[i] == _worldCanvas_DetectionIndicator_DemoScripts.TargetSenses)
                        {
                            m_wasDeected = true;
                        };
                    }
                    _worldCanvas_DetectionIndicator_DemoScripts.SetColor(m_wasDeected ? 1 : 0);

                }

            }
        }
    }

}
