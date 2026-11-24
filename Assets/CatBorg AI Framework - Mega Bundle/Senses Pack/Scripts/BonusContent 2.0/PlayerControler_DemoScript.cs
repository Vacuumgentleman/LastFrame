using UnityEngine;

namespace Senses2026
{
    public class PlayerControler_DemoScript : MonoBehaviour
    {
        Vector3 m_moveVector;
        [SerializeField] float m_speed = 5f;
        [SerializeField] Rigidbody m_rigidbody;

        void Update()
        {
            m_moveVector = Vector3.zero;

            if (SensesDemoInput.GetKey(KeyCode.W))
            {
                m_moveVector.z += 1f;
            }
            if (SensesDemoInput.GetKey(KeyCode.S))
            {
                m_moveVector.z -= 1f;
            }
            if (SensesDemoInput.GetKey(KeyCode.A))
            {
                m_moveVector.x -= 1f;
            }
            if (SensesDemoInput.GetKey(KeyCode.D))
            {
                m_moveVector.x += 1f;
            }

            m_moveVector = m_moveVector.normalized * m_speed;
        }

        void FixedUpdate()
        {
            m_rigidbody.linearVelocity = m_moveVector;
        }
    }
}


