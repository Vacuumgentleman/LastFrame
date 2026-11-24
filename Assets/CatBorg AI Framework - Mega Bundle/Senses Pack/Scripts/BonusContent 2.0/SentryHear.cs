using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Sentries/Bonus Content 2: Sentry Hear", 2)]
    public class SentryHear : MonoBehaviour, ISentry
    {
        [SerializeField] Senses m_senses;
        bool m_somethingWasDetected;
        [SerializeField][Range(0.01f, 100f)] float m_detectionSensitive;
        [SerializeField] Animator m_animator;
        [SerializeField] HearSenseSearchType m_lookFor;

        [Header("Here you will inspect resuts")]
        [ReadOnly][SerializeField] TargetSenses m_targetSenses;
        [ReadOnly][SerializeField] float m_hearAwarness;
        [ReadOnly][SerializeField] Vector3 m_soundLastPosition;
        public float RequiredAwarness() => m_detectionSensitive;
        public float CurrentAwarnes() => m_hearAwarness;
        public bool SomethingWasDetected() => m_somethingWasDetected;
        [SerializeField] FactionNames[] m_factionNames = new FactionNames[] { FactionNames.None, FactionNames.Faction1, FactionNames.Faction2, FactionNames.Faction3 };


        void Update()
        {
            if (m_senses.Use_Hear)
            {
                m_somethingWasDetected = m_senses.WasAnythingHeard(m_detectionSensitive, out m_targetSenses, out m_hearAwarness, out m_soundLastPosition, m_factionNames, m_lookFor);

                if (m_somethingWasDetected)
                {
                    Debug.DrawRay(m_senses.Eyes.position, m_soundLastPosition - m_senses.Eyes.position, Color.red, .1f);
                }
            }
        }
    }
}

