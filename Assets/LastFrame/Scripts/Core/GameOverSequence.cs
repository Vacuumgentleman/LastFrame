using System; // Action delegates for events
using System.Collections; // IEnumerator for coroutines
using System.Collections.Generic; // List of enemies
using LastFrame.Enemies; // EnemyAI, EnemyState
using LastFrame.Player; // PlayerHealth
using TMPro; // TextMeshProUGUI
using UnityEngine; // MonoBehaviour, Time
using UnityEngine.InputSystem; // Keyboard, Mouse, Gamepad (skip)
using UnityEngine.SceneManagement; // SceneManager
using UnityEngine.UI; // Canvas, Image, CanvasScaler

namespace LastFrame.Core // Shared scripts of Last Frame
{
    /// <summary>
    /// Game over when Ollie runs out of hearts (issue #9, no time limit for now).
    /// Sequence: Ollie fades out (erased) -> a frame of the AI cartoons erasing her -> her score -> back to the start of the game.
    /// The UI is built at runtime as a placeholder; the frame sprite and texts are Inspector fields for the final art (UI/art team).
    /// Score is provisional until the results screen and S-E rating exist (#19): time, times detected, enemies erased.
    /// </summary>
    public class GameOverSequence : MonoBehaviour // Lose flow controller
    {
        [Header("Source")]
        [SerializeField] PlayerHealth m_player; // Empty = the first PlayerHealth in the scene

        [Header("1. Erase")]
        [SerializeField] float m_eraseDuration = 1f; // Seconds Ollie takes to fade out
        [Tooltip("Sprites that fade out. Empty = every SpriteRenderer under the player.")]
        [SerializeField] SpriteRenderer[] m_ollieSprites; // Ollie's drawing

        [Header("2. Frame (enemies erasing Ollie)")]
        [SerializeField] Sprite m_erasedFrame; // Final art (empty = placeholder text)
        [SerializeField] string m_frameTitle = "BORRADA"; // Title over the frame
        [SerializeField, TextArea] string m_framePlaceholder = "[ Cuadro provisional ]\nLas caricaturas de IA borran a Ollie con sus borradores."; // Shown while there is no art
        [SerializeField] float m_frameDuration = 3f; // Seconds on screen (skippable)

        [Header("3. Score (provisional until #19)")]
        [SerializeField] string m_scoreTitle = "PUNTAJE"; // Title of the score screen
        [SerializeField] float m_scoreDuration = 6f; // Seconds on screen (skippable)

        [Header("4. Back to the start")]
        [Tooltip("Scene of the start of the game (e.g. 01_MainMenu). It must be in Build Settings. Empty or not loadable = reload this scene (prototype).")]
        [SerializeField] string m_startScene = ""; // Start of the game
        [SerializeField] string m_continueHint = "Pulsa cualquier tecla para continuar"; // Footer text
        [SerializeField] float m_minTimeBeforeSkip = 0.5f; // Avoids skipping by accident with the key that was held

        [Header("Look (placeholder)")]
        [SerializeField] Color m_background = new Color(0.04f, 0.04f, 0.07f, 0.96f); // Ink black
        [SerializeField] Color m_textColor = new Color(0.95f, 0.93f, 0.88f); // Paper white
        [SerializeField] int m_sortingOrder = 100; // Above the HUD

        readonly List<EnemyAI> m_enemies = new List<EnemyAI>(); // Enemies watched for the score
        readonly Dictionary<EnemyAI, EnemyState> m_lastStates = new Dictionary<EnemyAI, EnemyState>(); // Previous state per enemy
        float m_startTime; // Time the level started (for the score)
        int m_timesDetected; // Times an enemy started chasing
        bool m_running; // True while the sequence plays

        Canvas m_canvas; // Runtime canvas
        Image m_frameImage; // Frame art
        TextMeshProUGUI m_title, m_body, m_footer; // Texts

        public int TimesDetected => m_timesDetected; // For other systems (results #19)
        public event Action OnGameOverStarted; // Ollie was erased (music, sound)
        public event Action OnGameOverFinished; // Right before leaving the scene

