using LastFrame.Player; // PlayerHealth
using UnityEngine; // GameObject

namespace LastFrame.Collectibles // Pickups of Last Frame
{
    /// <summary>
    /// Heart (issue #4): gives back one heart. Refused when the hearts are full.
    /// </summary>
    public class HeartPickup : Collectible // Health refill
    {
        [SerializeField, Min(1)] int m_hearts = 1; // GDD: gives back one heart
        [SerializeField] string m_fullText = "Corazones llenos"; // Prompt when it cannot be taken

        protected override string CannotCollectText => m_fullText; // "Corazones llenos"

        // Only when a heart is missing (and Ollie is not erased).
        protected override bool CanCollect(GameObject player)
        {
            var health = player.GetComponent<PlayerHealth>(); // Ollie's hearts
            return health && !health.IsDead && health.Hearts < health.MaxHearts; // A heart is missing
        }

        // Heals.
        protected override void OnCollect(GameObject player)
        {
            player.GetComponent<PlayerHealth>().Heal(m_hearts); // +1 heart (HUD updates)
        }
    }
}
