using UnityEngine;

namespace Senses2026BonusContent
{
    public class Billboard : MonoBehaviour
    {
        Camera m_mainCamera;

        void Awake()
        {
            // Get the main camera
            m_mainCamera = Camera.main;
        }

        void LateUpdate()
        {
            // Make the object face the camera
            if (m_mainCamera != null)
            {
                transform.LookAt(transform.position + m_mainCamera.transform.rotation * Vector3.forward,
                                 m_mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
}