        // Finds the player and the enemies and starts counting.
        void Start()
        {
            if (!m_player) m_player = FindFirstObjectByType<PlayerHealth>(); // Default player
            if (m_player) m_player.OnDied += HandleDied; // Listen for 0 hearts
            else Debug.LogWarning("[GameOverSequence] No PlayerHealth in the scene.", this); // Nothing to watch
            m_enemies.AddRange(FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)); // Enemies for the score
            foreach (var e in m_enemies) m_lastStates[e] = e.State; // Initial states
            m_startTime = Time.time; // Level start
        }

        // Stops listening.
        void OnDestroy()
        {
            if (m_player) m_player.OnDied -= HandleDied; // Unhook
            if (m_running) Time.timeScale = 1f; // Never leave the game frozen
        }

        // Counts every time an enemy starts chasing Ollie.
        void Update()
        {
            if (m_running) return; // Score is frozen
            foreach (var e in m_enemies) // Each enemy
            {
                if (!e) continue; // Destroyed
                EnemyState now = e.State; // Current state
                if (now == EnemyState.Chase && m_lastStates[e] != EnemyState.Chase) m_timesDetected++; // New detection
                m_lastStates[e] = now; // Remember
            }
        }

        // 0 hearts: plays the lose sequence once.
        void HandleDied()
        {
            if (m_running) return; // Already playing
            m_running = true; // Lock
            StartCoroutine(Sequence()); // Go
        }

        // Erase -> frame -> score -> start of the game.
        IEnumerator Sequence()
        {
            OnGameOverStarted?.Invoke(); // Listeners
            float survived = Time.time - m_startTime; // Time alive in this level
            int erased = 0; // Enemies erased by Ollie
            foreach (var e in m_enemies) if (e && e.State == EnemyState.Dead) erased++; // Count
            yield return FadeOllie(); // 1. She disappears
            Time.timeScale = 0f; // Freeze the level behind the screens
            BuildUI(); // Runtime placeholder UI

            ShowFrame(); // 2. Enemies erasing Ollie
            yield return WaitOrSkip(m_frameDuration); // Hold

            ShowScore(survived, erased); // 3. Score
            yield return WaitOrSkip(m_scoreDuration); // Hold

            OnGameOverFinished?.Invoke(); // Listeners
            Time.timeScale = 1f; // Unfreeze before loading
            LoadStart(); // 4. Start of the game
        }

        // Fades Ollie's sprites to transparent (she is erased).
        IEnumerator FadeOllie()
        {
            if (m_ollieSprites == null || m_ollieSprites.Length == 0) m_ollieSprites = m_player.GetComponentsInChildren<SpriteRenderer>(); // Default sprites
            var start = new Color[m_ollieSprites.Length]; // Original colors
            for (int i = 0; i < m_ollieSprites.Length; i++) if (m_ollieSprites[i]) start[i] = m_ollieSprites[i].color; // Save
            for (float t = 0f; t < m_eraseDuration; t += Time.unscaledDeltaTime) // Fade loop
            {
                float a = 1f - t / m_eraseDuration; // 1 -> 0
                for (int i = 0; i < m_ollieSprites.Length; i++) if (m_ollieSprites[i]) m_ollieSprites[i].color = new Color(start[i].r, start[i].g, start[i].b, start[i].a * a); // Apply
                yield return null; // Next frame
            }
            foreach (var s in m_ollieSprites) if (s) s.enabled = false; // Gone
        }

