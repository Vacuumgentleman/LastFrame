using CatBorgStudio.Movement;
using System.Collections;
using UnityEngine;

namespace Senses2026BonusContent
{
    public class RagdolActivator : MonoBehaviour
    {
        PlayerMover_MK2 m_controller;
        Animator m_animator;
        Rigidbody[] m_ragdollRigidBodies;
        Collider[] m_colliders;
        CharacterController m_characterController;

        void Awake()
        {
            m_controller = GetComponent<PlayerMover_MK2>();
            m_animator = GetComponent<Animator>();
            m_ragdollRigidBodies = GetComponentsInChildren<Rigidbody>();
            m_colliders = GetComponentsInChildren<Collider>();
            m_characterController = GetComponent<CharacterController>();

            foreach (var rb in m_ragdollRigidBodies)
            {
                rb.isKinematic = true;
            }
            foreach (var col in m_colliders)
            {
                if (col != m_characterController) col.enabled = false;
            }
        }

        public void ActivateRagdol()
        {
            StartCoroutine(ActivateRagdollCoroutine());
        }

        IEnumerator ActivateRagdollCoroutine()
        {
            m_controller.enabled = false;
            m_characterController.enabled = false;
            yield return null; // wait one frame to let transforms settle

            foreach (var rb in m_ragdollRigidBodies)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            foreach (var col in m_colliders)
            {
                if (col != m_characterController) col.enabled = true;
            }

            m_animator.enabled = false;
        }
    }
}

