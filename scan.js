const fs = require('fs');

const files = [
  "Assets/Scripts/Core/MetaProgressionManager.cs",
  "Assets/Scripts/Core/CampaignManager.cs",
  "Assets/Scripts/Core/SceneLoader.cs",
  "Assets/Scripts/Core/NetworkTimeManager.cs",
  "Assets/Scripts/Core/HeroManager.cs",
  "Assets/Scripts/Core/SpellManager.cs",
  "Assets/Scripts/Core/CameraAspectRatioScaler.cs",
  "Assets/Scripts/Core/AdManager.cs",
  "Assets/Scripts/Core/GameSpeedManager.cs",
  "Assets/Scripts/Core/PhaseManager.cs",
  "Assets/Scripts/Core/DailyRewardManager.cs",
  "Assets/Scripts/Core/GameManager.cs",
  "Assets/Scripts/Core/CurrencyManager.cs",
  "Assets/Scripts/Core/SaveManager.cs",
  "Assets/Scripts/Core/Security/ObscuredInt.cs",
  "Assets/Scripts/Data/SpellData.cs",
  "Assets/Scripts/Data/SkillNodeData.cs",
  "Assets/Scripts/Data/TowerData.cs",
  "Assets/Scripts/Data/LevelThemeEnvironmentConfig.cs",
  "Assets/Scripts/Data/WaveData.cs",
  "Assets/Scripts/Data/HeroData.cs",
  "Assets/Scripts/Data/LevelData.cs",
  "Assets/Scripts/Data/UnitData.cs",
  "Assets/Scripts/Grid/TowerPlacementManager.cs",
  "Assets/Scripts/Grid/TowerSlot.cs",
  "Assets/Scripts/Grid/LevelInitializer.cs",
  "Assets/Scripts/Editor/LevelMapConfigurator.cs"
];

for (const file of files) {
  if (!fs.existsSync(file)) {
    console.log(`MISSING: ${file}`);
    continue;
  }
  const content = fs.readFileSync(file, 'utf8');
  const lines = content.split('\n');
  const trLines = [];
  lines.forEach((line, idx) => {
    // Check for Turkish specific chars or common words in comments/tooltips
    if (/[çğıöşüÇĞİÖŞÜ]/.test(line)) {
      trLines.push({ lineNum: idx + 1, text: line.trim() });
    }
  });
  console.log(`${file}: ${trLines.length} lines with Turkish characters`);
  if (trLines.length > 0 && trLines.length <= 10) {
    trLines.forEach(l => console.log(`  L${l.lineNum}: ${l.text}`));
  }
}