        // Waits the given seconds (unscaled) or until any key / click / button after a short delay.
        IEnumerator WaitOrSkip(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime) // Timer
            {
                if (t >= m_minTimeBeforeSkip && SkipPressed()) yield break; // Player skipped
                yield return null; // Next frame
            }
        }

        // True on any key, mouse click or gamepad button this frame.
        static bool SkipPressed()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true; // Keyboard
            if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)) return true; // Mouse
            if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)) return true; // Gamepad
            return false; // Nothing
        }

        // Shows the frame art, or the placeholder text while there is no art.
        void ShowFrame()
        {
            m_title.text = m_frameTitle; // "BORRADA"
            m_frameImage.sprite = m_erasedFrame; // Art (may be null)
            m_frameImage.enabled = m_erasedFrame; // Only with art
            m_body.text = m_erasedFrame ? "" : m_framePlaceholder; // Placeholder text
            m_footer.text = m_continueHint; // Skip hint
        }

        // Shows the provisional score.
        void ShowScore(float survived, int erased)
        {
            m_frameImage.enabled = false; // Hide the frame
            int minutes = Mathf.FloorToInt(survived / 60f); // mm
            int seconds = Mathf.FloorToInt(survived % 60f); // ss
            m_title.text = m_scoreTitle; // "PUNTAJE"
            m_body.text = $"Tiempo: {minutes:00}:{seconds:00}\nVeces detectada: {m_timesDetected}\nEnemigos borrados: {erased}\n\n<size=60%>(Provisional: la calificación S-E llega con la pantalla de resultados, #19)</size>"; // Lines
            m_footer.text = m_continueHint; // Skip hint
        }

        // Loads the start of the game, or reloads this scene while the menu is not in Build Settings.
        void LoadStart()
        {
            if (!string.IsNullOrEmpty(m_startScene) && Application.CanStreamedLevelBeLoaded(m_startScene)) { SceneManager.LoadScene(m_startScene); return; } // Real start
            if (!string.IsNullOrEmpty(m_startScene)) Debug.LogWarning($"[GameOverSequence] Scene '{m_startScene}' is not in Build Settings. Reloading this scene.", this); // Explain fallback
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex >= 0 ? SceneManager.GetActiveScene().name : SceneManager.GetActiveScene().path); // Prototype restart
        }

        // Builds a full-screen overlay: background, frame image and three texts.
        void BuildUI()
        {
            if (m_canvas) { m_canvas.gameObject.SetActive(true); return; } // Already built
            var root = new GameObject("GameOverUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); // Canvas object
            root.transform.SetParent(transform, false); // Keep the hierarchy tidy
            m_canvas = root.GetComponent<Canvas>(); // Canvas
            m_canvas.renderMode = RenderMode.ScreenSpaceOverlay; // On top of everything
            m_canvas.sortingOrder = m_sortingOrder; // Above the HUD
            var scaler = root.GetComponent<CanvasScaler>(); // Resolution independence
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // Scale with screen
            scaler.referenceResolution = new Vector2(1920, 1080); // Reference size

            var bg = NewRect("Background", root.transform, Vector2.zero, Vector2.one); // Full screen
            bg.gameObject.AddComponent<Image>().color = m_background; // Ink black

            m_frameImage = NewRect("Frame", root.transform, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.8f)).gameObject.AddComponent<Image>(); // Center area
            m_frameImage.preserveAspect = true; // Do not stretch the art
            m_frameImage.enabled = false; // Until there is art

            m_title = NewText("Title", root.transform, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f), 96, FontStyles.Bold); // Top
            m_body = NewText("Body", root.transform, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.8f), 48, FontStyles.Normal); // Middle
            m_footer = NewText("Footer", root.transform, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.12f), 32, FontStyles.Italic); // Bottom
        }

        // Creates a stretched RectTransform between two anchors.
        static RectTransform NewRect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform)); // UI object
            var rt = (RectTransform)go.transform; // Its rect
            rt.SetParent(parent, false); // Under the canvas
            rt.anchorMin = min; rt.anchorMax = max; // Anchors
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; // Fill the anchors
            return rt; // Ready
        }

        // Creates a centered TextMeshPro text in an anchored area.
        TextMeshProUGUI NewText(string name, Transform parent, Vector2 min, Vector2 max, float size, FontStyles style)
        {
            var text = NewRect(name, parent, min, max).gameObject.AddComponent<TextMeshProUGUI>(); // Text component
            text.alignment = TextAlignmentOptions.Center; // Centered
            text.fontSize = size; // Size
            text.fontStyle = style; // Style
            text.color = m_textColor; // Paper white
            text.raycastTarget = false; // No clicks
            return text; // Ready
        }
    }
}
