using System.Collections.Generic;
using UnityEngine;

namespace Senses2026
{
    public class Updater : MonoBehaviour
    {
        public static Updater Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return null;
#endif

                if (m_isQuiting)
                    return null;

                if (instance == null)
                {
                    Updater existingInstance;

#if UNITY_6000_0_OR_NEWER

                    // Unity 6+
                    // FindFirstObjectByType is deprecated in newer Unity 6 versions.
                    // We do not care which Updater is found, so FindAnyObjectByType
                    // is the correct and faster replacement.
                    existingInstance = FindAnyObjectByType<Updater>();

#elif UNITY_2022_2_OR_NEWER

                    // Unity 2022.2 - Unity 2023.x
                    existingInstance = FindFirstObjectByType<Updater>();

#else

                    // Older Unity versions
                    existingInstance = FindObjectOfType<Updater>();

#endif

                    if (existingInstance == null)
                    {
                        GameObject obj = new GameObject("- Updater");
                        instance = obj.AddComponent<Updater>();
                        DontDestroyOnLoad(obj);
                    }
                    else
                    {
                        instance = existingInstance;
                    }
                }

                return instance;
            }

            set => instance = value;
        }

        private static Updater instance;

        // ----------------------------------------------
        // Update Queues
        // ----------------------------------------------
        private readonly List<IUpdate> UpdateQueue = new List<IUpdate>();
        private readonly List<IUpdate> UpdateAddQueue = new List<IUpdate>();
        private readonly List<IUpdate> UpdateRemovalQueue = new List<IUpdate>();

        // ----------------------------------------------
        // Load Spreading Settings
        // ----------------------------------------------
        [Header("Load Spreading Settings")]
        public bool m_useLoadSpread = true;

        [Tooltip("How many IUpdate objects to process per frame (only when load spreading is enabled).")]
        public int m_spreadBatchSize = 20;

        private int m_currentSpreadIndex = 0;

        static bool m_isQuiting = false;

        // ----------------------------------------------
        // Registration
        // ----------------------------------------------
        public void RegisterUpdate(IUpdate script)
        {
            UpdateAddQueue.Add(script);
        }

        public void UnegisterUpdate(IUpdate script)
        {
            UpdateRemovalQueue.Add(script);
        }

        // ----------------------------------------------
        // UNITY
        // ----------------------------------------------
        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            // --------------------------------------------
            // FAST ADD
            // --------------------------------------------
            if (UpdateAddQueue.Count > 0)
            {
                UpdateQueue.AddRange(UpdateAddQueue);
                UpdateAddQueue.Clear();
            }

            // --------------------------------------------
            // FAST REMOVE
            // --------------------------------------------
            if (UpdateRemovalQueue.Count > 0)
            {
                for (int i = 0; i < UpdateRemovalQueue.Count; i++)
                {
                    UpdateQueue.Remove(UpdateRemovalQueue[i]);
                }

                UpdateRemovalQueue.Clear();
            }

            int count = UpdateQueue.Count;

            if (count == 0)
                return;

            float dt = Time.deltaTime;

            // --------------------------------------------
            // NORMAL UPDATE MODE (NO SPREADING)
            // --------------------------------------------
            if (!m_useLoadSpread)
            {
                for (int i = 0; i < count; i++)
                {
                    IUpdate obj = UpdateQueue[i];

                    if (obj != null)
                        obj.PerformUpdate(dt);
                }

                return;
            }

            // --------------------------------------------
            // LOAD-SPREADING MODE
            // --------------------------------------------
            int batchCount = Mathf.Min(m_spreadBatchSize, count);

            for (int i = 0; i < batchCount; i++)
            {
                if (m_currentSpreadIndex >= count)
                    m_currentSpreadIndex = 0;

                IUpdate target = UpdateQueue[m_currentSpreadIndex];

                if (target != null)
                    target.PerformUpdate(dt);

                m_currentSpreadIndex++;
            }
        }

        void OnApplicationQuit()
        {
            m_isQuiting = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetQuitFlag()
        {
            m_isQuiting = false;
        }
    }
}