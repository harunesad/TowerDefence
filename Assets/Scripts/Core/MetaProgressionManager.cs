using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TowerDefence.Data;
using TowerDefence.Core.Security;

namespace TowerDefence.Core
{
    [System.Serializable]
    public class LevelProgress
    {
        public string levelID;
        public int difficultyLevel = 1; // 1: Normal, 2: Hard, 3: Expert
        public int stars;
        public bool isCompleted;
    }

    [System.Serializable]
    public class HeroProgressEntry
    {
        public string heroID;
        public int level = 1;
    }

    [System.Serializable]
    public class SaveData
    {
        public int totalKarma;
        public int totalCrystals; // Yeni ikinci para birimi
        public long lastDailyRewardTime; // Son günlük ödül zamanı (Ticks)
        public int dailyRewardDayIndex; // 0-30 arası, kaçıncı günde olduğu (0=henüz başlamamış, 30=tamamlandı)
        public List<string> unlockedSkillIDs = new List<string>();
        public List<string> unlockedSpellIDs = new List<string>(); // Açılan aktif büyüler
        public List<string> equippedSpellIDs = new List<string>(); // Kuşatılan büyüler
        public List<string> unlockedHeroIDs = new List<string>();
        public List<string> equippedHeroIDs = new List<string>(); // Maksimum 2
        public List<string> equippedUnitNames = new List<string>(); // Maksimum 5
        // Taraf bazli loadout kayitlari (Light ve Dark ayri saklanir)
        public List<string> lightEquippedHeroIDs = new List<string>();
        public List<string> darkEquippedHeroIDs = new List<string>();
        public List<string> lightEquippedUnitNames = new List<string>();
        public List<string> darkEquippedUnitNames = new List<string>();
        public List<string> lightEquippedSpellIDs = new List<string>();
        public List<string> darkEquippedSpellIDs = new List<string>();
        public List<HeroProgressEntry> heroProgressList = new List<HeroProgressEntry>();
        
        // Geriye dönük uyumluluk için tutuluyor
        public int highestUnlockedLevelIndex = 0;
        
        // Yeni Zorluk Sistemi Değişkenleri
        public int highestUnlockedGlobalDifficulty = 1;
        public List<int> highestUnlockedLevelIndexPerDifficulty = new List<int> { 0, 0, 0, 0 }; // İndeksler: 1=Normal, 2=Hard, 3=Expert
        
        public List<LevelProgress> levelProgressList = new List<LevelProgress>();
        public long lastFortuneWheelTime; // Son çark çevirme zamanı (Ticks)
        public long lastFortuneWheelAdTime; // Son reklamla çark çevirme zamanı (Ticks)
    }

    public class MetaProgressionManager : MonoBehaviour
    {
        public static MetaProgressionManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private SaveData saveData = new SaveData();
        
        private string savePath;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                savePath = Path.Combine(Application.persistentDataPath, "meta_progression.json");
                LoadGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            EnsureStarterHeroUnlocked();
        }

        [Header("Config")]
        [SerializeField] private List<SkillNodeData> allAvailableSkills;
        [SerializeField] private List<HeroData> allAvailableHeroes;

        public event System.Action OnHeroProgressChanged;

        [Header("Skill Tree Limits")]
        [SerializeField] private int maxSkillsUnlocked = 10; // Maksimum açılabilir skill sayısı

        public int GetMaxSkillsUnlocked() => maxSkillsUnlocked;
        public int GetCurrentSkillsUnlocked() => saveData.unlockedSkillIDs.Count;

        public void SetMaxSkillsUnlocked(int value) => maxSkillsUnlocked = value;

        public void AddKarma(int amount)
        {
            saveData.totalKarma += amount;
            SaveGame();
        }

        public void AddCrystals(int amount)
        {
            saveData.totalCrystals += amount;
            SaveGame();
        }

        public int GetTotalCrystals() => saveData.totalCrystals;

        public long GetLastFortuneWheelTime() => saveData.lastFortuneWheelTime;

