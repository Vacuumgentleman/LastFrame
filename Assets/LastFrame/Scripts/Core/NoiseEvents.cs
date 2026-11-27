using System; // Action delegate for events
using UnityEngine; // Vector3

namespace LastFrame.Core // Shared scripts of Last Frame
{
    public enum NoiseSource { Footsteps, InkImpact } // What made the noise (for the enemy's reaction and debug)

    /// <summary>
    /// Noises heard by enemies at an exact point and radius (Ollie running, ink drop impact).
    /// Senses hearing reported noises far outside its range and with Ollie's live position, so enemies listen here instead.
    /// </summary>
    public static class NoiseEvents // Point noise bus (Observer pattern)
    {
        public static event Action<Vector3, float, NoiseSource> OnPointNoise; // (position, radius in meters, source)

        // Announces a noise at a point; enemies closer than the radius go to investigate that point.
        public static void Emit(Vector3 position, float radius, NoiseSource source)
        {
            OnPointNoise?.Invoke(position, radius, source); // Notify every listening enemy
        }
    }
}
