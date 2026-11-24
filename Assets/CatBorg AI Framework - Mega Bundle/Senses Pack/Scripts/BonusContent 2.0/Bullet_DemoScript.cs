using System.Collections;
using UnityEngine;

namespace Senses2026
{
    public class Bullet_DemoScript : MonoBehaviour
    {
        [SerializeField] Rigidbody m_rigidbody;
        [SerializeField] Collider m_collider;
        [SerializeField] MeshRenderer m_renderer;
        [SerializeField] float m_selfDestroyTime = 2f;
        [SerializeField] TrailRenderer m_trailRenderer;
        WaitForSecondsRealtime m_selfDestroyDelay;
        bool m_wasEnabled = false;

        void enable()
        {
            m_selfDestroyDelay = new WaitForSecondsRealtime(m_selfDestroyTime);
            m_wasEnabled = true;
        }

        Quaternion m_roatation;
        public void Shot(Vector3 _initialPosition, Vector3 _direction, float _speed)
        {
            if (!m_wasEnabled) enable();
            m_rigidbody.WakeUp();
            m_collider.enabled = true;
            m_renderer.enabled = true;

            StopAllCoroutines();

            transform.position = _initialPosition;
            m_roatation = Quaternion.LookRotation(_direction);
            transform.rotation = m_roatation;
            m_rigidbody.AddForce(_direction * _speed, ForceMode.Impulse);
            StartCoroutine(SelfDestroy());
            m_trailRenderer.enabled = true;

        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                ArrowDummy_DemoScript _arrowDummy_DemoScript = DummyQuene_DemoScript.Instance.Provide_Bullet();
                _arrowDummy_DemoScript.Setup(this.transform.position, m_roatation);
                returnToQuene();
                return;
            }

            if (collision.gameObject.TryGetComponent<MinionHealth_DemoScript>(out MinionHealth_DemoScript _minionHealth_DemoScript))
            {
                _minionHealth_DemoScript.Damage(1);
                ArrowDummy_DemoScript _arrowDummy_DemoScript = DummyQuene_DemoScript.Instance.Provide_Bullet();
                _arrowDummy_DemoScript.Setup(this.transform.position, m_roatation, _minionHealth_DemoScript.transform);
                returnToQuene();
                return;
            }
        }

        IEnumerator SelfDestroy()
        {
            yield return m_selfDestroyDelay;
            returnToQuene();
        }

        void returnToQuene()
        {
            m_rigidbody.Sleep();
            m_collider.enabled = false;
            m_renderer.enabled = false;
            m_trailRenderer.enabled = false;
            StopAllCoroutines();


            BulletQuene_DemoScript.Instance.ReturnToQuene(this);
        }

    }
}