        public void SetLastFortuneWheelTime(long ticks)
        {
            saveData.lastFortuneWheelTime = ticks;
            SaveGame();
        }

        public long GetLastFortuneWheelAdTime() => saveData.lastFortuneWheelAdTime;

        public void SetLastFortuneWheelAdTime(long ticks)
        {
            saveData.lastFortuneWheelAdTime = ticks;
            SaveGame();
        }

        public long GetLastDailyRewardTime() => saveData.lastDailyRewardTime;

        public void SetLastDailyRewardTime(long ticks)
        {
            saveData.lastDailyRewardTime = ticks;
            SaveGame();
        }

        public int GetDailyRewardDayIndex() => saveData.dailyRewardDayIndex;

        public void SetDailyRewardDayIndex(int index)
        {
            saveData.dailyRewardDayIndex = index;
            SaveGame();
        }

        public bool TryUnlockSkill(SkillNodeData skill)
        {
            if (saveData.totalKarma >= skill.karmaCost && saveData.totalCrystals >= skill.crystalCost && !IsSkillUnlocked(skill.skillID))
            {
                // Önkoşul kontrolü
                foreach (var req in skill.requiredSkills)
                {
                    if (!IsSkillUnlocked(req.skillID)) return false;
                }

                // Sınır kontrolü: Maksimum skill sayısı kontrolü
                if (saveData.unlockedSkillIDs.Count >= maxSkillsUnlocked && maxSkillsUnlocked > 0)
                {
                    Debug.LogWarning($"[Progression] Skill limit reached! Maximum {maxSkillsUnlocked} skills can be unlocked. Current: {saveData.unlockedSkillIDs.Count}");
                    return false;
                }

                saveData.totalKarma -= skill.karmaCost;
                saveData.totalCrystals -= skill.crystalCost;
                saveData.unlockedSkillIDs.Add(skill.skillID);

                // Eğer bu yetenek bir büyü açıyorsa, büyüyü de ekle
                if (skill.upgradeType == UpgradeType.UnlockSpell && skill.grantedSpell != null)
                {
                    if (!saveData.unlockedSpellIDs.Contains(skill.grantedSpell.spellID))
                    {
                        saveData.unlockedSpellIDs.Add(skill.grantedSpell.spellID);
                    }
                }

                SaveGame();
                return true;
            }
            return false;
        }

        // Skill ID -> Spell ID eşleşme tablosu (grantedSpell referansı kırıldığında fallback)
        private static readonly Dictionary<string, string> skillToSpellMap = new Dictionary<string, string>()
        {
            { "Skill_Unlock_GoldRush", "Spell_Neutral_Gold" },
            { "Skill_Unlock_Earthquake", "Spell_Neutral_Earthquake" },
            { "Skill_Thunder_1", "Spell_Light_Thunder_1" },
            { "Skill_Thunder_2", "Spell_Light_Thunder_2" },
            { "Skill_Unlock_Shield", "Spell_Light_Shield" },
            { "Skill_Unlock_Blessing", "Spell_Light_Blessing" },
            { "Skill_Unlock_Bloodlust", "Spell_Dark_Bloodlust" },
            { "Skill_Unlock_PlagueRain", "Spell_Dark_PlagueRain" },
            { "Skill_Unlock_Rift", "Spell_Dark_Rift" },
            { "Skill_Unlock_Freeze", "Spell_Dark_Freeze" },
            { "Skill_Unlock_Reinforce_1", "Spell_Light_Reinforce_1" },
            { "Skill_Unlock_Reinforce_2", "Spell_Light_Reinforce_2" },
        };

