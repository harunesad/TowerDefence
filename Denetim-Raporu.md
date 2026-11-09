# Tower Defence — Kod Denetimi

**Tarih:** 6 Kasım 2026  
**Kapsam:** `Assets/Scripts`, skill ve kule verileri, gameplay HUD prefabı  
**Yöntem:** İlgili dosyalar okunarak doğrulandı. Maç oynanmadı. Oyun kodu bu rapor için değiştirilmedi.

**Özet:** 2 kritik, 11 yüksek, 15 orta, 3 düşük bulgu.

Maçı bugün doğrudan bozan iki yer: Poison Spitter kilitlendiği hedefe zehir hasarı işletemez. 3 yıldız alınmış bir bölümü tekrar açıp reklam izlemek, ekranda görünen tam karma ve kristali kasaya bir kez daha yazar.

---

## Kritik

### 1. Zehir ve yanma, kule vurmaya devam ettikçe işlemez

**Konu:** Savaş  
**Yer:** `StatusEffect`, `Unit.AddStatusEffect`, Poison Spitter

Tick bir saniye sonraya kurulur. Yeni vuruş süreyi uzatınca eski efekt silinip saat baştan kurulur. Poison Spitter saniyede 1 atar, zehir 5 saniye sürer. Fizik vuruşu `Update` öncesinde geldiği için tick hiç birikmez. Hasar yalnız mermiden gelir. Kule vurmayı bırakınca tick başlar.

### 2. Tekrar oynanan bölümde reklam, ekrandaki tam ödülü basar

**Konu:** Ekonomi  
**Yer:** `LevelResultUI.Show`, `CampaignManager.CompleteCurrentLevel`

Yıldız artmamışsa kasaya 0 yazılır. Panel hâlâ `25 × yıldız × zorluk` karmayı gösterir. Reklam başarı callback'i bu görünen tutarı `AddKarma` / `AddCrystals` ile ekler. 3 yıldızlı bölümü tekrar açıp reklam izlemek bölüm ödülünü yeniden verir.

---

## Yüksek

**Durum (7 Kasım 2026):** Bu bölümdeki maddeler kodda kapatıldı. Aşağıdaki metin, düzeltmeden önceki hali anlatır.

### 3. Kahraman canı, o tarafın bütün birimlerine gider

**Konu:** Meta  
**Yer:** `Skill_Hero_1`, `Skill_Hero_2`, `Unit.Initialize`

Yazı “Hero Health +15% / +30%”. Tip `HealthBonus`. Bu çarpan her birimin canına uygulanır. Işık askerleri ve diğer ışık birimleri de aynı bonusu alır. Ayrı bir kahraman can çarpanı bu düğümlerden okunmaz.

### 4. Okçu hasarı ve menzili bütün kulelere gider

**Konu:** Meta  
**Yer:** `Skill_Archer_1`, `Skill_Archer_2`, `Skill_Archer_3`, `Tower.Initialize`

Yazı “Light Archer damage / range”. Tipler `TowerDamageBonus` ve `RangeBonus`. `Tower.Initialize` bu çarpanları o tarafın her kulesine uygular. Top, büyücü ve kışla da okçu düğümünden güçlenir.

### 5. Kapatma, ışını, yavaşı ve kışlayı durdurmaz

**Konu:** Savaş  
**Yer:** `Tower.Disable`, `BeamTower`, `SlowTower`, `BarracksTower`

Karanlık taraf yeteneği rakip kuleye 5 saniye `Disable` basar. `Tower.Update` erken döner, hedefi silmez. `BeamTower` kendi `Update` içinde son hedefe saniye başı hasar vermeye devam eder. `SlowTower` `isDisabled` okumaz. `BarracksTower` `base.Update` çağırmaz, asker üretmeye devam eder.

### 6. Aura kulesini yükseltmek aurayı değiştirmez

**Konu:** Savaş  
**Yer:** `AuraTower.Start`, `Tower.Upgrade`

Yarıçap ve bonuslar `Start` içinde bir kez kopyalanır. `Upgrade` yalnız `range`, `damage` ve `fireRate` yazar. Aura kulelerinin hasarı ve atış hızı 0’dır, aura bu alanları okumaz. Ücret düşer, halka ve buff aynı kalır. Üst üste iki aura birikmez; ilki kilitleyince ikincisi yok sayılır.

### 7. Crystal Vault her bölüm girişinde kalıcı +15 kristal yazar

**Konu:** Ekonomi  
**Yer:** `CampaignManager.GrantLevelStartCrystals`, `Skill_Neutral_CrystalVault`

