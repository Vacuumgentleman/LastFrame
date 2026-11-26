using UnityEngine; // Vector3, GameObject

namespace LastFrame.Core // Shared scripts of Last Frame
{
    public enum DamageType { Ink, Corrector } // Ink = pen drop, Corrector = melee hit

    /// <summary>
    /// Data sent with every hit so the receiver can react (stain, dissolve, turn around, blind).
    /// </summary>
    public struct DamageInfo // Hit description
    {
        public DamageType Type; // Which weapon hit
        public int Amount; // Damage points (enemies have 2)
        public bool InstantKill; // True for a silent backstab with the corrector
        public bool Silent; // True if the hit makes no noise (receiver should not alert others)
        public Vector3 HitPoint; // Where the hit landed
        public Vector3 Origin; // Where the attack came from (Ollie's position)
        public GameObject Source; // Who attacked (Ollie)
    }

    /// <summary>
    /// Anything that can be hit by Ollie's weapons: enemies, El Reemplazo, security cameras.
    /// </summary>
    public interface IDamageable // GDD: IDamageable only asks for TakeDamage()
    {
        void TakeDamage(DamageInfo info); // Apply a hit
    }

    /// <summary>
    /// Optional: lets weapons know if the target has already seen Ollie (used by the corrector backstab).
    /// </summary>
    public interface IAlertable // Implemented by enemies
    {
        bool IsAlerted { get; } // True when the enemy is chasing or has seen Ollie
    }
}
