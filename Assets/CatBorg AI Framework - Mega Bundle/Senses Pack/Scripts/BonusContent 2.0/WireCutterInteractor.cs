using UnityEngine;

namespace Senses2026BonusContent
{
    public class WireCutterInteractor : MonoBehaviour
    {
        [SerializeField] Transform m_afterActionPrefab;

        public void ActionWasFinished()
        {
            Transform _newTransform = Instantiate(m_afterActionPrefab);
            _newTransform.position = this.transform.position;
            _newTransform.rotation = this.transform.rotation;
            _newTransform.localScale = this.transform.localScale;
            _newTransform.SetParent(this.transform.parent);

            Destroy(this.gameObject);
        }
    }

}