        public bool IsSpellUnlocked(string spellID)
        {
            if (saveData.unlockedSpellIDs.Contains(spellID))
                return true;

            // Fallback: skill tree'den kontrol et (önce grantedSpell referansı, sonra hardcoded map)
            foreach (string skillID in saveData.unlockedSkillIDs)
            {
                // Yol 1: allAvailableSkills üzerinden grantedSpell referansı
                if (allAvailableSkills != null)
                {
                    SkillNodeData skill = allAvailableSkills.Find(s => s != null && s.skillID == skillID);
                    if (skill != null && skill.grantedSpell != null && skill.grantedSpell.spellID == spellID)
                    {
                        saveData.unlockedSpellIDs.Add(spellID);
                        SaveGame();
                        return true;
                    }
                }

                // Yol 2: Hardcoded eşleşme tablosu (grantedSpell null geldiğinde)
                if (skillToSpellMap.TryGetValue(skillID, out string mappedSpellID) && mappedSpellID == spellID)
                {
                    saveData.unlockedSpellIDs.Add(spellID);
                    SaveGame();
                    return true;
                }
            }
            return false;
        }

        public void EquipSpell(string spellID)
        {
            if (IsSpellUnlocked(spellID) && !saveData.equippedSpellIDs.Contains(spellID))
            {
                if (saveData.equippedSpellIDs.Count < 3) // Maksimum 3 büyü sınırı
                {
                    saveData.equippedSpellIDs.Add(spellID);
                    SyncActiveToSideList();
                    SaveGame();
                }
            }
        }

        public void UnequipSpell(string spellID)
        {
            if (saveData.equippedSpellIDs.Contains(spellID))
            {
                saveData.equippedSpellIDs.Remove(spellID);
                SyncActiveToSideList();
                SaveGame();
            }
        }

        public List<string> GetEquippedSpellIDs() => saveData.equippedSpellIDs;

        // --- UNIT PROGRESSION / LOADOUT ---

        public bool IsUnitEquipped(string unitName) => saveData.equippedUnitNames.Contains(unitName);

        public void EquipUnit(string unitName)
        {
            if (!saveData.equippedUnitNames.Contains(unitName))
            {
                if (saveData.equippedUnitNames.Count < 5) // Maksimum 5 unit sınırı
                {
                    saveData.equippedUnitNames.Add(unitName);
                    SyncActiveToSideList();
                    SaveGame();
                }
            }
        }

        public void UnequipUnit(string unitName)
        {
            if (saveData.equippedUnitNames.Contains(unitName))
            {
                saveData.equippedUnitNames.Remove(unitName);
                SyncActiveToSideList();
                SaveGame();
            }
        }

        public List<string> GetEquippedUnitNames() => saveData.equippedUnitNames;

        public void SanitizeEquippedUnitsForSide(Side side)
        {
            // Menüde taraf değiştiğinde farklı tarafın ünitelerini temizle
            // Tamamen sıfırlamak en kolayıdır, oyuncu baştan seçsin.
            saveData.equippedUnitNames.Clear();
            SaveGame();
        }

        // --- HERO PROGRESSION ---

        public IReadOnlyList<HeroData> GetAllHeroes() => allAvailableHeroes;

        public HeroData GetHeroByID(string heroID)
        {
            return allAvailableHeroes.Find(h => h != null && h.heroID == heroID);
        }

        public bool IsHeroUnlocked(string heroID) => saveData.unlockedHeroIDs.Contains(heroID);

        public int GetHeroLevel(string heroID)
        {
            if (!IsHeroUnlocked(heroID)) return 0;
            HeroProgressEntry entry = saveData.heroProgressList.Find(p => p.heroID == heroID);
            return entry != null ? entry.level : 1;
        }

        public bool TryUnlockHero(HeroData hero)
        {
            if (hero == null || IsHeroUnlocked(hero.heroID)) return false;
            if (saveData.totalKarma < hero.unlockKarmaCost || saveData.totalCrystals < hero.unlockCrystalCost) return false;

            saveData.totalKarma -= hero.unlockKarmaCost;
            saveData.totalCrystals -= hero.unlockCrystalCost;
            saveData.unlockedHeroIDs.Add(hero.heroID);
            saveData.heroProgressList.Add(new HeroProgressEntry { heroID = hero.heroID, level = 1 });
            SaveGame();
            OnHeroProgressChanged?.Invoke();
            return true;
        }

