using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Sentries/Bonus Content 0: Sentry Camera", 0)]
    [RequireComponent(typeof(Senses))]
    public class SentryCamera : MonoBehaviour, ISentry
    {
        Transform m_TransformToLookFor;
        Senses m_senses;
        public float DetectionSensitive => m_detectionSensitive;

        [SerializeField][Range(0.01f, 100f)] float m_detectionSensitive;


        [Header("Sentry Properies")]
        [SerializeField] float rotationSpeed = 50f;
        [SerializeField][Range(0.01f, 1f)] float m_angleSensitive = .2f;
        [SerializeField] FactionNames[] m_factionNames = new FactionNames[] { FactionNames.None, FactionNames.Faction1, FactionNames.Faction2, FactionNames.Faction3 };

        bool m_playerWasDetected = false;
        public bool PlayerWasDetected => m_playerWasDetected;
        bool m_playerWasTracked = false;
        bool rotateClockWise = false;

        float m_limitEuler_Left;
        float m_limitEuler_Right;
        float m_limitEuler_Front;
        float m_offset;

        float m_targetPerfect_EulerY;
        float m_clamped_targetPerfect_EulerY;
        Vector3 m_directionToTarget;
        Quaternion m_lookRotation;
        Vector3 m_newEulerAngles;
        float m_angleDifference;
        float m_currentRotation;
        float m_currentAwarnes;

        void Awake()
        {
            m_senses = GetComponent<Senses>();
        }

        void Start()
        {
            // Euler 360 <-> 0 --> IOT prevent issue best to use 90 -270, so 180 is front, 0 rear
            m_offset = 180f - transform.eulerAngles.y;
            if (m_offset < 0) m_offset += 360;

            m_limitEuler_Front = 180f;
            m_limitEuler_Left = 90;
            m_limitEuler_Right = 270;

            m_playerWasTracked = m_playerWasDetected;
        }

        void Update()
        {
            m_playerWasDetected = m_senses.WasAnythingSeen(m_detectionSensitive, out TargetSenses _targetSenses, out m_currentAwarnes, out float _distanceSQR, out float _angle, m_factionNames, SeeSenseSearchType.HighestAwarness, false);
            if (_targetSenses != null) m_TransformToLookFor = _targetSenses.transform;
            if (!m_playerWasTracked && !m_playerWasDetected)
            {
                m_newEulerAngles = transform.eulerAngles;
                m_newEulerAngles.y += rotateClockWise ? rotationSpeed * Time.deltaTime : -rotationSpeed * Time.deltaTime;
                transform.eulerAngles = m_newEulerAngles;

                m_currentRotation = transform.eulerAngles.y;
                m_currentRotation += m_offset;
                if (m_currentRotation > 360) m_currentRotation -= 360;

                if (Mathf.Abs(m_currentRotation - m_limitEuler_Right) < 1 && rotateClockWise)
                {
                    rotateClockWise = false;
                }
                else if (Mathf.Abs(m_currentRotation - m_limitEuler_Left) < 1 && !rotateClockWise)
                {
                    rotateClockWise = true;
                }
            }
            else
            {
                m_playerWasTracked = true;
                m_directionToTarget = (m_TransformToLookFor.position - transform.position).normalized;
                m_lookRotation = Quaternion.LookRotation(m_directionToTarget);
                m_targetPerfect_EulerY = m_lookRotation.eulerAngles.y;

                m_targetPerfect_EulerY += m_offset;
                if (m_targetPerfect_EulerY > 360) m_targetPerfect_EulerY -= 360;

                if (m_targetPerfect_EulerY < m_limitEuler_Left) m_clamped_targetPerfect_EulerY = m_limitEuler_Left;
                else if (m_targetPerfect_EulerY > m_limitEuler_Right) m_clamped_targetPerfect_EulerY = m_limitEuler_Right;
                else
                    m_clamped_targetPerfect_EulerY = m_targetPerfect_EulerY;

                m_lookRotation.eulerAngles = new Vector3(transform.eulerAngles.x, m_clamped_targetPerfect_EulerY, transform.eulerAngles.z);

                m_currentRotation = transform.eulerAngles.y;
                m_currentRotation += m_offset;
                if (m_currentRotation > 360) m_currentRotation -= 360;

                rotateClockWise = m_clamped_targetPerfect_EulerY > m_currentRotation;
                m_angleDifference = (m_currentRotation - m_clamped_targetPerfect_EulerY);

                if (m_angleDifference > m_angleSensitive || m_angleDifference < -m_angleSensitive)
                {
                    m_newEulerAngles = transform.eulerAngles;
                    m_newEulerAngles.y += rotateClockWise ? rotationSpeed * Time.deltaTime : -rotationSpeed * Time.deltaTime;
                    transform.eulerAngles = m_newEulerAngles;
                }

            }

            if (!m_playerWasDetected && m_playerWasTracked)
            {
                // We checking which direction should be selected after contact with target was lost
                m_currentRotation = transform.eulerAngles.y;
                m_currentRotation -= m_offset;
                if (m_currentRotation < 0) m_currentRotation += 360;

                rotateClockWise = (m_currentRotation > m_limitEuler_Front);
                m_playerWasTracked = false;
            }
        }

        public float RequiredAwarness() => m_detectionSensitive;
        public float CurrentAwarnes() => m_currentAwarnes;

        public bool SomethingWasDetected() => m_playerWasDetected;
    }
}
