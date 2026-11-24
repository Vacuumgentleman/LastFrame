using System.Collections.Generic;
using UnityEngine;

namespace Senses2026
{
    public class ScenVisualisatorsPoll : MonoBehaviour
    {
        public static ScenVisualisatorsPoll Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return null;
#endif

                if (m_isQuitting)
                    return null;

                if (instance == null)
                {
                    ScenVisualisatorsPoll existing;

#if UNITY_6000_0_OR_NEWER

                    existing = FindAnyObjectByType<ScenVisualisatorsPoll>();

#elif UNITY_2022_2_OR_NEWER

            existing = FindFirstObjectByType<ScenVisualisatorsPoll>();

#else

            existing = FindObjectOfType<ScenVisualisatorsPoll>();

#endif

                    if (existing == null)
                    {
                        GameObject obj = new GameObject("- ScenVisualisatorsPoll (part of Senses Asset)");
                        instance = obj.AddComponent<ScenVisualisatorsPoll>();
                        DontDestroyOnLoad(obj);
                    }
                    else
                    {
                        instance = existing;
                    }
                }

                return instance;
            }

            private set => instance = value;
        }
        private static ScenVisualisatorsPoll instance;
        private static bool m_isQuitting = false;

        //=====================================================
        // DATA
        //=====================================================
        Transform m_transform;
        Queue<ScentVisualisation> m_pool = new Queue<ScentVisualisation>();

        ScentVisualisation m_loadedPrefab;   // Loaded from Resources
        const string PREFAB_NAME = "- ScentVisualisation MK2 (part of Senses Asset)";

        int m_createdCounter = 0;

        void Awake()
        {
            Instance = this;
            m_transform = transform;

            LoadPrefabFromResources();
        }

        void LoadPrefabFromResources()
        {
            m_loadedPrefab = Resources.Load<ScentVisualisation>(PREFAB_NAME);

            if (m_loadedPrefab != null)
            {
                Debug.Log(
                    $"[ScenVisualisatorsPoll] Loaded visualisation prefab:\n" +
                    $" → Name: \"{PREFAB_NAME}\"\n" +
                    $" → From: Resources folder\n" +
                    $" → Using as runtime template.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    $"[ScenVisualisatorsPoll] FAILED to load ScentVisualisation prefab!\n" +
                    $" → Expected at: Resources/{PREFAB_NAME}.prefab\n" +
                    $" → System will continue but visualisation WILL NOT WORK.\n\n" +
                    $"**How to fix:**\n" +
                    $" • Ensure prefab exists in a Resources folder.\n" +
                    $" • Name must match EXACTLY:\n" +
                    $"     \"{PREFAB_NAME}\"",
                    this
                );

                // SAFETY FALLBACK — avoid crashes
                m_loadedPrefab = CreateFallbackObject();
            }
        }

        ScentVisualisation CreateFallbackObject()
        {
            GameObject obj = new GameObject("-- Fallback ScentVisualisation (INVALID)");
            obj.tag = "Untagged";
            obj.layer = 0;

            var vis = obj.AddComponent<ScentVisualisation>();

            // Deactivate, this is only used to prevent null crashes
            obj.SetActive(false);

            return vis;
        }

        public ScentVisualisation GetNext_ScentNode()
        {
            if (m_loadedPrefab == null)
            {
                Debug.LogError("[ScenVisualisatorsPoll] No valid prefab available!", this);
                return null;
            }

            if (m_pool.Count > 0)
            {
                return m_pool.Dequeue();
            }
            else
            {
                ScentVisualisation newVis =
                    Instantiate(m_loadedPrefab, m_transform);

                newVis.gameObject.name =
                    $"-- ScentVisualisation (part of Senses Asset) [{m_createdCounter++}]";

                return newVis;
            }
        }

        public void ReturnToPool(ScentVisualisation vis)
        {
            if (vis == null) return;

            vis.gameObject.SetActive(false);
            m_pool.Enqueue(vis);
        }

        void OnApplicationQuit()
        {
            m_isQuitting = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetQuitFlag()
        {
            m_isQuitting = false;
        }
    }
}
