using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TowerDefence.Data;

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
        public List<HeroProgressEntry> heroProgressList = new List<HeroProgressEntry>();
        
        // Geriye dönük uyumluluk için tutuluyor
        public int highestUnlockedLevelIndex = 0;
        
        // Yeni Zorluk Sistemi Değişkenleri
        public int highestUnlockedGlobalDifficulty = 1;
        public List<int> highestUnlockedLevelIndexPerDifficulty = new List<int> { 0, 0, 0, 0 }; // İndeksler: 1=Normal, 2=Hard, 3=Expert
        
        public List<LevelProgress> levelProgressList = new List<LevelProgress>();
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

        public bool IsSpellUnlocked(string spellID) => saveData.unlockedSpellIDs.Contains(spellID);

        public void EquipSpell(string spellID)
        {
            if (IsSpellUnlocked(spellID) && !saveData.equippedSpellIDs.Contains(spellID))
            {
                if (saveData.equippedSpellIDs.Count < 3) // Maksimum 3 büyü sınırı
                {
                    saveData.equippedSpellIDs.Add(spellID);
                    SaveGame();
                }
            }
        }

        public void UnequipSpell(string spellID)
        {
            if (saveData.equippedSpellIDs.Contains(spellID))
            {
                saveData.equippedSpellIDs.Remove(spellID);
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
                    SaveGame();
                }
            }
        }

        public void UnequipUnit(string unitName)
        {
            if (saveData.equippedUnitNames.Contains(unitName))
            {
                saveData.equippedUnitNames.Remove(unitName);
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
            SaveGame();
            OnHeroProgressChanged?.Invoke();
        }

        public void UnequipHero(string heroID)
        {
            if (saveData.equippedHeroIDs.Remove(heroID))
            {
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

        private static readonly string encryptionKey = "TowerDefenceSecretKey123";

        private string EncryptDecrypt(string text)
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                result.Append((char)(text[i] ^ encryptionKey[i % encryptionKey.Length]));
            }
            return result.ToString();
        }

        private void SaveGame()
        {
            try
            {
                string json = JsonUtility.ToJson(saveData);
                string encrypted = EncryptDecrypt(json);
                File.WriteAllText(savePath, encrypted);
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
                    string fileContent = File.ReadAllText(savePath);
                    
                    // Geriye dönük uyumluluk: Eğer kayıt dosyası şifrelenmemiş düz JSON ise doğrudan oku
                    if (fileContent.TrimStart().StartsWith("{"))
                    {
                        saveData = JsonUtility.FromJson<SaveData>(fileContent);
                        Debug.Log("Game Loaded from plain-text JSON (Will be encrypted on next save)");
                    }
                    else
                    {
                        string decrypted = EncryptDecrypt(fileContent);
                        saveData = JsonUtility.FromJson<SaveData>(decrypted);
                        Debug.Log("Game Loaded and Decrypted successfully");
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
            List<string> validSpells = new List<string>();
            foreach (string skillID in saveData.unlockedSkillIDs)
            {
                SkillNodeData skill = allAvailableSkills.Find(s => s != null && s.skillID == skillID);
                if (skill != null && skill.grantedSpell != null && !validSpells.Contains(skill.grantedSpell.spellID))
                    validSpells.Add(skill.grantedSpell.spellID);
            }
            saveData.unlockedSpellIDs = validSpells;

            EnsureStarterHeroUnlocked();
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
