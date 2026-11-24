using UnityEngine;

namespace Senses2026
{
    public class MinionTowerHealth_DemoScript : MonoBehaviour
    {
        [SerializeField] int m_health;
        MinionTower_DemoScript m_controler;
        FactionNames m_faction;
        public FactionNames Faction => m_faction;
        bool m_wasEnabled = false;

        void Awake()
        {
            enable();
        }

        void enable()
        {
            m_controler = GetComponent<MinionTower_DemoScript>();
            m_faction = GetComponent<TargetSenses>().FactionName;

            m_wasEnabled = true;
        }

        public void Damage(int _damage)
        {
            if (!m_wasEnabled) enable();

            m_health -= _damage;
            if (m_health <= 0)
            {
                m_health = 0;
                dead();
            }
        }

        void dead()
        {
            m_controler.Destroyed();
        }

    }

}
