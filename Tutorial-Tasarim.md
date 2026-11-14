# Tutorial tasarımı

**Tarih:** 11 Kasım 2026  
**Durum:** Oyunda. Kartlar, kayıt ve editör sıfırlama bağlı. Metinler test için Türkçe.

Oyuncu metinleri test için Türkçedir. Oyun yayınlanınca İngilizceye çevrilir.

---

## Kararlar

1. Konu, Light ile Dark arasındaki savaştır. Oyuncu bu savaşta bir tarafı oynayıp kazanır.
2. Oyun bu sırayla başlar. Menü serbest bırakılmaz.
   - Level 1 zorla Light.
   - Level 1 bitince skill ağacından bir skill satın alınır.
   - Satın alma bitince Level 2 zorla Dark.
3. İlk iki level, bugünkü hallerinden kolay olur. Level 3’ten itibaren mevcut zorluk durur.
4. Kart altta durur. Üstte görsel, altta metin, tek düğme. Anlatıcı Mira’dır.
5. **Anlat** kartı Next ile geçer. **Yaptır** kartı iş bitince kapanır.
6. Maç içindeki kart açıkken oyun durur. Level 1’in ilk dalgası, kule kurulmadan başlamaz.
7. Çift tıkla açılan bilgi kartları yaptırılır. Çift tık, 0.3 saniye içinde ikinci dokunuştur. Bilgi paneli açılınca adım biter. Tek tık seçim veya satın alma içindir.
8. Biten adım meta kayda hemen yazılır. Yarım kalan adım tekrar gelir.
9. İki level ve onları izleyen menü turu bitince öğretici kapanır. Level 3’ten Level 50’ye kadar yeni kart çıkmaz.
10. Level 50 ilk kez kazanılınca son diyalog gelir. Ondan sonra öğretici bir daha açılmaz.
11. Skip yoktur. Taraf, skill ve çift tık adımları atlanmaz.
12. Editörde bir düğme, testi yeni oyun gibi başlatır. Bütün kayıt silinir. Oyuncu sürümünde yoktur.

---

## Kartın hali

Ekranın alt bandı. Üstte görsel, altta en fazla üç cümle, sağ altta tek düğme.

Yaptır adımında düğme eylemin adıdır ve basılamaz. İş bitince kart kapanır.

---

## Kayıt

Meta kaydına biten adım kimlikleri yazılır. Adım kapanınca kayıt iner.

Zorla seçilen taraf da kayda yazılır. Level 1 bitmeden Dark açılmaz. Skill satın alınmadan Level 2 açılmaz. Level 2 bitmeden menü turu başlamaz. Level 50 kazanılmadan son diyalog gelmez.

Öğretici ilerlemesi, kaydın geri kalanından ayrı durur. Sıfırlama yalnız bu listeyi siler.

---

## Hikaye

İki ordu aynı yollar için savaşır. Light yaşayanları yürütür. Dark ölülerini ve gölgelerini yürütür. Her bölüm bir muharebedir. Kapıyı tutan taraf o muharebeyi kazanır.

Mira oyuncuyu önce Light saflarında ilk yola sokar. Kapı tutulunca, kazanılan güçle bir skill alınır. Sonra aynı savaşın öbür yanında, Dark ile ikinci yol oynanır. İki muharebe de bitince oyuncu savaşı kendi seçtiği tarafla sürdürür. Son yol da düşünce Mira savaşı kapatır.

---

## Level 1 ve Level 2 kolaylığı

Bugün Level 1 altı dalgadır. İlk dalgada 14 birim vardır. Level 2 de altı dalgadır. İlk dalgada 15 birim vardır.

Öğreticide ikisi de kısalır:

- Level 1: 3 dalga. Birimler bugünkü ilk dalgaların yaklaşık yarısı. İlk hazırlık, kule kurulana kadar bekler.
- Level 2: 4 dalga. Level 1’den biraz kalabalık, bugünkü Level 2’den seyrek.
- Level 3 ve sonrası değişmez.

Sayılar oynanınca bu bölümde güncellenir.

---

## Bölüm 1 — Light, Level 1

İlk açılışta taraf sorulmaz. Light kilitlidir. Dark düğmesi kapalıdır. Hazır bir Light kadrosu yola çıkar. Oyuncu kadro kurmaz.

