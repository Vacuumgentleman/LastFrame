using LastFrame.Player; // InkAmmo
using UnityEngine; // GameObject

namespace LastFrame.Collectibles // Pickups of Last Frame
{
    /// <summary>
    /// Inkwell (issue #4): refills the pen's ink. Refused when the ink is already full.
    /// </summary>
    public class InkPickup : Collectible // Ink refill
    {
        [Tooltip("On = fills the ink to the max. Off = adds Amount.")]
        [SerializeField] bool m_fillToMax = true; // GDD: the inkwell refills the pen
        [SerializeField, Min(1)] int m_amount = 1; // Drops added when Fill To Max is off
        [SerializeField] string m_fullText = "Tinta llena"; // Prompt when it cannot be taken

        protected override string CannotCollectText => m_fullText; // "Tinta llena"

        // Only when there is room for more ink.
        protected override bool CanCollect(GameObject player)
        {
            var ink = player.GetComponent<InkAmmo>(); // Ollie's ink
            return ink && ink.Current < ink.Max; // Room left
        }

        // Adds the ink.
        protected override void OnCollect(GameObject player)
        {
            var ink = player.GetComponent<InkAmmo>(); // Ollie's ink
            ink.Add(m_fillToMax ? ink.Max : m_amount); // Refill (Add clamps to the max)
        }
    }
}
