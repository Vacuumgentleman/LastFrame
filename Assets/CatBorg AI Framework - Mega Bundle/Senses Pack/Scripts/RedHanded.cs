using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 70: RedHanded", 70)]
    [RequireComponent(typeof(TargetSenses))]
    public class RedHanded : MonoBehaviour, IUpdate
    {
        FactionNames m_initialFaction;
        FactionNames m_lastSetFaction;
        FactionNames m_whileInZonePointOfReturn;
        Collider[] m_hitColliders = new Collider[1];
        TargetSenses m_targetSenses;
        [field: SerializeField]
        public int Limit_CollidersDetectionZoner { get; private set; } = 10;

        int m_previous_LimitCollidersDetectionZoner;
        public void Set_Limit_CollidersDetectionZoner(int _value)
        {
            if (m_previous_LimitCollidersDetectionZoner != _value)
            {
                Limit_CollidersDetectionZoner = _value;
                m_hitColliders = new Collider[_value];
                m_previous_LimitCollidersDetectionZoner = _value;
            }
        }

        public float m_radiusOfCollider { get; private set; }

        [field: SerializeField]
        public bool Use_CustomDetectionRadius { get; private set; }

        public void Set_UseCustomDetectionRadius(bool _state) => Use_CustomDetectionRadius = _state;
        [field: SerializeField]
        public float Radius_RedHanded { get; private set; }

        public void Set_RadiusRedHanded(float _value) => Radius_RedHanded = _value;
        [field: SerializeField]
        public LayerMask RedHandedDetectionLayer { get; private set; } = ~0;

        public void Set_RedHandedDetectionLayer(LayerMask _value) => RedHandedDetectionLayer = _value;
        [field: SerializeField]
        public bool Use_DebugDrawRadius { get; private set; }

        public void Set_UseDebugDrawRadius(bool _state)
        {
            Use_DebugDrawRadius = _state;
        }

        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;

        [field: SerializeField]
        bool m_helpPropertiesFoldout;
        public void Toggle_HelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;
        IRedHandedInterface[] m_redHandedInterfaces;
        bool[] m_interfaceDetectedDuringUpdate;
        Dictionary<IRedHandedInterface, int> m_indexOfInterface = new Dictionary<IRedHandedInterface, int>();
        int m_maxInterfaceSlots = 10;
        int m_usedInterfaceSlots = 0;
        ColliderReference m_colliderReference;
        bool m_wasEnabled;
        public bool NeedsUpdate() => false;
        public void PerformUpdate(float _delta)
        {
            if (!m_wasEnabled)
            {
                return;
            }

            detect_RedHandedElements();
        }

        void enable()
        {
            if (!TryGetComponent(out m_targetSenses))
            {
                Debug.LogError("[RedHanded] Missing TargetSenses.", this);
                m_wasEnabled = false;
                return;
            }

            m_initialFaction = m_targetSenses.FactionName;
            m_lastSetFaction = m_initialFaction;
            bool _validCollider = false;
            if (m_targetSenses.TargetType == TargetType.Collider)
            {
                if (TryGetComponent(out CharacterController _cc))
                {
                    m_radiusOfCollider = _cc.radius * 1.1f;
                    _validCollider = true;
                }
                else if (TryGetComponent(out Collider _col))
                {
                    m_radiusOfCollider = _col.bounds.extents.x * 1.1f;
                    _validCollider = true;
                }
            }
            else if (m_targetSenses.TargetType == TargetType.RigCollider)
            {
                if (TryGetComponent(out RigCollider _rig))
                {
                    Bounds _b = new Bounds(transform.position, Vector3.zero);
                    foreach (var _c in _rig.Colliders)
                    {
                        if (_c != null)
                        {
                            _b.Encapsulate(_c.bounds);
                        }
                    }

                    m_radiusOfCollider = _b.extents.x;
                    _validCollider = true;
                }
            }

            if (!_validCollider)
            {
                Debug.LogError("[RedHanded] No valid collider found.", this);
                m_wasEnabled = false;
                return;
            }

            m_hitColliders = new Collider[Limit_CollidersDetectionZoner];
            m_previous_LimitCollidersDetectionZoner = Limit_CollidersDetectionZoner;
            m_redHandedInterfaces = new IRedHandedInterface[m_maxInterfaceSlots];
            m_interfaceDetectedDuringUpdate = new bool[m_maxInterfaceSlots];
            m_wasEnabled = true;
        }

        void detect_RedHandedElements()
        {
            resetDetectionStatus();
            float _radius = Use_CustomDetectionRadius ? Radius_RedHanded : m_radiusOfCollider;
            int _hitCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, m_hitColliders, RedHandedDetectionLayer);
            for (int _i = 0; _i < _hitCount; _i++)
            {
                m_colliderReference = GlobalColliderReferenceRegistry.Get(m_hitColliders[_i]);
                if (m_colliderReference.isRedHandedEntity(out IRedHandedInterface _entity))
                {
                    if (_entity != null)
                    {
                        markInterfaceDetected(_entity);
                    }
                }
            }

            bool _restart;
            if (m_usedInterfaceSlots > 0)
            {
                do
                {
                    _restart = false;
                    for (int _i = 0; _i < m_usedInterfaceSlots; _i++)
                    {
                        var _cur = m_redHandedInterfaces[_i];
                        if (_cur == null || _cur.Equals(null))
                        {
                            removeInterface(_i);
                            _restart = true;
                            break;
                        }

                        if (!m_interfaceDetectedDuringUpdate[_i])
                        {
                            applyExitLogic(_cur);
                            removeInterface(_i);
                            _restart = true;
                            break;
                        }
                    }
                }
                while (_restart);
            }

            for (int _i = 0; _i < m_usedInterfaceSlots; _i++)
            {
                var _cur = m_redHandedInterfaces[_i];
                if (_cur == null || _cur.Equals(null))
                {
                    continue;
                }

                handleInterface(_cur);
            }
        }

        void removeInterface(int _index)
        {
            if (_index < 0 || _index >= m_usedInterfaceSlots)
            {
                return;
            }

            var _toRemove = m_redHandedInterfaces[_index];
            if (_toRemove != null)
            {
                m_indexOfInterface.Remove(_toRemove);
            }

            if (_index == m_usedInterfaceSlots - 1)
            {
                m_redHandedInterfaces[_index] = null;
            }
            else
            {
                var _move = m_redHandedInterfaces[m_usedInterfaceSlots - 1];
                m_redHandedInterfaces[_index] = _move;
                m_redHandedInterfaces[m_usedInterfaceSlots - 1] = null;
                if (_move != null)
                {
                    m_indexOfInterface[_move] = _index;
                }

                m_interfaceDetectedDuringUpdate[_index] = m_interfaceDetectedDuringUpdate[m_usedInterfaceSlots - 1];
            }

            m_usedInterfaceSlots--;
        }

        void markInterfaceDetected(IRedHandedInterface _entity)
        {
            if (_entity == null || _entity.Equals(null))
            {
                return;
            }

            if (!m_indexOfInterface.TryGetValue(_entity, out int _idx))
            {
                if (m_usedInterfaceSlots >= m_maxInterfaceSlots)
                {
                    Debug.LogWarning($"[RedHanded] Max limit ({m_maxInterfaceSlots}) reached.", this);
                    return;
                }

                m_redHandedInterfaces[m_usedInterfaceSlots] = _entity;
                _idx = m_usedInterfaceSlots;
                m_indexOfInterface[_entity] = _idx;
                m_usedInterfaceSlots++;
            }

            m_interfaceDetectedDuringUpdate[_idx] = true;
        }

        void handleInterface(IRedHandedInterface _entity)
        {
            if (_entity == null || _entity.Equals(null))
            {
                return;
            }

            if (_entity.FactionNameWasChanged(m_targetSenses.FactionName, out FactionNames _result, out int _mode))
            {
                if (_mode == 1)
                {
                    if (m_lastSetFaction != _result)
                    {
                        m_lastSetFaction = _result;
                        m_initialFaction = _result;
                        m_targetSenses.Set_FactionName(_result);
                    }
                }
                else if (_mode == 2)
                {
                    if (m_whileInZonePointOfReturn != _result)
                    {
                        m_whileInZonePointOfReturn = m_targetSenses.FactionName;
                        m_targetSenses.Set_FactionName(_result);
                    }
                }
                else if (_mode == 3)
                {
                    if (m_lastSetFaction != _result)
                    {
                        m_lastSetFaction = _result;
                        m_targetSenses.ResetWasDetected();
                    }
                    else
                    {
                        if (m_targetSenses.WasDetected)
                        {
                            m_initialFaction = _result;
                            m_targetSenses.Set_FactionName(_result);
                        }
                    }
                }
            }
        }

        void applyExitLogic(IRedHandedInterface _entity)
        {
            if (_entity.Mode() == 4 && _entity.FactionNameWasChanged(m_targetSenses.FactionName, out FactionNames _res, out _))
            {
                m_initialFaction = _res;
                m_lastSetFaction = _res;
                m_targetSenses.Set_FactionName(_res);
            }

            if (_entity.Mode() == 2 && _entity.FactionNameWasChanged(m_whileInZonePointOfReturn, out FactionNames _res2, out _))
            {
                m_initialFaction = m_whileInZonePointOfReturn;
                m_targetSenses.Set_FactionName(m_whileInZonePointOfReturn);
                m_whileInZonePointOfReturn = FactionNames.None;
            }
        }

        void resetDetectionStatus()
        {
            for (int _i = 0; _i < m_usedInterfaceSlots; _i++)
            {
                m_interfaceDetectedDuringUpdate[_i] = false;
            }
        }

        void OnEnable()
        {
            enable();
            if (m_wasEnabled)
            {
                Updater.Instance.RegisterUpdate(this);
            }
        }

        void OnDestroy()
        {
            if (Updater.Instance != null)
            {
                Updater.Instance.UnegisterUpdate(this);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Use_DebugDrawRadius)
            {
                return;
            }

            float _r = Use_CustomDetectionRadius ? Radius_RedHanded : (Application.isPlaying ? m_radiusOfCollider : EditorGizmoHelpers.CalculateDetectionRadius(this, 1.1f));
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _r);
            TargetSenses _target = m_targetSenses;
            if (_target == null)
            {
                TryGetComponent(out _target);
            }
            if (_target != null)
            {
                Handles.color = Color.white;
                Vector3 _position = transform.position + Vector3.up * 3f;
                EditorGizmoHelpers.DrawFactionLabel(_position, _target.FactionName);
            }
        }