Yazı “Start every level with +15 Crystals”. Kod bunu maç içi geçici puana değil, kayıtlı `totalCrystals` alanına ekler. Her yeni giriş +15 biriktirir. Aynı bölümü tekrar açmak kristal farmıdır. Yalnız bir restart bu eklemeyi bir kez atlar.

### 8. Karanlık tarafta sızma can düşürmeyebilir

**Konu:** Dalga  
**Yer:** `Unit.FindTargetBase`, `Base.Start`, `LivesManager.ReduceLives`

Harita üsleri kayıtta Light. `Base.Start` onları oyuncu tarafına çeker. Birim üssü `Start` içinde bir kez arar. Oyuncu Dark ise ve birim daha önce çalışırsa karşı üs bulunmaz, `targetBase` boş kalır. Yol sonu hasar vermez. Üs bulunsa bile `ReduceLives` yalnız oyuncu tarafını düşürür.

### 9. Eksik veya bozuk kayıt 200 karmaya sıfırlanır

**Konu:** Kayıt  
**Yer:** `MetaProgressionManager.LoadGame`

`JsonUtility` eksik listeleri null bırakır. Yükleyici `highestUnlockedLevelIndexPerDifficulty.Count` ve `levelProgressList` üzerinde null kontrolü yapmadan döner. İstisna yedeği de açamazsa profil silinir, `totalKarma` 200 olur. Şifresi çözülemeyen dosyada aynı yol vardır.

### 10. Şifre anahtarı kayıt klasöründe düz duruyor

**Konu:** Kayıt  
**Yer:** `MetaProgressionManager.DeriveInstallKeys`

AES anahtarı `meta_install.key` dosyasına yazılır. Karma ve kristal bu anahtarla çözülen JSON’un içindedir. Persistent data klasörünü okuyan kişi dosyayı değiştirip aynı anahtarla geri yazabilir. Honeypot alanı hiç okunmaz.

### 11. Bilgi kartına basmak kule menüsünü kapatır

**Konu:** Arayüz  
**Yer:** `TowerPlacementManager.Update`

Üstteki nesne `Selectable` değilse ve adında `Text` geçiyorsa tıklama dünyaya bırakılır. Yükseltme metni `StatsText`. Karta basınca `CloseAllTowerUI` çalışır. Dokunmatikte `IsPointerOverGameObject` kimliksiz çağrılır; basış karesinde false dönerse buton `onClick` öncesinde menü kapanır.

### 12. Yavaşlayan kahraman dirilince yavaş kalır

**Konu:** Savaş  
**Yer:** `Unit.ClearAllStatusEffects`, `HeroUnit.Respawn`

Ölüm efekt listesini siler, `slowMultiplier` değerini 1 yapmaz. Dirilme de yapmaz. Ölü kahramanın `Update` metodu hemen döner, `RefreshSlowFactor` çalışmaz. Hareket hızı ölüm anındaki yavaş çarpanla kalır.

### 13. Taunt, askerin tek blok yerini kalıcı doldurur

**Konu:** Savaş  
**Yer:** `Unit.Block`, `HeroUnit.PullNearbyEnemies`

`Block` eski engelleyene haber vermeden `currentBlocker` değerini ezer. `StartBlocking` sayıya bakmaz. Çekilen düşman ölünce yalnız o anki engelleyene haber gider. Askerin listesinde ölü kayıt kalır. `CanBlockMore` sayımın 1’in altında olmasını istediği için o asker bir daha kimseyi tutamaz.

---

## Orta

**Durum (7 Kasım 2026):** Bu bölümdeki maddeler kodda kapatıldı. Aşağıdaki metin, düzeltmeden önceki hali anlatır.

### 14. Ölen birim başlamış vuruşu tamamlar

**Konu:** Savaş  
**Yer:** `Unit.ExecuteAttackHit`, `Unit.Die`

Ölüm coroutine durdurmaz. `ExecuteAttackHit` saldıranın canlı olduğuna bakmaz. `ResetAttackState` birim ölüyse çıkar. Kahraman sahnede kaldığı için dirilene kadar bu vuruş gidebilir. Normal birimde ceset penceresi 2 saniyedir.

### 15. Hedef değiştirme 0.5 saniye değil, yaklaşık 5 saniye sürer

**Konu:** Savaş  
**Yer:** `Tower.UpdateTarget`

Tarama 0.15 saniyede bir yapılır. Bekleme sayacı yalnız o karede `Time.deltaTime` ekler. 0.5 saniyelik eşik onlarca taramaya yayılır. İlk değişim `previousTarget` boş olduğu için anındadır. Sonraki bekleme, o an vurulan birime değil bir öncekine bakar.

