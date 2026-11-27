using LastFrame.Player; // PlayerInputReader, InkAmmo
using UnityEngine; // Camera, Plane, Ray

namespace LastFrame.Weapons // Weapon scripts of Last Frame
{
    /// <summary>
    /// Ollie's pen: throws an ink drop in an arc toward the mouse point on the ground. Uses 1 ink per shot.
    /// </summary>
    public class InkPen : Weapon // Ranged weapon (OOP: inherits Weapon)
    {
        [Header("Projectile")] // Inspector group title
        [SerializeField] InkProjectile m_projectilePrefab; // PF_InkProjectile
        [Tooltip("Where the drop comes out. Empty = above Ollie.")]
        [SerializeField] Transform m_muzzle; // Spawn point
        [SerializeField] Vector3 m_muzzleOffset = new Vector3(0f, 1.2f, 0.4f); // Used when there is no muzzle (local space)

        [Header("Arc")] // Inspector group title
        [SerializeField] float m_maxRange = 10f; // Farthest landing point in meters
        [SerializeField] float m_horizontalSpeed = 12f; // Meters per second on the ground plane
        [SerializeField] float m_minFlightTime = 0.25f; // Keeps a visible arc on short throws

        [Header("References")] // Inspector group title
        [SerializeField] PlayerInputReader m_input; // Mouse position
        [SerializeField] InkAmmo m_ammo; // Ink pool
        [SerializeField] Camera m_camera; // Camera used to aim

        public Vector3 LastAimPoint { get; private set; } // Last target point (for an aim marker later)

        // Finds references on the player if they are not assigned.
        void Awake()
        {
            if (!m_input) m_input = GetComponentInParent<PlayerInputReader>(); // Player root
            if (!m_ammo) m_ammo = GetComponentInParent<InkAmmo>(); // Player root
        }

        // Fires one drop if there is ink and a prefab.
        protected override bool PerformAttack()
        {
            if (!m_projectilePrefab) { Debug.LogWarning("[InkPen] Missing projectile prefab.", this); return false; } // Setup error
            if (!m_ammo || !m_ammo.TryConsume()) return false; // No ink, no shot

            Transform owner = m_input.transform; // Player root
            Vector3 start = m_muzzle ? m_muzzle.position : owner.TransformPoint(m_muzzleOffset); // Spawn position
            Vector3 target = GetAimPoint(owner.position.y); // Mouse point on the floor
            LastAimPoint = target; // Save for UI

            InkProjectile drop = Instantiate(m_projectilePrefab, start, Quaternion.identity); // Create the drop
            drop.Launch(owner.gameObject, GetArcVelocity(start, target)); // Throw it in an arc
            return true; // Shot done
        }

        // Returns the point on the floor under the mouse, clamped to the max range.
        Vector3 GetAimPoint(float floorY)
        {
            if (!m_camera) m_camera = Camera.main; // Default camera
            Transform owner = m_input.transform; // Player root
            Vector3 fallback = owner.position + owner.forward * m_maxRange * 0.5f; // Straight ahead if aiming fails
            if (!m_camera) return fallback; // No camera

            Ray ray = m_camera.ScreenPointToRay(m_input.PointerPosition); // Ray from the mouse
            Plane floor = new Plane(Vector3.up, new Vector3(0f, floorY, 0f)); // Flat floor at Ollie's feet
            if (!floor.Raycast(ray, out float distance)) return fallback; // Mouse above the horizon

            Vector3 point = ray.GetPoint(distance); // Point on the floor
            Vector3 flat = point - owner.position; // Vector from Ollie to the point
            flat.y = 0f; // Ignore height
            if (flat.magnitude > m_maxRange) point = owner.position + flat.normalized * m_maxRange; // Clamp range
            point.y = floorY; // Keep on the floor
            return point; // Final aim point
        }

        // Calculates the start velocity so the drop lands on the target (simple ballistic arc).
        Vector3 GetArcVelocity(Vector3 start, Vector3 target)
        {
            Vector3 delta = target - start; // Full offset
            Vector3 flat = new Vector3(delta.x, 0f, delta.z); // Horizontal offset
            float time = Mathf.Max(m_minFlightTime, flat.magnitude / m_horizontalSpeed); // Flight time
            Vector3 velocity = flat / time; // Horizontal speed
            velocity.y = (delta.y - 0.5f * Physics.gravity.y * time * time) / time; // Vertical speed to reach the target height
            return velocity; // Launch velocity
        }
    }
}
