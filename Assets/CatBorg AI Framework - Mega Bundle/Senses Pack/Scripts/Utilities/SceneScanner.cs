#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

namespace Senses2026
{
    /// <summary>
    /// SceneScanner automatically scans the scene when opened in the Unity Editor.
    /// - Runs only if the project is using the Built-In Render Pipeline.
    /// - Removes missing script components from all GameObjects in the scene.
    /// - Includes inactive GameObjects.
    /// - Skips execution if URP or HDRP is detected.
    /// - Deactivates itself after execution instead of destroying itself.
    /// </summary>
    [ExecuteInEditMode]
    public class SceneScanner : MonoBehaviour
    {
        private bool isTaskComplete = false;

        void Awake()
        {
            if (Application.isPlaying)
                return;

            // Built-In Render Pipeline has no current RenderPipelineAsset.
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                Debug.Log(
                    "Skipping scene scan: Project is not using the Built-In Render Pipeline."
                );

                isTaskComplete = true;
                SafeDeactivate();
                return;
            }

            Debug.Log(
                $"Scene '{SceneManager.GetActiveScene().name}' opened! Running scene scan..."
            );

            RemoveMissingScripts();

            isTaskComplete = true;
            SafeDeactivate();
        }

        void RemoveMissingScripts()
        {
            GameObject[] allObjects;

#if UNITY_6000_5_OR_NEWER

            // Unity 6.5+
            // FindObjectsSortMode is deprecated in newer Unity versions.
            allObjects = FindObjectsByType<GameObject>(
                FindObjectsInactive.Include
            );

#elif UNITY_2022_2_OR_NEWER

            // Unity 2022.2 - Unity 6.4
            allObjects = FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

#else

            // Older Unity versions
            allObjects = FindObjectsOfType<GameObject>(true);

#endif

            int removedCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj == null)
                    continue;

                removedCount +=
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            }

            if (removedCount > 0)
            {
                Debug.Log(
                    $"Scene scan complete. Removed {removedCount} missing script(s)."
                );
            }
        }

        void SafeDeactivate()
        {
            EditorApplication.delayCall += () =>
            {
                if (!isTaskComplete)
                    return;

                if (this == null || gameObject == null)
                    return;

                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
            };
        }
    }
}

#endif