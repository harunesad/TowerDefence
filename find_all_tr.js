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

// Common Turkish words that might not have Turkish-specific letters
const trWords = [
  "ve", "veya", "için", "olan", "olarak", "ile", "kadar", "bir", "tüm", "yeni",
  "bu", "şu", "daha", "sonra", "önce", "kullan", "varsayılan", "sadece", "zaman",
  "gerek", "yok", "var", "eğer", "et", "ol", "yap", "ver", "al", "dön",
  "başlat", "bitir", "yükle", "kaydet", "güncelle", "kontrol"
];

for (const file of files) {
  const content = fs.readFileSync(file, 'utf8');
  const lines = content.split('\n');
  const suspected = [];
  lines.forEach((line, idx) => {
    // Check if line contains comments (// or /* or *) or [Header or [Tooltip
    const isCommentOrAttribute = /^\s*(\/\/|\/\*|\*|\[Tooltip|\[Header)/.test(line) || /\/\/|\[Tooltip|\[Header/.test(line);
    if (!isCommentOrAttribute) return;

    if (/[çğıöşüÇĞİÖŞÜ]/.test(line)) {
      suspected.push({ lineNum: idx + 1, text: line.trim() });
      return;
    }

    // Check words without accents
    const lower = line.toLowerCase();
    for (const word of trWords) {
      const regex = new RegExp(`\\b${word}\\b`);
      if (regex.test(lower)) {
        suspected.push({ lineNum: idx + 1, text: line.trim(), word });
        break;
      }
    }
  });

  console.log(`=== ${file} (${suspected.length} suspected) ===`);
  suspected.forEach(s => console.log(`  L${s.lineNum}: ${s.text}`));
}
