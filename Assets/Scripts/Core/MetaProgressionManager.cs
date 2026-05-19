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
        public int stars;
        public bool isCompleted;
    }

    [System.Serializable]
    public class SaveData
    {
        public int totalKarma;
        public List<string> unlockedSkillIDs = new List<string>();
        public List<string> unlockedSpellIDs = new List<string>(); // Açılan aktif büyüler
        public List<string> equippedSpellIDs = new List<string>(); // Kuşatılan büyüler
        public int highestUnlockedLevelIndex = 0;
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

        [Header("Config")]
        [SerializeField] private List<SkillNodeData> allAvailableSkills;

        public void AddKarma(int amount)
        {
            saveData.totalKarma += amount;
            SaveGame();
        }

        public bool TryUnlockSkill(SkillNodeData skill)
        {
            if (saveData.totalKarma >= skill.karmaCost && !IsSkillUnlocked(skill.skillID))
            {
                // Önkoşul kontrolü
                foreach (var req in skill.requiredSkills)
                {
                    if (!IsSkillUnlocked(req.skillID)) return false;
                }

                saveData.totalKarma -= skill.karmaCost;
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

        public bool IsSkillUnlocked(string skillID)
        {
            return saveData.unlockedSkillIDs.Contains(skillID);
        }

        public int GetHighestUnlockedLevel() => saveData.highestUnlockedLevelIndex;

        public void UpdateHighestLevel(int index)
        {
            if (index > saveData.highestUnlockedLevelIndex)
            {
                saveData.highestUnlockedLevelIndex = index;
                SaveGame();
            }
        }

        public void SaveLevelProgress(string levelID, int stars)
        {
            LevelProgress progress = saveData.levelProgressList.Find(p => p.levelID == levelID);
            if (progress == null)
            {
                progress = new LevelProgress { levelID = levelID };
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

        public int GetLevelStars(string levelID)
        {
            LevelProgress progress = saveData.levelProgressList.Find(p => p.levelID == levelID);
            return progress != null ? progress.stars : 0;
        }

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

            // Başlangıç büyülerini ve yeteneklerini otomatik aç (Eğer hiç büyü yoksa)
            if (saveData.unlockedSpellIDs.Count == 0)
            {
                // 1 Light, 1 Dark Başlangıç Büyüsü
                saveData.unlockedSpellIDs.Add("Spell_Light_Meteor_1");
                saveData.unlockedSpellIDs.Add("Spell_Dark_Bloodlust");

                // İlgili yetenekleri de satın alınmış işaretle
                if (!saveData.unlockedSkillIDs.Contains("Skill_Unlock_Light_Meteor_1"))
                    saveData.unlockedSkillIDs.Add("Skill_Unlock_Light_Meteor_1");
                if (!saveData.unlockedSkillIDs.Contains("Skill_Unlock_Dark_Bloodlust"))
                    saveData.unlockedSkillIDs.Add("Skill_Unlock_Dark_Bloodlust");

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
