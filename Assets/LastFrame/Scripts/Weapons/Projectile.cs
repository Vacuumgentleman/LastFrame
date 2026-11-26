using UnityEngine; // MonoBehaviour, Rigidbody, Collision

namespace LastFrame.Weapons // Weapon scripts of Last Frame
{
    /// <summary>
    /// Base class for thrown objects (GDD: Projectile base). Handles launch, owner and lifetime.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))] // Moves with physics
    public abstract class Projectile : MonoBehaviour // Shared projectile behaviour
    {
        [SerializeField] float m_lifetime = 5f; // Seconds before it is removed if it hits nothing

        protected Rigidbody Body { get; private set; } // Cached Rigidbody
        protected GameObject Owner { get; private set; } // Who fired it (Ollie)
        bool m_hasHit; // Prevents double hits in the same frame

        // Caches the Rigidbody.
        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody>(); // Required component
        }

        // Sets the owner, ignores the owner's colliders and gives the start velocity.
        public void Launch(GameObject owner, Vector3 velocity)
        {
            Owner = owner; // Store shooter
            foreach (var ownCol in owner.GetComponentsInChildren<Collider>()) // Every collider of the shooter
                foreach (var myCol in GetComponentsInChildren<Collider>()) // Every collider of the projectile
                    Physics.IgnoreCollision(ownCol, myCol); // Do not hit the shooter
            Body.linearVelocity = velocity; // Throw it
            Destroy(gameObject, m_lifetime); // Safety cleanup
        }

        // Calls OnHit once on the first collision.
        void OnCollisionEnter(Collision collision)
        {
            if (m_hasHit) return; // Already handled
            m_hasHit = true; // Mark as handled
            Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position; // Contact point
            OnHit(collision.collider, point); // Child decides what happens
        }

        // Each projectile implements what happens when it hits something.
        protected abstract void OnHit(Collider other, Vector3 point);
    }
}
