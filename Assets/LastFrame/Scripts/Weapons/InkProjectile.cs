using LastFrame.Core; // IDamageable, DamageInfo
using Senses2026; // NoiseRecivers, TargetSenses
using UnityEngine; // Collider, Vector3

namespace LastFrame.Weapons // Weapon scripts of Last Frame
{
    /// <summary>
    /// Ink drop fired by the pen. Kills the enemy it hits in one shot (team decision 26/11) and makes noise at the impact.
    /// Enemies hear it at the impact point, and the origin is Ollie's TargetSenses.
    /// </summary>
    public class InkProjectile : Projectile // Ink drop (OOP: inherits Projectile)
    {
        [Tooltip("Team decision 26/11: one ink hit dissolves the enemy. Untick to go back to damage points.")]
        [SerializeField] bool m_oneHitKill = true; // True = enemy dissolves on the first hit
        [SerializeField] int m_damage = 1; // Damage points used only when one-hit kill is off
        [Tooltip("Noise at the impact, in dB (Senses). GDD target is about 5 m. Tune by ear.")]
        [SerializeField] float m_impactNoiseDb = 60f; // Impact noise strength

        // Applies ink damage and releases the impact noise, then removes the drop.
        protected override void OnHit(Collider other, Vector3 point)
        {
            var target = other.GetComponentInParent<IDamageable>(); // Enemy, camera or dummy
            if (target != null) // Hit something that takes damage
            {
                target.TakeDamage(new DamageInfo // Build hit data
                {
                    Type = DamageType.Ink, // Pen hit
                    Amount = m_damage, // 1 point
                    InstantKill = m_oneHitKill, // One shot dissolves the enemy
                    Silent = false, // Ink makes noise
                    HitPoint = point, // Impact point
                    Origin = Owner ? Owner.transform.position : point, // Where it was fired from
                    Source = Owner // Ollie
                });
            }

            TargetSenses origin = Owner ? Owner.GetComponent<TargetSenses>() : null; // Ollie as the noise origin
            NoiseRecivers.Instance.SpreadNoise(m_impactNoiseDb, point, origin, false, false); // Enemies near the impact hear it

            Destroy(gameObject); // The drop is used up
        }
    }
}