| id | Ne zaman | Tür | Görsel | Metin | Bitirme |
|---|---|---|---|---|---|
| war_intro | İlk açılış | Anlat | Karşı karşıya iki ordu | İki ordu savaşıyor. Light yaşayanlarla yürür. Dark ölüleriyle. Her yol bir muharebedir. | Next |
| force_light | war_intro bitince | Yaptır | Light sancağı | İlk muharebeni Light için vereceksin. Bu savaşı Light kazanırsa kapı onların olur. | Light seçildi |
| to_battle_1 | Light seçilince | Yaptır | Green Glade yolu | Ordun hazır. Green Glade’e gir. | Start Battle |
| currency_gold | Maç hazırlığı | Anlat | Altın | Light kuleden altınla tutar yolu. Altının kuleye gider. | Next |
| place_tower | Para kartı bitince | Yaptır | Boş kule yeri | Taş halkalar senin. İlk yürüyüş gelmeden bir kule kur. | Bir kule kuruldu |
| the_road | Kule kurulunca | Anlat | Yolda Dark birlikleri | Karşı taraf yolda kalır. Kapıya varmadan durdur. | Next |
| lives | Yol kartı bitince | Anlat | Kapıda çatlak | Kapıya varan vurur. Kapı düşerse bu muharebe Dark’ındır. | Next |
| tower_menu | İlk dalga bitince | Yaptır | Info, Upgrade, Sell, Priority | Kuleye dokun. Yükselt. Satış harcadığının yarısını geri verir. Öncelik kimin vurulacağını seçer. | Kule menüsü açıldı |
| victory_1 | Level 1 zaferi | Anlat | Tutulan kapı | Kapı durdu. Bu muharebe Light’ın. Aldığın güçle bir skill alacaksın. | Next |

`lives` kartından sonra ilk dalga başlar. `tower_menu` sonraki hazırlıkta açılır. Üçüncü dalgaya kadar açılmazsa kapanır ve bitmiş sayılır.

---

## Bölüm 2 — Skill, sonra Dark

Level 1 zaferinden sonra ana menüye dönünce skill ağacı açılır. Başka düğme bu durak bitene kadar beklemez. Satın alınan düğüm Light Initiation'dır. Karma 150, kristal 0, ön koşul yok. Bedeli 0 olan Light düğümü yok. Bu adımda karma 150'nin altındaysa fark bir kez eklenir. Düğüm zaten alınmışsa satın alma adımı kendiliğinden biter.

Çift tık bilgiyi açar. Tek tık satın alma çerçevesini açar. İkisi de yaptırılır. Sıra bozulmaz: önce bilgi, sonra satın alma.

| id | Ne zaman | Tür | Görsel | Metin | Bitirme |
|---|---|---|---|---|---|
| skill_go | Level 1 zaferinden sonra | Yaptır | Yetenek ağacı düğmesi | Savaştan güç kaldı. Yetenek ağacını aç. | Ağaç açıldı |
| skill_info | Ağaç açılınca | Yaptır | Bir düğüme çift tık | Bir skill’e çift dokun. Ne işe yaradığını oku. | Bilgi paneli açıldı |
| skill_buy | Bilgi kapanınca | Yaptır | Satın alma çerçevesi | Şimdi Light Initiation'ı satın al. Light bu muharebeden güçlendi. | Skill satın alındı |
| force_dark | Satın alma bitince | Yaptır | Dark sancağı | Aynı savaşı bir de Dark tarafından gör. İkinci muharebe onların. | Dark seçildi |
| to_battle_2 | Dark seçilince | Yaptır | Whispering Woods | Dark ile Whispering Woods’a gir. Bu sefer kadronu sen kuracaksın. | Kadro ekranı açıldı |

Light düğmesi bu durakta kapalıdır. Dark seçilmeden Level 2 başlamaz.

---

## Bölüm 3 — Dark kadrosu, Level 2

Kadro ekranında çift tık bilgiyi açar. Tek tık seçer. Üç bilgi de açılmadan savaş başlamaz. Bu derste liste serbest değildir. Görünenler Bone Commander, dört belirli asker, bir boss ve Bloodlust'tır. Başka kahraman, asker veya büyü seçilmez. Yetenek ağacında da yalnız Light Initiation açılır.

| id | Ne zaman | Tür | Görsel | Metin | Bitirme |
|---|---|---|---|---|---|
| hero_info | Kadro, kahramanlar | Yaptır | Bir kahraman | Bir kahramana çift dokun. Kim olduğunu oku. Sonra en fazla ikisini seç. | Kahraman bilgisi açıldı |
| unit_info | Kahraman bilgisi bitince | Yaptır | Bir birim | Bir askere çift dokun. Sonra dört asker ve bir boss seç. Yol onlarındır. | Birim bilgisi açıldı |
| spell_info | Birim bilgisi bitince | Yaptır | Bir büyü | Bir büyüye çift dokun. İstersen üçünü de al. Boş da bırakabilirsin. | Büyü bilgisi açıldı |
| start_dark | Üç bilgi bitince | Yaptır | Dark yolu | Dark hazır. İkinci muharebeyi başlat. | Start Battle |
| currency_soul | Maç hazırlığı | Anlat | Ruh | Dark kuleyi ruhla kurar. Ruh, altının gördüğü işi görür. | Next |
| place_tower_2 | Ruh kartı bitince | Yaptır | Boş kule yeri | Bir kule kur. Bu yol da tutulacak. | Bir kule kuruldu |
| victory_2 | Level 2 zaferi | Anlat | İki sancak | İki tarafı da gördün. Kapıyı ikisinde de tuttun. Savaş bundan sonra senin seçtiğin tarafla sürer. | Next |

