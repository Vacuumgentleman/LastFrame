using CatBorgStudio.Movement;
using Senses2026;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Senses2026
{
    public class DemoManager : MonoBehaviour
    {
        bool m_playerPressedStart;
        bool m_playerPressedEnd;
        bool m_escaped;
        bool m_dead;

        [SerializeField] CanvasGroup m_startGameCanvas;
        [SerializeField] CanvasGroup m_activeGameCanvas;
        [SerializeField] CanvasGroup m_deadCanvas;
        [SerializeField] CanvasGroup m_escapedCanvas;

        [SerializeField] PlayerControler_MK2 m_playerControler;
        [SerializeField] PlayerMover_MK2 m_playerMover;

        void Awake()
        {
            m_activeGameCanvas.alpha = 0f;
            m_activeGameCanvas.blocksRaycasts = false;

            m_deadCanvas.alpha = 0f;
            m_deadCanvas.blocksRaycasts = false;

            m_escapedCanvas.alpha = 0f;
            m_escapedCanvas.blocksRaycasts = false;

            m_playerControler.enabled = false;
        }

        void Start()
        {
            StartCoroutine(Game());
        }

        IEnumerator Game()
        {
            yield return StartCoroutine(StartOfGame());
            yield return StartCoroutine(MainGame());
            yield return StartCoroutine(EndGame());
        }

        IEnumerator StartOfGame()
        {
            m_startGameCanvas.alpha = 1.0f;
            m_startGameCanvas.blocksRaycasts = true;
            m_playerControler.enabled = false;
            m_playerMover.enabled = false;

            // Wait until player presses start
            while (!m_playerPressedStart)
            {
                yield return null;
            }

            m_startGameCanvas.alpha = 0f;
            m_startGameCanvas.blocksRaycasts = false;
        }

        IEnumerator MainGame()
        {
            m_activeGameCanvas.alpha = 1.0f;
            m_activeGameCanvas.blocksRaycasts = true;
            m_playerControler.enabled = true;
            m_playerMover.enabled = true;

            while (!m_dead && !m_escaped)
            {
                yield return null;
            }

            m_activeGameCanvas.alpha = 0f;
            m_activeGameCanvas.blocksRaycasts = false;
            m_playerControler.enabled = false;
            m_playerMover.enabled = false;
        }

        IEnumerator EndGame()
        {
            if (m_escaped)
            {
                m_escapedCanvas.alpha = 1.0f;
                m_escapedCanvas.blocksRaycasts = true;
            }
            else if (m_dead)
            {
                m_deadCanvas.alpha = 1.0f;
                m_deadCanvas.blocksRaycasts = true;
            }

            // Wait for player to acknowledge end screen
            while (!m_playerPressedEnd)
            {
                yield return null;
            }

            Debug.Log("Game over screen acknowledged. Ready for restart or exit.");
        }

        // --- External event triggers ---
        public void PlayerStartGame() => m_playerPressedStart = true;
        public void PlayerEndGame() => m_playerPressedEnd = true;
        public void PlayerEscaped() => m_escaped = true;
        public void PlayerWasKilled() => m_dead = true;
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

}
