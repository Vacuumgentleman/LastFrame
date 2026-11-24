using System.Collections.Generic;
using System;
using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 72: RedHandedZone", 72)]
    public class RedHandedZone : MonoBehaviour, IRedHandedInterface
    {
        [field: SerializeField] public ZoneMode ZoneMode { get; private set; }
        public void Set_ZoneMode(ZoneMode _zoneMode) => ZoneMode = _zoneMode;

        [field: SerializeField] public List<int> SelectedFactionNameIndices { get; private set; } = new List<int>();
        public void Set_SelectedFactionNames(List<int> _selectedFactionNameIndices)
        {
            SelectedFactionNameIndices = _selectedFactionNameIndices;

            FactionNames[] _factionNamesArray = new FactionNames[SelectedFactionNameIndices.Count];

            // Use the selected faction name index
            for (int i = 0; i < SelectedFactionNameIndices.Count; i++)
            {
                var _factionNameValue = Enum.GetValues(typeof(FactionNames)).GetValue(SelectedFactionNameIndices[i]);
                _factionNamesArray.SetValue(_factionNameValue, i);
            }

            m_requiredFactions = _factionNamesArray;
        }
        [field: SerializeField] FactionNames[] m_requiredFactions;

        [field: SerializeField] public FactionNames ResultFactionName { get; private set; }
        public void Set_ResultFactionName(FactionNames _resultFactionName) => ResultFactionName = _resultFactionName;

        //
        public bool HelpPropertiesFoldout => m_helpPropertiesFoldout;
        [field: SerializeField] bool m_helpPropertiesFoldout;
        public void Toggle_HelpPropertiesFoldout() => m_helpPropertiesFoldout = !m_helpPropertiesFoldout;
        //

        public bool FactionNameWasChanged(FactionNames _initialFactionName, out FactionNames _resultFactionName, out int _modeIndex)
        {
            _resultFactionName = ResultFactionName;
            _modeIndex = (int)ZoneMode;

            if (!enabled) return false;

            return SelectedFactionNameIndices.Contains((int)_initialFactionName);
        }

        public int Mode() => (int)ZoneMode;

        // Instead of RequiredComponent
        void Awake()
        {
            if (!TryGetComponent(out ColliderReference _colliderReference))
                gameObject.AddComponent<ColliderReference>();
        }
    }
    public enum ZoneMode
    {
        None,                   // No faction change
        PermanentOnEnter,        // Faction changes permanently when first entering the zone
        WhileInside,             // Faction changes only while inside; reverts on exit
        CaughtInZone,            // Faction changes while inside; becomes permanent if detected
        PermanentOnExit          // Faction changes permanently when leaving the zone
    }
}

