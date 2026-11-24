using Senses2026;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
#endif

namespace CatBorgStudio.Movement
{
    // [RequireComponent(typeof(CharacterController))]
    // [RequireComponent(typeof(Animator))]
    // [RequireComponent(typeof(PlayerStats))]
    // [RequireComponent(typeof(PlayerControler_MK2))]
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Player/BC 2: Player Mover MK2", 2)]
    public class PlayerMover_MK2 : MonoBehaviour
    {
        #region -- Config ----------------------------------------------------
        [Header("Grounding")]
        [SerializeField] float m_groundedOffset = -0.14f;
        [SerializeField] float m_groundedRadius = 0.28f;
        [SerializeField] LayerMask m_groundLayers;

        [Header("Stealth")]
        [SerializeField] Stealth m_stealth;
        #endregion

        #region -- Runtime ---------------------------------------------------
        CharacterController m_controller;
        Animator m_animator;
        PlayerControler_MK2 m_playerControler;
        GameObject m_mainCamera;
        PlayerStats m_playerStats;

        float m_threshold = 0.01f;
        float m_speed;
        float m_animationBlend;
        float m_targetRotation;
        float m_rotationVelocity;
        float m_verticalVelocity;
        float m_verticalVelocityDuringJump;
        float m_terminalVelocity = 53f;
        float m_jumpStartHorizontalSpeed;

        float m_jumpTimeoutDelta;
        float m_fallTimeoutDelta;

        Vector3 m_standCenter;
        float m_standHeight;
        bool m_isCrouching;
        bool m_wasEnabled;

        bool m_hadStealthComponent;
        bool m_previousStealthState;

        float m_airborneRotation;
        #endregion

        #region -- Animation Cache -------------------------------------------
        int m_hashSpeed;
        int m_hashGrounded;
        int m_hashJump;
        int m_hashFreeFall;
        int m_hashMotionSpeed;
        int m_hashIsCrouching;

        float m_lastSpeed;
        float m_lastMotionSpeed;
        bool m_lastGrounded;
        bool m_lastJump;
        bool m_lastFreeFall;
        bool m_lastIsCrouching;
        #endregion

        #region -- Grounding -------------------------------------------------
        Collider[] m_groundCheckBuffer = new Collider[4];
        bool m_grounded;
        #endregion

        void Start()
        {
            if (!m_wasEnabled) Enable();
        }

        void Enable()
        {
            m_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            if (!m_mainCamera) return;

            if (!TryGetComponent(out m_animator)) return;
            if (!TryGetComponent(out m_controller)) return;
            if (!TryGetComponent(out m_playerControler)) return;
            if (!TryGetComponent(out m_playerStats)) return;

            m_hadStealthComponent = TryGetComponent(out m_stealth);
            if (m_hadStealthComponent) m_previousStealthState = m_stealth.enabled;

            if (!AssignAnimationIDs()) return;

            m_jumpTimeoutDelta = m_playerStats.JumpTimeout;
            m_fallTimeoutDelta = m_playerStats.FallTimeout;
            m_standCenter = m_controller.center;
            m_standHeight = m_controller.height;
            m_verticalVelocityDuringJump = Mathf.Sqrt(2f * Mathf.Abs(m_playerStats.Gravity) * m_playerStats.JumpHeight);

            m_wasEnabled = true;
        }

        bool AssignAnimationIDs()
        {
            bool success = true;
            success &= ValidateAndAssignID(ref m_hashSpeed, "Speed");
            success &= ValidateAndAssignID(ref m_hashGrounded, "Grounded");
            success &= ValidateAndAssignID(ref m_hashJump, "Jump");
            success &= ValidateAndAssignID(ref m_hashFreeFall, "FreeFall");
            success &= ValidateAndAssignID(ref m_hashMotionSpeed, "MotionSpeed");
            success &= ValidateAndAssignID(ref m_hashIsCrouching, "IsCrouching");
            return success;
        }

