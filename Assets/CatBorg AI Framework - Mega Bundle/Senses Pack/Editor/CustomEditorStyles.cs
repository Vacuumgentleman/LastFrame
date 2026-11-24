using UnityEngine;

namespace Senses2026
{
    public static class CustomEditorStyles
    {
        private static GUIStyle _noMarginWindow;

        public static GUIStyle NoMarginWindow
        {
            get
            {
                if (_noMarginWindow == null)
                {
                    _noMarginWindow = new GUIStyle(GUI.skin.window)
                    {
                        margin = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(5, 5, 5, 5)
                    };
                }
                return _noMarginWindow;
            }
        }
    }
}

