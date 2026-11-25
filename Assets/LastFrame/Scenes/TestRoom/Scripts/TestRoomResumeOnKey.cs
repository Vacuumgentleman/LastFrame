using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SensesTestRoom
{
    /// <summary>
    /// Sustituye a PressAnyKeyToExit en los paneles de victoria de la demo de facciones:
    /// en vez de cerrar el Play Mode, reanuda el tiempo y oculta el panel.
    /// </summary>
    public class TestRoomResumeOnKey : MonoBehaviour
    {
        float m_enabledAt;

        void OnEnable() => m_enabledAt = Time.unscaledTime;

        void Update()
        {
            if (Time.unscaledTime - m_enabledAt < 1f) return;
            if (!anyKeyDown()) return;

            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }

        static bool anyKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                   (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.anyKeyDown;
#else
            return false;
#endif
        }
    }
}
