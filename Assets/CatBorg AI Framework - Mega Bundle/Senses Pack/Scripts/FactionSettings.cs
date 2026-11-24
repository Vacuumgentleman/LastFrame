using UnityEngine;

namespace Senses2026
{
    [CreateAssetMenu(menuName = "CatBorg Studio/Senses/FactionSettings")]
    public class FactionSettings : ScriptableObject
    {
        [Tooltip("Display names for all factions (index 0 is always 'None').")]
        public string[] factionDisplayNames = new string[11]
        {
        "None", "Faction 1", "Faction 2", "Faction 3", "Faction 4",
        "Faction 5", "Faction 6", "Faction 7", "Faction 8", "Faction 9", "Faction 10"
        };

        public string GetFactionDisplayName(FactionNames faction)
        {
            int index = (int)faction;
            if (index >= 0 && index < factionDisplayNames.Length)
            {
                // Hard-lock index 0 to "None"
                return index == 0 ? "None" : factionDisplayNames[index];
            }

            return faction.ToString();
        }

        public string GetFactionDisplayName(int factionIndex)
        {
            if (factionIndex >= 0 && factionIndex < factionDisplayNames.Length)
            {
                // Always enforce "None" for index 0
                if (factionIndex == 0)
                    return "None";

                return string.IsNullOrEmpty(factionDisplayNames[factionIndex])
                    ? $"Faction {factionIndex}"
                    : factionDisplayNames[factionIndex];
            }

            return $"Faction {factionIndex}";
        }
    }
}


