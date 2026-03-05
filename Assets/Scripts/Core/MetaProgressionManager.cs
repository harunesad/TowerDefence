using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TowerDefence.Data;

namespace TowerDefence.Core
{
    [System.Serializable]
    public class SaveData
    {
        public int totalKarma;
        public List<string> unlockedSkillIDs = new List<string>();
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
                SaveGame();
                return true;
            }
            return false;
        }

        public bool IsSkillUnlocked(string skillID)
        {
            return saveData.unlockedSkillIDs.Contains(skillID);
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

        private void SaveGame()
        {
            string json = JsonUtility.ToJson(saveData);
            File.WriteAllText(savePath, json);
            Debug.Log($"Game Saved to: {savePath}");
        }

        private void LoadGame()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                saveData = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("Game Loaded");
            }
        }
    }
}
