using UnityEngine;


namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 31: Scent Zone Distractor", 31)]
    [RequireComponent(typeof(ColliderReference))]
    public class ScentZoneDistractor : MonoBehaviour
    {
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;
        [field: SerializeField] bool m_helpPropertiesFoldout;
        public void Toggle_HelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;

        [SerializeField] private float m_scentStrengthChange;
        public float ScentStrengthChange
        {
            get
            {
                if (!enabled)
                    return 0f;

                return m_scentStrengthChange;
            }
            private set => m_scentStrengthChange = value;
        }
        public void Set_ScentStrengthChange(float value)
            => m_scentStrengthChange = value;

        [SerializeField] private ScentModification m_scentModification;
        public ScentModification ScentModyfication
        {
            get
            {
                // When component is disabled → ALWAYS treat as None
                if (!enabled)
                    return ScentModification.None;

                return m_scentModification;
            }
            private set => m_scentModification = value;
        }
        public void Set_ScentModyfication(ScentModification _value)
            => m_scentModification = _value;
    }
}

