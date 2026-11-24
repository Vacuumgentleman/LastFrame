using System.Collections;
using UnityEngine;

namespace Senses2026
{
    public class MinionDead_DemoScript : MonoBehaviour
    {
        [SerializeField] int m_index;
        [SerializeField] Rigidbody m_rigidbody;
        [SerializeField] Collider m_colider;
        IntervalEnumerator m_layingStill_IntervalEnumerator;
        [SerializeField] float m_timeOf_LayingStill;
        Transform m_transform;
        [SerializeField] float m_required_Y;
        Vector3 m_posiion;
        bool m_wasEanbled = false;

        void enable()
        {
            m_transform = this.transform;
            m_wasEanbled = true;
        }

        public void Enable(Vector3 _position, Quaternion _rotation)
        {
            if (!m_wasEanbled) enable();

            m_transform.position = _position;
            m_transform.rotation = _rotation;

            m_rigidbody.WakeUp();
            m_colider.enabled = true;

            this.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(LayStillOnGround());
        }



        IEnumerator LayStillOnGround()
        {
            yield return new WaitForSecondsRealtime(m_timeOf_LayingStill);
            m_rigidbody.Sleep();
            m_rigidbody.linearVelocity = Vector3.zero;
            m_colider.enabled = false;
            m_posiion = m_transform.position;

            float _current_Y = m_posiion.y;
            while (_current_Y > m_required_Y)
            {
                m_posiion.y -= .01f;
                m_transform.position = m_posiion;
                _current_Y = m_posiion.y;
                yield return null;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).TryGetComponent<ArrowDummy_DemoScript>(out ArrowDummy_DemoScript _arrowDummy_DemoScript))
                {
                    _arrowDummy_DemoScript.ReturnToQuene();
                }
            }


            DummyQuene_DemoScript.Instance.ReturnToQuene(this, m_index);
            this.gameObject.SetActive(false);
        }
    }

}