### 16. Rally, HUD tıklamasını da yer

**Konu:** Arayüz  
**Yer:** `TowerPlacementManager` rally dalı

Rally açıkken UI testi atlanır. Pause, hız veya dalga atlama hem kendi işini yapar hem rally koyar veya iptal eder. Zemin ışını Default ve Path dışında bir şeye çarpmazsa rally kapanır, menzil gizlenir.

### 17. İki slot arasında 0.2 saniyelik tık yutulur

**Konu:** Arayüz  
**Yer:** `TowerSelectionUI.ShowForSlot`, `TowerUpgradeUI`

`lastShowTime` bütün slotlar için tek statik saattir. Yeni slot önce diğer menüyü kapatır, sonra `Show` bu saate takılıp döner. Önceki menü kapanır, yenisi açılmaz.

### 18. Doğmayan dalga birimi doğmuş sayılır

**Konu:** Dalga  
**Yer:** `Spawner.HandleSpawning`, `Spawner.SpawnUnit`

Karşılık yoksa veya prefab boşsa `SpawnUnit` birim yaratmadan döner. Sayaç yine artar. Spawner bitince ve sahada düşman yoksa dalga 1.5 saniye sonra kapanır. Eksik prefablı dalga boş geçilebilir.

### 19. Dalga verisindeki bekleme süresi kullanılmıyor

**Konu:** Dalga  
**Yer:** `WaveData.timeBeforeNextWave`, `PhaseManager`

Alan normal dalgada 15, finalde 30. Hazırlık her zaman prefabdaki `prepPhaseDuration` ile açılır. `timeBeforeNextWave` hiçbir scriptte okunmaz.

### 20. Taraf yeteneğinin butonu bağlı değil

**Konu:** Arayüz  
**Yer:** `GameplayHUD_MasterPrefab`, `HUDController.UpdateAbilityUI`

`abilityButton` ve `abilityOverlay` prefabda boş. `OnAbilityClicked` hiç bağlanmaz. Işık iyileştirmesi ve karanlık kapatma bu HUD’dan basılamaz. Cooldown olayı yine de `UpdateAbilityUI` metoduna gider ve boş butonda `interactable` yazmaya çalışır.

### 21. Holy Shield kalkan vermez

**Konu:** Savaş  
**Yer:** `HeroUnit.TryUseAbility`

Can yüzde 35’in altındaysa maksimum canın bir kısmını iyileştirir. Üstündeyse yetenek başarısız döner ve bekleme harcanmaz. `Shield` bileşeni bu yetenekten çağrılmaz. Bone Armor da aynı şekilde yalnız iyileştirmedir.

### 22. Kışlada ölümler tek sayaçta sıraya girer

**Konu:** Savaş  
**Yer:** `BarracksTower.Update`

Her doğumda `respawnTimer` sıfırlanır. Üç asker aynı anda ölürse toplam süre `3 × respawnDelay` olur. Ayrı ayrı sayaç yoktur. Barracks verisinde bu süre 10 saniye, yükseltme seviye başı yüzde 15 kısaltır.

### 23. Sızma canı hasarın yüzde biri, en az 1

**Konu:** Dalga  
**Yer:** `Base.TakeDamage`

Can kaybı `Ceil(hasar / 100)`, taban 1. 12 hasarlık birim de 97 hasarlık birim de 1 can siler. 180 hasar 2 can siler. Yol sonu ham `attackDamage` kullanır. Geçici hasar buff’ı sızmaya yansımaz.

### 24. Reklam başarısı tüketilmeden duruyor

**Konu:** Ekonomi  
**Yer:** `AdManager.OnUnityAdsShowComplete`

`COMPLETED` olunca callback çalışır ve delegate yerinde kalır. İkinci tamamlanma aynı ödülü tekrar ekler. Çark bu yolla ikinci bir `SpinRoutine` başlatabilir. Zafer panelindeki `hasDoubled` kontrolü reklam gösterilmeden önce yapılır, callback içinde tekrar bakılmaz.

### 25. Günlük ödül yerel gece yarısına bakıyor

**Konu:** Ekonomi  
**Yer:** `DailyRewardManager`

Son alışın ertesi gününün tarihi açılır, 24 saat değil. 23:59’da alınan ödül 00:00’da yine açıktır. Ağ saati yoksa `DateTime.Now` kullanılır. Cihaz saati bir sonraki hakkı açar.

### 26. ObscuredInt bozulunca 0 okunur

**Konu:** Kayıt  
**Yer:** `ObscuredInt.Get`, `LivesManager`, `CurrencyManager`

