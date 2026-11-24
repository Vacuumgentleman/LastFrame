using System.Collections.Generic;
using UnityEngine;

namespace Senses2026
{
    public class ScenNodePoll : MonoBehaviour
    {
        public static ScenNodePoll Instance
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
                    ScenNodePoll existing;

#if UNITY_6000_0_OR_NEWER

                    existing = FindAnyObjectByType<ScenNodePoll>();

#elif UNITY_2022_2_OR_NEWER

            existing = FindFirstObjectByType<ScenNodePoll>();

#else

            existing = FindObjectOfType<ScenNodePoll>();

#endif

                    if (existing == null)
                    {
                        // Auto-create the poll object
                        GameObject obj = new GameObject("- ScentNodePoll (part of Senses Asset)");
                        instance = obj.AddComponent<ScenNodePoll>();
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
        static ScenNodePoll instance;
        static bool m_isQuitting = false;

        Transform m_transform;
        Queue<ScentNode> m_pool = new Queue<ScentNode>();

        GameObject m_generatedPrefab;
        int m_releaseCounter = 0;

        void Awake()
        {
            Instance = this;
            m_transform = transform;

            CreateRuntimePrefab();
        }

        void CreateRuntimePrefab()
        {
            // Create object
            m_generatedPrefab = new GameObject("-- ScentNode (part of Senses Asset)");

            // Default configuration
            m_generatedPrefab.tag = "Untagged";
            m_generatedPrefab.layer = 0; // Default layer

            // Add sphere collider
            SphereCollider col = m_generatedPrefab.AddComponent<SphereCollider>();
            col.isTrigger = true;

            // Add the ScentNode component
            m_generatedPrefab.AddComponent<ScentNode>();

            // Disable default instance template
            m_generatedPrefab.SetActive(false);
        }

        public ScentNode GetNext_ScentNode()
        {
            if (m_generatedPrefab == null)
            {
                CreateRuntimePrefab();
            }

            if (m_pool.Count > 0)
            {
                return m_pool.Dequeue();
            }
            else
            {
                // Create new node instance
                GameObject obj = Instantiate(m_generatedPrefab, m_transform);
                obj.name = $"-- ScentNode (part of Senses Asset) [{m_releaseCounter}]";

                ScentNode newNode = obj.GetComponent<ScentNode>();
                newNode.Assign_ID(m_releaseCounter);

                m_releaseCounter++;
                return newNode;
            }
        }

        public void ReturnToPool(ScentNode node)
        {
            if (node == null) return;

            node.gameObject.SetActive(false);
            m_pool.Enqueue(node);
        }

        void OnApplicationQuit()
        {
            m_isQuitting = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetQuitFlag()
        {
            instance = null;
            m_isQuitting = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            m_isQuitting = false;
        }


    }
}
