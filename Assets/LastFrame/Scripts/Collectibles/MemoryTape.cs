using System; // Action delegate for events
using UnityEngine; // GameObject, RuntimeInitializeOnLoadMethod
using UnityEngine.SceneManagement; // SceneManager.sceneLoaded

namespace LastFrame.Collectibles // Pickups of Last Frame
{
    /// <summary>
    /// The set's memory tape (issues #4 and #10). It is required: without it the set exit door does not open.
    /// Holding the tape lasts for the current set; loading a scene clears it.
    /// The memory clip it plays is not part of this script yet (cutscenes, #20).
    /// </summary>
    public class MemoryTape : Collectible // Required pickup of each set
    {
        [Tooltip("Adventure of Ollie's own show stored in this tape (e.g. Vaquera, Detective, Exploradora).")]
        [SerializeField] string m_memoryName = "Vaquera"; // Memory shown in the debug label and the results

        /// <summary>True once Ollie picked up the tape of the current set.</summary>
        public static bool HasTapeOfThisSet { get; private set; } // Read by the exit door (#10)
        /// <summary>Name of the memory picked up in this set (empty if none).</summary>
        public static string CurrentMemory { get; private set; } = ""; // Debug label / results
        /// <summary>The tape of this set was picked up.</summary>
        public static event Action<MemoryTape> OnTapeCollected; // Door, HUD, sound

        public string MemoryName => m_memoryName; // Read-only name

        // Stores the tape for this set.
        protected override void OnCollect(GameObject player)
        {
            HasTapeOfThisSet = true; // The door can open now
            CurrentMemory = m_memoryName; // Remember which memory
            OnTapeCollected?.Invoke(this); // Listeners
        }

        // Clears the tape when any scene loads (new set, retry or menu).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void HookSceneLoad()
        {
            ResetTape(); // Also covers "Enter Play Mode" without domain reload
            SceneManager.sceneLoaded -= OnSceneLoaded; // Avoid double hooks
            SceneManager.sceneLoaded += OnSceneLoaded; // Reset on every load
        }

        // Scene loaded: the new set starts without its tape.
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single) ResetTape(); // Additive loads keep it
        }

        // Clears the static state.
        static void ResetTape()
        {
            HasTapeOfThisSet = false; // No tape
            CurrentMemory = ""; // No memory
        }
    }
}
