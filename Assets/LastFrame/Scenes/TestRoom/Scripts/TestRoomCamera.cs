using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SensesTestRoom
{
    /// <summary>
    /// Cámara cenital que sigue al jugador con la misma inclinación que las demos del Senses Pack.
    /// La rotación en Y se mantiene fija, así el movimiento WASD (relativo a cámara) es siempre igual.
    /// Rueda del ratón = zoom.
    /// </summary>
    public class TestRoomCamera : MonoBehaviour
    {
        public Transform Target;
        [Range(30f, 80f)] public float Pitch = 50.64f;
        public float Distance = 15f;
        public float MinDistance = 7f;
        public float MaxDistance = 32f;
        public float FollowSharpness = 6f;
        public Vector3 TargetOffset = new Vector3(0f, 1f, 2f);

        void LateUpdate()
        {
            if (Target == null) return;

            float _scroll = readScroll();
            if (Mathf.Abs(_scroll) > 0.01f)
            {
                Distance = Mathf.Clamp(Distance - _scroll * 1.5f, MinDistance, MaxDistance);
            }

            Vector3 _desired = desiredPosition();
            transform.position = Vector3.Lerp(transform.position, _desired, 1f - Mathf.Exp(-FollowSharpness * Time.unscaledDeltaTime));
            transform.rotation = Quaternion.Euler(Pitch, 0f, 0f);
        }

        public void Snap()
        {
            if (Target == null) return;
            transform.position = desiredPosition();
            transform.rotation = Quaternion.Euler(Pitch, 0f, 0f);
        }

        Vector3 desiredPosition()
        {
            Quaternion _rotation = Quaternion.Euler(Pitch, 0f, 0f);
            return Target.position + TargetOffset - _rotation * Vector3.forward * Distance;
        }

        static float readScroll()
        {
#if ENABLE_INPUT_SYSTEM
            Mouse _mouse = Mouse.current;
            return _mouse != null ? _mouse.scroll.ReadValue().y / 120f : 0f;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.mouseScrollDelta.y;
#else
            return 0f;
#endif
        }
    }
}
