using UnityEngine;
namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Player/BC 5: NoiseTrapPusher", 5)]

    public class NoiseTrapPusher : MonoBehaviour
    {
        [SerializeField] float m_force;
        ColliderReference m_colliderReference;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!hit.gameObject.TryGetComponent<ColliderReference>(out m_colliderReference))
            {
                m_colliderReference = hit.gameObject.AddComponent<ColliderReference>();
            }

            if (m_colliderReference.isNoiseTrap(out NoiseTrap _noiseTrap))
            {
                Vector3 _forceDirection = hit.transform.position - transform.position;
                _forceDirection.y = 0;
                _forceDirection.Normalize();
                _noiseTrap.AddForceAtPosition(_forceDirection * m_force, transform.position, ForceMode.Impulse);
            }
        }
    }
}
