using System.Collections;
using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 30: Scent", 30)]
    public class Scent : MonoBehaviour
    {
        TargetSenses m_targetSenses;
        [field: SerializeField]
        public float ScentStrength { get; private set; } = 5.5f;

        public void Set_ScentStrength(float _scentStrength)
        {
            if (ScentStrength == _scentStrength)
            {
                return;
            }

            ScentStrength = _scentStrength;
            if (!m_wasEnabled)
            {
                return;
            }

            if (UseVisualisation && m_scentVisualisation != null)
            {
                m_scentVisualisation.SetEmmision(false);
                m_scentVisualisation.ClearTransform();
                m_scentVisualisation = null;
                createScentVisualisation();
            }
        }

        [field: SerializeField]
        public bool UseVisualisation { get; private set; }

        public void Set_UseVisualisation(bool _useVisualisation)
        {
            if (UseVisualisation == _useVisualisation)
            {
                return;
            }

            UseVisualisation = _useVisualisation;
            if (!m_wasEnabled || !Application.isPlaying)
            {
                return;
            }

            if (UseVisualisation)
            {
                if (m_scentVisualisation == null)
                {
                    createScentVisualisation();
                }
                else
                {
                    m_scentVisualisation.SetEmmision(true);
                }
            }
            else if (m_scentVisualisation != null)
            {
                m_scentVisualisation.SetEmmision(false);
            }
        }

        [field: SerializeField]
        public Gradient ScentColorGradient { get; private set; }

        public void Set_ScentColorGradient(Gradient _scentColorGradient)
        {
            if (areGradientsEqual(ScentColorGradient, _scentColorGradient))
            {
                return;
            }

            ScentColorGradient = deepCopyGradient(_scentColorGradient);
            if (!m_wasEnabled)
            {
                return;
            }

            if (m_scentVisualisation != null)
            {
                m_scentVisualisation.SetEmmision(false);
                m_scentVisualisation.ClearTransform();
                m_scentVisualisation = null;
                createScentVisualisation();
            }
        }

        bool areGradientsEqual(Gradient _first, Gradient _second)
        {
            if (ReferenceEquals(_first, _second))
            {
                return true;
            }

            if (_first == null || _second == null || _first.mode != _second.mode)
            {
                return false;
            }

            GradientColorKey[] _firstColors = _first.colorKeys;
            GradientColorKey[] _secondColors = _second.colorKeys;
            GradientAlphaKey[] _firstAlphas = _first.alphaKeys;
            GradientAlphaKey[] _secondAlphas = _second.alphaKeys;
            if (_firstColors.Length != _secondColors.Length || _firstAlphas.Length != _secondAlphas.Length)
            {
                return false;
            }

            for (int _index = 0; _index < _firstColors.Length; _index++)
            {
                if (!_firstColors[_index].Equals(_secondColors[_index]))
                {
                    return false;
                }
            }

            for (int _index = 0; _index < _firstAlphas.Length; _index++)
            {
                if (!_firstAlphas[_index].Equals(_secondAlphas[_index]))
                {
                    return false;
                }
            }

            return true;
        }

        Gradient deepCopyGradient(Gradient _original)
        {
            Gradient _copy = new Gradient();
            if (_original != null)
            {
                _copy.SetKeys(_original.colorKeys, _original.alphaKeys);
                _copy.mode = _original.mode;
            }

            return _copy;
        }

        [field: SerializeField]
        public float NodeSeparation { get; private set; } = 1f;

        public void Set_NodeSeparation(float _nodeSeparation)
        {
            if (NodeSeparation == _nodeSeparation)
            {
                return;
            }

            NodeSeparation = _nodeSeparation;
            m_distance_NodeSeparation_SQR = NodeSeparation * NodeSeparation;
        }

        [field: SerializeField]
        public bool Use_Zoner_ScentAddon { get; private set; }

        public void Set_UseZonerScentAddon(bool _useZonerScentAddon)
        {
            if (Use_Zoner_ScentAddon == _useZonerScentAddon)
            {
                return;
            }

            Use_Zoner_ScentAddon = _useZonerScentAddon;
            if (!Use_Zoner_ScentAddon)
            {
                ScentZoneModificationValue = 0f;
                ScentZonner_BlockScentEmmision = false;
            }
            else
            {
                m_coorutineOgnoing_UpdateCooldown = false;
            }
        }

        [field: SerializeField]
        public bool Use_CustomRefreshRate { get; private set; }

        public void Set_UseCustomRefreshRate(bool _useCustomRefreshRate) => Use_CustomRefreshRate = _useCustomRefreshRate;
        [field: SerializeField]
        public float CooldownRefreshRate { get; private set; }

        public void Set_CooldownRefreshRate(float _rate)
        {
            if (CooldownRefreshRate != _rate)
            {
                CooldownRefreshRate = _rate;
                if (Application.isPlaying && m_wasEnabled)
                {
                    StopAllCoroutines();
                    StartCoroutine(update_cooldown());
                }
            }
        }

        [field: SerializeField]
        public bool Use_CustomDetectionRadius { get; private set; }

        public void Set_UseCustomDetectionRadius(bool _state) => Use_CustomDetectionRadius = _state;
        [field: SerializeField]
        public float Radius_ZonerScentAddon { get; private set; }

        float m_radiusOfCollider;
        public void Set_Radius_ZonerScentAddon(float _radius) => Radius_ZonerScentAddon = _radius;
        [field: SerializeField]
        public bool Use_DebugDrawRadius { get; private set; }

        public void Set_UseDebugDrawRadius(bool _state)
        {
            Use_DebugDrawRadius = _state;
        }

        [field: SerializeField]
        public int Limit_CollidersDetectionZoner { get; private set; }

        int m_previous_LimitCollidersDetectionZoner;
        public void Set_Limit_CollidersDetectionZoner(int _amount)
        {
            if (m_previous_LimitCollidersDetectionZoner != _amount)
            {
                Limit_CollidersDetectionZoner = _amount;
                m_hitColliders = new Collider[_amount];
                m_previous_LimitCollidersDetectionZoner = _amount;
            }
        }

        [field: SerializeField]
        public LayerMask DistractorDetectionLayer { get; private set; } = ~0;

        public void Set_DistractorDetectionLayer(LayerMask layer) => DistractorDetectionLayer = layer;
        [field: SerializeField]
        public int ScentLayer { get; private set; }

        public void Set_ScentLayer(int _layer) => ScentLayer = _layer;
        [field: SerializeField]
        public float VisualisationOffsetY { get; private set; }

        public void Set_VisualisationOffsetY(float _offset)
        {
            if (VisualisationOffsetY == _offset)
            {
                return;
            }

            VisualisationOffsetY = _offset;
            if (UseVisualisation && Application.isPlaying)
            {
                m_scentVisualisation.Set_OffsetY(_offset);
            }
        }

        [field: SerializeField]
        public bool ScentZonner_BlockScentEmmision { get; private set; }

        [field: SerializeField]
        public float ScentZoneModificationValue { get; private set; }
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;

        [field: SerializeField]
        bool m_helpPropertiesFoldout;
        public void Toggle_HelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;
        public bool ZonerScentAddonFoldout => m_zonerScentAddonFoldout;

        [field: SerializeField]
        bool m_zonerScentAddonFoldout;
        public void Toggle_ZonerScentAddonFoldout() => m_zonerScentAddonFoldout = !m_zonerScentAddonFoldout;
        // runtime
        float m_distance_NodeSeparation_SQR;
        Vector3 m_latest_ReleasedPosition;
        float m_current_DistanceSquared;
        ScentNode m_scentNode;
        ScentVisualisation m_scentVisualisation;
        bool m_wasEnabled;
        bool m_coorutineOgnoing_UpdateCooldown;
        IntervalEnumerator m_updateCooldown;
        Collider[] m_hitColliders;
        ColliderReference m_colliderReference;
        void enable()
        {
            if (Application.isPlaying)
            {
                if (ScenNodePoll.Instance == null)
                {
                    Debug.LogError("ScenNodePoll instance could not be created!", this);
                    return;
                }

                if (ScenVisualisatorsPoll.Instance == null)
                {
                    Debug.LogError("ScenVisualisatorsPoll instance could not be created!", this);
                    return;
                }

                if (UseVisualisation)
                {
                    createScentVisualisation();
                }

                if (Use_CustomRefreshRate)
                {
                    StartCoroutine(update_cooldown());
                }

                m_hitColliders = new Collider[Limit_CollidersDetectionZoner];
                m_previous_LimitCollidersDetectionZoner = Limit_CollidersDetectionZoner;
            }

            if (!TryGetComponent(out m_targetSenses))
            {
                Debug.LogError("Scent component requires TargetSenses component!", this);
                return;
            }

            if (m_targetSenses.TargetType == TargetType.Collider)
            {
                if (!TryGetComponent(out Collider _col))
                {
                    Debug.LogError("Cannot find Collider component!", this);
                    return;
                }

                m_radiusOfCollider = _col.bounds.extents.x;
            }
            else if (m_targetSenses.TargetType == TargetType.RigCollider)
            {
                if (!TryGetComponent(out RigCollider _rig))
                {
                    Debug.LogError("Cannot find RigCollider component!", this);
                    return;
                }

                Bounds _b = new Bounds(transform.position, Vector3.zero);
                foreach (Collider _collider in _rig.Colliders)
                {
                    if (_collider != null)
                    {
                        _b.Encapsulate(_collider.bounds);
                    }
                }

                m_radiusOfCollider = _b.extents.x;
            }

            m_wasEnabled = true;
        }

        void createScentVisualisation()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            m_scentVisualisation = ScenVisualisatorsPoll.Instance.GetNext_ScentNode();
            if (m_scentVisualisation == null)
            {
                Debug.LogError("ScenVisualisatorsPoll returned NULL ScentVisualisation!", this);
                return;
            }

            m_scentVisualisation.Setup(transform, ScentStrength, ScentColorGradient, VisualisationOffsetY);
        }

        void Start()
        {
            if (!m_wasEnabled)
            {
                enable();
            }

            m_distance_NodeSeparation_SQR = NodeSeparation * NodeSeparation;
            m_latest_ReleasedPosition = transform.position;
        }

        void Update()
        {
            if (!m_wasEnabled)
            {
                return;
            }

            m_current_DistanceSquared = (transform.position - m_latest_ReleasedPosition).sqrMagnitude;
            if (Use_Zoner_ScentAddon)
            {
                if (Use_CustomRefreshRate)
                {
                    if (!m_coorutineOgnoing_UpdateCooldown)
                    {
                        check_valueOfZoonerScentBuff();
                        m_coorutineOgnoing_UpdateCooldown = true;
                    }
                }
                else
                {
                    check_valueOfZoonerScentBuff();
                }
            }

            if (m_current_DistanceSquared > m_distance_NodeSeparation_SQR)
            {
                if (m_scentNode != null)
                {
                    m_scentNode.UpdateDirection(transform.position);
                }

                if (!ScentZonner_BlockScentEmmision)
                {
                    m_scentNode = ScenNodePoll.Instance.GetNext_ScentNode();
                    if (m_scentNode == null)
                    {
                        Debug.LogError("ScenNodePoll returned NULL ScentNode!", this);
                        return;
                    }

                    m_scentNode.gameObject.SetActive(true);
                    m_scentNode.Release(m_targetSenses, Use_Zoner_ScentAddon ? ScentStrength + ScentZoneModificationValue : ScentStrength, transform.forward, transform.position, ScentLayer);
                    m_latest_ReleasedPosition = transform.position;
                }
            }
            else
            {
                if (m_scentNode != null)
                {
                    m_scentNode.UpdateDirection(transform.position);
                }
            }
        }

        IEnumerator update_cooldown()
        {
            m_updateCooldown = new IntervalEnumerator(CooldownRefreshRate, miniUpdateAction);
            while (true)
            {
                m_updateCooldown.MoveNext(Time.deltaTime);
                yield return null;
            }
        }

        void miniUpdateAction()
        {
            m_coorutineOgnoing_UpdateCooldown = false;
        }

        void check_valueOfZoonerScentBuff()
        {
            ScentZoneModificationValue = 0f;
            ScentZonner_BlockScentEmmision = false;
            int _hitCount = Physics.OverlapSphereNonAlloc(transform.position, Use_CustomDetectionRadius ? Radius_ZonerScentAddon : m_radiusOfCollider, m_hitColliders, DistractorDetectionLayer);
            for (int _i = 0; _i < _hitCount; _i++)
            {
                m_colliderReference = GlobalColliderReferenceRegistry.Get(m_hitColliders[_i]);
                if (m_colliderReference.iS_ZonerScentDisctractor(out ScentZoneDistractor _distractor))
                {
                    if (_distractor.ScentModyfication == ScentModification.BlockScentEmmision)
                    {
                        ScentZonner_BlockScentEmmision = true;
                        return;
                    }

                    ScentZoneModificationValue += _distractor.ScentStrengthChange;
                }
            }

            ScentZoneModificationValue = Mathf.Clamp(ScentZoneModificationValue, -ScentStrength, ScentStrength);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Use_DebugDrawRadius)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Use_CustomDetectionRadius ? Radius_ZonerScentAddon : (Application.isPlaying ? m_radiusOfCollider : EditorGizmoHelpers.CalculateDetectionRadius(this, 1f)));
        }

#endif
        void OnEnable()
        {
            if (UseVisualisation && m_scentVisualisation != null)
            {
                m_scentVisualisation.SetEmmision(true);
            }
        }

        void OnDisable()
        {
            if (UseVisualisation && m_scentVisualisation != null)
            {
                m_scentVisualisation.SetEmmision(false);
            }

            m_hitColliders = null;
        }

        void OnDestroy()
        {
            if (UseVisualisation && m_scentVisualisation != null)
            {
                Destroy(m_scentVisualisation.gameObject);
            }
        }
    }
}
