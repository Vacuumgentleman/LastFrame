using UnityEngine;

namespace Senses2026
{
    public class PlayerControler_MK1 : MonoBehaviour
    {
        float m_velocity_X;
        float m_velocity_Z;

        float m_previous_velocity_X;
        float m_previous_velocity_Z;

        [SerializeField] float m_acceleration = 5f;
        [SerializeField] Animator m_animator;
        [SerializeField] Stealth m_stealth;
        [SerializeField] bool m_inverDirectons = false;
        [SerializeField] bool m_useAdvanceSettings = false;
        [SerializeField] LayerMask m_groundLayerMask;

        bool m_shouldRun = false;
        bool m_shouldGoForward = false;
        bool m_shouldGoBackwards = false;
        bool m_shouldGoRight = false;
        bool m_shouldGoLeft = false;
        bool m_shouldBeStealth = false;
        bool m_previous_shouldBeStealth;

        int m_Velocity_X_HashValue;
        int m_Velocity_Z_HashValue;
        int m_isStealth_HashValue;

        int m_lasSelectedValue = 0;
        [SerializeField] int m_enablingValue = 0;
        [SerializeField] Transform m_playerSprite;

        Rigidbody m_rigidbody;
        bool m_hasRigidbody = false;
        Vector3 m_delta;

        void Awake()
        {
            m_Velocity_X_HashValue = Animator.StringToHash("Velocity_X");
            m_Velocity_Z_HashValue = Animator.StringToHash("Velocity_Z");
            m_isStealth_HashValue = Animator.StringToHash("isStealth");
            m_stealth.enabled = m_shouldBeStealth;
            m_hasRigidbody = TryGetComponent<Rigidbody>(out m_rigidbody);
        }

        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(SensesDemoInput.MousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, m_groundLayerMask))
            {
                if (!m_useAdvanceSettings || m_lasSelectedValue == m_enablingValue && hit.collider.CompareTag("Ground"))
                {
                    Vector3 cameraToPlayerOffset = hit.point - transform.position;

                    // Define the maximum values for X and Z offsets (these should be set based on your needs)
                    float maxOffsetX = 5f; // Replace with your max X value
                    float maxOffsetZ = 5f; // Replace with your max Z value

                    // Normalize the offsets by dividing by the maximum values and clamping them between 0 and 1
                    float _offsetX = Mathf.Clamp(cameraToPlayerOffset.x / maxOffsetX, -1f, 1f);
                    float _offsetZ = Mathf.Clamp(cameraToPlayerOffset.z / maxOffsetZ, -1f, 1f);

                    // Apply the normalized offsets to the player's position
                    m_playerSprite.position = new Vector3(transform.position.x + _offsetX, transform.position.y + .02f, transform.position.z + _offsetZ);

                    if (cameraToPlayerOffset.x * cameraToPlayerOffset.x >= 0.01f || cameraToPlayerOffset.z + cameraToPlayerOffset.z >= 0.01f)
                    {
                        transform.LookAt(new Vector3(m_playerSprite.position.x, 0f, m_playerSprite.position.z));
                        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
                    }

                }
            }


            if (!m_useAdvanceSettings)
            {
                m_shouldRun = SensesDemoInput.GetKey(KeyCode.LeftShift);
                m_shouldBeStealth = SensesDemoInput.GetKey(KeyCode.LeftControl);

                m_shouldGoForward = m_inverDirectons ? SensesDemoInput.GetKey(KeyCode.W) : SensesDemoInput.GetKey(KeyCode.S);
                m_shouldGoBackwards = m_inverDirectons ? SensesDemoInput.GetKey(KeyCode.S) : SensesDemoInput.GetKey(KeyCode.W);
                m_shouldGoRight = m_inverDirectons ? SensesDemoInput.GetKey(KeyCode.D) : SensesDemoInput.GetKey(KeyCode.A);
                m_shouldGoLeft = m_inverDirectons ? SensesDemoInput.GetKey(KeyCode.A) : SensesDemoInput.GetKey(KeyCode.D);
            }
            else
            {
                if (SensesDemoInput.GetKey(KeyCode.Alpha1))
                {
                    m_lasSelectedValue = 0;
                }
                if (SensesDemoInput.GetKey(KeyCode.Alpha2))
                {
                    m_lasSelectedValue = 1;
                }
                if (SensesDemoInput.GetKey(KeyCode.Alpha3))
                {
                    m_lasSelectedValue = 2;
                }

                if (m_lasSelectedValue == m_enablingValue)
                {
                    m_shouldRun = SensesDemoInput.GetKey(KeyCode.LeftShift);
                    m_shouldBeStealth = SensesDemoInput.GetKey(KeyCode.LeftControl);

                    m_shouldGoForward = m_inverDirectons ? SensesDemoInput.GetKey(KeyCode.W) : SensesDemoInput.GetKey(KeyCode.S);
                    m_shouldGoBackwards = m_inverDirectons ? SensesDemoInput.GetKey(KeyCode.S) : SensesDemoInput.GetKey(KeyCode.W);
                    m_shouldGoRight = m_inverDirectons ? SensesDemoInput.GetKey(KeyCode.D) : SensesDemoInput.GetKey(KeyCode.A);
                    m_shouldGoLeft = m_inverDirectons ? SensesDemoInput.GetKey(KeyCode.A) : SensesDemoInput.GetKey(KeyCode.D);
                }
                else
                {
                    m_shouldRun = false;
                    m_shouldBeStealth = false;

                    m_shouldGoForward = false;
                    m_shouldGoBackwards = false;
                    m_shouldGoRight = false;
                    m_shouldGoLeft = false;
                }

            }

