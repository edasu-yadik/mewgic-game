# Oyun güçleri entegrasyonu

11 Kasım 2026 · Unity 6000.3.27f2 · Gameplay ve Home · İzole oyuncu profili

## Bulmaca kuralları

Standart bölümlerde hamle sınırı yok. Hamle sayısı yıldız performansında kullanılıyor. Şişe kapasitesi dört katman. Geçerli döküş, aynı renkteki üst katmanları boş veya üst rengi eşleşen hedefe, kapasiteye sığan azami miktarda aktarır. Prosedürel tarif değişmedi. Sonuçta boş şişeler ile dört eşit renk katmanlı dolu şişeler kalır.

| Güç | Fiyat | Kullanım |
|---|---:|---|
| Ekstra Hamle | 500 önizleme | Tasarım onayı bekliyor. Hamle sınırı ve satış eklenmedi. Sınırlı bir bölüm türü onaylanırsa yalnızca orada anlamlı olur. Mevcut standart bölümlerde yakın alternatif Geri Al. |
| Ek Şişe | 800 altın | Deneme başına bir boş şişe. Çekiç sonradan bir şişe kaldırsa bile aynı denemeye ikinci boş şişe eklenmez. Yeni şişe kısa parlar. |
| İpucu | 1.000 altın | Mevcut tahta için doğrulanan çözümün ilk döküşünü gösterir. Kaynak ve hedef işaretlenir. Açık ipucu ikinci kez stok harcamaz. |
| Karıştır | 1.200 altın | Çözülebilirliği doğrular, şişeleri bütün halinde döndürür ve renk katmanlarını korur. Şişeler kısa parlar. |
| Renk Değiştir | 1.500 altın | 101. bölümden itibaren. Seçilen ve uygun ikinci şişenin üst renkleri takas edilir. Çözüm kısalır veya çözümsüz tahta doğrulanmış çözüme kavuşursa çalışır. Denemede bir kez. İki şişe parlar. |
| Şişe Çekici | 1.800 altın | 101. bölümden itibaren. Seçilen dolu ve tamamlanmamış şişeyi geçerli döküşlerle boşaltır, ardından boş kabı kaldırır. Çözüm kısalmalı ve en az bir gerçek döküş kalmalı. Denemede bir kez. Çıkan kabın altın kıvılcım ve solma animasyonu var. |

Çekiç araması 120 ms, 64 aktarım düğümü ve her çözücü çağrısında 6.000 durumla sınırlı. Her aktarma normal maksimum döküş kuralını kullanır. Şişe ancak tamamen boşaltıldıktan sonra silinir. Renk adetleri ve kapasite korunur. Tamamlanmış şişe, geçersiz seçim, çözüm bulunamaması, süre sınırı veya kayıt hatasında stok değişmez. Çekiç ve Renk Değiştir önceki döküşlerin Geri Al geçmişini sıfırlar. Yalnızca döküş iziyle doğrulanan rekabet akışına Çekiçli deneme gönderilmez.

Satın alma, altın ve stok değişikliğini mevcut işlem numarasıyla tek kayda yazar. Eski profilde Çekiç stoğu sıfırdan başlar. Stok yoksa oyun içindeki düğme Oyun Güçleri mağazasını açar. Mağaza kapandığında bölüm ve şişe durumu korunur. Oyun içi güç stoku bir kayıt alanıdır. Yarım kalan denemenin şişe düzeni mevcut oyun davranışı gereği diske kaydedilmez.

## Unity Game View

- [Ek Şişe](ExtraBottle.png) · [Yedi şişe 9:16](ExtraBottleSeven.png) · [Yedi şişe 9:20](ExtraBottleSevenTall.png)
- [İpucu](Hint.png)
- [Karıştır](Shuffle.png)
- [Renk Değiştir](ColorSwap.png)
- [Şişe Çekici](BottleHammer.png) · [Oyun içinden açılan mağaza](HammerShop.png)

10.001. bölümde beş güç aynı denemede gerçek UI tıklamalarıyla kullanıldı. Ardından normal döküşlerle kazanış ve yeni bölümün açılması doğrulandı. Tekrar Oyna özgün düzeni geri getirdi ve tekrar kazanış ek ödül vermedi. 10.005. bölümde yedi şişeli düzenin çözülebilirliği aramayla doğrulandı ve Game View yerleşimi kontrol edildi. Ayrıca bağımsız güç işlemleri, stok ve bozuk ana dosyadan yedek kurtarma, yinelenen satın alma, başarısız kayıtta stok ve tahta korunması denendi.

- [138 oynanış kontrolü](PLAY_MODE.json)
- [722 mağaza regresyon kontrolü](SHOP_PLAY_MODE.json) · Dört mağaza sekmesi ve üç portre oranı. Çekiç erken seviyede kilitli, Ekstra Hamle önizleme. Oyuncu kaydı özeti aynı ve Console hatası yok.
- [437 bölüm ilerleme regresyon kontrolü](PROGRESSION_PLAY_MODE.json) · Erken bölüm zincirleri, tekrar oynama, kayıt ve sonraki bölüm.
- Unity menüsü. `Mewgic → QA → Run Gameplay Booster Integration`.

## Değişen dosyalar

- Bulmaca ve envanter. `Puzzle.cs`, `BoosterActions.cs`, `MagicShop.cs`, `EconomyConfig.cs`, `Balance.json`, `GameplayProgress.cs`.
- UI ve etkiler. `GameplayTools.cs`, `PrototypeApp.cs`, `PremiumButton.cs`, `MagicShopPanel.cs`, `Offers.json`. Oynanışta kullanılan iki ikon, mevcut onaylı mağaza atlasından alındı.
- Uyumluluk. `ClanClient.cs` döküş izi kuralı, yerelleştirme kaynak ve sekiz tablo, `LevelProgressionProbe.cs`, `MagicShopProbe.cs`.

Fiziksel cihaz testi yapılmadı. Yeni mobil build ve sunucu dağıtımı yapılmadı.
