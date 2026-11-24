using UnityEngine;
using UnityEngine.UI;

namespace Senses2026
{
    public class DistractorsManger : MonoBehaviour
    {
        [SerializeField] LayerMask m_groundLayer; // Assign the ground layer in Inspector
        [SerializeField] Transform m_ghostObject;
        [SerializeField] Vector3 m_targetPosition;

        bool m_state = false;
        bool m_distractorAvaible = true;
        [SerializeField] Button m_button;

        void Update()
        {
            if (!m_state || !m_distractorAvaible) return;
            moveGhostObjectToMouse();
        }

        void moveGhostObjectToMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(SensesDemoInput.MousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_groundLayer))
            {
                m_targetPosition = hit.point;
                m_targetPosition.y = m_ghostObject.position.y; // Keep object at same height
                m_ghostObject.position = m_targetPosition;
            }

            if (SensesDemoInput.GetMouseButton(0)) // Left mouse button
            {
                //drop item
                m_distractorAvaible = false;
                m_ghostObject = null;
                m_button.interactable = false;
            }

            if (SensesDemoInput.GetMouseButtonDown(1))
            {
                ChangeDDistractorState();
            }
        }

        public void ChangeDDistractorState()
        {
            m_state = !m_state;
            if (m_distractorAvaible) m_ghostObject.gameObject.SetActive(m_state);
            m_button.interactable = !m_state;
        }
    }

}
