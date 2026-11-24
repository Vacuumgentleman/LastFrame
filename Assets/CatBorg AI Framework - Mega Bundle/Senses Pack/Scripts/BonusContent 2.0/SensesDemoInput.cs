using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Senses2026
{
    /// <summary>
    /// Input compatibility for the keyboard and mouse controls used by Senses demos.
    /// Uses the new Input System when enabled, including Active Input Handling = Both.
    /// The legacy branch does not require the Input System package.
    /// </summary>
    public static class SensesDemoInput
    {
        public static bool UsesNewInputSystem
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return true;
#else
                return false;
#endif
            }
        }

        public static Vector3 MousePosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Mouse _mouse = Mouse.current;
                return _mouse != null ? (Vector3)_mouse.position.ReadValue() : Vector3.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.mousePosition;
#else
                return Vector3.zero;
#endif
            }
        }

        public static bool AnyKeyDown
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Keyboard _keyboard = Keyboard.current;
                if (_keyboard != null && _keyboard.anyKey.wasPressedThisFrame)
                {
                    return true;
                }

                Mouse _mouse = Mouse.current;
                if (_mouse != null && (_mouse.leftButton.wasPressedThisFrame || _mouse.rightButton.wasPressedThisFrame || _mouse.middleButton.wasPressedThisFrame || _mouse.forwardButton.wasPressedThisFrame || _mouse.backButton.wasPressedThisFrame))
                {
                    return true;
                }

                foreach (Gamepad _gamepad in Gamepad.all)
                {
                    foreach (InputControl _control in _gamepad.allControls)
                    {
                        if (_control is ButtonControl _button && !_button.synthetic && _button.wasPressedThisFrame)
                        {
                            return true;
                        }
                    }
                }

                return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.anyKeyDown;
#else
                return false;
#endif
            }
        }

        public static bool GetKey(KeyCode _key)
        {
#if ENABLE_INPUT_SYSTEM
            KeyControl _control = GetKeyControl(_key);
            return _control != null && _control.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(_key);
#else
            return false;
#endif
        }

        public static bool GetKeyDown(KeyCode _key)
        {
#if ENABLE_INPUT_SYSTEM
            KeyControl _control = GetKeyControl(_key);
            return _control != null && _control.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(_key);
#else
            return false;
#endif
        }

        public static bool GetMouseButton(int _button)
        {
#if ENABLE_INPUT_SYSTEM
            ButtonControl _control = GetMouseButtonControl(_button);
            return _control != null && _control.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(_button);
#else
            return false;
#endif
        }

        public static bool GetMouseButtonDown(int _button)
        {
#if ENABLE_INPUT_SYSTEM
            ButtonControl _control = GetMouseButtonControl(_button);
            return _control != null && _control.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(_button);
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        // Only the keys used by the bundled demos are mapped here.
        private static KeyControl GetKeyControl(KeyCode _key)
        {
            Keyboard _keyboard = Keyboard.current;
            if (_keyboard == null)
            {
                return null;
            }

            switch (_key)
            {
                case KeyCode.W:
                    return _keyboard.wKey;
                case KeyCode.A:
                    return _keyboard.aKey;
                case KeyCode.S:
                    return _keyboard.sKey;
                case KeyCode.D:
                    return _keyboard.dKey;
                case KeyCode.UpArrow:
                    return _keyboard.upArrowKey;
                case KeyCode.DownArrow:
                    return _keyboard.downArrowKey;
                case KeyCode.LeftArrow:
                    return _keyboard.leftArrowKey;
                case KeyCode.RightArrow:
                    return _keyboard.rightArrowKey;
                case KeyCode.LeftShift:
                    return _keyboard.leftShiftKey;
                case KeyCode.RightShift:
                    return _keyboard.rightShiftKey;
                case KeyCode.LeftControl:
                    return _keyboard.leftCtrlKey;
                case KeyCode.RightControl:
                    return _keyboard.rightCtrlKey;
                case KeyCode.Space:
                    return _keyboard.spaceKey;
                case KeyCode.Escape:
                    return _keyboard.escapeKey;
                case KeyCode.Alpha1:
                    return _keyboard.digit1Key;
                case KeyCode.Alpha2:
                    return _keyboard.digit2Key;
                case KeyCode.Alpha3:
                    return _keyboard.digit3Key;
                default:
                    return null;
            }
        }

        private static ButtonControl GetMouseButtonControl(int _button)
        {
            Mouse _mouse = Mouse.current;
            if (_mouse == null)
            {
                return null;
            }

            switch (_button)
            {
                case 0:
                    return _mouse.leftButton;
                case 1:
                    return _mouse.rightButton;
                case 2:
                    return _mouse.middleButton;
                case 3:
                    return _mouse.backButton;
                case 4:
                    return _mouse.forwardButton;
                default:
                    return null;
            }
        }
#endif
    }
}
