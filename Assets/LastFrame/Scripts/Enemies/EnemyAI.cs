using System; // Action delegate for events
using System.Collections; // IEnumerator for coroutines
using LastFrame.Core; // IDamageable, IAlertable, DamageInfo
using LastFrame.Player; // PlayerMovement (crouch state)
using Senses2026; // Senses, TargetSenses, FactionNames
using UnityEngine; // MonoBehaviour, Vector3
using UnityEngine.AI; // NavMeshAgent

namespace LastFrame.Enemies // Enemy scripts of Last Frame
{
    public enum EnemyState { Patrol, Investigate, Chase, Dead } // GDD state machine (Stunned removed: one-hit kill)
    public enum EnemyStimulus { None, Seen, Heard, InkImpact } // Why the enemy is going to its last known position

    /// <summary>
    /// AI cartoon behaviour: patrols, investigates what it hears and chases Ollie when it sees her.
    /// Perception comes from the Senses component (configured by Sebas); this script decides what to do with it.
    /// </summary>
    [DisallowMultipleComponent] // One brain per enemy
    [RequireComponent(typeof(NavMeshAgent))] // Moves on the NavMesh
    [RequireComponent(typeof(Senses))] // Sees, hears and smells
    public class EnemyAI : MonoBehaviour, IDamageable, IAlertable // Enemy brain
    {
        [Header("Patrol")] // Inspector group title
        [SerializeField] Transform[] m_patrolPoints; // Route in order, loops back to the first
        [SerializeField] float m_patrolSpeed = 2f; // GDD: patrolSpeed 2
        [SerializeField] float m_waitAtPoint = 1.5f; // Seconds standing at each point

        [Header("Investigate")] // Inspector group title
        [SerializeField] float m_investigateSpeed = 3f; // Walk to the noise a bit faster
        [SerializeField] float m_lookAroundTime = 3f; // Seconds looking around at the noise

        [Header("Chase")] // Inspector group title
        [SerializeField] float m_chaseSpeed = 3.5f; // GDD: chaseSpeed 3.5 (lowered from 4.5 on 26/11 so Ollie can escape running)
        [Tooltip("Seconds without seeing Ollie before switching to '?' (small grace so the state does not flicker).")]
        [SerializeField] float m_loseSightTime = 0.3f; // Was 2 s: the enemy now gives up the chase as soon as it loses sight
        [SerializeField] float m_attackRange = 1.3f; // Eraser reach
        [SerializeField] float m_attackCooldown = 1.2f; // Seconds between eraser hits

        [Header("Senses")] // Inspector group title
        [Tooltip("Test 26/11: untick to make the enemy blind (only hearing). Vision made hiding almost impossible; the team will decide.")]
        [SerializeField] bool m_useVision = true; // False = the enemy never sees Ollie
        [SerializeField] FactionNames m_targetFaction = FactionNames.Faction1; // Ollie's faction
        [SerializeField] float m_requiredAwareness = 1f; // Awareness needed to react
        [SerializeField] float m_arriveDistance = 0.6f; // Distance that counts as "arrived"
        [Tooltip("Eye height used to confirm line of sight (Senses keeps a memory, so we double-check).")]
        [SerializeField] float m_eyeHeight = 1.5f; // Same as the Senses eye offset
        [SerializeField] LayerMask m_visionBlockers = ~0; // What blocks the view (walls, props)
        [Tooltip("Vision range multiplier while Ollie is crouching (GDD: crouching reduces detection range).")]
        [SerializeField, Range(0.1f, 1f)] float m_crouchVisionMultiplier = 0.5f; // Half range when crouching
        [Tooltip("A heard position closer than this to the last investigated one is treated as the same noise.")]
        [SerializeField] float m_sameNoiseDistance = 0.75f; // Avoids re-investigating Senses' remembered noise

        NavMeshAgent m_agent; // Cached NavMeshAgent
        Senses m_senses; // Cached Senses component
        FactionNames[] m_factionFilter; // Filter array for Senses queries (no garbage per frame)
        int m_patrolIndex; // Current patrol point
        float m_stateTimer; // Generic timer used by each state
        float m_lastSeenTime; // Time when Ollie was last seen
        float m_nextAttackTime; // Time when the eraser can hit again
        Vector3 m_targetPosition; // Where the enemy is going (noise or last seen position)
        Transform m_target; // Ollie while chasing
        Vector3 m_lastHeardPosition; // Last noise position already reacted to
        bool m_hasHeard; // True after the first noise

