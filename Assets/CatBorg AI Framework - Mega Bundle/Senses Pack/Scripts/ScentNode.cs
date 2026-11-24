using System.Collections;
using UnityEngine;

namespace Senses2026
{
    public class ScentNode : MonoBehaviour
    {

        [SerializeField] float m_initialStrength;
        [SerializeField] float m_remainingStrength;
        public float m_Strength => m_remainingStrength;
        public float Strength_Relative => m_remainingStrength / m_initialStrength;
        [SerializeField] Vector3 m_direction;
        public Vector3 Direction => m_direction;

        TargetSenses m_targetSenses;
        public TargetSenses TargetSenses => m_targetSenses;

        public int NodeID => m_nodeID;
        int m_nodeID = -1;

        WaitForSecondsRealtime m_delay;
        Collider m_collider;
        Transform m_transform;
        bool m_wasEnabled = false;
        void enable()
        {
            m_collider = GetComponent<Collider>();
            m_transform = GetComponent<Transform>();
            m_wasEnabled = true;
        }

        public void Assign_ID(int _nodeID) => m_nodeID = _nodeID;
        public void Release(TargetSenses _targetSenses, float _initialStrength, Vector3 _initialDirection, Vector3 _initialPosition, LayerMask _scentLayerMask = default)
        {
            if (!m_wasEnabled) enable();

            m_transform.position = _initialPosition;
            m_targetSenses = _targetSenses;
            if (m_initialStrength != _initialStrength)
            {
                m_delay = new WaitForSecondsRealtime(_initialStrength);
                m_initialStrength = _initialStrength;
            }
            m_direction = _initialDirection;

            m_collider.enabled = true;

            gameObject.layer = _scentLayerMask;

            StartCoroutine(scentStrengthWeakener());
            StartCoroutine(selfDestroy());
        }
        public void UpdateDirection(Vector3 _nextNodePostion)
        {
            m_direction = _nextNodePostion - this.transform.position;
            //m_direction.Normalize();
        }

        IEnumerator selfDestroy()
        {
            yield return m_delay;
            ReturnToQuene();
        }

        IEnumerator scentStrengthWeakener()
        {
            m_remainingStrength = m_initialStrength;
            while (m_remainingStrength > 0)
            {
                m_remainingStrength -= Time.deltaTime;
                yield return null;
            }
        }

        public void ReturnToQuene()
        {
            m_collider.enabled = false;
            StopAllCoroutines();
            ScenNodePoll.Instance.ReturnToPool(this);
            gameObject.SetActive(false);
        }
    }

}
