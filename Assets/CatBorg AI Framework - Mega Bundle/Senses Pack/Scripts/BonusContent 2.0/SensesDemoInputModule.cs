using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Senses2026
{
    /// <summary>
    /// Selects the UI input backend on the EventSystem used by a Senses demo.
    /// Existing legacy navigation settings remain serialized for old-input projects.
    /// </summary>
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/UI/Senses Demo Input Module")]
    public class SensesDemoInputModule : StandaloneInputModule
    {
        protected override void OnEnable()
        {
            base.OnEnable();
#if ENABLE_INPUT_SYSTEM
            // Disable before EventSystem can poll legacy Input APIs.
            enabled = false;
            if (!TryGetComponent(out InputSystemUIInputModule _module))
            {
                _module = gameObject.AddComponent<InputSystemUIInputModule>();
                _module.moveRepeatDelay = repeatDelay;
                _module.moveRepeatRate = 1f / Mathf.Max(inputActionsPerSecond, 0.01f);
            }
#endif
        }
    }
}