        public EnemyState State { get; private set; } = EnemyState.Patrol; // Current state
        public bool IsAlerted => State == EnemyState.Chase; // IAlertable: true while chasing
        public bool HasLastKnownPosition => State == EnemyState.Investigate || State == EnemyState.Chase; // True when going to a heard or seen spot
        public Vector3 LastKnownPosition => m_targetPosition; // Where it believes Ollie is (debug and HUD)
        public EnemyStimulus LastStimulus { get; private set; } // What gave it that position (debug)
        public bool UseVision => m_useVision; // Read by the vision cone debug
        PlayerMovement m_ollie; // Cached Ollie movement (crouch state) for the effective vision range

        // Vision range against Ollie right now: Senses radius, halved while she crouches (used by the AI and the debug cone).
        public float EffectiveVisionRange
        {
            get
            {
                if (!m_ollie) m_ollie = FindAnyObjectByType<PlayerMovement>(); // Find Ollie once
                float range = m_senses ? m_senses.Central_VisionRadius : 0f; // Base range from Senses
                if (m_ollie && m_ollie.IsCrouching) range *= m_crouchVisionMultiplier; // Crouching is harder to spot
                return range; // Effective range
            }
        }
        public event Action<EnemyState> OnStateChanged; // For the "?" / "!" label and audio
        public event Action<GameObject> OnAttack; // Raised when the eraser hits

        // Caches components and the faction filter.
        void Awake()
        {
            m_agent = GetComponent<NavMeshAgent>(); // Required component
            m_senses = GetComponent<Senses>(); // Required component
            m_factionFilter = new[] { m_targetFaction }; // Only look for Ollie's faction
        }

        // Listens to point noises (ink impacts) while the enemy is active.
        void OnEnable()
        {
            NoiseEvents.OnPointNoise += HandlePointNoise; // Subscribe
        }

        // Stops listening to point noises.
        void OnDisable()
        {
            NoiseEvents.OnPointNoise -= HandlePointNoise; // Unsubscribe
        }

        // Hearing: goes to investigate a noise point if it happened within the noise radius (Ollie running or an ink impact).
        void HandlePointNoise(Vector3 position, float radius, NoiseSource source)
        {
            if (State == EnemyState.Dead || State == EnemyState.Chase) return; // Busy or erased (sight drives the chase)
            Vector3 flat = position - transform.position; // Distance to the noise
            flat.y = 0f; // Ignore height
            if (flat.magnitude > radius) return; // Too far to hear it
            m_hasHeard = true; // Remember a noise was handled
            m_lastHeardPosition = position; // Last heard point
            m_targetPosition = position; // Go to the exact point of the noise
            LastStimulus = source == NoiseSource.InkImpact ? EnemyStimulus.InkImpact : EnemyStimulus.Heard; // Reason for the debug label
            Debug.Log($"[EnemyAI] {name} heard {source} at {position}, {flat.magnitude:0.0} m away (radius {radius:0.0} m, state was {State})", this); // Debug
            SetState(EnemyState.Investigate); // Show "?" and walk there
        }

        // Starts patrolling.
        void Start()
        {
            SetState(EnemyState.Patrol); // Initial state
        }

        // Reads the senses and runs the current state.
        void Update()
        {
            if (State == EnemyState.Dead) return; // Dead enemies do nothing

            Sense(); // Update target info from Senses

            switch (State) // Run the active state
            {
                case EnemyState.Patrol: UpdatePatrol(); break; // Walk the route
                case EnemyState.Investigate: UpdateInvestigate(); break; // Check the noise
                case EnemyState.Chase: UpdateChase(); break; // Hunt Ollie
            }
        }

