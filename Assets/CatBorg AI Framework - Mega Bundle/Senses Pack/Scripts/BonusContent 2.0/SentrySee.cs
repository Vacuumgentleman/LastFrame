using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Sentries/Bonus Content 1: Sentry See", 1)]
    public class SentrySee : MonoBehaviour, ISentry
    {
        [SerializeField] Senses m_senses;
        bool m_somethingWasDetected;
        [SerializeField][Range(0.01f, 100f)] float m_detectionSensitive;
        [SerializeField] Animator m_animator;
        [SerializeField] SeeSenseSearchType m_lookFor;
        [SerializeField] FactionNames m_factionToLookFor = FactionNames.None;

        [Header("Here you will inspect resuts")]
        [ReadOnly][SerializeField] TargetSenses m_targetSenses;
        [ReadOnly][SerializeField] float m_currentAwarness;
        [ReadOnly][SerializeField] float m_distanceSQR;
        [ReadOnly][SerializeField] float m_angle;
        public float RequiredAwarness() => m_detectionSensitive;
        public float CurrentAwarnes() => m_currentAwarness;
        public bool SomethingWasDetected() => m_somethingWasDetected;


        void Update()
        {
            if (m_senses.Use_See)
            {
                m_somethingWasDetected = m_senses.WasAnythingSeen((int)m_detectionSensitive, out m_targetSenses, out m_currentAwarness, out m_distanceSQR, out m_angle, new FactionNames[1] { m_factionToLookFor }, m_lookFor, false);
                m_animator.speed = m_somethingWasDetected ? 0f : 1;

            }
        }



    }
}

