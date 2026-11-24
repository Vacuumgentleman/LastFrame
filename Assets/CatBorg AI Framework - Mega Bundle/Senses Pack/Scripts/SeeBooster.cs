using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 11: See Booster", 11)]
    [RequireComponent(typeof(Senses))]
    public class SeeBooster : MonoBehaviour
    {
        // Compatibility wrapper for PinePack / old Senses API / I hate this... but such fast fix...
        public void Set_ActiveState(bool state)
        {
            enabled = state;
        }

        Senses m_senses;
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;
        [field: SerializeField] bool m_helpPropertiesFoldout;
        public void Toggle_HelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;

        [field: SerializeField] public AffectedAsspect AffectedAspect { get; private set; }
        public void Set_AffectedAsspect(AffectedAsspect aspect)
        {
            AffectedAspect = aspect;
            RecalculateIfNeeded();
        }

        [field: SerializeField] public OperationType OperationType { get; private set; }
        public void Set_OperationType(OperationType operation)
        {
            OperationType = operation;
        }

        [field: SerializeField] public float Value { get; private set; }
        public void Set_Value(float v)
        {
            Value = v;
        }

        // --------------------------------------------------------------------
        // APPLY BOOST
        // --------------------------------------------------------------------
        public float Apply_BuffTo(float deltaSeeAwareness)
        {
            if (!enabled) return deltaSeeAwareness;

            switch (OperationType)
            {
                case OperationType.Addition:
                    return deltaSeeAwareness + Value;
                case OperationType.Subtraction:
                    return deltaSeeAwareness - Value;
                case OperationType.Multiplication:
                    return deltaSeeAwareness * Value;
                case OperationType.Division:
                    return deltaSeeAwareness / Value;
            }

            return deltaSeeAwareness;
        }

        // --------------------------------------------------------------------
        // UNITY LIFECYCLE
        // --------------------------------------------------------------------
        void Awake()
        {
            m_senses = GetComponent<Senses>();
        }

        void OnEnable()
        {
            RecalculateIfNeeded();
        }

        void OnDisable()
        {
            RecalculateIfNeeded();
        }

        void OnValidate()
        {
            // Unity might run this before Awake — safe check
            if (m_senses == null)
                m_senses = GetComponent<Senses>();

            RecalculateIfNeeded();
        }

        // --------------------------------------------------------------------
        // HELPERS
        // --------------------------------------------------------------------
        void RecalculateIfNeeded()
        {
            if (AffectedAspect == AffectedAsspect.cetralVisionRadius ||
                AffectedAspect == AffectedAsspect.peripheralVisionRadius)
            {
                if (m_senses != null)
                    m_senses.Recalcuate_SQRs();
            }
        }
    }

    public enum OperationType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }

    public enum AffectedAsspect
    {
        deltaSeeAwarness,
        cetralVisionRadius,
        peripheralVisionRadius,
    }
}