        // Asks Senses what the enemy sees or hears and changes state if needed.
        void Sense()
        {
            if (m_useVision && m_senses.WasAnythingSeen(m_requiredAwareness, out TargetSenses seen, out _, out _, out _, m_factionFilter, SeeSenseSearchType.Nearest, true) && CanReallySee(seen.transform)) // Ollie really in view (not just remembered)
            {
                m_target = seen.transform; // Remember who
                m_targetPosition = seen.transform.position; // Remember where
                m_lastSeenTime = Time.time; // Remember when
                LastStimulus = EnemyStimulus.Seen; // Reason: seen
                if (State != EnemyState.Chase) // First frame it sees her
                {
                    Debug.Log($"[EnemyAI] {name} SAW Ollie at {Vector3.Distance(transform.position, seen.transform.position):0.0} m (range {EffectiveVisionRange:0.0} m, crouching={m_ollie && m_ollie.IsCrouching})", this); // Debug
                    SetState(EnemyState.Chase); // Start chasing
                }
                return; // Seeing wins over hearing
            }

            // Hearing is handled by HandlePointNoise (NoiseEvents), not by Senses: its hearing reported noises far out of range.
        }

        // Confirms Ollie is inside the vision cone and nothing blocks the view (Senses may report a remembered target).
        bool CanReallySee(Transform target)
        {
            Vector3 eyes = transform.position + Vector3.up * m_eyeHeight; // Eye position
            Collider body = target.GetComponent<Collider>(); // Ollie's capsule (lower when crouching)
            Vector3 targetPoint = body ? body.bounds.center : target.position + Vector3.up * 1f; // Center of her body
            Vector3 toTarget = targetPoint - eyes; // Direction to Ollie
            Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z); // Ignore height for the cone check
            if (flat.magnitude > EffectiveVisionRange) return false; // Too far (shorter when Ollie crouches)
            if (Vector3.Angle(transform.forward, flat) > m_senses.Central_VisionAngle * 0.5f) return false; // Outside the cone
            if (Physics.Linecast(eyes, targetPoint, out RaycastHit hit, m_visionBlockers, QueryTriggerInteraction.Ignore)) // Something in between?
                return hit.transform == target || hit.transform.IsChildOf(target); // Only Ollie herself may be hit
            return true; // Clear view
        }

        // Walks between patrol points and waits a moment at each one.
        void UpdatePatrol()
        {
            if (m_patrolPoints == null || m_patrolPoints.Length == 0) return; // No route: stand guard
            if (!HasArrived()) return; // Still walking

            m_stateTimer -= Time.deltaTime; // Waiting at the point
            if (m_stateTimer > 0f) return; // Keep waiting

            m_patrolIndex = (m_patrolIndex + 1) % m_patrolPoints.Length; // Next point (loop)
            GoTo(m_patrolPoints[m_patrolIndex].position, m_patrolSpeed); // Walk there
            m_stateTimer = m_waitAtPoint; // Reset wait for the next point
        }

        // Goes to the noise, looks around, then returns to patrol.
        void UpdateInvestigate()
        {
            if (!HasArrived()) return; // Still walking to the noise

            transform.Rotate(0f, 90f * Time.deltaTime, 0f); // Look around slowly
            m_stateTimer -= Time.deltaTime; // Look-around time
            if (m_stateTimer <= 0f) SetState(EnemyState.Patrol); // Nothing found: back to work
        }

        // Follows Ollie, hits her with the eraser in range, and gives up if she hides long enough.
        void UpdateChase()
        {
            if (Time.time - m_lastSeenTime > m_loseSightTime) // Lost her
            {
                Debug.Log($"[EnemyAI] {name} LOST sight of Ollie, going to last seen position {m_targetPosition}", this); // Debug
                SetState(EnemyState.Investigate); // Check the last place she was seen
                return; // Stop chasing
            }

            float distance = m_target ? Vector3.Distance(transform.position, m_target.position) : float.MaxValue; // Distance to Ollie
            if (distance > m_attackRange * 0.8f) // Not close enough yet
            {
                GoTo(m_targetPosition, m_chaseSpeed); // Run to Ollie
                return; // Keep running
            }

            if (m_agent.isOnNavMesh) { m_agent.isStopped = true; m_agent.ResetPath(); m_agent.velocity = Vector3.zero; } // Stop in front of her (do not push her)
            FaceTarget(m_target.position); // Look at Ollie while hitting
            if (Time.time < m_nextAttackTime) return; // Cooling down

            m_nextAttackTime = Time.time + m_attackCooldown; // Start cooldown
            var damageable = m_target.GetComponentInParent<IDamageable>(); // Ollie's health (when it exists)
            damageable?.TakeDamage(new DamageInfo { Type = DamageType.Corrector, Amount = 1, HitPoint = m_target.position, Origin = transform.position, Source = gameObject }); // Eraser hit = 1 heart
            OnAttack?.Invoke(m_target.gameObject); // Notify visual / audio
            Debug.Log($"[EnemyAI] {name} hit Ollie with the eraser", this); // Visible in the console until hearts exist
        }

