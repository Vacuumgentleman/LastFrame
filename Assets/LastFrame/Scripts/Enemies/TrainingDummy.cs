using System.Collections; // IEnumerator for coroutines
using LastFrame.Core; // IDamageable, IAlertable, DamageInfo
using UnityEngine; // MonoBehaviour, Renderer, MaterialPropertyBlock

namespace LastFrame.Enemies // Enemy scripts of Last Frame
{
    /// <summary>
    /// Test target for the weapons until the real enemies (#6) exist. Flat 2D drawing like the real enemies.
    /// 2 health: first ink hit stains it, second dissolves it. A backstab kills it in one hit.
    /// Respawns after a delay so you can keep testing.
    /// </summary>
    public class TrainingDummy : MonoBehaviour, IDamageable, IAlertable // Test target
    {
        [SerializeField] int m_maxHealth = 2; // GDD: enemies take 2 hits
        [Tooltip("Tick to test the 'already seen' case: the corrector then needs 2 hits.")]
        [SerializeField] bool m_isAlerted; // Simulates an enemy that saw Ollie
        [SerializeField] float m_respawnDelay = 3f; // Seconds before it comes back
        [SerializeField] Color m_normalColor = new Color(1f, 0.85f, 0.2f); // Bright AI yellow
        [SerializeField] Color m_alertedColor = new Color(1f, 0.25f, 0.25f); // Red when alerted
        [SerializeField] Color m_stainedColor = new Color(0.15f, 0.15f, 0.45f); // Ink blue when stained

        Renderer[] m_renderers; // Meshes to tint
        MaterialPropertyBlock m_block; // Tints without creating new materials
        int m_health; // Current health
        Vector3 m_startScale; // Scale before dissolving

        public bool IsAlerted => m_isAlerted; // IAlertable

        // Caches renderers and sets full health.
        void Awake()
        {
            m_renderers = GetComponentsInChildren<Renderer>(); // All meshes
            m_block = new MaterialPropertyBlock(); // Reusable property block
            m_startScale = transform.localScale; // Normal size
            ResetDummy(); // Full health and color
        }

        // Updates the color if the alerted box is changed in the Inspector during play.
        void OnValidate()
        {
            if (Application.isPlaying && m_block != null && m_health == m_maxHealth) Tint(m_isAlerted ? m_alertedColor : m_normalColor); // Live preview
        }

        // Receives a hit from Ollie's weapons.
        public void TakeDamage(DamageInfo info)
        {
            if (m_health <= 0) return; // Already dissolved
            m_health = info.InstantKill ? 0 : m_health - info.Amount; // Apply damage
            Debug.Log($"[TrainingDummy] {info.Type} hit. Kill={info.InstantKill} Silent={info.Silent} Health={m_health}", this); // Debug feedback

            if (m_health <= 0) StartCoroutine(DissolveAndRespawn()); // Dead: dissolve
            else Tint(m_stainedColor); // Alive: stained with ink
        }

        // Shrinks the dummy like wet paper, hides it, then brings it back.
        IEnumerator DissolveAndRespawn()
        {
            float t = 0f; // Animation time
            while (t < 0.4f) // Short shrink
            {
                t += Time.deltaTime; // Advance time
                transform.localScale = Vector3.Lerp(m_startScale, new Vector3(m_startScale.x * 1.3f, 0.05f, m_startScale.z * 1.3f), t / 0.4f); // Flatten like a puddle
                yield return null; // Next frame
            }
            SetVisible(false); // Hide
            yield return new WaitForSeconds(m_respawnDelay); // Wait
            ResetDummy(); // Back to normal
            SetVisible(true); // Show
        }

        // Restores health, size and color.
        void ResetDummy()
        {
            m_health = m_maxHealth; // Full health
            transform.localScale = m_startScale; // Normal size
            Tint(m_isAlerted ? m_alertedColor : m_normalColor); // Normal color
        }

        // Shows or hides renderers and colliders.
        void SetVisible(bool visible)
        {
            foreach (var r in m_renderers) r.enabled = visible; // Meshes
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = visible; // Collisions
        }

        // Applies a color to every renderer without leaking materials.
        void Tint(Color color)
        {
            if (m_renderers == null) return; // Not ready
            foreach (var r in m_renderers) // Every mesh or sprite
            {
                if (r is SpriteRenderer sprite) { sprite.color = color; continue; } // 2D drawings use the sprite color
                r.GetPropertyBlock(m_block); // Read current block
                m_block.SetColor("_BaseColor", color); // URP Lit color
                m_block.SetColor("_Color", color); // Built-in / other shaders
                r.SetPropertyBlock(m_block); // Apply
            }
        }
    }
}