Level 2’de kule menüsü tekrar anlatılmaz. `place_tower_1` bitmişse `place_tower_2` yine yaptırılır. Dark’ın parası ayrı bir derstir.

Yenilgide `victory_2` yazılmaz. Level 2 yeniden Dark ile açılır.

---

## Bölüm 4 — İki levelden sonra

Level 2 zaferinden sonra menü turu bir kez oynanır. Sonra öğretici kapanır. Level 3’ten Level 50’ye kadar Mira konuşmaz.

Sıra: Fortune, Daily Reward, Compendium, Heroes.

| id | Ne zaman | Tür | Görsel | Metin | Bitirme |
|---|---|---|---|---|---|
| fortune_go | Menüye dönüş | Yaptır | Çark düğmesi | Savaş yalnız kapıdan ibaret değil. Çarkı aç. | Çark açıldı |
| fortune_tell | Çark ekranı | Anlat | Çark | Günde bir kez öder. Reklamla bir kez daha döner. | Next |
| daily_go | Menüde | Yaptır | Günlük ödül | Ordunun günlük hakkı var. Aç. | Günlük ödül açıldı |
| daily_tell | Günlük ödül | Anlat | Takvim | Gece yarısında açılır. Kaçırılan gün durur. Otuz gün sonra kapanır. | Next |
| compendium_go | Menüde | Yaptır | Ansiklopedi | İki orduyu da tanı. Ansiklopediyi aç. | Ansiklopedi açıldı |
| compendium_tell | Ansiklopedi | Anlat | İki tarafın birimleri | Light ve Dark askerleri buradadır. Sonraki yoldan önce oku. | Next |
| heroes_go | Menüde | Yaptır | Kahramanlar | Yeni kahramanlar burada bekler. Aç. | Kahraman ekranı açıldı |
| heroes_tell | Kahraman ekranı | Anlat | Kilitli bir kahraman | Birini aç, sonraki yürüyüşe en fazla iki kahraman götür. | Next |
| tutorial_rest | Dört durak bitince | Anlat | Uzakta bir yol | Öğrendin. Bundan sonra yollar senin. Savaş bitince yine konuşuruz. | Next |

`tutorial_rest` bitince öğretici kapanır.

---

## Son diyalog

Level 50 ilk kez kazanılınca, zafer ekranından menüye dönünce bir kez oynanır.

| id | Ne zaman | Tür | Görsel | Metin | Bitirme |
|---|---|---|---|---|---|
| war_end | Level 50 zaferinden sonra | Anlat | İki sancak, kapanan bir kapı | Son yol da düştü. Light ve Dark bu savaşta karşı karşıya geldi. Sen iki tarafı da tuttun. Kapılar senin. Savaş bitti. | Next |

Oyuncu Level 50’yi hangi zorlukta kazanırsa kazansın bu kart gelir. Bir kez yazılır.

---

## Savaşın içinde, rastlanırsa

İki levelin içinde, o şey ilk kez olursa bir kez çıkar. Maç durur. Level 3’ten sonra çıkmaz.

| id | Ne zaman | Metin |
|---|---|---|
| barracks | İlk kışla kurulunca | Kışla ateş etmez. Askerleri eder. Move, dönecekleri noktayı seçer. Nokta çemberin içinde kalır. |
| spell_cast | Maçta ilk büyü seçilince | Nişan al, sonra bırak. Light altın öder. Dark ruh öder. Sonra büyü dinlenir. |

---

## Editör testi

Unity menüsünde **Tools → Tower Defence → Tutorialı Baştan Başlat** vardır. Yalnız editörde görünür. Derlenmiş oyunda yoktur.

Düğme bütün kaydı siler. Karma, kristal, levellar, kahramanlar, skiller ve öğretici adımları yeni oyun haline döner.

Oyun çalışırken basılırsa bellek temizlenir, kayıt iner ve ana menü açılır. İlk kart `war_intro` olur. Oyun kapalıyken basılırsa kayıt dosyası silinir. Sonraki Play yeni oyun olur.

---

## Değişecek yerler

Türkçe test metinleri, kolay level sayıları ve son diyaloğun hangi zafere bağlanacağı bu dosyada değiştirilir. Satın alınan skill Light Initiation'dır.