        bool ValidateAndAssignID(ref int id, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new System.ArgumentException($"Animator parameter name is null: {name}");

            bool found = false;
            foreach (var param in m_animator.parameters)
            {
                if (param.name == name)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new System.Exception($"Animator missing parameter: {name}");

            id = Animator.StringToHash(name);
            return found;
        }

        void Update()
        {
            if (!m_wasEnabled) return;

            HandleVertical();
            PassValueToAnimatorIfDifferent(m_hashGrounded, IsGrounded(), ref m_lastGrounded);

            HandleHorizontal();
            HandleCharacterCollider();
            HandleCrouch();
        }

        bool IsGrounded()
        {
            Vector3 pos = transform.position;
            pos.y -= m_groundedOffset;

            int hits = Physics.OverlapSphereNonAlloc(pos, m_groundedRadius, m_groundCheckBuffer, m_groundLayers, QueryTriggerInteraction.Ignore);
            m_grounded = hits > 0;
            return m_grounded;
        }

        void HandleVertical()
        {
            if (m_grounded)
            {
                if (m_verticalVelocity < 0f)
                {
                    m_fallTimeoutDelta = m_playerStats.FallTimeout;
                    m_verticalVelocity = -2f;
                }

                PassValueToAnimatorIfDifferent(m_hashJump, false, ref m_lastJump);
                PassValueToAnimatorIfDifferent(m_hashFreeFall, false, ref m_lastFreeFall);

                if (m_playerControler.m_Jump && m_jumpTimeoutDelta <= 0f)
                {
                    m_verticalVelocity = m_verticalVelocityDuringJump;
                    PassValueToAnimatorIfDifferent(m_hashJump, true, ref m_lastJump);
                    m_jumpStartHorizontalSpeed = m_speed;
                    m_jumpTimeoutDelta = 0.1f;
                }

                if (m_jumpTimeoutDelta >= 0f)
                    m_jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                m_playerControler.SetJump(false);

                if (m_fallTimeoutDelta > 0f)
                {
                    m_fallTimeoutDelta -= Time.deltaTime;
                    if (m_fallTimeoutDelta <= 0f)
                        PassValueToAnimatorIfDifferent(m_hashFreeFall, true, ref m_lastFreeFall);
                }

                m_jumpTimeoutDelta = m_playerStats.JumpTimeout;
            }

            if (!m_grounded && m_verticalVelocity < m_terminalVelocity)
                m_verticalVelocity += m_playerStats.Gravity * Time.deltaTime;
        }

        void HandleHorizontal()
        {
            if (m_playerControler.Sprints) m_isCrouching = false;

            float targetSpeed = m_playerControler.Sprints ? m_playerStats.SprintSpeed :
                                m_isCrouching ? m_playerStats.CrouchSpeed :
                                m_playerStats.WalkSpeed;

            if (m_playerControler.MoveVector == Vector2.zero) targetSpeed = 0f;

            if (!m_grounded)
            {
                targetSpeed = m_jumpStartHorizontalSpeed; // no horizontal speed change mid-jump
            }

            float currentHoriz = new Vector3(m_controller.velocity.x, 0f, m_controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMag = 1f;

            if (currentHoriz < targetSpeed - speedOffset || currentHoriz > targetSpeed + speedOffset)
            {
                m_speed = Mathf.Lerp(currentHoriz, targetSpeed * inputMag, Time.deltaTime * m_playerStats.SpeedChangeRate);
                m_speed = Mathf.Round(m_speed * 1000f) / 1000f;
            }
            else
            {
                m_speed = targetSpeed;
            }

            m_animationBlend = Mathf.Lerp(m_animationBlend, targetSpeed, Time.deltaTime * m_playerStats.SpeedChangeRate);
            if (m_animationBlend < 0.01f) m_animationBlend = 0f;

            Vector3 inputDir = new Vector3(m_playerControler.MoveVector.x, 0f, m_playerControler.MoveVector.y).normalized;

            if (m_grounded && inputDir.sqrMagnitude > m_threshold)
            {
                m_targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + m_mainCamera.transform.eulerAngles.y;
                float smooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_targetRotation, ref m_rotationVelocity, m_playerStats.RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smooth, 0f);
                m_airborneRotation = transform.eulerAngles.y;
            }
            else if (!m_grounded)
            {
                transform.rotation = Quaternion.Euler(0f, m_airborneRotation, 0f);
            }

            Vector3 moveDir = Quaternion.Euler(0f, m_airborneRotation, 0f) * Vector3.forward;
            m_controller.Move(moveDir.normalized * (m_speed * Time.deltaTime) + Vector3.up * m_verticalVelocity * Time.deltaTime);

            PassValueToAnimatorIfDifferent(m_hashSpeed, m_animationBlend, ref m_lastSpeed);
            PassValueToAnimatorIfDifferent(m_hashMotionSpeed, inputMag, ref m_lastMotionSpeed);
        }

        void HandleCrouch()
        {
            PassValueToAnimatorIfDifferent(m_hashIsCrouching, m_isCrouching, ref m_lastIsCrouching);

            if (m_hadStealthComponent && m_isCrouching != m_previousStealthState)
            {
                m_previousStealthState = m_isCrouching;
                m_stealth.enabled = m_isCrouching;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(m_stealth);
#endif
            }

            if (m_playerControler.m_Stealth)
            {
                m_isCrouching = !m_isCrouching;
                m_playerControler.SetStealth(false);
            }
        }

        void HandleCharacterCollider()
        {
            Vector3 targetCenter = m_isCrouching ? m_playerStats.CrouchCenter : m_standCenter;
            float targetHeight = m_isCrouching ? m_playerStats.CrouchHeight : m_standHeight;

            m_controller.height = Mathf.Lerp(m_controller.height, targetHeight, m_playerStats.CrouchTransitionSpeed * Time.deltaTime);
            m_controller.center = Vector3.Lerp(m_controller.center, targetCenter, m_playerStats.CrouchTransitionSpeed * Time.deltaTime);
        }

        void PassValueToAnimatorIfDifferent(int animID, float newValue, ref float lastValue)
        {
            if (!Mathf.Approximately(newValue, lastValue))
            {
                lastValue = newValue;
                m_animator.SetFloat(animID, newValue);
            }
        }

        void PassValueToAnimatorIfDifferent(int animID, bool newValue, ref bool lastValue)
        {
            if (newValue != lastValue)
            {
                lastValue = newValue;
                m_animator.SetBool(animID, newValue);
            }
        }

        void OnDrawGizmosSelected()
        {
            Color color = m_grounded ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position + Vector3.down * m_groundedOffset, m_groundedRadius);
        }

        void OnFootstep(AnimationEvent evt)
        {
            if (m_playerStats.FootstepSounds == null || m_playerStats.FootstepSounds.Length == 0) return;
            if (evt.animatorClipInfo.weight > 0.5f)
            {
                int idx = Random.Range(0, m_playerStats.FootstepSounds.Length);
                AudioSource.PlayClipAtPoint(m_playerStats.FootstepSounds[idx],
                    transform.TransformPoint(m_controller.center),
                    m_playerStats.FootstepVolume);
            }
        }

        void OnLand(AnimationEvent evt)
        {
            if (m_playerStats.LandingSound && evt.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(m_playerStats.LandingSound,
                    transform.TransformPoint(m_controller.center),
                    m_playerStats.FootstepVolume);
            }
        }
    }
}
