using UnityEngine;

namespace Senses2026
{
    public class MinionHealth_DemoScript : MonoBehaviour
    {
        [SerializeField] int m_health;
        int m_initialHealth;
        MinionControler_DemoScript m_controler;
        Rigidbody m_rigidBody;
        bool m_wasEnabled = false;
        Vector3 m_forceVector;
        FactionNames m_ownFaction;
        public FactionNames Faction => m_ownFaction;
        void enable()
        {
            m_controler = GetComponent<MinionControler_DemoScript>();
            m_rigidBody = GetComponent<Rigidbody>();
            m_ownFaction = GetComponent<TargetSenses>().FactionName;

            m_initialHealth = m_health;
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

        public void Damage(int _damage, TargetSenses _targetSenses)
        {
            if (!m_wasEnabled) enable();

            m_controler.LastAttackignUnityWas(_targetSenses);
            m_health -= _damage;
            if (m_health <= 0)
            {
                m_health = 0;
                dead();
            }
        }

        void dead()
        {
            m_controler.Dead();
        }

        public void Reset()
        {
            if (!m_wasEnabled) enable();
            m_health = m_initialHealth;
        }
    }
}
