using UnityEngine; // Mathf

namespace LastFrame.Core // Shared scripts of Last Frame
{
    /// <summary>
    /// Converts between noise radius (meters) and Senses decibels.
    /// Senses: an enemy hears a noise if dB - 20 * log10(distance) is above its hearing threshold
    /// (Low 40, Medium 20, High 5, Perfect 0).
    /// </summary>
    public static class NoiseMath // Noise helper
    {
        public const float HighHearingThresholdDb = 5f; // Senses "High" sensitivity (our enemies)

        // Returns the dB needed so an enemy with the given threshold hears the noise up to the radius.
        public static float DbForRadius(float radiusMeters, float thresholdDb = HighHearingThresholdDb)
        {
            return thresholdDb + 20f * Mathf.Log10(Mathf.Max(radiusMeters, 0.01f)); // Inverse of the Senses falloff
        }

        // Returns how far a noise of the given dB can be heard by an enemy with the given threshold.
        public static float RadiusForDb(float db, float thresholdDb = HighHearingThresholdDb)
        {
            return Mathf.Pow(10f, (db - thresholdDb) / 20f); // Senses falloff solved for distance
        }
    }
}
