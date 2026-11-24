using Senses2026;
using UnityEngine;

namespace Senses2026BonusContent
{
    public class RagdolActivatorByDistance : MonoBehaviour
    {
        [Header("Hardcoded Reference")]
        [SerializeField] RagdolActivator m_targetRagdoll; // drag reference in Inspector
        TargetSenses m_targetSenses;
        Senses m_senses;
        [SerializeField] FactionNames m_factionNameForActivation = FactionNames.Faction3;
        [SerializeField] float m_activationDistance = 1f;
        [SerializeField] GameObject m_deadCam;
        [SerializeField] DemoManager m_demoManager;
        [SerializeField] float m_cuurentAwarness;
        [SerializeField] sentryType m_sentryType = sentryType.See;

        bool m_hasActivated = false;

        void Awake()
        {
            m_targetSenses = m_targetRagdoll.GetComponent<TargetSenses>();
            m_senses = GetComponent<Senses>();
        }
        void Update()
        {
            if (!m_targetRagdoll || m_hasActivated || !m_targetSenses || m_targetSenses.FactionName != m_factionNameForActivation)
                return;

            float distance = Vector3.Distance(transform.position, m_targetRagdoll.transform.position);

            if (!m_hasActivated && distance <= m_activationDistance)
            {
                if (m_sentryType == sentryType.See && m_senses.DidSee(m_targetSenses, 30, out m_cuurentAwarness, out float _distnceSQR, out float _angle))
                {
                    m_targetRagdoll.ActivateRagdol();
                    m_deadCam.SetActive(true);
                    m_demoManager.PlayerWasKilled();
                    m_hasActivated = true;
                }
                else
                if (m_sentryType == sentryType.Smell && m_senses.DidSmell(m_targetSenses, 30, out m_cuurentAwarness, out Vector3 _scentLastPosition, out float _scentStrength))
                {
                    m_targetRagdoll.ActivateRagdol();
                    m_deadCam.SetActive(true);
                    m_demoManager.PlayerWasKilled();
                    m_hasActivated = true;
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_activationDistance);
        }

        enum sentryType
        {
            See,
            Hear,
            Smell
        }
    }

}
