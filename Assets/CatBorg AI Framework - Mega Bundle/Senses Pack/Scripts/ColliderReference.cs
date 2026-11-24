using UnityEngine;

namespace Senses2026
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")] // Hide from Add Component menu
    public class ColliderReference : MonoBehaviour
    {
        TargetSenses m_targetSenses;
        ScentZoneDistractor m_scentZoneDistractor;
        ScentNode m_scentNode;
        NoiseTrap m_noiseTrap;
        StealthZone m_stealthZone;
        RedHandedZone m_redHandedZone;
        RedHandedObject m_redHandedObject;
        IRedHandedInterface m_redHandedInterface;

        // NEW: obstacle caching
        Obstacle m_obstacle;
        bool m_is_Obstacle;

        bool m_iS_SensesTarget;
        bool m_iS_ScentNode;
        bool m_is_ScentZoneDistractor;
        bool m_is_NoiseTrap;
        bool m_is_StealthZone;
        bool m_is_RedHandedZone;
        bool m_is_RedHandedObject;
        bool m_uses_RedHandedInterface;


        void Awake()
        {
            hideFlags = HideFlags.HideInInspector | HideFlags.NotEditable;
            enable();
        }

        void OnValidate()
        {
            hideFlags = HideFlags.HideInInspector | HideFlags.NotEditable;
        }

        void OnEnable()
        {
            hideFlags = HideFlags.HideInInspector | HideFlags.NotEditable;
        }

        void enable()
        {
            m_targetSenses = GetComponentInParent<TargetSenses>();
            m_iS_SensesTarget = m_targetSenses != null;

            m_scentNode = GetComponent<ScentNode>();
            m_iS_ScentNode = m_scentNode != null;

            m_scentZoneDistractor = GetComponentInParent<ScentZoneDistractor>();
            m_is_ScentZoneDistractor = m_scentZoneDistractor != null;

            m_noiseTrap = GetComponentInParent<NoiseTrap>();
            m_is_NoiseTrap = m_noiseTrap != null;

            m_stealthZone = GetComponent<StealthZone>();
            m_is_StealthZone = m_stealthZone != null;

            m_redHandedZone = GetComponentInParent<RedHandedZone>();
            m_is_RedHandedZone = m_redHandedZone != null;

            m_redHandedObject = GetComponentInParent<RedHandedObject>();
            m_is_RedHandedObject = m_redHandedObject != null;

            m_redHandedInterface = GetComponentInParent<IRedHandedInterface>();
            m_uses_RedHandedInterface = m_redHandedInterface != null;

            m_obstacle = GetComponentInParent<Obstacle>();
            m_is_Obstacle = m_obstacle != null;
        }

        public bool iS_SensesTarget(out TargetSenses _targetSenses)
        {
            _targetSenses = m_targetSenses;
            return m_iS_SensesTarget;
        }

        public bool iS_ScentNode(out ScentNode _scentNode)
        {
            _scentNode = m_scentNode;
            return m_iS_ScentNode;
        }

        public bool iS_ZonerScentDisctractor(out ScentZoneDistractor _scentZoneDistractor)
        {
            _scentZoneDistractor = m_scentZoneDistractor;
            return m_is_ScentZoneDistractor;
        }

        //Here still slow
        public bool isNoiseTrap(out NoiseTrap _noiseTrap)
        {
            _noiseTrap = m_noiseTrap;
            return m_is_NoiseTrap;
        }

        public bool isStealthZone(out StealthZone _stealthZone)
        {
            _stealthZone = m_stealthZone;
            return m_is_StealthZone;
        }

        public bool isRedHandedEntity(out IRedHandedInterface _redHandedEntity)
        {
            _redHandedEntity = m_redHandedInterface;
            return m_uses_RedHandedInterface;
        }

        public bool isObstacle(out Obstacle _obstacleRef)
        {
            _obstacleRef = m_obstacle;
            return m_is_Obstacle;
        }
    }
}
