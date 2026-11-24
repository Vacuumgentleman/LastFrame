using UnityEngine;
using System;
using System.Collections;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Sentries/Bonus Content 1: Sentry See MK2", 1)]
    public class SentrySee_MK2 : MonoBehaviour, ISentry
    {
        [SerializeField] Senses m_senses;
        bool m_importantThreatDetected;
        bool m_distractionDetected;
        bool m_importantThreatWasEverDetected;

        [SerializeField][Range(0.01f, 100f)] float m_detectionSensitive;
        [SerializeField] Animator m_animator;
        [SerializeField] SeeSenseSearchType m_lookFor;
        [SerializeField] bool m_rotateTowardsDetected = false;
        [SerializeField] FactionNames[] m_factionNames = new FactionNames[1] { FactionNames.Faction1 };

        [Header("Here you will inspect resuts")]
        [ReadOnly][SerializeField] TargetSenses m_targetSenses;
        [ReadOnly][SerializeField] float m_currentAwarness;
        [ReadOnly][SerializeField] float m_distanceSQR;
        [ReadOnly][SerializeField] float m_angle;

        [ReadOnly][SerializeField] float m_distractionAwarness; //we use separate awarness float so it will not trigger awarness UI in Canvas

        public float RequiredAwarness() => m_detectionSensitive;
        public float CurrentAwarnes() => m_currentAwarness;
        public bool SomethingWasDetected() => m_importantThreatDetected;


        WaitForSecondsRealtime m_delay = new WaitForSecondsRealtime(3f);
        Quaternion m_rotation;
        bool m_coorutineOngoing = false;

        void Update()
        {
            if (m_senses.Use_See)
            {
                // m_importantThreatDetected is flag for tracking detection of important threats - in this example is player
                // we using WasAnythingSeen() with FactionNames.YourFaction1, cause Player had YourFaction1 assigned in his TargetSenses Component

                m_importantThreatDetected = m_senses.WasAnythingSeen((int)m_detectionSensitive, out m_targetSenses, out m_currentAwarness, out m_distanceSQR, out m_angle, m_factionNames, m_lookFor, false);
                m_animator.speed = m_importantThreatDetected ? 0f : 1;

                if (m_importantThreatDetected)
                {
                    Debug.DrawRay(m_senses.Eyes.position, m_targetSenses.transform.position - m_senses.Eyes.position, Color.red, .1f);
                    StopAllCoroutines();

                    if (m_rotateTowardsDetected) rotateToward(m_targetSenses.transform.position); // Smoothly rotate toward distraction

                    m_coorutineOngoing = false;
                    m_importantThreatWasEverDetected = true;
                    return;
                }
                else
                {
                    if (!m_importantThreatWasEverDetected)
                    {
                        // sentry do not find important Threat so we will look for distraction
                        // distractions (for purpose of example) in TargetSenses Component had FactionNames.None assigned

                        m_distractionDetected = m_senses.WasAnythingSeen((int)m_detectionSensitive, out m_targetSenses, out m_distractionAwarness, out m_distanceSQR, out m_angle, new FactionNames[1] { FactionNames.None }, m_lookFor, false);
                        m_animator.speed = m_distractionDetected ? 0f : 1;

                        if (m_distractionDetected)
                        {
                            Debug.DrawRay(m_senses.Eyes.position, m_targetSenses.transform.position - m_senses.Eyes.position, Color.red, .1f);
                            StopAllCoroutines();
                            m_coorutineOngoing = false;
                            return;
                        }
                    }
                }
            }

            if (!m_coorutineOngoing) StartCoroutine(ChangeRotation());

        }

        IEnumerator ChangeRotation()
        {
            m_coorutineOngoing = true;
            yield return m_delay;

            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(startRotation.eulerAngles.x, startRotation.eulerAngles.y + 180, startRotation.eulerAngles.z);
            float elapsedTime = 0f;
            float duration = 0.5f;

            while (elapsedTime < duration)
            {
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.rotation = targetRotation;
            m_coorutineOngoing = false;
        }

        void rotateToward(Vector3 targetPosition)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);

            // Apply smooth rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
        }
    }

}

