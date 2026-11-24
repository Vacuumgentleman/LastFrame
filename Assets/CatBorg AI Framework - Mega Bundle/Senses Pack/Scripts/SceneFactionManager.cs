namespace Senses2026
{
    using UnityEngine;

    public class SceneFactionManager : MonoBehaviour
    {
        const string m_defaultFactionSettingsPath = "FactionSettings (SO)/DefaultFactionSettings"; // Resources path (no .asset)

        public FactionSettings FactionSettings;

        public static SceneFactionManager Instance
        {
            get
            {
                // Lazy initialization
                if (m_instance == null)
                {
#if UNITY_6000_0_OR_NEWER

                    // Unity 6+
                    m_instance = Object.FindAnyObjectByType<SceneFactionManager>();

#elif UNITY_2022_2_OR_NEWER

            // Unity 2022.2 - Unity 2023.x
            m_instance = Object.FindFirstObjectByType<SceneFactionManager>();

#else

            // Older Unity versions
            m_instance = Object.FindObjectOfType<SceneFactionManager>();

#endif

                    if (m_instance == null)
                    {
                        // Auto-create GameObject
                        GameObject go = new GameObject(nameof(SceneFactionManager));
                        m_instance = go.AddComponent<SceneFactionManager>();

                        Debug.Log(
                            $"[{nameof(SceneFactionManager)}] Created automatically in scene: {go.scene.name}"
                        );
                    }

                    // Ensure FactionSettings loaded
                    if (m_instance.FactionSettings == null)
                    {
                        m_instance.FactionSettings =
                            Resources.Load<FactionSettings>(m_defaultFactionSettingsPath);

                        if (m_instance.FactionSettings == null)
                        {
                            Debug.LogError(
                                $"[{nameof(SceneFactionManager)}] Failed to load default FactionSettings from Resources/{m_defaultFactionSettingsPath}"
                            );
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(SceneFactionManager)}] Loaded default settings from Resources/{m_defaultFactionSettingsPath}"
                            );
                        }
                    }
                }

                return m_instance;
            }
        }
        static SceneFactionManager m_instance;

        void Awake()
        {
            if (m_instance != null && m_instance != this)
            {
                Debug.LogWarning($"Duplicate {nameof(SceneFactionManager)} found and destroyed in {gameObject.scene.name}");
                Destroy(gameObject);
                return;
            }

            m_instance = this;
        }

        void OnDestroy()
        {
            if (m_instance == this)
                m_instance = null;
        }

        public static string GetFactionName(FactionNames faction)
        {
            if (Instance == null)
            {
                Debug.LogError($"[{nameof(SceneFactionManager)}] No instance available.");
                return faction.ToString();
            }

            if (Instance.FactionSettings == null)
            {
                Debug.LogError($"[{nameof(SceneFactionManager)}] No FactionSettings assigned.");
                return faction.ToString();
            }

            return Instance.FactionSettings.GetFactionDisplayName(faction);
        }
        public int GetFactionCount()
        {
            if (Instance.FactionSettings == null)
            {
                Debug.LogError($"[{nameof(SceneFactionManager)}] No FactionSettings assigned.");
                return 0;
            }

            return Instance.FactionSettings.factionDisplayNames.Length;
        }
    }



}

