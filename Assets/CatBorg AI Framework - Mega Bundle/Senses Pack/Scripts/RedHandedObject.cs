using System.Collections.Generic;
using System;
using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ A: Main/ 71: RedHandedObject", 71)]

    public class RedHandedObject : MonoBehaviour, IRedHandedInterface
    {
        [field: SerializeField] public ObjectMode ObjectsMode = ObjectMode.None;
        public void Set_ZoneMode(ObjectMode _objectsMode) => ObjectsMode = _objectsMode;

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
            _modeIndex = (int)ObjectsMode;

            if (!enabled) return false;

            return SelectedFactionNameIndices.Contains((int)_initialFactionName);
        }

        public int Mode() => (int)ObjectsMode;

        // Instead of RequiredComponent
        void Awake()
        {
            if (!TryGetComponent(out ColliderReference _colliderReference))
                gameObject.AddComponent<ColliderReference>();
        }
    }
    public enum ObjectMode
    {
        None,                    // No faction change
        PermanentOnContact,         // Faction changes permanently when first making contact
        WhileContact,             // Faction changes only while in contact; reverts when contact ends
        CaughtWhileContact,       // Faction changes while in contact; becomes permanent if detected
        PermanentOnContactEnd           // Faction changes permanently when contact ends
    }

}

