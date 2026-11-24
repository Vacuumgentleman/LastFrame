using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 50: Stealth", 50)]
    [RequireComponent(typeof(TargetSenses))]
    public class Stealth : MonoBehaviour, IUpdate
    {
        [field: SerializeField]
        public float StealthBuff { get; private set; }

        public void SetStealthBuff(float _value) => StealthBuff = _value;
        [field: SerializeField]
        public float OffsetAxisY { get; private set; }

        public void SetOffsetAxisY(float _value) => OffsetAxisY = _value;
        public Vector3 PositionLumenMeasurement => new Vector3(transform.position.x, OffsetAxisY, transform.position.z);

        float m_totalStealthBuff;
        public float ProvideTotalStealthBuff()
        {
            if (UseZonerStealthAddon && UseZonerGodMode && ZonerStealthBuff == 1f)
            {
                return 1f;
            }

            if (UseLumenStealthAddon && UseLumenGodMode && LumenStealthBuff == 1f)
            {
                return 1f;
            }

            float _weightLumen = UseLumenStealthAddon ? 1f : 0f;
            float _weightZoner = UseZonerStealthAddon ? 1f : 0f;
            float _weightBase = 1f;
            float _valueBase = enabled ? StealthBuff : 0f;
            float _total = (_weightLumen * LumenStealthBuff + _weightZoner * ZonerStealthBuff + _weightBase * _valueBase) / (_weightLumen + _weightZoner + _weightBase);
            return _total;
        }

        public float ProvideLumenStealthAddon(bool _returnZeroIfDisabled = false) => (_returnZeroIfDisabled && !enabled) ? 0f : LumenStealthBuff;
        public float ProvideZonerStealthAddon(bool _returnZeroIfDisabled = false) => (_returnZeroIfDisabled && !enabled) ? 0f : ZonerStealthBuff;
        [field: SerializeField]
        public bool UseLumenStealthAddon { get; private set; }

        public void SetUseLumenStealthAddon(bool _state)
        {
            if (UseLumenStealthAddon == _state)
            {
                return;
            }

            UseLumenStealthAddon = _state;
            if (UseLumenStealthAddon)
            {
                autoSetLumenOffsetIfNeeded();
                if (Application.isPlaying)
                {
                    LumenHandler.Instance.Register(this);
                    if (!LumenHandler.Instance.WasEnabled)
                    {
                        LightDetector _detector;
#if UNITY_6000_0_OR_NEWER
                        _detector = FindAnyObjectByType<LightDetector>();
#elif UNITY_2022_2_OR_NEWER

                _detector = FindFirstObjectByType<LightDetector>();

#else

                _detector = FindObjectOfType<LightDetector>();

#endif
                        LumenHandler.Instance.Enable(_detector ? _detector : addLightDetectorPrefab());
                    }
                }
            }
            else
            {
                LumenHandler.Instance.Remove(this);
                m_lumenStealthBuff = 0f;
            }
        }

        [field: SerializeField]
        public bool UseLumenGodMode { get; private set; }

        public void SetLumenGodMode(bool _state) => UseLumenGodMode = _state;
        public float LumenStealthBuff => m_lumenStealthBuff;

        [SerializeField]
        float m_lumenStealthBuff;
        internal void SetLumenStealthValue(float _value) => m_lumenStealthBuff = _value;
        [field: SerializeField]
        public bool UseZonerGodMode { get; private set; }

        public void SetZonerGodMode(bool _state) => UseZonerGodMode = _state;
        [field: SerializeField]
        public bool UseZonerStealthAddon { get; private set; }

        public void SetUseZonerStealthAddon(bool _state)
        {
            if (UseZonerStealthAddon == _state)
            {
                return;
            }

            UseZonerStealthAddon = _state;
            if (m_wasEnabled)
            {
                ZonerStealthBuff = 0f;
                System.Array.Clear(m_hitColliders, 0, m_hitColliders.Length);
            }
        }

        Collider[] m_hitColliders = new Collider[1];
        [field: SerializeField]
        public float ZonerStealthBuff { get; private set; }

        [field: SerializeField]
        public LayerMask ZonerDetectionLayer { get; private set; } = ~0;

        public void SetZonerDetectionLayer(LayerMask _mask) => ZonerDetectionLayer = _mask;
        [field: SerializeField]
        public bool UseCustomDetectionRadius { get; private set; }

        public void SetUseCustomDetectionRadius(bool _state) => UseCustomDetectionRadius = _state;
        [field: SerializeField]
        public float RadiusZonerStealthAddon { get; private set; }

        public void SetRadiusZonerStealthAddon(float _value) => RadiusZonerStealthAddon = _value;
        [field: SerializeField]
        public bool UseDebugDrawRadius { get; private set; }

        public void SetUseDebugDrawRadius(bool _state)
        {
            UseDebugDrawRadius = _state;
        }

        [field: SerializeField]
        public int LimitCollidersDetectionZoner { get; private set; } = 10;

        int m_previousLimitColliders;
        public void SetLimitCollidersDetectionZoner(int _limit)
        {
            if (m_previousLimitColliders == _limit)
            {
                return;
            }

            LimitCollidersDetectionZoner = _limit;
            m_hitColliders = new Collider[_limit];
            m_previousLimitColliders = _limit;
        }

        float m_radiusOfCollider;
        [field: SerializeField]
        public bool LookForBuffZonesPropertiesFoldout { get; private set; }

        public void SetLookForBuffZonesPropertiesFoldout(bool _state) => LookForBuffZonesPropertiesFoldout = _state;
        [field: SerializeField]
        public bool LumenPropertiesFoldout { get; private set; }

        public void SetLumenPropertiesFoldout(bool _state) => LumenPropertiesFoldout = _state;
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;

        [field: SerializeField]
        bool m_helpPropertiesFoldout;
        public void ToggleHelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;
        ColliderReference m_colliderReference;
        bool m_wasEnabled;
        public void PerformUpdate(float _delta)
        {
            if (!m_wasEnabled)
            {
                return;
            }

            if (!UseZonerStealthAddon)
            {
                return;
            }

            updateZonerStealthValue();
        }

        void Start()
        {
            enable();
        }

        void enable()
        {
            m_hitColliders = new Collider[LimitCollidersDetectionZoner];
            m_previousLimitColliders = LimitCollidersDetectionZoner;
            if (TryGetComponent<TargetSenses>(out TargetSenses _senses))
            {
                if (_senses.TargetType == TargetType.Collider)
                {
                    if (TryGetComponent<CharacterController>(out CharacterController _cc))
                    {
                        m_radiusOfCollider = _cc.radius * 1.1f;
                    }
                    else if (TryGetComponent<Collider>(out Collider _col))
                    {
                        m_radiusOfCollider = _col.bounds.extents.x * 1.1f;
                    }
                    else
                    {
                        Debug.LogError("Stealth: No Collider found on object.", this);
                        return;
                    }
                }
                else if (_senses.TargetType == TargetType.RigCollider)
                {
                    if (TryGetComponent<RigCollider>(out RigCollider _rig))
                    {
                        Bounds _bounds = new Bounds(transform.position, Vector3.zero);
                        foreach (Collider _col in _rig.Colliders)
                        {
                            if (_col != null)
                            {
                                _bounds.Encapsulate(_col.bounds);
                            }
                        }

                        m_radiusOfCollider = _bounds.extents.x;
                    }
                    else
                    {
                        Debug.LogError("Stealth: RigCollider not found.", this);
                        return;
                    }
                }
            }
            else
            {
                Debug.LogError("Stealth requires TargetSenses component.", this);
                return;
            }

            m_wasEnabled = true;
        }

        void updateZonerStealthValue()
        {
            float _total = 0f;
            float _radius = UseCustomDetectionRadius ? RadiusZonerStealthAddon : m_radiusOfCollider;
            int _hitCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, m_hitColliders, ZonerDetectionLayer);
            for (int _i = 0; _i < _hitCount; _i++)
            {
                m_colliderReference = GlobalColliderReferenceRegistry.Get(m_hitColliders[_i]);
                if (m_colliderReference.isStealthZone(out StealthZone _zone))
                {
                    _total += _zone.StealthBuff;
                }
            }

            ZonerStealthBuff = Mathf.Clamp01(_total);
        }

        void autoSetLumenOffsetIfNeeded()
        {
            if (OffsetAxisY != 0f)
            {
                return;
            }

            SkinnedMeshRenderer _skin = GetComponentInChildren<SkinnedMeshRenderer>();
            MeshRenderer _mesh = _skin ? null : GetComponentInChildren<MeshRenderer>();
            Renderer _renderer = _skin ? _skin : _mesh;
            if (_renderer != null)
            {
                OffsetAxisY = _renderer.bounds.max.y + 0.2f;
            }
        }

        void OnEnable()
        {
            if (UseLumenStealthAddon)
            {
                autoSetLumenOffsetIfNeeded();
                LumenHandler.Instance.Register(this);
                if (!LumenHandler.Instance.WasEnabled)
                {
                    LumenHandler.Instance.Enable(addLightDetectorPrefab());
                }
            }

            if (UseZonerStealthAddon)
            {
                Updater.Instance.RegisterUpdate(this);
            }
        }

        void OnDestroy()
        {
            if (UseLumenStealthAddon)
            {
                LumenHandler.Instance.Remove(this);
            }

            if (Updater.Instance && UseZonerStealthAddon)
            {
                Updater.Instance.UnegisterUpdate(this);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!UseDebugDrawRadius)
            {
                return;
            }

            float _radius = UseCustomDetectionRadius ? RadiusZonerStealthAddon : (Application.isPlaying ? m_radiusOfCollider : EditorGizmoHelpers.CalculateDetectionRadius(this, 1.1f));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }

#endif
        LightDetector addLightDetectorPrefab()
        {
            GameObject _prefab = Resources.Load<GameObject>("- LightDetector (part of Senses Asset)");
            if (_prefab == null)
            {
                Debug.LogError("LightDetector prefab not found in Resources!");
                return null;
            }

            GameObject _inst = Instantiate(_prefab);
            _inst.name = "- LightDetector (part of Senses Asset)";
            _inst.transform.position = Vector3.one * 5000f;
            return _inst.GetComponent<LightDetector>();
        }

        public bool NeedsUpdate() => false;
    }
}
