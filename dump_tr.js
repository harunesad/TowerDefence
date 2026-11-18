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

const out = {};

files.forEach(f => {
  const content = fs.readFileSync(f, 'utf8');
  const lines = content.split('\n');
  const results = [];
  lines.forEach((line, idx) => {
    if (/(\/\/|\/\*|\*|\[Tooltip|\[Header)/.test(line)) {
      if (/[çğıöşüÇĞİÖŞÜ]/.test(line) || /\b(ve|veya|icin|için|olan|olarak|ile|kadar|bir|tum|tüm|yeni|bu|su|şu|daha|sonra|once|önce|kullan|varsayilan|varsayılan|sadece|zaman|gerek|yok|var|eger|eğer|et|ol|yap|ver|al|don|dön|baslat|başlat|bitir|yukle|yükle|kaydet|guncelle|güncelle|kontrol)\b/i.test(line)) {
        results.push({ line: idx + 1, text: line.trim() });
      }
    }
  });
  if (results.length > 0) {
    out[f] = results;
  }
});

fs.writeFileSync('tr_matches.json', JSON.stringify(out, null, 2), 'utf8');
console.log('Saved to tr_matches.json. File count:', Object.keys(out).length);
