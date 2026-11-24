using UnityEngine;

namespace Senses2026
{
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerControler_MK2))]
    [RequireComponent(typeof(CharacterController))]

    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Player/BC 2: Player Mover", 2)]
    public class PlayerMover : MonoBehaviour
    {

        CharacterController m_characterController;
        PlayerControler_MK2 m_playerControler;
        PlayerStats m_playerStats;

        Vector3 m_normalizedInputDirection;

        [SerializeField] Transform m_cameraRotationTransform;

        float m_rotationVelocity;
        float m_speed;
        float m_verticalVelocity;
        float m_terminalVelocity = 53.0f;

        Vector3 m_targetDirection;
        float m_rotation;
        float m_targetSpeed;

        float m_jumpTimeoutDelta;
        float m_fallTimeoutDelta;

        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;


        Collider[] m_groundColliders = new Collider[10];
        float m_targetRotation;
        Vector3 m_motion;

        Animator m_animator;
        bool m_detectedAnimator;

        Stealth m_stealth;
        bool m_detectedStealthComponent;

        void Start()
        {
            m_characterController = GetComponent<CharacterController>();
            m_playerControler = GetComponent<PlayerControler_MK2>();
            m_playerStats = GetComponent<PlayerStats>();

            m_detectedAnimator = TryGetComponent<Animator>(out m_animator);
            m_detectedStealthComponent = TryGetComponent<Stealth>(out m_stealth);

            if (m_cameraRotationTransform == null) m_cameraRotationTransform = Camera.main.transform;
        }

        void LateUpdate()
        {
            jumpAndGravity();
            groundedCheck();
            movePlayer();

            if (m_detectedStealthComponent) m_stealth.enabled = m_playerControler.m_Stealth;
        }



        void movePlayer()
        {
            m_targetSpeed = getSpeed();

            if (m_playerControler.MoveVector == Vector2.zero) m_targetSpeed = 0.0f;

            m_speed = m_targetSpeed;


            m_normalizedInputDirection = new Vector3(m_playerControler.MoveVector.x, 0.0f, m_playerControler.MoveVector.y).normalized;


            if (m_playerControler.MoveVector != Vector2.zero)
            {
                m_targetRotation = Mathf.Atan2(m_normalizedInputDirection.x, m_normalizedInputDirection.z) * Mathf.Rad2Deg +
                m_cameraRotationTransform.transform.eulerAngles.y;

                m_rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_targetRotation, ref m_rotationVelocity,
                    m_playerStats.RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, m_rotation, 0.0f);
            };


            m_targetDirection = Quaternion.Euler(0.0f, m_targetRotation, 0.0f) * Vector3.forward;

            m_motion = m_targetDirection.normalized * (m_speed * Time.deltaTime) +
                             new Vector3(0.0f, m_verticalVelocity, 0.0f) * Time.deltaTime;
            if (m_detectedAnimator)
            {
                if (m_playerControler.m_Stealth)
                {
                    float _normalizedToMaxSpeed = m_speed / m_playerStats.CrouchSpeed;
                    m_animator.SetFloat("Velocity_Z", _normalizedToMaxSpeed);
                    m_animator.SetBool("isStealth", true);
                }
                else
                {
                    float _normalizedToMaxSpeed = m_speed / m_playerStats.SprintSpeed;
                    m_animator.SetFloat("Velocity_Z", _normalizedToMaxSpeed);
                    m_animator.SetBool("isStealth", false);
                }
            }

            m_characterController.Move(m_motion);
        }

        void jumpAndGravity()
        {
            if (Grounded)
            {
                m_fallTimeoutDelta = FallTimeout;

                if (m_verticalVelocity < 0.0f)
                {
                    m_verticalVelocity = -2f;
                }

                if (m_playerControler.m_Jump && m_jumpTimeoutDelta <= 0.0f)
                {
                    m_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }

                if (m_jumpTimeoutDelta >= 0.0f)
                {
                    m_jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                m_jumpTimeoutDelta = JumpTimeout;

                if (m_fallTimeoutDelta >= 0.0f)
                {
                    m_fallTimeoutDelta -= Time.deltaTime;
                }

                m_playerControler.SetJump(false);
            }

            if (m_verticalVelocity < m_terminalVelocity)
            {
                m_verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        void groundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

            int _hitCount = Physics.OverlapSphereNonAlloc(spherePosition, GroundedRadius, m_groundColliders, GroundLayers);

            for (int i = 0; i < _hitCount; i++)
            {
                if (m_groundColliders[i].CompareTag("Ground"))
                {
                    Grounded = true;
                    return;
                }
            }

            Grounded = false;
        }

        void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        float getSpeed()
        {
            if (m_playerControler.Sprints) return m_playerStats.SprintSpeed;
            if (m_playerControler.m_Stealth) return m_playerStats.CrouchSpeed;
            return m_playerStats.WalkSpeed;
        }
    }
}
