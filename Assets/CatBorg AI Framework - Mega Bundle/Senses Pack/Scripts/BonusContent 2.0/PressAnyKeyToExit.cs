using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Senses2026
{
    public class PressAnyKeyToExit : MonoBehaviour
    {
        void Update()
        {
            if (SensesDemoInput.AnyKeyDown)
            {
                ExitGame();
            }
        }

        void ExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
                Application.Quit(); // 🔥 Quit application in build
#endif
        }
    }
}
