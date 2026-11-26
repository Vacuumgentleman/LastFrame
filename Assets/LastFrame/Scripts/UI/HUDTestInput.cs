using UnityEngine;
using UnityEngine.InputSystem;

namespace LastFrame.UI
{
    /// <summary>
    /// TEMPORARY: drives the HUD from the keyboard until the real combat/health systems exist.
    /// Q = melee (starts the cooldown), Left click = ink shot (uses ammo), H = take a hit,
    /// R = refill ammo, N = next level. Remove this component once gameplay calls HUDController directly.
    /// </summary>
    public class HUDTestInput : MonoBehaviour
    {
        [SerializeField] HUDController m_hud;

        void Awake()
        {
            if (m_hud == null) m_hud = FindAnyObjectByType<HUDController>();
            if (m_hud != null) m_hud.OnDeath += () => Debug.Log("[HUD] Player erased (0 hearts): game over.");
        }

        void Update()
        {
            if (m_hud == null) return;
            Keyboard _kb = Keyboard.current;
            Mouse _mouse = Mouse.current;

            if (_kb != null && _kb.qKey.wasPressedThisFrame) m_hud.TryUseMelee();
            if (_mouse != null && _mouse.leftButton.wasPressedThisFrame) m_hud.TryUseAmmo();
            if (_kb != null && _kb.hKey.wasPressedThisFrame) m_hud.TakeHit();
            if (_kb != null && _kb.rKey.wasPressedThisFrame) m_hud.SetAmmo(3);
            if (_kb != null && _kb.nKey.wasPressedThisFrame) m_hud.NextLevel();
        }
    }
}
