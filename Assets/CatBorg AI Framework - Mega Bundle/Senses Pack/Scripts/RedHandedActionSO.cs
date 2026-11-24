using UnityEngine;
using System.Collections.Generic;
using System;

namespace Senses2026
{
    [CreateAssetMenu(menuName = "CatBorg Studio/Senses/RedHandedActionSO")]
    public class RedHandedActionSO : ScriptableObject
    {
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

        public FactionNames ResultFactionName;
        public bool ApplyIsGotCaught = false;
        public bool ApplyGlobaly = false;

        public bool RequiredFactionsContains(FactionNames _factionToCheck)
        {
            return SelectedFactionNameIndices.Contains((int)_factionToCheck);
        }
    }
}

