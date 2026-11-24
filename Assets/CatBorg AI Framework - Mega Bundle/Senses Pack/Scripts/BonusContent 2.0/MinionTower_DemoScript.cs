using System;
using System.Collections;
using UnityEngine;

namespace Senses2026
{
    public class MinionTower_DemoScript : MonoBehaviour
    {
        [SerializeField] Senses m_senses;
        [SerializeField][Range(1, 100)] int m_requiredAwarness;
        [SerializeField] FactionNames m_enemyFaction;
        public FactionNames OwnFaction => m_ownFaction;
        FactionNames m_ownFaction;
        Bullet_DemoScript m_currentBullet;
        [SerializeField] Transform[] m_shotInitialTransforms;
        Vector3[] m_shotInitialPositions;

        [SerializeField] float m_reloadTime;
        bool m_isReloading = false;
        [SerializeField] float m_bulletForce;
        Vector3 m_targetPoistion;
        Vector3 m_direction;
        Vector3 m_bulletOriginPoint;
        TargetSenses m_targetSenses;
        IntervalEnumerator m_attackCooldown;

        [SerializeField] Waypoint_DemoScene[] m_waypoints;
        float m_range;

        void Awake()
        {
            m_ownFaction = GetComponent<TargetSenses>().FactionName;
            m_transform_Position = this.transform.position;
            m_shotInitialPositions = new Vector3[m_shotInitialTransforms.Length];
            for (int i = 0; i < m_shotInitialTransforms.Length; i++)
                m_shotInitialPositions[i] = m_shotInitialTransforms[i].position;
            StartCoroutine(reload());
            m_range = m_senses.Central_VisionRadius;
        }

        void Update()
        {
            if (!m_isReloading)

                if (m_senses.WasAnythingSeen(m_requiredAwarness, out m_targetSenses, out float _currentAwarness, out float _distanceSQR, out float _angle, new FactionNames[1] { m_enemyFaction }, SeeSenseSearchType.Nearest, true))
                {

                    m_targetPoistion = m_targetSenses.transform.position;

                    float _distance = Vector3.Distance(m_targetPoistion, this.transform.position);

                    if (_distance < m_range)
                    {
                        m_currentBullet = BulletQuene_DemoScript.Instance.Provide_Bullet();
                        m_bulletOriginPoint = provideNearestBulletOriginPoint(m_targetPoistion);
                        m_direction = m_targetPoistion - m_bulletOriginPoint;

                        m_currentBullet.Shot(m_bulletOriginPoint, m_direction, m_bulletForce);
                        m_isReloading = true;
                    }

                }
        }




        IEnumerator reload()
        {
            m_attackCooldown = new IntervalEnumerator(m_reloadTime, reload_Action);
            while (true)
            {
                m_attackCooldown.MoveNext(Time.deltaTime);
                yield return null;
            }
        }
        void reload_Action()
        {
            m_isReloading = false;
        }

        public void Destroyed()
        {
            foreach (Waypoint_DemoScene _waypoint_DemoScene in m_waypoints)
                _waypoint_DemoScene.DestroyWaypoint();

            Destroy(gameObject);
        }

        int m_index;
        float m_xD;
        float m_zD;
        Vector3 m_transform_Position;
        float m_curerntDistance;
        float m_nearestDistance;

        Vector3 provideNearestBulletOriginPoint(Vector3 _position)
        {
            m_index = 0;
            m_nearestDistance = float.MaxValue;

            for (int i = 0; i < m_shotInitialTransforms.Length; i++)
            {
                m_curerntDistance = currentDistanceSQR_XZ(m_shotInitialPositions[i]);
                if (m_curerntDistance < m_nearestDistance)
                {
                    m_index = i;
                    m_nearestDistance = m_curerntDistance;
                }
            }
            return m_shotInitialPositions[m_index];
        }

        float currentDistanceSQR_XZ(Vector3 _originPosition)
        {
            m_xD = m_targetPoistion.x - _originPosition.x;
            m_zD = m_targetPoistion.z - _originPosition.z;
            return m_xD * m_xD + m_zD * m_zD;
        }
    }
}

