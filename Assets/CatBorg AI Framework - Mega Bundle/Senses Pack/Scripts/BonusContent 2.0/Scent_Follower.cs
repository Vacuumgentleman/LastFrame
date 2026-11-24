using System.Collections;
using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Sentries/Bonus Content 3: Scent Follower", 3)]

    [RequireComponent(typeof(Senses))]

    public class ScentFollower : MonoBehaviour, ISentry
    {
        Senses m_senses;
        [SerializeField][Range(0.01f, 100f)] float m_detectionSensitive;
        [SerializeField] TargetSenses m_tracked;

        [Header("Sentry Properies")]
        [SerializeField][Range(.5f, 3f)] float m_speedForwardMovment;
        [SerializeField][Range(0f, 1f)] float m_speedTurnSmoothing;
        [SerializeField][Range(0.1f, 2f)] float m_distanceStoping;
        [SerializeField] FactionNames[] m_factionNames = new FactionNames[] { FactionNames.None, FactionNames.Faction1, FactionNames.Faction2, FactionNames.Faction3 };

        Animator m_animator;
        bool m_isIdle = true;
        bool m_isTracking = false;
        int m_isTrackingHashValue;
        int m_isIdleHashValue;
        Quaternion m_rotation;
        bool m_hasAnimator;

        [SerializeField] Transform[] m_waypoints;
        int m_currentWaypoint;
        int m_waypointLenght;
        bool m_idleEnumetaratorOngoing = false;
        bool m_curernt_DetectionResult;
        bool m_previous_DetectionResult;

        Vector3 m_trackedPosition;
        Vector3 m_transformPostion;

        public float RequiredAwarness() => m_detectionSensitive;

        float m_currentAwarness;
        public float CurrentAwarnes() => m_currentAwarness;

        void Awake()

        {
            m_senses = GetComponent<Senses>();
            m_hasAnimator = TryGetComponent<Animator>(out m_animator);

            m_isTrackingHashValue = Animator.StringToHash("isTracking");
            m_isIdleHashValue = Animator.StringToHash("isIdle");

            m_waypointLenght = m_waypoints.Length;
            m_currentWaypoint = 0;
        }

        void Update()
        {
            m_curernt_DetectionResult = m_senses.WasAnythingSmelled(m_detectionSensitive, out m_tracked, out m_currentAwarness, out Vector3 _scentNodeDirection, out float _scentStrength, m_factionNames);

            #region Idle Break After Loosing Trail
            if (m_curernt_DetectionResult != m_previous_DetectionResult)
            {
                m_previous_DetectionResult = m_curernt_DetectionResult;
                if (!m_curernt_DetectionResult)
                {
                    if (!m_idleEnumetaratorOngoing) StartCoroutine(LostTractRoutine());
                }
                else
                {
                    StopAllCoroutines();
                    m_idleEnumetaratorOngoing = false;
                }
            }

            if (m_idleEnumetaratorOngoing) return;
            #endregion

            if (m_curernt_DetectionResult)
            {
                m_trackedPosition = m_tracked.transform.position;
                m_transformPostion = transform.position;
                m_trackedPosition.y = m_transformPostion.y;

                float _distance = Vector3.Distance(this.transform.position, m_trackedPosition);
                if (_distance > m_distanceStoping)
                {
                    if (_scentNodeDirection != Vector3.zero)
                    {
                        float _distanceToScentNode = Vector3.Distance(this.transform.position, _scentNodeDirection);

                        if (_distanceToScentNode > m_distanceStoping)
                        {
                            m_rotation = Quaternion.LookRotation(_scentNodeDirection - transform.position, Vector3.up);

                            if (m_speedTurnSmoothing > 0f)
                            {
                                m_rotation = Quaternion.Slerp(this.transform.rotation, m_rotation, m_speedTurnSmoothing);
                            }

                            m_rotation.eulerAngles = new Vector3(0f, m_rotation.eulerAngles.y, 0f);
                            transform.rotation = m_rotation;
                            transform.Translate(transform.forward * m_speedForwardMovment * Time.deltaTime, Space.World);
                            setAnimatorForWalk();
                            return;
                        }
                    }
                }
                setAniamtorForIdle();
            }
            else
            {
                #region Waypoints Following
                Vector3 _directionToWaypoint = m_waypoints[m_currentWaypoint].position - this.transform.position;
                m_rotation = Quaternion.LookRotation(_directionToWaypoint, Vector3.up);
                if (m_speedTurnSmoothing > 0f)
                {
                    m_rotation = Quaternion.Slerp(this.transform.rotation, m_rotation, m_speedTurnSmoothing);
                }
                m_rotation.eulerAngles = new Vector3(0f, m_rotation.eulerAngles.y, 0f);
                transform.rotation = m_rotation;

                float _distance = Vector3.Distance(this.transform.position, m_waypoints[m_currentWaypoint].position);
                if (_distance > m_distanceStoping)
                {
                    transform.Translate(transform.forward * m_speedForwardMovment * Time.deltaTime, Space.World);

                    setAnimatorForWalk();
                }
                else
                {
                    if (!m_idleEnumetaratorOngoing) StartCoroutine(WaypointReachedRoutine());
                }
                #endregion
            }


        }

        void setAnimatorForWalk()
        {
            if (m_hasAnimator && !m_isTracking && !m_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                m_isTracking = true;
                m_isIdle = false;
                m_animator.SetTrigger(m_isTrackingHashValue);
            }
        }
        void setAniamtorForIdle()
        {
            if (m_hasAnimator && !m_isIdle && !m_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                m_isTracking = false;
                m_isIdle = true;
                m_animator.SetTrigger(m_isIdleHashValue);
            }
        }

        IEnumerator LostTractRoutine()
        {
            m_idleEnumetaratorOngoing = true;
            if (m_hasAnimator && !m_isIdle && !m_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                m_isTracking = false;
                m_isIdle = true;
                m_animator.SetTrigger(m_isIdleHashValue);
            }

            yield return new WaitForSecondsRealtime(Random.Range(3f, 5f));

            if (m_hasAnimator && !m_isTracking && !m_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                m_isTracking = true;
                m_isIdle = false;
                m_animator.SetTrigger(m_isTrackingHashValue);
            }
            m_idleEnumetaratorOngoing = false;

        }
        IEnumerator WaypointReachedRoutine()
        {
            m_idleEnumetaratorOngoing = true;
            if (Random.Range(0, 100) > 50)
            {
                if (m_hasAnimator && !m_isIdle && !m_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    m_isTracking = false;
                    m_isIdle = true;
                    m_animator.SetTrigger(m_isIdleHashValue);
                }

                yield return new WaitForSecondsRealtime(Random.Range(3f, 5f));

                if (m_hasAnimator && !m_isTracking && !m_animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    m_isTracking = true;
                    m_isIdle = false;
                    m_animator.SetTrigger(m_isTrackingHashValue);
                }
            }


            yield return null;
            m_currentWaypoint++;
            if (m_currentWaypoint > m_waypointLenght - 1)
            {
                m_currentWaypoint = 0;
            }
            m_idleEnumetaratorOngoing = false;

        }

        public bool SomethingWasDetected() => m_curernt_DetectionResult;
    }
}