#endif
        public IEnumerator ConductInstantAction(RedHandedActionSO _so, Action<bool> _onCompleted)
        {
            if (!m_wasEnabled)
            {
                enable();
            }

            bool _success = false;
            if (_so.RequiredFactionsContains(m_targetSenses.FactionName))
            {
                m_targetSenses.Set_FactionName(_so.ResultFactionName);
                m_lastSetFaction = _so.ResultFactionName;
                _success = true;
            }

            _onCompleted?.Invoke(_success);
            yield return null;
        }

        internal void StartLastingAction()
        {
            if (!enabled)
            {
                return;
            }

            if (!m_wasEnabled)
            {
                enable();
            }

            m_targetSenses.ResetWasDetected();
        }

        internal void FinishLastingAction(RedHandedActionSO _so)
        {
            if (!enabled)
            {
                return;
            }

            if (!_so.ApplyGlobaly)
            {
                if (_so.RequiredFactionsContains(m_targetSenses.FactionName))
                {
                    if (_so.ApplyIsGotCaught)
                    {
                        if (m_targetSenses.WasDetected)
                        {
                            m_targetSenses.Set_FactionName(_so.ResultFactionName);
                            m_lastSetFaction = _so.ResultFactionName;
                        }
                    }
                    else
                    {
                        m_targetSenses.Set_FactionName(_so.ResultFactionName);
                        m_lastSetFaction = _so.ResultFactionName;
                    }
                }
            }
            else
            {
                if (_so.RequiredFactionsContains(m_targetSenses.FactionName))
                {
                    if (_so.ApplyIsGotCaught)
                    {
                        if (m_targetSenses.WasDetected)
                        {
                            m_targetSenses.Set_FactionName(_so.ResultFactionName);
                            m_lastSetFaction = _so.ResultFactionName;
                            applyGlobal(_so);
                        }
                    }
                    else
                    {
                        m_targetSenses.Set_FactionName(_so.ResultFactionName);
                        m_lastSetFaction = _so.ResultFactionName;
                        applyGlobal(_so);
                    }
                }
            }
        }

        internal void FinishInstantAction(RedHandedActionSO _so)
        {
            if (!enabled)
            {
                return;
            }

            StartCoroutine(FinishInstantActionCoorutine(_so));
        }

        internal IEnumerator FinishInstantActionCoorutine(RedHandedActionSO _so)
        {
            if (!_so.ApplyGlobaly)
            {
                if (_so.RequiredFactionsContains(m_targetSenses.FactionName))
                {
                    if (_so.ApplyIsGotCaught)
                    {
                        m_targetSenses.ResetWasDetected();
                        yield return new WaitForSecondsRealtime(.1f);
                        if (m_targetSenses.WasDetected)
                        {
                            m_targetSenses.Set_FactionName(_so.ResultFactionName);
                            m_lastSetFaction = _so.ResultFactionName;
                        }
                    }
                    else
                    {
                        m_targetSenses.Set_FactionName(_so.ResultFactionName);
                        m_lastSetFaction = _so.ResultFactionName;
                    }
                }
            }
            else
            {
                if (_so.RequiredFactionsContains(m_targetSenses.FactionName))
                {
                    if (_so.ApplyIsGotCaught)
                    {
                        m_targetSenses.ResetWasDetected();
                        yield return new WaitForSecondsRealtime(.1f);
                        if (m_targetSenses.WasDetected)
                        {
                            m_targetSenses.Set_FactionName(_so.ResultFactionName);
                            m_lastSetFaction = _so.ResultFactionName;
                            applyGlobal(_so);
                        }
                    }
                    else
                    {
                        m_targetSenses.Set_FactionName(_so.ResultFactionName);
                        m_lastSetFaction = _so.ResultFactionName;
                        applyGlobal(_so);
                    }
                }
            }

            yield return null;
        }

        void applyGlobal(RedHandedActionSO _so)
        {
            TargetSenses[] _all;
#if UNITY_6000_5_OR_NEWER

            _all = FindObjectsByType<TargetSenses>();

#elif UNITY_2022_2_OR_NEWER
            _all = FindObjectsByType<TargetSenses>(FindObjectsSortMode.None);
#else

    _all = FindObjectsOfType<TargetSenses>()
#endif
            FactionNames _result = _so.ResultFactionName;
            for (int _i = 0; _i < _all.Length; _i++)
            {
                if (_all[_i].enabled)
                {
                    if (_so.RequiredFactionsContains(_all[_i].FactionName))
                    {
                        _all[_i].Set_FactionName(_result);
                    }
                }
            }
        }
    }
}
