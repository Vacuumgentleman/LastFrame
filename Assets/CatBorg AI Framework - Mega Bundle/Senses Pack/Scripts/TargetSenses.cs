using UnityEngine;
using System;
using System.Collections.Generic;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 20: Target Senses", 20)]
    public class TargetSenses : MonoBehaviour
    {
        [SerializeField] TargetType m_targetType;
        public TargetType TargetType => m_targetType;

        public void Set_TargetType(TargetType _enumValueIndex)
        {
            m_targetType = (TargetType)_enumValueIndex;
        }


        bool m_wasEnabled = false;
        public int AmountOfColliders()
        {
            if (!m_wasEnabled) enable();
            return m_amountOfColiders;
        }
        public RigCollider Provide_RigCollider()
        {
            if (!m_wasEnabled) enable();
            if (m_targetType != TargetType.RigCollider)
            {
                return null;
            }
            return m_rigCollider;
        }
        public Collider[] Provide_RigCollider_Colliders()
        {
            if (!m_wasEnabled) enable();
            if (m_targetType != TargetType.RigCollider)
            {
                return null;
            }
            return m_rigCollider.Colliders;
        }

        RigCollider m_rigCollider;
        int m_amountOfColiders;
        void enable()
        {
            m_rigCollider = GetComponent<RigCollider>();
            m_amountOfColiders = m_rigCollider.Colliders.Length;
            m_wasEnabled = true;
        }

        [SerializeField] FactionNames m_factionName;
        public FactionNames FactionName => m_factionName;
        public void Set_FactionName(FactionNames _newFactionName)
        {
            if (m_factionName == _newFactionName) return;
            m_factionName = _newFactionName;
        }


        bool m_wasEnabled_CachedStealthComponent = false;
        bool m_had_CachedStealthComponent = false;
        Stealth m_stealth;
        public bool Cached_StealthComponent(out Stealth _stealth)
        {
            if (!m_wasEnabled_CachedStealthComponent)
            {
                m_had_CachedStealthComponent = TryGetComponent<Stealth>(out m_stealth);
                m_wasEnabled_CachedStealthComponent = true;
            }
            _stealth = m_stealth;
            return m_had_CachedStealthComponent;
        }


        readonly Dictionary<Type, object> m_cachedComponents = new Dictionary<Type, object>();

        public bool HadCachedComponent<T>(Type _componentType, out T component)
        {
            if (m_cachedComponents.TryGetValue(_componentType, out var obj))
            {
                component = (T)obj;
                return obj != null;
            }

            if (TryGetComponent<T>(out component))
            {
                m_cachedComponents.TryAdd(_componentType, component);
                return true;
            }
            else
            {
                m_cachedComponents.TryAdd(_componentType, null);
                return false;
            }
        }

        //Feedback for RedHand System
        bool m_wasDetected = false;
        public bool WasDetected => m_wasDetected;
        internal void SetWasDetected() => m_wasDetected = true;
        internal void ResetWasDetected() => m_wasDetected = false;

        internal void Set_FactionName(object resultFactionName)
        {
            throw new NotImplementedException();
        }

        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;
        [field: SerializeField] bool m_helpPropertiesFoldout;
        public void ToggleHelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;
    }
}