        public bool TryUpgradeHero(HeroData hero)
        {
            if (hero == null || !IsHeroUnlocked(hero.heroID)) return false;

            int currentLevel = GetHeroLevel(hero.heroID);
            if (currentLevel >= hero.maxUpgradeLevel) return false;
            if (saveData.totalKarma < hero.upgradeKarmaCost || saveData.totalCrystals < hero.upgradeCrystalCost) return false;

            saveData.totalKarma -= hero.upgradeKarmaCost;
            saveData.totalCrystals -= hero.upgradeCrystalCost;
            HeroProgressEntry entry = saveData.heroProgressList.Find(p => p.heroID == hero.heroID);
            if (entry == null)
            {
                entry = new HeroProgressEntry { heroID = hero.heroID, level = 1 };
                saveData.heroProgressList.Add(entry);
            }
            entry.level++;
            SaveGame();
            OnHeroProgressChanged?.Invoke();
            return true;
        }

        public float GetHeroStatMultiplier(string heroID)
        {
            HeroData hero = GetHeroByID(heroID);
            if (hero == null) return 1f;

            int level = GetHeroLevel(heroID);
            if (level <= 1) return 1f;

            float healthMult = 1f + (level - 1) * hero.healthBonusPerLevel;
            float damageMult = 1f + (level - 1) * hero.damageBonusPerLevel;
            return (healthMult + damageMult) * 0.5f;
        }

        public float GetHeroHealthMultiplier(string heroID)
        {
            HeroData hero = GetHeroByID(heroID);
            if (hero == null) return 1f;
            int level = GetHeroLevel(heroID);
            return 1f + Mathf.Max(0, level - 1) * hero.healthBonusPerLevel;
        }

        public float GetHeroDamageMultiplier(string heroID)
        {
            HeroData hero = GetHeroByID(heroID);
            if (hero == null) return 1f;
            int level = GetHeroLevel(heroID);
            return 1f + Mathf.Max(0, level - 1) * hero.damageBonusPerLevel;
        }

        public void GetHeroCombatStats(HeroData hero, out float health, out float damage, out float speed, out float range, out float attackRate)
        {
            health = damage = speed = range = attackRate = 0f;
            if (hero == null || hero.unitData == null) return;

            float healthMult = GetHeroHealthMultiplier(hero.heroID);
            float damageMult = GetHeroDamageMultiplier(hero.heroID);

            health = hero.unitData.maxHealth * healthMult;
            damage = hero.unitData.attackDamage * damageMult;
            speed = hero.unitData.moveSpeed;
            range = hero.unitData.attackRange;
            attackRate = hero.unitData.attackRate;
        }

        public IReadOnlyList<HeroData> GetAllHeroesSorted()
        {
            var sorted = new List<HeroData>(allAvailableHeroes);
            sorted.RemoveAll(h => h == null);
            sorted.Sort((a, b) =>
            {
                int side = a.side.CompareTo(b.side);
                return side != 0 ? side : string.Compare(a.displayName, b.displayName, System.StringComparison.Ordinal);
            });
            return sorted;
        }

        public void EquipHero(string heroID)
        {
            if (!IsHeroUnlocked(heroID)) return;
            if (saveData.equippedHeroIDs.Contains(heroID)) return;
            if (saveData.equippedHeroIDs.Count >= 2) return;

            saveData.equippedHeroIDs.Add(heroID);
            SyncActiveToSideList();
            SaveGame();
            OnHeroProgressChanged?.Invoke();
        }

        public void UnequipHero(string heroID)
        {
            if (saveData.equippedHeroIDs.Remove(heroID))
            {
                SyncActiveToSideList();
                SaveGame();
                OnHeroProgressChanged?.Invoke();
            }
        }

        public List<string> GetEquippedHeroIDs() => saveData.equippedHeroIDs;

        public void SanitizeEquippedHeroesForSide(Side side)
        {
            saveData.equippedHeroIDs.RemoveAll(id =>
            {
                HeroData hero = GetHeroByID(id);
                return hero == null || hero.side != side;
            });
            SaveGame();
        }

