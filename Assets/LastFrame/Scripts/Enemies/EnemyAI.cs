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
    public enum EnemyStimulus { None, Seen, Glimpsed, Heard, Smelled, InkImpact } // Why the enemy is going to its last known position

    /// <summary>
    /// AI cartoon behaviour: patrols, investigates what it hears and chases Ollie when it sees her.
    /// Perception comes only from the Senses component (sight, hearing, smell awareness 0-100); this script decides what to do with it:
    /// suspicion threshold = "?" (investigate), alert threshold = "!" (chase). Ink impacts arrive as point noises (NoiseEvents).
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
        [Tooltip("Untick to make the enemy blind (only hearing and smell).")]
        [SerializeField] bool m_useVision = true; // False = the enemy never sees Ollie
        [Tooltip("Awareness (0-100) at which the enemy gets suspicious and investigates: '?'. Provisional, to agree with Sebas.")]
        [SerializeField, Range(1, 100)] int m_suspicionAwareness = 20; // Demos use 20-50
        [Tooltip("Sight awareness (0-100) at which the enemy recognises Ollie and chases her: '!'. Provisional, to agree with Sebas.")]
        [SerializeField, Range(1, 100)] int m_alertAwareness = 50; // Must be higher than suspicion
        [Tooltip("Hearing awareness = dB that reach the enemy (noise dB - 20*log10(distance)). The enemy investigates when it is at least this value.\nRadius at which a noise is heard = 10^((noise dB - this) / 20). 41 -> running (60 dB, demo clips) heard at ~8 m (GDD), walking (20-30 dB) almost silent. Provisional.")]
        [SerializeField, Range(1, 100)] int m_hearAwareness = 41; // Received dB needed to react to a noise
        [Tooltip("Optional second Senses (hearing only) on a child object, e.g. \"Ears\". Senses lowers sight awareness when sight and hearing share one component (Ollie's steps keep hitting it), like the demos that never mix them. Empty = the first child Senses, or this one.")]
        [SerializeField] Senses m_hearingSenses; // Ears: hearing-only Senses
        [Tooltip("Child Senses with only smell (Ollie's ink trail, #8). Empty = a child named \"Nose\", else the main Senses.")]
        [SerializeField] Senses m_smellSenses; // Nose: smell-only Senses
        [Tooltip("Factions whose noises the enemy reacts to: Ollie (Faction1) and noise traps / distractions (None).")]
        [SerializeField] FactionNames[] m_hearFactions = { FactionNames.Faction1, FactionNames.None }; // Ollie + objects that make noise
        [SerializeField] float m_arriveDistance = 0.6f; // Distance that counts as "arrived"
        [Tooltip("If the sprite wall guard blocks the enemy this long near its destination, the point counts as reached (it is behind a wall from the camera view).")]
        [SerializeField] float m_giveUpBlockedTime = 0.5f; // Seconds blocked before giving up on a point
        [SerializeField] float m_giveUpExtraDistance = 1.5f; // Extra distance accepted when blocked (meters)

        NavMeshAgent m_agent; // Cached NavMeshAgent
        SpriteWallGuard m_wallGuard; // Optional: keeps the sprite out of walls (issue #27)
        Senses m_senses; // Cached Senses component
        int m_patrolIndex; // Current patrol point
        float m_stateTimer; // Generic timer used by each state
        float m_lastSeenTime; // Time when Ollie was last seen
        float m_nextAttackTime; // Time when the eraser can hit again
        Vector3 m_targetPosition; // Where the enemy is going (noise or last seen position)
        Transform m_target; // Ollie while chasing
        TargetSenses m_ollieTarget; // Ollie's TargetSenses (what Senses tracks)
        float m_prevSeeAwareness; // Sight awareness last frame (to know if it is rising or fading)
        float m_prevHearAwareness; // Hearing awareness last frame (a rise means a new noise)
        float m_prevSmellAwareness; // Smell awareness last frame (a rise means a fresh scent)
        bool m_seeingNow; // True while sight awareness is not fading (Ollie in view, not just remembered)

        public EnemyState State { get; private set; } = EnemyState.Patrol; // Current state
        public bool IsAlerted => State == EnemyState.Chase; // IAlertable: true while chasing
        public bool HasLastKnownPosition => State == EnemyState.Investigate || State == EnemyState.Chase; // True when going to a heard or seen spot
        public Vector3 LastKnownPosition => m_targetPosition; // Where it believes Ollie is (debug and HUD)
        public EnemyStimulus LastStimulus { get; private set; } // What gave it that position (debug)
        public bool UseVision => m_useVision; // Read by the vision cone debug
        public float SeeAwareness { get; private set; } // Current sight awareness of Ollie (0-100, debug and HUD)
        public float HearAwareness { get; private set; } // Current hearing awareness (0-100, debug)
        public int SuspicionAwareness => m_suspicionAwareness; // Threshold for "?" (debug)
        public int AlertAwareness => m_alertAwareness; // Threshold for "!" (debug)
        public int HearThreshold => m_hearAwareness; // Received dB needed to investigate a noise (debug)
        // Distance at which a noise of this strength is heard with the current hearing threshold (debug ring, design tuning).
        public float HearingRadiusFor(float noiseDb) => Mathf.Pow(10f, (noiseDb - m_hearAwareness) / 20f);
        public float EffectiveVisionRange => m_senses ? m_senses.Central_VisionRadius : 0f; // Central vision radius (stealth is applied by Senses itself)
        public event Action<EnemyState> OnStateChanged; // For the "?" / "!" label and audio
        public event Action<GameObject> OnAttack; // Raised when the eraser hits

        // Caches components and the faction filter.
        void Awake()
        {
            m_agent = GetComponent<NavMeshAgent>(); // Required component
            m_wallGuard = GetComponent<SpriteWallGuard>(); // Optional sprite vs wall guard
            m_senses = GetComponent<Senses>(); // Required component
            if (!m_smellSenses) foreach (var child in GetComponentsInChildren<Senses>()) if (child != m_senses && child.name == "Nose") { m_smellSenses = child; break; } // Child "Nose" if any
            if (!m_smellSenses) m_smellSenses = m_senses; // Fall back to the main Senses
            if (!m_hearingSenses) foreach (var child in GetComponentsInChildren<Senses>()) if (child != m_senses && child != m_smellSenses) { m_hearingSenses = child; break; } // Child "Ears" if any
            if (!m_hearingSenses) m_hearingSenses = m_senses; // Fall back to the main Senses (demo sentries that only hear)
            if (m_hearingSenses != m_senses && m_hearingSenses.Use_Forgeting == false) Debug.LogWarning($"[EnemyAI] {name}: enable 'Able To Forget' on the Ears Senses too.", this); // Hearing awareness must fade
            if (!m_senses.Use_Forgeting) Debug.LogWarning($"[EnemyAI] {name}: enable 'Able To Forget' in Senses > Brain, otherwise the enemy never loses Ollie.", this); // Fading awareness is how we know she escaped
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

        // Ink impacts: the drop lands far from Ollie, so it is sent as a point noise (Senses would report Ollie's position instead).
        void HandlePointNoise(Vector3 position, float radius, NoiseSource source)
        {
            if (source != NoiseSource.InkImpact) return; // Footsteps are heard through Senses now
            if (State == EnemyState.Dead || State == EnemyState.Chase) return; // Busy or erased
            Vector3 flat = position - transform.position; // Distance to the impact
            flat.y = 0f; // Ignore height
            if (flat.magnitude > radius) return; // Too far to hear it
            Investigate(position, EnemyStimulus.InkImpact); // Go and check the splash
            Debug.Log($"[EnemyAI] {name} heard an ink impact {flat.magnitude:0.0} m away", this); // Debug
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

        // Reads Senses awareness (sight, hearing, smell) and decides between patrol, "?" and "!".
        void Sense()
        {
            if (!m_ollieTarget) // Find Ollie once
            {
                var ollie = FindAnyObjectByType<PlayerMovement>(); // Ollie's movement script
                if (ollie) m_ollieTarget = ollie.GetComponent<TargetSenses>(); // What Senses tracks
                if (!m_ollieTarget) return; // No Ollie in the scene
            }

            // Sight: awareness rises while Ollie is in view and fades (Able To Forget) when she is gone.
            float see = 0f; // Sight awareness this frame
            if (m_useVision) m_senses.DidSee(m_ollieTarget, 0, out see, out _, out _); // Read it even below the thresholds
            if (see > m_prevSeeAwareness) m_seeingNow = true; // Rising: she is in view
            else if (see < m_prevSeeAwareness) m_seeingNow = false; // Fading: only a memory
            if (see <= 0f) m_seeingNow = false; // Nothing at all
            m_prevSeeAwareness = see; // Remember for next frame
            SeeAwareness = see; // Expose for debug

            if (m_seeingNow && see >= m_alertAwareness) // Recognised: chase
            {
                m_target = m_ollieTarget.transform; // Remember who
                m_targetPosition = m_target.position; // Live position
                m_lastSeenTime = Time.time; // Remember when
                LastStimulus = EnemyStimulus.Seen; // Reason: seen
                if (State != EnemyState.Chase) { Debug.Log($"[EnemyAI] {name} SAW Ollie (awareness {see:0})", this); SetState(EnemyState.Chase); } // "!"
                return; // Seeing wins over the other senses
            }
            if (State == EnemyState.Chase) return; // Chasing: only sight matters (UpdateChase handles losing her)

            if (m_seeingNow && see >= m_suspicionAwareness) // Something moved over there: "?"
            {
                Investigate(m_ollieTarget.transform.position, EnemyStimulus.Glimpsed); // Walk toward the glimpse
                return; // Sight wins over hearing
            }

            // Hearing: a rise in awareness means a new noise (Ollie's steps or a noise trap like the pencil shavings).
            bool heard = m_hearingSenses.WasAnythingHeard(m_hearAwareness, out _, out float hear, out Vector3 soundPos, m_hearFactions, HearSenseSearchType.HighestAwarness); // Loudest noise
            if (heard && hear > m_prevHearAwareness + 0.01f) Investigate(soundPos, EnemyStimulus.Heard); // Go where it sounded
            m_prevHearAwareness = heard ? hear : 0f; // Remember for next frame
            HearAwareness = m_prevHearAwareness; // Expose for debug

            // Smell: a rise means a fresher scent node (e.g. Ollie bleeding ink).
            bool smelled = m_smellSenses.DidSmell(m_ollieTarget, m_suspicionAwareness, out float smell, out Vector3 scentPos, out _); // Ollie's trail
            if (smelled && smell > m_prevSmellAwareness + 0.01f) Investigate(scentPos, EnemyStimulus.Smelled); // Follow the trail
            m_prevSmellAwareness = smelled ? smell : 0f; // Remember for next frame
        }

        // Goes to a point of interest with "?" (re-paths if it is already investigating).
        void Investigate(Vector3 point, EnemyStimulus reason)
        {
            m_targetPosition = point; // Where to look
            LastStimulus = reason; // Why
            if (State == EnemyState.Investigate) { m_stateTimer = m_lookAroundTime; GoTo(point, m_investigateSpeed); return; } // Update the destination
            SetState(EnemyState.Investigate); // Show "?" and walk there
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
            if (m_agent.remainingDistance <= m_arriveDistance) return true; // Close enough
            return m_wallGuard && m_wallGuard.BlockedTime >= m_giveUpBlockedTime && m_agent.remainingDistance <= m_arriveDistance + m_giveUpExtraDistance; // Point is in the blocked band near a wall
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
            if (m_hearingSenses) m_hearingSenses.enabled = false; // Stop hearing too
            if (m_smellSenses) m_smellSenses.enabled = false; // Stop smelling too
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
