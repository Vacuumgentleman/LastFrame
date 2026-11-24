using System.Collections.Generic;
using UnityEngine;

namespace Senses2026
{
    public class BulletQuene_DemoScript : MonoBehaviour
    {
        public static BulletQuene_DemoScript Instance;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }


        Queue<Bullet_DemoScript> m_bulletsQuene = new Queue<Bullet_DemoScript>();
        [SerializeField] Bullet_DemoScript m_bulletPrefab;
        public Bullet_DemoScript Provide_Bullet()
        {
            if (m_bulletsQuene.Count > 0) return m_bulletsQuene.Dequeue();
            else
            {
                Bullet_DemoScript _bullet_DemoScript = Instantiate(m_bulletPrefab, this.transform);
                return _bullet_DemoScript;
            }
        }
        public void ReturnToQuene(Bullet_DemoScript _bullet_DemoScript)
        {
            m_bulletsQuene.Enqueue(_bullet_DemoScript);
        }
    }
}