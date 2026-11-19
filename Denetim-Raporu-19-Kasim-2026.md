# Tower Defence — Kod Denetimi

**Tarih:** 19 Kasım 2026  
**Kapsam:** Kule seviyesi, uzmanlık, birim vuruşu, kahraman yeteneği, kule kartları  
**Yöntem:** İlgili dosyalar okunarak doğrulandı. Maç oynanmadı. Oyun kodu bu belge için değiştirilmedi.

**Özet:** 3 mantık hatası, 2 ölü kule kartı.

---

## Mantık hataları

### 1. Uzmanlık kulesi seviye 1’den başlıyor ve iki kez daha yükseltilebiliyor

**Konu:** Kule  
**Yer:** `TowerSlot.SpecializeTower`, `Tower.Upgrade`, `Tower.DamageLevelScale`, `TowerUpgradeUI.RefreshRingButtons`

Uzmanlık seçilince eski kule yok edilir ve uzmanlık prefabı yeniden kurulur. `Initialize` seviyeyi 1’de bırakır. Yükseltme düğmesi, kulenin iki uzmanlığı yoksa ve seviye 3’ün altındaysa açık kalır. Uzman kulelerin ikinci bir uzmanlık listesi yoktur.

Hasar çarpanı bu yeni kuleye de biner. Seviye 2 ×1.4, seviye 3 ×1.8. Menzil her seviyede %20 artar.

Ranger Outpost kartı 4 hasar ve 2.5 atış hızıdır. Bu, saniyede 10 hasardır. Aynı kule seviye 3’e çıkınca hasar 7.2 olur, saniyede 18 hasar verir. Menzil 11’den 15.4’e çıkar. Denge hesabı kart değerine göre yapılmıştı. Oyun o değerin üstüne bir yükseltme daha koyuyor.

### 2. Çağrılan birimin vuruşu, infoda yazanın üçte biri

**Konu:** Birim  
**Yer:** `Unit.Initialize`, `UnitInfoPanelUI.Setup`

Düşman dalgası karttaki hasarı vurur. Oyuncunun bıraktığı birim ve kışla askeri, aynı kartın üçte birini vurur. Kahraman bu bölmeye girmez.

Info paneli iki taraf için de `attackDamage` alanını yazar. Light Swordsman infoda 12 görünür. Sahada oyuncunun askeri 4 vurur. Düşman dalgasındaki aynı kart 12 vurur.

### 3. Kahraman yetenek yazısı seviyeyle güncellenmiyor

**Konu:** Kahraman  
**Yer:** `HeroData.AbilityCooldownAtLevel`, `HeroData.AbilityPowerAtLevel`, kahraman info metinleri

Bekleme ve güç seviyeyle değişir. Her seviye beklemeyi %10 kısaltır, gücü %15 artırır. Seviye 5’te bekleme tabanın %60’ıdır, güç 1.6 kattır. Alt sınır beklemede %50’dir.

Paneldeki süre bu çarpanı kullanır. Yetenek cümlesi karttaki sabit metindir. Arthur’un yazısı “canın %15’i” olarak kalır. Seviye 5’te iyileştirme bu oranın 1.6 katıdır.

Kahraman kartındaki `upgradeKarmaCost` ve `upgradeCrystalCost` alanları da okunmaz. Ücret kodda sabittir: karma 150, 250, 400, 600 ve kristal 15, 25, 40, 60.

---

## Ölü kartlar

### 4. Void Assassin ve Soul Prison

**Konu:** Kule verisi  
**Yer:** `Assets/Data/Towers/Void_Assassin.asset`, `Assets/Data/Towers/Soul_Prison.asset`

İkisinin canı 0. `Tower.Initialize` 0 canı 100 kabul eder. İkisi de temel kule değildir. Hiçbir uzmanlık listesinde geçmezler. Kurulum menüsüne çıkmazlar.

Sahaya konurlarsa 100 canla dururlar. Aynı kuşaktaki uzman kuleler 700–1200 can taşır.
