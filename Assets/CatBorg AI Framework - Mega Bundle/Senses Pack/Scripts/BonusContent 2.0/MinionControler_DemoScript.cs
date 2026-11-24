using System.Collections;
using UnityEngine;

namespace Senses2026
{
    public class MinionControler_DemoScript : MonoBehaviour
    {
        [SerializeField] Waypoint_DemoScene[] m_waypoints;
        [SerializeField] float m_speed;
        int m_currentWaypoint;
        Vector3 m_lookAtDestiantion;
        Vector3 m_direction;
        bool m_waypointReached = false;
        [SerializeField] float m_speedRotation;
        bool m_isDead = false;

        [SerializeField] Collider m_collider;
        [SerializeField] MeshRenderer m_meshRenderer;
        [SerializeField] Rigidbody m_rigidbody;
        [SerializeField] MinionHealth_DemoScript m_minionHealth;
        [SerializeField] Senses m_senses;
        [SerializeField] int m_requiredAwarness;
        [SerializeField] FactionNames m_enemyFaction;
        FactionNames m_ownFaction;
        public FactionNames Faction => m_ownFaction;
        TargetSenses m_enemy;
        [SerializeField] TargetSenses m_targetSenses;

        MinionSpawner_DemoScript m_minionSpawner_DemoScript;
        MinionSpawner_DemoScript m_enemy_MinionSpawner_DemoScript;

        Vector3 m_destiantion;
        bool m_targetNearBy;
        IntervalEnumerator m_attackCooldown;
        bool m_readyToAttack = false;
        Transform m_ownTransform;
        Vector3 m_ownPosition;

        void Awake()
        {
            StartCoroutine(cooldown_Attack());

            m_ownFaction = GetComponent<TargetSenses>().FactionName;

            MinionSpawner_DemoScript[] _minionSpawner_DemoScripts;

#if UNITY_6000_5_OR_NEWER

            // Newer Unity 6 versions
            _minionSpawner_DemoScripts = 
                GameObject.FindObjectsByType<MinionSpawner_DemoScript>();

#elif UNITY_2022_2_OR_NEWER

    // Unity 2022.2 - older Unity 6 versions
    _minionSpawner_DemoScripts =
        GameObject.FindObjectsByType<MinionSpawner_DemoScript>(
            FindObjectsSortMode.None
        );

#else

    // Older Unity versions
    _minionSpawner_DemoScripts =
        GameObject.FindObjectsOfType<MinionSpawner_DemoScript>();

#endif

            foreach (MinionSpawner_DemoScript minionSpawner_DemoScript in _minionSpawner_DemoScripts)
            {
                if (minionSpawner_DemoScript.Faction != m_ownFaction)
                    m_enemy_MinionSpawner_DemoScript = minionSpawner_DemoScript;
            }

            m_ownTransform = GetComponent<Transform>();
        }