        // Turns the enemy toward a point on the ground.
        void FaceTarget(Vector3 point)
        {
            Vector3 dir = point - transform.position; // Direction to the point
            dir.y = 0f; // Flat
            if (dir.sqrMagnitude < 0.001f) return; // Already there
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime); // Smooth turn
        }

        // Changes state and prepares it.
        void SetState(EnemyState state)
        {
            State = state; // Store new state
            switch (state) // Enter the state
            {
                case EnemyState.Patrol: // Back to the route
                    LastStimulus = EnemyStimulus.None; // No target any more
                    m_stateTimer = 0f; // Leave the current point right away
                    if (m_patrolPoints != null && m_patrolPoints.Length > 0) GoTo(m_patrolPoints[m_patrolIndex].position, m_patrolSpeed); // Resume route
                    break;
                case EnemyState.Investigate: // Go to a point of interest
                    m_stateTimer = m_lookAroundTime; // Look-around time on arrival
                    GoTo(m_targetPosition, m_investigateSpeed); // Walk to the noise / last seen spot
                    break;
                case EnemyState.Chase: // Hunt
                    GoTo(m_targetPosition, m_chaseSpeed); // Run to Ollie
                    break;
                case EnemyState.Dead: // Erased
                    if (m_agent.isOnNavMesh) m_agent.isStopped = true; // Stop moving
                    break;
            }
            OnStateChanged?.Invoke(state); // Notify label / audio
        }

        // Sends the agent to a point at a given speed.
        void GoTo(Vector3 position, float speed)
        {
            if (!m_agent.isOnNavMesh) return; // Not placed on the NavMesh
            m_agent.speed = speed; // Movement speed
            m_agent.isStopped = false; // Make sure it moves
            m_agent.SetDestination(position); // Path to the point
        }

        // True when the agent reached its destination.
        bool HasArrived()
        {
            if (!m_agent.isOnNavMesh || m_agent.pathPending) return false; // Still calculating
            return m_agent.remainingDistance <= m_arriveDistance; // Close enough
        }

        // Receives a hit from Ollie: one ink drop or one corrector hit erases the enemy (team decision 26/11).
        public void TakeDamage(DamageInfo info)
        {
            if (State == EnemyState.Dead) return; // Already erased
            Debug.Log($"[EnemyAI] {name} erased by {info.Type}. Silent={info.Silent}", this); // Console feedback
            SetState(EnemyState.Dead); // Stop the brain
            StartCoroutine(Dissolve()); // Wet paper effect
        }

        // Flattens the enemy like wet paper and removes it.
        IEnumerator Dissolve()
        {
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false; // No more hits or blocking
            m_senses.enabled = false; // Stop sensing
            Vector3 start = transform.localScale; // Normal size
            Vector3 end = new Vector3(start.x * 1.3f, 0.05f, start.z * 1.3f); // Puddle size
            float t = 0f; // Animation time
            while (t < 0.4f) // Short squash
            {
                t += Time.deltaTime; // Advance time
                transform.localScale = Vector3.Lerp(start, end, t / 0.4f); // Flatten
                yield return null; // Next frame
            }
            Destroy(gameObject, 0.5f); // Remove after the puddle is visible
        }

        // Draws the patrol route and attack range in the Scene view.
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow; // Route color
            if (m_patrolPoints != null) // Draw route lines
                for (int i = 0; i < m_patrolPoints.Length; i++) // Every point
                    if (m_patrolPoints[i] && m_patrolPoints[(i + 1) % m_patrolPoints.Length]) // Both ends exist
                        Gizmos.DrawLine(m_patrolPoints[i].position, m_patrolPoints[(i + 1) % m_patrolPoints.Length].position); // Segment
            Gizmos.color = Color.red; // Attack color
            Gizmos.DrawWireSphere(transform.position, m_attackRange); // Eraser reach
        }
    }
}
