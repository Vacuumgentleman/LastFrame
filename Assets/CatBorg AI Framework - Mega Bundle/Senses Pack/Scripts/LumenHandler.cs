using System.Collections.Generic;

namespace Senses2026
{
    public sealed class LumenHandler
    {
        private static volatile LumenHandler instance;
        private static object syncRoot = new System.Object();
        public static LumenHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new LumenHandler();
                    }
                }

                return instance;
            }
        }

        List<Stealth> m_lumen_StealthAddons = new List<Stealth>();
        LightDetector m_lightDetector;
        int m_currentIndex;
        int m_nextIndex;
        bool m_wasEnabled = false;

        public bool WasEnabled => m_wasEnabled;
        public void Enable(LightDetector _lightDetector)
        {
            m_lightDetector = _lightDetector;
            m_wasEnabled = true;
        }
        public void Register(Stealth _stealth)
        {
            m_lumen_StealthAddons.Add(_stealth);
        }
        public void Remove(Stealth _stealth)
        {
            m_lumen_StealthAddons.Remove(_stealth);
        }


        public Stealth GetNext_Stealth()
        {
            if (m_lumen_StealthAddons.Count == 0) return null;
            m_currentIndex = m_nextIndex;
            m_nextIndex++;
            if (m_nextIndex > m_lumen_StealthAddons.Count - 1) m_nextIndex = 0;
            if (m_currentIndex > m_lumen_StealthAddons.Count - 1) m_currentIndex = 0;

            return m_lumen_StealthAddons[m_currentIndex];
        }
    }
}