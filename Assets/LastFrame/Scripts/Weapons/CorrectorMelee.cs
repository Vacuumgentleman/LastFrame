using LastFrame.Core; // IDamageable, IAlertable, DamageInfo
using UnityEngine; // Physics, Vector3

namespace LastFrame.Weapons // Weapon scripts of Last Frame
{
    /// <summary>
    /// Ollie's correction fluid: melee hit in front of her. Kills in one hit (team decision 26/11).
    /// From behind (more than 120 degrees from the enemy's front) and unseen, the kill is silent.
    /// </summary>
    public class CorrectorMelee : Weapon // Melee weapon (OOP: inherits Weapon)
    {
        [SerializeField] float m_range = 1.3f; // Reach in meters
        [SerializeField] float m_frontAngle = 70f; // Half-angle of the hit cone in front of Ollie
        [SerializeField] float m_backstabAngle = 120f; // GDD: more than 120 degrees from the enemy's front
        [Tooltip("Team decision 26/11: the corrector always kills in one hit. Untick to require a backstab for the one-hit kill.")]
        [SerializeField] bool m_oneHitKill = true; // True = every hit erases the enemy
        [SerializeField] int m_damage = 1; // Damage points used only when the hit is not a kill
        [SerializeField] LayerMask m_hitLayers = ~0; // What can be hit

        readonly Collider[] m_hits = new Collider[16]; // Reused buffer, no garbage

        public bool LastWasBackstab { get; private set; } // For feedback / debug

        // Finds the closest target in front and hits it.
        protected override bool PerformAttack()
        {
            Transform owner = transform.root; // Player root
            Vector3 center = owner.position + Vector3.up * 0.9f + owner.forward * (m_range * 0.5f); // Sphere in front of Ollie
            int count = Physics.OverlapSphereNonAlloc(center, m_range, m_hits, m_hitLayers, QueryTriggerInteraction.Ignore); // Nearby colliders

            IDamageable best = null; // Closest valid target
            Transform bestTransform = null; // Its transform
            float bestDistance = float.MaxValue; // Its distance
            for (int i = 0; i < count; i++) // Check every collider
            {
                if (m_hits[i].transform.root == owner) continue; // Skip Ollie
                var target = m_hits[i].GetComponentInParent<IDamageable>(); // Damageable on it or its parents
                if (target == null) continue; // Not hittable
                Transform t = ((Component)target).transform; // Target transform
                Vector3 toTarget = t.position - owner.position; // Direction to the target
                toTarget.y = 0f; // Flat
                if (Vector3.Angle(owner.forward, toTarget) > m_frontAngle) continue; // Not in front of Ollie
                float d = toTarget.magnitude; // Distance
                if (d < bestDistance) { bestDistance = d; best = target; bestTransform = t; } // Keep the closest
            }

            if (best == null) return true; // Swing in the air still uses the cooldown

            Vector3 toOllie = owner.position - bestTransform.position; // From the enemy to Ollie
            toOllie.y = 0f; // Flat
            bool fromBehind = Vector3.Angle(bestTransform.forward, toOllie) > m_backstabAngle; // Behind the enemy
            bool alerted = best is IAlertable a && a.IsAlerted; // Has the enemy seen Ollie?
            LastWasBackstab = fromBehind && !alerted; // Silent hit condition

            best.TakeDamage(new DamageInfo // Build hit data
            {
                Type = DamageType.Corrector, // Melee hit
                Amount = m_damage, // Ignored on instant kill
                InstantKill = m_oneHitKill || LastWasBackstab, // One hit kill (always, or only from behind if the option is off)
                Silent = LastWasBackstab, // Only a backstab makes no noise
                HitPoint = bestTransform.position, // Where it landed
                Origin = owner.position, // Ollie's position
                Source = owner.gameObject // Ollie
            });
            return true; // Attack done
        }

        // Draws the hit area in the Scene view when the player is selected.
        void OnDrawGizmosSelected()
        {
            Transform owner = transform.root; // Player root
            Gizmos.color = new Color(1f, 1f, 1f, 0.25f); // Soft white
            Gizmos.DrawWireSphere(owner.position + Vector3.up * 0.9f + owner.forward * (m_range * 0.5f), m_range); // Hit sphere
        }
    }
}
