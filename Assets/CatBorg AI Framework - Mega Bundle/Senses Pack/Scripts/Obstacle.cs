using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 60: Obstacle", 60)]
    public class Obstacle : MonoBehaviour
    {
        [field: SerializeField] public float Range_DecreaseFactor { get; private set; }
        public void Set_RangeDecreaseFactor(float _range_DecreaseFactor) => Range_DecreaseFactor = _range_DecreaseFactor;

        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;
        [field: SerializeField] bool m_helpPropertiesFoldout;
        public void Toggle_HelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;

    }
}
