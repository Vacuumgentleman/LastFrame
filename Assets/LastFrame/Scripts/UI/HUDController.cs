using System;
using TMPro;
using UnityEngine;

namespace LastFrame.UI
{
    /// <summary>
    /// In-game HUD: health (3 hits, then death), current level, melee cooldown (Q) and ink ammo (max 3).
    /// Every icon swaps sprites by state. While a sprite slot is empty the icon falls back to a palette
    /// color plus a text label, so the HUD is readable before the final art is assigned.
    /// Gameplay systems call the public methods; nothing here reads input.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Health (top left)")]
        [SerializeField] UnityEngine.UI.Image m_healthIcon;
        [SerializeField] TMP_Text m_healthLabel;
        [Tooltip("Index 0 = 1 hit left, 1 = 2 hits left, 2 = 3 hits left (full).")]
        [SerializeField] Sprite[] m_healthSprites = new Sprite[3];
        [SerializeField] int m_maxHealth = 3;

        [Header("Level (top right)")]
        [SerializeField] TMP_Text m_levelLabel;
        [SerializeField] int m_currentLevel = 1;
        [SerializeField] int m_totalLevels = 5;

        [Header("Melee cooldown (bottom right)")]
        [Tooltip("Radial Filled image drawn over the melee icon; fill 1 = just used, 0 = ready.")]
        [SerializeField] UnityEngine.UI.Image m_meleeCooldownOverlay;
        [SerializeField] UnityEngine.UI.Image m_meleeIcon;
        [SerializeField] float m_meleeCooldown = 1.5f;

        [Header("Ammo (bottom right)")]
        [SerializeField] UnityEngine.UI.Image m_ammoIcon;
        [SerializeField] TMP_Text m_ammoLabel;
        [Tooltip("Index = ammo left: 0 = empty, 1, 2, 3 = full.")]
        [SerializeField] Sprite[] m_ammoSprites = new Sprite[4];
        [SerializeField] int m_maxAmmo = 3;

        [Header("Fallback colors (used while a sprite is missing)")]
        [SerializeField] Color[] m_healthColors = { new Color32(0x8A, 0x3B, 0x3A, 0xFF), new Color32(0xB8, 0x4E, 0x4C, 0xFF), new Color32(0xD2, 0x63, 0x61, 0xFF) };
        [SerializeField] Color[] m_ammoColors = { new Color32(0x30, 0x2A, 0x38, 0xFF), new Color32(0x45, 0x34, 0x4A, 0xFF), new Color32(0x50, 0x3A, 0x53, 0xFF), new Color32(0x59, 0x40, 0x5C, 0xFF) };
        [SerializeField] Color m_meleeReadyColor = new Color32(0xD5, 0xA3, 0x4F, 0xFF);
        [SerializeField] Color m_meleeCoolingColor = new Color32(0x66, 0x73, 0x6A, 0xFF);

        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        public event Action<int> OnAmmoChanged;

        public int Health { get; private set; }
        public int Ammo { get; private set; }
        public bool IsDead => Health <= 0;
        public bool MeleeReady => m_meleeTimer <= 0f;
        public float MeleeCooldownProgress => m_meleeCooldown <= 0f ? 0f : Mathf.Clamp01(m_meleeTimer / m_meleeCooldown);

        float m_meleeTimer;

        void Awake()
        {
            Health = m_maxHealth;
            Ammo = m_maxAmmo;
        }

        void Start()
        {
            RefreshHealth();
            RefreshAmmo();
            RefreshLevel();
            RefreshMelee();
        }

        void Update()
        {
            if (m_meleeTimer <= 0f) return;
            m_meleeTimer = Mathf.Max(0f, m_meleeTimer - Time.deltaTime);
            RefreshMelee();
        }

        // ---------- Health ----------

        public void TakeHit(int _amount = 1) => SetHealth(Health - _amount);
        public void Heal(int _amount = 1) => SetHealth(Health + _amount);

        public void SetHealth(int _value)
        {
            if (IsDead) return;
            Health = Mathf.Clamp(_value, 0, m_maxHealth);
            RefreshHealth();
            OnHealthChanged?.Invoke(Health);
            if (IsDead) OnDeath?.Invoke();
        }

        void RefreshHealth()
        {
            int _index = Mathf.Clamp(Health - 1, 0, m_healthSprites.Length - 1);
            ApplyState(m_healthIcon, m_healthLabel, m_healthSprites, _index, m_healthColors, IsDead ? "X" : Health.ToString());
            if (m_healthIcon != null && IsDead) m_healthIcon.color = new Color(1f, 1f, 1f, 0.25f);
        }

        // ---------- Level ----------

        public void SetLevel(int _current, int _total)
        {
            m_currentLevel = _current;
            m_totalLevels = _total;
            RefreshLevel();
        }

        public void NextLevel() => SetLevel(Mathf.Min(m_currentLevel + 1, m_totalLevels), m_totalLevels);

        void RefreshLevel()
        {
            if (m_levelLabel != null) m_levelLabel.text = $"{m_currentLevel}/{m_totalLevels}";
        }

        // ---------- Melee ----------

        /// <summary>Returns false (and does nothing) while the melee attack is cooling down.</summary>
        public bool TryUseMelee()
        {
            if (!MeleeReady) return false;
            m_meleeTimer = m_meleeCooldown;
            RefreshMelee();
            return true;
        }

        void RefreshMelee()
        {
            if (m_meleeCooldownOverlay != null) m_meleeCooldownOverlay.fillAmount = MeleeCooldownProgress;
            if (m_meleeIcon != null && m_meleeIcon.sprite == null) m_meleeIcon.color = MeleeReady ? m_meleeReadyColor : m_meleeCoolingColor;
        }

        // ---------- Ammo ----------

        /// <summary>Returns false when there is no ammo left.</summary>
        public bool TryUseAmmo()
        {
            if (Ammo <= 0) return false;
            SetAmmo(Ammo - 1);
            return true;
        }

        public void AddAmmo(int _amount = 1) => SetAmmo(Ammo + _amount);

        public void SetAmmo(int _value)
        {
            Ammo = Mathf.Clamp(_value, 0, m_maxAmmo);
            RefreshAmmo();
            OnAmmoChanged?.Invoke(Ammo);
        }

        void RefreshAmmo()
        {
            ApplyState(m_ammoIcon, m_ammoLabel, m_ammoSprites, Mathf.Clamp(Ammo, 0, m_ammoSprites.Length - 1), m_ammoColors, $"{Ammo}/{m_maxAmmo}");
        }

        // ---------- Shared ----------

        static void ApplyState(UnityEngine.UI.Image _icon, TMP_Text _label, Sprite[] _sprites, int _index, Color[] _fallbackColors, string _fallbackText)
        {
            Sprite _sprite = _sprites != null && _index >= 0 && _index < _sprites.Length ? _sprites[_index] : null;
            if (_icon != null)
            {
                _icon.sprite = _sprite;
                _icon.color = _sprite != null ? Color.white : _fallbackColors[Mathf.Clamp(_index, 0, _fallbackColors.Length - 1)];
            }
            if (_label != null)
            {
                _label.gameObject.SetActive(_sprite == null);
                _label.text = _fallbackText;
            }
        }
    }
}
