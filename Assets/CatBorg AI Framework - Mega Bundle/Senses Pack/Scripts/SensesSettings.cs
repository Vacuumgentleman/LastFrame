using UnityEngine;
using System.Text;

namespace Senses2026
{
    public class SensesSettings : MonoBehaviour
    {
        static SensesSettings instance;
        public static SensesSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("- SensesSettings");
                    instance = obj.AddComponent<SensesSettings>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        [SerializeField] SoundPropagationSettings m_soundSettings;

        public SoundPropagationSettings SoundSettings => m_soundSettings;
        bool[] m_tagValid;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (m_soundSettings == null)
            {
                m_soundSettings = Resources.Load<SoundPropagationSettings>("SoundPropagationSettings");
                if (m_soundSettings == null)
                {
                    Debug.LogError("<color=red>ERROR:</color> SoundPropagationSettings asset not found in Resources!");
                    return;
                }
            }

            // Validate tags and store results
            m_tagValid = new bool[m_soundSettings.SoundData.Length];

            StringBuilder logBuilder = new StringBuilder();

            for (int i = 0; i < m_tagValid.Length; i++)
            {
                string tagToCheck = m_soundSettings.SoundData[i].TAG;
                m_tagValid[i] = IsTagValid(tagToCheck);

                if (!m_tagValid[i])
                {
                    logBuilder.Append("<color=yellow>WARNING:</color> Tag '<color=red>");
                    logBuilder.Append(tagToCheck);
                    logBuilder.Append("</color>' does not exist in the project. Check the Tags in ");
                    logBuilder.Append(m_soundSettings.name);
                    logBuilder.AppendLine(".");
                    logBuilder.Append("Sorry for the errors in the console, but as of 2025, Unity does not provide a way to validate whether tags are valid in a way that compiles and works outside of the editor. I might be wrong—just human :)");
                }
            }

            // Print all warnings at once (better performance)
            if (logBuilder.Length > 0)
            {
                Debug.LogWarning(logBuilder.ToString());
            }
        }

        public bool ProvidePropagationFactor(Collider _collider, out float _factor)
        {
            _factor = 1f; // Default value

            for (int i = 0; i < m_soundSettings.SoundData.Length; i++)
            {
                if (m_tagValid[i] && _collider.CompareTag(m_soundSettings.SoundData[i].TAG))
                {
                    _factor = m_soundSettings.SoundData[i].Factor;
                    return true;
                }
            }

            return false;
        }

        private bool IsTagValid(string tag)
        {
            GameObject temp = new GameObject();

            try
            {
                temp.tag = tag;
                return temp.CompareTag(tag); // Will throw an error if tag is invalid
            }
            catch
            {
                return false;
            }
            finally
            {
                Destroy(temp); // Clean up temp object
            }
        }
    }
}
