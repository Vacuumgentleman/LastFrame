using UnityEngine;

namespace Senses2026
{
    public class Waypoint_DemoScene : MonoBehaviour
    {
        [SerializeField] MeshRenderer m_meshRenderer;
        [SerializeField] Vector3[] m_randomPostions;
        [SerializeField] float m_randomRange;
        [SerializeField] int m_amountOfPoints;
        int m_currentPoint;
        bool m_wasDestroyed = false;

        bool m_wasEnabled = false;
        void Awake()
        {
            if (!m_wasEnabled) enable();
        }

        void enable()
        {
            m_randomPostions = new Vector3[m_amountOfPoints];
            Vector3 _poisiton = transform.position;
            for (int i = 0; i < m_amountOfPoints; i++)
            {
                m_randomPostions[i] = _poisiton + new Vector3(Random.Range(-m_randomRange, m_randomRange), 0f, Random.Range(-m_randomRange, m_randomRange));
            }
            m_wasEnabled = true;
        }

        public Vector3 ProvidePosition()
        {
            if (!m_wasEnabled) enable();
            m_currentPoint++;
            if (m_currentPoint >= m_amountOfPoints) m_currentPoint = 0;
            return m_randomPostions[m_currentPoint];
        }

        public bool WasDestroyed => m_wasDestroyed;

        public void DestroyWaypoint() => m_wasDestroyed = true;

    }
}