        public bool IsSkillUnlocked(string skillID)
        {
            return saveData.unlockedSkillIDs.Contains(skillID);
        }

        public int GetHighestUnlockedLevel(int difficulty = 1)
        {
            if (difficulty < 1 || difficulty > 3) difficulty = 1;
            return saveData.highestUnlockedLevelIndexPerDifficulty[difficulty];
        }

        public void UpdateHighestLevel(int index, int difficulty = 1)
        {
            if (difficulty < 1 || difficulty > 3) difficulty = 1;
            
            if (index > saveData.highestUnlockedLevelIndexPerDifficulty[difficulty])
            {
                saveData.highestUnlockedLevelIndexPerDifficulty[difficulty] = index;
                SaveGame();
            }
        }

        public void SaveLevelProgress(string levelID, int stars, int difficulty = 1)
        {
            LevelProgress progress = saveData.levelProgressList.Find(p => p.levelID == levelID && p.difficultyLevel == difficulty);
            if (progress == null)
            {
                progress = new LevelProgress { levelID = levelID, difficultyLevel = difficulty };
                saveData.levelProgressList.Add(progress);
            }

            // Sadece daha yüksek bir yıldız almışsak güncelle
            if (stars > progress.stars)
            {
                progress.stars = stars;
            }
            progress.isCompleted = true;
            SaveGame();
        }

        public int GetLevelStars(string levelID, int difficulty = 1)
        {
            LevelProgress progress = saveData.levelProgressList.Find(p => p.levelID == levelID && p.difficultyLevel == difficulty);
            return progress != null ? progress.stars : 0;
        }

        public bool IsLevelCompleted(string levelID, int difficulty = 1)
        {
            LevelProgress progress = saveData.levelProgressList.Find(p => p.levelID == levelID && p.difficultyLevel == difficulty);
            return progress != null && progress.isCompleted;
        }

        public void UnlockNextGlobalDifficulty()
        {
            if (saveData.highestUnlockedGlobalDifficulty < 3)
            {
                saveData.highestUnlockedGlobalDifficulty++;
                SaveGame();
                Debug.Log($"[Progression] Global Difficulty Unlocked: {saveData.highestUnlockedGlobalDifficulty}");
            }
        }

        public int GetHighestUnlockedGlobalDifficulty() => saveData.highestUnlockedGlobalDifficulty;

        public int GetTotalKarma() => saveData.totalKarma;

        public float GetMultiplierForType(UpgradeType type, Side side)
        {
            float totalMultiplier = 1f;

            foreach (var skillID in saveData.unlockedSkillIDs)
            {
                SkillNodeData skill = allAvailableSkills.Find(s => s.skillID == skillID);
                if (skill != null && skill.upgradeType == type)
                {
                    // Taraf kontrolü: Ya genel bir yetenek ya da istenen tarafın yeteneği olmalı
                    if (skill.side == Side.Neutral || skill.side == side)
                    {
                        totalMultiplier *= skill.multiplier;
                    }
                }
            }

            return totalMultiplier; 
        }

        public float GetTotalAdditiveBonus(UpgradeType type, Side side)
        {
            float totalBonus = 0f;

            foreach (var skillID in saveData.unlockedSkillIDs)
            {
                SkillNodeData skill = allAvailableSkills.Find(s => s.skillID == skillID);
                if (skill != null && skill.upgradeType == type)
                {
                    if (skill.side == Side.Neutral || skill.side == side)
                    {
                        totalBonus += skill.multiplier;
                    }
                }
            }

            return totalBonus;
        }

        // --- Secure Storage (AES-256-CBC + HMAC-SHA256 + Device-bound Key) ---
        private static readonly string LegacyXorKey = "TowerDefenceSecretKey123";
        private static readonly byte[] SaltBytes = Encoding.UTF8.GetBytes("TowerDefence_AntiTamper_Salt_2026_SecureKey");
        private const string AES_HEADER = "ENC_V2::";

