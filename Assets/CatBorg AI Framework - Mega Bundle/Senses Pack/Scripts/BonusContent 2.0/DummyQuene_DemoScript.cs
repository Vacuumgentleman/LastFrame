using System.Collections.Generic;
using UnityEngine;

namespace Senses2026
{
    public class DummyQuene_DemoScript : MonoBehaviour
    {
        public static DummyQuene_DemoScript Instance;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            m_deadMinionsQuene[0] = new Queue<MinionDead_DemoScript>();
            m_deadMinionsQuene[1] = new Queue<MinionDead_DemoScript>();

        }


        Queue<ArrowDummy_DemoScript> m_bulletsQuene = new Queue<ArrowDummy_DemoScript>();
        [SerializeField] ArrowDummy_DemoScript m_bulletPrefab;
        public ArrowDummy_DemoScript Provide_Bullet()
        {
            if (m_bulletsQuene.Count > 0) return m_bulletsQuene.Dequeue();
            else
            {
                ArrowDummy_DemoScript _bullet_DemoScript = Instantiate(m_bulletPrefab, this.transform);
                return _bullet_DemoScript;
            }
        }
        public void ReturnToQuene(ArrowDummy_DemoScript _bullet_DemoScript)
        {

            m_bulletsQuene.Enqueue(_bullet_DemoScript);

        }


        Queue<MinionDead_DemoScript>[] m_deadMinionsQuene = new Queue<MinionDead_DemoScript>[2];
        [SerializeField] MinionDead_DemoScript[] m_deadMinionPrefab;
        public MinionDead_DemoScript Provide_DeadMinion(int _index)
        {
            if (m_deadMinionsQuene[_index].Count > 0) return m_deadMinionsQuene[_index].Dequeue();
            else
            {
                MinionDead_DemoScript _deadMInion_DemoScript = Instantiate(m_deadMinionPrefab[_index], this.transform);
                return _deadMInion_DemoScript;
            }
        }
        public void ReturnToQuene(MinionDead_DemoScript _deadMInion_DemoScript, int _idnex)
        {
            m_deadMinionsQuene[_idnex].Enqueue(_deadMInion_DemoScript);
        }

    }

}