        void Update()
        {
            if (m_isDead) return;


            if (m_readyToAttack)
            {
                m_targetNearBy = m_senses.WasAnythingSeen(m_requiredAwarness, out m_enemy, out float _currentAwarness, out float _distanceSQR, out float _angle, new FactionNames[1] { m_enemyFaction }, SeeSenseSearchType.Nearest, true);
                if (m_targetNearBy)
                {
                    m_ownPosition = m_ownTransform.position;
                    m_lookAtDestiantion = m_enemy.transform.position;
                    m_lookAtDestiantion.y = m_ownPosition.y;

                    m_direction = m_lookAtDestiantion - m_ownPosition;
                    Quaternion _rotaion = Quaternion.Slerp(m_ownTransform.rotation, Quaternion.LookRotation(m_direction), Time.deltaTime * m_speedRotation);

                    if (m_direction.magnitude <= .5f)
                    {
                        if (m_readyToAttack && m_enemy.TryGetComponent<MinionHealth_DemoScript>(out MinionHealth_DemoScript _minionHealth_DemoScript))
                        {
                            _minionHealth_DemoScript.Damage(1, m_targetSenses);
                            m_readyToAttack = false;
                        }
                    }
                    else
                    {
                        m_rigidbody.linearVelocity = m_ownTransform.forward * m_speed;
                        m_rigidbody.MoveRotation(_rotaion);
                        m_targetNearBy = false;
                    }


                }
                else
                if (!m_waypointReached)
                {

                    if (m_waypoints.Length == 0)
                    {
                        m_waypointReached = true;
                        return;
                    }
                    m_ownPosition = m_ownTransform.position;
                    m_lookAtDestiantion = m_destiantion;
                    m_lookAtDestiantion.y = m_ownPosition.y;

                    m_direction = m_lookAtDestiantion - m_ownPosition;
                    Quaternion _rotaion = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_direction), Time.deltaTime * m_speedRotation);


                    if (m_direction.magnitude <= 1.5f)
                    {
                        m_currentWaypoint++;

                        while (m_currentWaypoint < m_waypoints.Length && m_waypoints[m_currentWaypoint].WasDestroyed)
                        {
                            m_currentWaypoint++;
                        }

                        if (m_currentWaypoint >= m_waypoints.Length)
                        {
                            m_waypointReached = true;
                            reachedDesitiantion();
                            return;
                        }

                        m_destiantion = m_waypoints[m_currentWaypoint].ProvidePosition();
                    }

                    m_rigidbody.linearVelocity = m_ownTransform.forward * m_speed;
                    m_rigidbody.MoveRotation(_rotaion);

                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (m_readyToAttack && collision.gameObject.TryGetComponent<MinionTowerHealth_DemoScript>(out MinionTowerHealth_DemoScript _minionTowerHealth_DemoScript))
            {
                if (_minionTowerHealth_DemoScript.Faction != m_ownFaction)
                {
                    _minionTowerHealth_DemoScript.Damage(1);
                    m_readyToAttack = false;
                }

            }
            else if (m_readyToAttack && collision.gameObject.TryGetComponent<MinionHealth_DemoScript>(out MinionHealth_DemoScript _minionHealth_DemoScript))
            {
                if (_minionHealth_DemoScript.Faction != m_ownFaction)
                {
                    _minionHealth_DemoScript.Damage(1, m_targetSenses);
                    m_readyToAttack = false;
                }

            }
        }

        public void Enable(Waypoint_DemoScene[] _waypoints, MinionSpawner_DemoScript _minionSpawner_DemoScript)
        {
            m_waypoints = _waypoints;
            m_minionSpawner_DemoScript = _minionSpawner_DemoScript;
            m_currentWaypoint = 0;
            m_isDead = false;
            m_collider.enabled = true;
            m_meshRenderer.enabled = true;
            m_waypointReached = false;

            m_minionHealth.Reset();
            this.gameObject.SetActive(true);
            m_rigidbody.WakeUp();
            m_readyToAttack = true;
            StopAllCoroutines();
            StartCoroutine(cooldown_Attack());

            m_destiantion = m_waypoints[m_currentWaypoint].ProvidePosition();
        }

        public void Dead()
        {
            m_enemy_MinionSpawner_DemoScript.ProvideKillReward();
            m_isDead = true;
            m_collider.enabled = false;
            m_meshRenderer.enabled = false;
            m_rigidbody.Sleep();



            this.gameObject.SetActive(false);
            m_senses.ResetAwarness();

            MinionDead_DemoScript _minionDead_DemoScript = DummyQuene_DemoScript.Instance.Provide_DeadMinion((int)m_ownFaction - 1);
            _minionDead_DemoScript.Enable(this.transform.position, this.transform.rotation);


            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).TryGetComponent<ArrowDummy_DemoScript>(out ArrowDummy_DemoScript _arrowDummy_DemoScript))
                {
                    _arrowDummy_DemoScript.transform.parent = _minionDead_DemoScript.transform;
                }
            }


            m_minionSpawner_DemoScript.ReturnToQuene(this);
        }
        void reachedDesitiantion()
        {
            m_isDead = true;
            m_collider.enabled = false;
            m_meshRenderer.enabled = false;

            this.gameObject.SetActive(false);
            m_minionSpawner_DemoScript.ReturnToQuene(this, true);

            m_minionSpawner_DemoScript.EnableWinCanvas();

        }


        IEnumerator cooldown_Attack()
        {
            m_attackCooldown = new IntervalEnumerator(.5f, cooldown_Attack_Action);
            while (true)
            {
                m_attackCooldown.MoveNext(Time.deltaTime);
                yield return null;
            }
        }
        void cooldown_Attack_Action()
        {
            m_readyToAttack = true;
        }

        public void LastAttackignUnityWas(TargetSenses _targetSenses)
        {
            m_enemy = _targetSenses;
        }
    }

}