        private static void DeriveKeys(out byte[] aesKey, out byte[] hmacKey)
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrEmpty(deviceId) || deviceId == SystemInfo.unsupportedIdentifier)
            {
                deviceId = "Fallback_TowerDefence_Device_Identifier_Secure_Seed";
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(deviceId, SaltBytes, 1000, HashAlgorithmName.SHA256))
            {
                aesKey = deriveBytes.GetBytes(32);  // AES-256 key
                hmacKey = deriveBytes.GetBytes(32); // HMAC-SHA256 key
            }
        }

        private static string EncryptSecure(string plainText)
        {
            DeriveKeys(out byte[] aesKey, out byte[] hmacKey);

            byte[] cipherBytes;
            byte[] iv;
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = aesKey;
                aes.GenerateIV();
                iv = aes.IV;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    cipherBytes = ms.ToArray();
                }
            }

            // Payload: [IV (16 bytes)] + [Ciphertext]
            byte[] payload = new byte[iv.Length + cipherBytes.Length];
            Buffer.BlockCopy(iv, 0, payload, 0, iv.Length);
            Buffer.BlockCopy(cipherBytes, 0, payload, iv.Length, cipherBytes.Length);

            // Compute HMAC-SHA256 over Payload
            byte[] hmac;
            using (var hmacSha256 = new HMACSHA256(hmacKey))
            {
                hmac = hmacSha256.ComputeHash(payload);
            }

            // Final Package: AES_HEADER + Base64(HMAC[32 bytes] + Payload)
            byte[] finalBytes = new byte[hmac.Length + payload.Length];
            Buffer.BlockCopy(hmac, 0, finalBytes, 0, hmac.Length);
            Buffer.BlockCopy(payload, 0, finalBytes, hmac.Length, payload.Length);

            return AES_HEADER + Convert.ToBase64String(finalBytes);
        }

        private static bool TryDecryptSecure(string cipherString, out string plainText)
        {
            plainText = null;
            if (string.IsNullOrEmpty(cipherString) || !cipherString.StartsWith(AES_HEADER))
                return false;

            try
            {
                string b64 = cipherString.Substring(AES_HEADER.Length);
                byte[] finalBytes = Convert.FromBase64String(b64);

                if (finalBytes.Length < 32 + 16) return false; // 32 bytes HMAC + 16 bytes IV minimum

                DeriveKeys(out byte[] aesKey, out byte[] hmacKey);

                byte[] receivedHmac = new byte[32];
                int payloadLength = finalBytes.Length - 32;
                byte[] payload = new byte[payloadLength];

                Buffer.BlockCopy(finalBytes, 0, receivedHmac, 0, 32);
                Buffer.BlockCopy(finalBytes, 32, payload, 0, payloadLength);

                // Verify HMAC (Tamper check)
                using (var hmacSha256 = new HMACSHA256(hmacKey))
                {
                    byte[] computedHmac = hmacSha256.ComputeHash(payload);
                    bool match = true;
                    for (int i = 0; i < 32; i++)
                    {
                        if (receivedHmac[i] != computedHmac[i]) match = false;
                    }
                    if (!match)
                    {
                        Debug.LogError("[SaveSecurity] Save file tamper or invalid device signature detected!");
                        return false;
                    }
                }

                // Extract IV and Ciphertext
                byte[] iv = new byte[16];
                int cipherLength = payloadLength - 16;
                byte[] cipherBytes = new byte[cipherLength];

                Buffer.BlockCopy(payload, 0, iv, 0, 16);
                Buffer.BlockCopy(payload, 16, cipherBytes, 0, cipherLength);

                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = aesKey;
                    aes.IV = iv;

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        plainText = Encoding.UTF8.GetString(ms.ToArray());
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SaveSecurity] Decryption failed: {ex.Message}");
                return false;
            }
        }

        private static string DecryptLegacyXor(string text)
        {
            var result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                result.Append((char)(text[i] ^ LegacyXorKey[i % LegacyXorKey.Length]));
            }
            return result.ToString();
        }

        private void SaveGame()
        {
            try
            {
                string json = JsonUtility.ToJson(saveData);
                string encrypted = EncryptSecure(json);
                File.WriteAllText(savePath, encrypted, Encoding.UTF8);
                Debug.Log($"Game Saved and Encrypted to: {savePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error saving game: {ex.Message}");
            }
        }

        private void LoadGame()
        {
            if (File.Exists(savePath))
            {
                try
                {
                    string fileContent = File.ReadAllText(savePath, Encoding.UTF8);
                    bool loadSuccess = false;

                    // 1. Yeni Güvenli AES-256 + HMAC formatını dene
                    if (fileContent.StartsWith(AES_HEADER))
                    {
                        if (TryDecryptSecure(fileContent, out string plainText))
                        {
                            saveData = JsonUtility.FromJson<SaveData>(plainText);
                            Debug.Log($"[SaveDebug] Raw Loaded JSON: {plainText}");
                            Debug.Log($"[SaveDebug] Parsed: Karma={saveData.totalKarma}, Crystals={saveData.totalCrystals}");
                            loadSuccess = true;
                            Debug.Log("Game Loaded and Decrypted securely (AES-256 + Device-Bound HMAC).");
                        }
                        else
                        {
                            Debug.LogError("[SaveSecurity] Failed to decrypt secure save data. Corrupted or tampered.");
                        }
                    }
                    // 2. Geriye dönük uyumluluk: Şifrelenmemiş düz JSON ise
                    else if (fileContent.TrimStart().StartsWith("{"))
                    {
                        saveData = JsonUtility.FromJson<SaveData>(fileContent);
                        loadSuccess = true;
                        Debug.Log("Game Loaded from plain-text JSON (Migrating to AES-256 on next save)");
                        SaveGame();
                    }
                    // 3. Geriye dönük uyumluluk: Eski Legacy XOR şifreli kayıt ise
                    else
                    {
                        try
                        {
                            string decrypted = DecryptLegacyXor(fileContent);
                            if (decrypted.TrimStart().StartsWith("{"))
                            {
                                saveData = JsonUtility.FromJson<SaveData>(decrypted);
                                loadSuccess = true;
                                Debug.Log("Game Loaded from legacy XOR format (Migrating to AES-256 on next save)");
                                SaveGame();
                            }
                        }
                        catch
                        {
                            loadSuccess = false;
                        }
                    }

                    if (!loadSuccess)
                    {
                        Debug.LogError("Error loading save game: Could not parse save data. Resetting progress.");
                        saveData = new SaveData();
                        saveData.totalKarma = 200;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error loading save game: {ex.Message}. Resetting progress.");
                    saveData = new SaveData();
                    saveData.totalKarma = 200;
                }
            }
            else
            {
                saveData = new SaveData();
                saveData.totalKarma = 200; // Başlangıç Karma puanı
            }

            // Migration: Yeni array sistemi için eksik eleman varsa doldur
            while (saveData.highestUnlockedLevelIndexPerDifficulty.Count <= 3)
            {
                saveData.highestUnlockedLevelIndexPerDifficulty.Add(0);
            }

            // Migration: Eski sistemdeki progress değerini Zorluk 1'e aktar
            if (saveData.highestUnlockedLevelIndex > saveData.highestUnlockedLevelIndexPerDifficulty[1])
            {
                saveData.highestUnlockedLevelIndexPerDifficulty[1] = saveData.highestUnlockedLevelIndex;
            }
            
            // Migration: Tüm mevcut level progress'lerde difficulty yoksa 1 yap
            foreach (var progress in saveData.levelProgressList)
            {
                if (progress.difficultyLevel == 0) progress.difficultyLevel = 1;
            }

            // Migration: Eski günlük ödül kullananlar için dailyRewardDayIndex başlat
            if (saveData.lastDailyRewardTime != 0 && saveData.dailyRewardDayIndex == 0)
            {
                saveData.dailyRewardDayIndex = 1;
            }

            // Migration: Büyü listesini skill tree ile senkronize et
            // Sadece unlock edilmiş skill'lerin grant ettiği büyüler kalsın
            // allAvailableSkills boşsa mevcut listeyi sıfırlama
            List<SkillNodeData> skillsPool = (allAvailableSkills != null && allAvailableSkills.Count > 0)
                ? allAvailableSkills
                : new List<SkillNodeData>(Resources.FindObjectsOfTypeAll<SkillNodeData>());

            if (skillsPool.Count > 0)
            {
                List<string> validSpells = new List<string>();
                foreach (string skillID in saveData.unlockedSkillIDs)
                {
                    SkillNodeData skill = skillsPool.Find(s => s != null && s.skillID == skillID);
                    if (skill != null && skill.grantedSpell != null && !validSpells.Contains(skill.grantedSpell.spellID))
                        validSpells.Add(skill.grantedSpell.spellID);
                }

                if (validSpells.Count > 0 || saveData.unlockedSkillIDs.Count == 0)
                {
                    saveData.unlockedSpellIDs = validSpells;
                }
            }

            EnsureStarterHeroUnlocked();
        }


        // --- SIDE-SPECIFIC LOADOUT MANAGEMENT ---

        private Side GetCurrentPlayerSide()
        {
            if (SideController.Instance != null)
                return SideController.Instance.GetPlayerSide();
            return Side.Light;
        }

        public void SwitchSideLoadout(Side newSide)
        {
            // Taraf secildiginde aktif loadout listelerini dogrudan o tarafa ait kaydedilmis listelerle dolduruyoruz.
            // Hicbir varsayilan eleman eklenmez veya eksiltilmez; kullanici ne sectiyse (veya bos biraktiysa) o gelir.
            if (newSide == Side.Light)
            {
                saveData.equippedHeroIDs = new List<string>(saveData.lightEquippedHeroIDs);
                saveData.equippedUnitNames = new List<string>(saveData.lightEquippedUnitNames);
                saveData.equippedSpellIDs = new List<string>(saveData.lightEquippedSpellIDs);
            }
            else
            {
                saveData.equippedHeroIDs = new List<string>(saveData.darkEquippedHeroIDs);
                saveData.equippedUnitNames = new List<string>(saveData.darkEquippedUnitNames);
                saveData.equippedSpellIDs = new List<string>(saveData.darkEquippedSpellIDs);
            }

            SaveGame();
            OnHeroProgressChanged?.Invoke();
        }

        private void SyncActiveToSideList()
        {
            Side side = GetCurrentPlayerSide();
            if (side == Side.Light)
            {
                saveData.lightEquippedHeroIDs = new List<string>(saveData.equippedHeroIDs);
                saveData.lightEquippedUnitNames = new List<string>(saveData.equippedUnitNames);
                saveData.lightEquippedSpellIDs = new List<string>(saveData.equippedSpellIDs);
            }
            else if (side == Side.Dark)
            {
                saveData.darkEquippedHeroIDs = new List<string>(saveData.equippedHeroIDs);
                saveData.darkEquippedUnitNames = new List<string>(saveData.equippedUnitNames);
                saveData.darkEquippedSpellIDs = new List<string>(saveData.equippedSpellIDs);
            }
        }

        private void EnsureStarterHeroUnlocked()
        {
            if (allAvailableHeroes == null || allAvailableHeroes.Count == 0) return;

            bool anyUnlocked = saveData.unlockedHeroIDs.Count > 0;
            if (anyUnlocked) return;

            HeroData starter = allAvailableHeroes.Find(h => h != null && h.isStarterHero);
            if (starter == null)
                starter = allAvailableHeroes.Find(h => h != null);

            if (starter != null)
            {
                saveData.unlockedHeroIDs.Add(starter.heroID);
                saveData.heroProgressList.Add(new HeroProgressEntry { heroID = starter.heroID, level = 1 });
                SaveGame();
            }
        }

        public void ResetSave()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            saveData = new SaveData();
            saveData.totalKarma = 200;
            Debug.Log("Game Progress Reset Successfully!");
            
            // Başlangıç ayarlarını tekrar yükle
            LoadGame();
        }
    }
}