Anahtar statik sözlükte. `vaultId` yoksa veya sağlama tutmazsa `Get` 0 döner. Can bu tiptedir. 0 okunursa üs ölü sayılır ve sızma `TakeDamage` içine girmez, yenilgi de çalışmaz. `fakeHoneyPot` yazılır, hiçbir yerde okunmaz.

### 27. First ve Last farklı yolların indeksini kıyaslar

**Konu:** Savaş  
**Yer:** `Tower.CalculateUnitProgress`

İlerleme `waypointIndex × 1000` eksi sonraki noktaya uzaklıktır. Kısa yoldaki yüksek indeks, üsse daha yakın uzun yoldaki birimi geçer. Tek yol içinde sıra doğrudur.

### 28. ResetSave yedeği geri yükler

**Konu:** Kayıt  
**Yer:** `MetaProgressionManager.ResetSave`

Yalnız `meta_progression.json` silinir, sonra `LoadGame` çağrılır. Dosya yoksa `.bak` çözülür ve `SaveGame` onu tekrar yazar. Şu an bu metodu çağıran bir buton yok. Bağlanırsa sıfırlama tutmaz.

---

## Düşük

**Durum (7 Kasım 2026):** Bu bölümdeki maddeler kodda kapatıldı. Aşağıdaki metin, düzeltmeden önceki hali anlatır.

### 29. Satış iadesi tek altını keser

**Konu:** Ekonomi  
**Yer:** `Tower.GetSellRefund`

İade `investedGold / 2`, tam sayı bölmesidir. İnşa, yükseltme ve uzmanlık bu toplama eklenir. Tek kalan 1 altın kaybolur.

### 30. Kurulum paneli hâlâ Türkçe satır basıyor

**Konu:** Arayüz  
**Yer:** `TowerSelectionUI`

Kışla satırları Asker ve Yenilenme. Yavaş kule Yavaş yazar. Yükseltme panelindeki aynı alanlar Soldiers, Respawn ve Slow.

### 31. Negatif harcama altın ekler

**Konu:** Ekonomi  
**Yer:** `CurrencyManager.TrySpendCurrency`

Tutar 0 veya altı reddedilmez. `CanAfford` negatif fiyatta doğrudur. `gold -= negatif tutar` altın basar. Mevcut kule fiyatları pozitif. Veri eksiye düşerse açık kalır.

---

## Bu turda sağlam çıkanlar

- Zafer ödülü `rewardGrantedThisAttempt` ile bir kez yazılıyor.
- Kule hasarı `TowerDamageBonus`, birim hızı `UnitSpeedBonus`, kule canı `TowerHealthBonus`, atış hızı `SpeedBonus` üzerinden ayrılmış.
- Ancient Wisdom genel çarpandan çıkarılmış, 1.1 ayrıca biniyor.
- Holy Blessing ışık birim hasarına ayrıca 1.15 basıyor.
- Satış, harcanan altının yarısı.
- Işın hasarı `GetCurrentDamage` üzerinden geliyor.
- Yavaş kule menzilini ve düşman katmanını kuleden alıyor.
- Taraf yeteneği dostu iyileştirir, rakip kuleyi hedefler.
- Maç bitince birimler `HasMatchEnded` ile duruyor.
- WIN butonu yalnız editör ve development build içinde kuruluyor.
- Reklam test modu release derlemesinde kapalı.
- Düz JSON yükleme yalnız editörde.
- Kayıt geçici dosyadan `Replace` ile yazılıyor.
- Kışla yükseltmesi yaşayan askerlere `MatchBarracksPower` uyguluyor.
- Yozlaşma çarpanı birim hareketinde okunuyor.
- Uzmanlık kulesinin 1. seviyeden başlaması bilinçli.
- Kahramanın dirilince yetenek beklemesinin dolu olması bilinçli.

## Denge notu

Hard ve Expert hem başlangıç altınını hem öldürme ödülünü 1.5 ve 2 ile çarpıyor. Hazırlığı atlamak kalan saniye × 2 altın veriyor, karma vermiyor. Erken çağırmak hâlâ güçlü bir altın kaynağı. Bunlar hata sayılmadı.

## Okunan çekirdek

`Tower`, `Unit`, `BarracksTower`, `BeamTower`, `SlowTower`, `AuraTower`, `HeroUnit`, `StatusEffect`, `Spawner`, `PhaseManager`, `CampaignManager`, `CurrencyManager`, `MetaProgressionManager`, `AdManager`, `LevelResultUI`, `TowerPlacementManager`, gameplay HUD prefabı, skill assetleri.
