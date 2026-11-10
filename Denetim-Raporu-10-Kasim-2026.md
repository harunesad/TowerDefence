# Tower Defence — Kod Denetimi

**Tarih:** 10 Kasım 2026  
**Kapsam:** `Assets/Scripts`, skill, büyü ve kule verileri  
**Yöntem:** İlgili dosyalar okunarak doğrulandı. Maç oynanmadı. Oyun kodu bu rapor için değiştirilmedi.

**Özet:** 0 kritik, 0 yüksek, 2 orta, 3 düşük bulgu.

6 Kasım raporundaki kapanmış maddeler yeniden okundu. Zehir süresi, kule kapatma, aura yükseltmesi, tek vuruşta tek hasar, kule etrafından dolaşma, bölüm tekrarı ödülü, reklamla ikiye katlama, satış iadesi, 30 günlük takvim, kahraman canı ve okçu hasarının hedefi yerinde duruyor.

---

## Orta

**Durum (10 Kasım 2026):** Bu bölümdeki maddeler kodda kapatıldı. Aşağıdaki metin, düzeltmeden önceki hali anlatır.

### 1. Skill ağacında açık panel, başka düğümü açmıyor

**Konu:** Arayüz  
**Yer:** `SkillTreeUI.OnNodeClicked`

Satın alma çerçevesi açıkken başka bir skille basınca çerçeve kapanıyor. Basılan skillin verisi kullanılmadan dönülüyor. Yeni düğüm görünmüyor. Aynı düğüme bir kez daha basmak gerekiyor.

### 2. Çark hakkı, ödül yazılmadan kayda geçiyor

**Konu:** Ekonomi  
**Yer:** `FortuneWheelUI.SpinRoutine`

Ücretsiz veya reklamlı çevirmede son çevirme zamanı, dönüş animasyonundan önce kaydediliyor. Karma veya kristal animasyon bitince ekleniyor. Uygulama o sırada kapanırsa hak kullanılmış sayılıyor, ödül kasaya girmiyor.

---

## Düşük

**Durum (10 Kasım 2026):** Bu bölümdeki maddeler kodda kapatıldı. Aşağıdaki metin, düzeltmeden önceki hali anlatır.

### 3. Kule inşa sesi iki kez çalıyor

**Konu:** Ses  
**Yer:** `TowerSlot.PlaceTower`, `TowerPlacementManager.SelectTower`

Yerleştirme hem slotta hem seçim kodunda aynı inşa sesini çalıyor. Kule bir kez kuruluyor. Ses üst üste biniyor.

### 4. Sweep paneli bağlı değilse ödül eksik kalır

**Konu:** Ekonomi  
**Yer:** `LevelSelectionUI.OnSweepButtonClicked`

Normal sweep, seviye zorluğu ile genel zorluğu çarpar. Sonuç paneli atanmamışsa yedek hesap yalnız seviye zorluğunu kullanır. Panel sahnede bağlıysa bu yol çalışmaz.

### 5. Crystal Vault kaydı, kristal yazılmadan bellekte duruyor

**Konu:** Ekonomi  
**Yer:** `MetaProgressionManager.TryClaimCrystalVault`, `CampaignManager.GrantLevelStartCrystals`

Hak listeye ekleniyor. Kristal bir sonraki kayıtta diske iniyor. Bu iki adımın arasında oyun kapanırsa aynı bölüm ve zorluk vault’u bir kez daha verebilir.

---

## Kontrol edildi, sorun çıkmadı

Zehir süresi vuruşla sıfırlanmıyor. Kapatma, ışın, yavaşlatma, kışla ve aurayı durduruyor. Aura yükseltmesi halkayı güncelliyor. Tek vuruşta tek hasar duruyor. Kule etrafından dolaşma ve saldırı mesafesi duruyor.

Bölüm tekrarı ödülü, reklamla ikiye katlama, satış iadesi ve 30 günlük takvim ödülü duruyor. Kahraman canı ve okçu hasarı yalnız kendi hedeflerine gidiyor. Büyü açan düğümlerin hepsinde büyü verisi var. Dalga listeleri boş değil. Yayın derlemesinde otomatik kazanma ve efekt deneme paneli yok.

Hasarı 0 olan kuleler yalnız asker çıkaran binalar: Barracks, Graveyard, Knights Guild, Lich Crypt, Paladin Barracks, Zombie Nest. Bunlar ateş etmez.

Kışla yeniden doğma kuyruğu, kadro doluyken fazla kaydı siliyor. Eksik asker bırakmıyor. Eksik askerin kaydı kadro dolmadan silinmiyor.
