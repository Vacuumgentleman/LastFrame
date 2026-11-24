using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Player/BC 0: Player Controler MK2", 0)]
    public class PlayerControler_MK2 : MonoBehaviour
    {
        Vector2 m_moveVector = Vector2.zero;
        public Vector2 MoveVector => m_moveVector;

        bool m_sprints;
        public bool Sprints => m_sprints;
        bool m_stealth;
        public bool m_Stealth => m_stealth;
        bool m_jump;
        public bool m_Jump => m_jump;


        void Update()
        {
            m_moveVector = Vector3.zero;

            if (SensesDemoInput.GetKey(KeyCode.UpArrow) || SensesDemoInput.GetKey(KeyCode.W))
            {
                m_moveVector.y = 1;
            }

            if (SensesDemoInput.GetKey(KeyCode.DownArrow) || SensesDemoInput.GetKey(KeyCode.S))
            {
                m_moveVector.y = -1;
            }

            if (SensesDemoInput.GetKey(KeyCode.LeftArrow) || SensesDemoInput.GetKey(KeyCode.A))
            {
                m_moveVector.x = -1;
            }

            if (SensesDemoInput.GetKey(KeyCode.RightArrow) || SensesDemoInput.GetKey(KeyCode.D))
            {
                m_moveVector.x = 1;
            }

            m_sprints = SensesDemoInput.GetKey(KeyCode.RightShift) || SensesDemoInput.GetKey(KeyCode.LeftShift);
            m_stealth = SensesDemoInput.GetKey(KeyCode.RightControl) || SensesDemoInput.GetKey(KeyCode.LeftControl);
            m_jump = SensesDemoInput.GetKeyDown(KeyCode.Space);
        }

        public void SetJump(bool _state) => m_jump = _state;
        public void SetStealth(bool _state) => m_stealth = _state;

    }
}
