using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Senses2026
{
    public class MinionSpawner_DemoScript : MonoBehaviour
    {
        [SerializeField] MinionControler_DemoScript m_minionPrefab;
        [SerializeField][Range(1f, 5f)] float m_spawnerCooldown;
        float m_previousSpawnerCooldown;
        bool m_isCoolingDown = false;
        [SerializeField] Waypoint_DemoScene[] m_waypoints;
        [SerializeField] TextMeshProUGUI m_amountOfMinions_Label;
        int m_amountOfMinions;
        IntervalEnumerator m_spawnCooldown;

        [SerializeField] FactionNames m_faction;
        public FactionNames Faction => m_faction;
        [SerializeField] GameObject m_winGameCanvas;

        MinionControler_DemoScript m_lastSpawned;
        [SerializeField] float m_requiredSeparation = 1f;

        void Awake()
        {
            m_previousSpawnerCooldown = m_spawnerCooldown;
            StartCoroutine(spawn());
        }

        void Update()
        {
            if (m_isCoolingDown) return;
            if (m_previousSpawnerCooldown != m_spawnerCooldown)
            {
                m_spawnCooldown.ChangeInterval(m_spawnerCooldown);
                m_previousSpawnerCooldown = m_spawnerCooldown;
            }

            if (m_lastSpawned != null && Vector3.Distance(m_lastSpawned.transform.position, this.transform.position) < m_requiredSeparation)
            {
                m_isCoolingDown = true;
                return;
            }

            m_lastSpawned = provide_Minion();
            m_lastSpawned.transform.position = this.transform.position;
            m_lastSpawned.Enable(m_waypoints, this);
            m_amountOfMinions++;
            displayAmoutOfMinions();
            m_isCoolingDown = true;
        }


        IEnumerator spawn()
        {
            m_spawnCooldown = new IntervalEnumerator(m_spawnerCooldown, spawn_Action);
            while (true)
            {
                m_spawnCooldown.MoveNext(Time.deltaTime);
                yield return null;
            }
        }
        void spawn_Action()
        {
            m_isCoolingDown = false;
        }


        Queue<MinionControler_DemoScript> m_minionsQuene = new Queue<MinionControler_DemoScript>();

        MinionControler_DemoScript provide_Minion()
        {
            if (m_minionsQuene.Count > 0) return m_minionsQuene.Dequeue();
            else
            {
                MinionControler_DemoScript _minionControler_DemoScript = Instantiate(m_minionPrefab, this.transform, false);
                return _minionControler_DemoScript;
            }
        }

        public void ReturnToQuene(MinionControler_DemoScript _minionControler_DemoScript, bool _withReward = false)
        {
            _minionControler_DemoScript.transform.position = this.transform.position;
            m_minionsQuene.Enqueue(_minionControler_DemoScript);
            m_amountOfMinions--;
            displayAmoutOfMinions();
            if (_withReward)
            {
                m_spawnerCooldown = m_spawnerCooldown * .75f;
                if (m_spawnerCooldown < .2f) m_spawnerCooldown = .2f;
                if (m_previousSpawnerCooldown != m_spawnerCooldown)
                {
                    m_spawnCooldown.ChangeInterval(m_spawnerCooldown);
                    m_previousSpawnerCooldown = m_spawnerCooldown;
                }
            }
        }


        void displayAmoutOfMinions()
        {
            m_amountOfMinions_Label.SetText(m_amountOfMinions.ToString());
        }

        public void ProvideKillReward()
        {
            m_spawnerCooldown = m_spawnerCooldown * .75f;
            if (m_spawnerCooldown < .1f) m_spawnerCooldown = .1f;
            if (m_previousSpawnerCooldown != m_spawnerCooldown)
            {
                m_spawnCooldown.ChangeInterval(m_spawnerCooldown);
                m_previousSpawnerCooldown = m_spawnerCooldown;
            }
        }

        public void EnableWinCanvas()
        {
            Time.timeScale = 0f;
            m_winGameCanvas.SetActive(true);
        }
    }
}