            if (m_shouldGoForward)
            {
                if (m_shouldRun)
                {
                    if (m_velocity_Z < 2f) m_velocity_Z += Time.deltaTime * m_acceleration;
                }
                else if (m_velocity_Z < .5f) m_velocity_Z += Time.deltaTime * m_acceleration;
            }
            if (m_shouldGoBackwards)
            {
                if (m_shouldRun)
                {
                    if (m_velocity_Z > -2f) m_velocity_Z -= Time.deltaTime * m_acceleration;
                }
                else if (m_velocity_Z > -.5f) m_velocity_Z -= Time.deltaTime * m_acceleration;
            }

            if (m_shouldGoRight)
            {
                if (m_shouldRun)
                {
                    if (m_velocity_X < 2f) m_velocity_X += Time.deltaTime * m_acceleration;
                }
                else if (m_velocity_X < .5f) m_velocity_X += Time.deltaTime * m_acceleration;
            }
            if (m_shouldGoLeft)
            {
                if (m_shouldRun)
                {
                    if (m_velocity_X > -2f) m_velocity_X -= Time.deltaTime * m_acceleration;
                }
                else if (m_velocity_X > -.5f) m_velocity_X -= Time.deltaTime * m_acceleration;
            }

            if (!m_shouldGoForward && m_velocity_Z > 0)
            {
                m_velocity_Z -= Time.deltaTime * m_acceleration;
                if (m_velocity_Z < 0) m_velocity_Z = 0;
            }

            if (!m_shouldGoBackwards && m_velocity_Z < 0)
            {
                m_velocity_Z += Time.deltaTime * m_acceleration;
                if (m_velocity_Z > 0) m_velocity_Z = 0;
            }

            if (!m_shouldGoRight && !m_shouldGoLeft)
            {
                if (m_velocity_X > 0)
                {
                    m_velocity_X -= Time.deltaTime * m_acceleration;
                    if (m_velocity_X < 0) m_velocity_X = 0;
                }
                else
                {
                    m_velocity_X += Time.deltaTime * m_acceleration;
                    if (m_velocity_X > 0) m_velocity_X = 0;
                }



            }
        }

        void FixedUpdate()
        {
            if (m_hasRigidbody)
            {
                m_rigidbody.MovePosition(transform.position + m_delta);
                m_delta = Vector3.zero;
            }


            if (m_previous_velocity_X != m_velocity_X)
            {
                m_previous_velocity_X = m_velocity_X;
                m_animator.SetFloat(m_Velocity_X_HashValue, m_velocity_X);
            }

            if (m_previous_velocity_Z != m_velocity_Z)
            {
                m_previous_velocity_Z = m_velocity_Z;
                m_animator.SetFloat(m_Velocity_Z_HashValue, m_velocity_Z);
            }

            if (!m_shouldRun)
            {
                if (m_previous_shouldBeStealth != m_shouldBeStealth)
                {
                    m_previous_shouldBeStealth = m_shouldBeStealth;
                    m_animator.SetBool(m_isStealth_HashValue, m_shouldBeStealth);
                    m_stealth.enabled = m_shouldBeStealth;
                }
            }
            else
            {
                if (m_previous_shouldBeStealth != m_shouldBeStealth && !m_shouldBeStealth)
                {
                    m_previous_shouldBeStealth = m_shouldBeStealth;
                    m_animator.SetBool(m_isStealth_HashValue, m_shouldBeStealth);
                }
            }
        }

        void OnAnimatorMove()
        {
            if (m_hasRigidbody) m_delta = m_animator.rootPosition - transform.position;
            else transform.position = m_animator.rootPosition;
        }
    }
}


