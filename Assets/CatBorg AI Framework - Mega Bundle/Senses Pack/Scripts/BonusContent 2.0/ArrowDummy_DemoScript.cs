using System.Collections;
using UnityEngine;

namespace Senses2026
{
    public class ArrowDummy_DemoScript : MonoBehaviour
    {
        [SerializeField] MeshRenderer m_meshRenderer;
        [SerializeField] float m_selfDestroyTime;
        WaitForSecondsRealtime m_waitForRealtime;
        bool m_wasEnabled = false;

        void enable()
        {
            m_waitForRealtime = new WaitForSecondsRealtime(m_selfDestroyTime);
            m_wasEnabled = true;
        }

        public void Setup(Vector3 _postiion, Quaternion _rotaion, Transform _parent = null)
        {
            if (!m_wasEnabled) enable();
            transform.position = _postiion;
            transform.rotation = _rotaion;
            m_meshRenderer.enabled = true;

            if (_parent != null)
            {
                transform.parent = _parent;
                if (_parent.gameObject.activeSelf) StartCoroutine(SelfDestroy());
                else ReturnToQuene();
            }
            else
                StartCoroutine(SelfDestroy());
        }

        public void ReturnToQuene()
        {
            StopAllCoroutines();
            m_meshRenderer.enabled = false;
            transform.parent = null;
            DummyQuene_DemoScript.Instance.ReturnToQuene(this);
        }

        IEnumerator SelfDestroy()
        {
            yield return m_waitForRealtime;
            transform.parent = null;
            ReturnToQuene();
        }
    }
}
