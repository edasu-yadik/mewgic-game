# MeWGic · Harita ve bölüm ilerleme akışı

5 Kasım 2026 · Ana proje: `C:/Dev/Mewgic` · Başlangıç: `23bf123`.

## Bulgu ve çözüm

Harita iki katmandan oluşuyor: 12 adalık dünya seçicisi ve her adada dikey kaydırılan 50 bölüm. `MapProgressionCatalog`, `MapProgression`, `WorldMapRules` ve kazanılmış `LevelRecord` kayıtları erişim/yıldız durumunu belirliyor. Detay haritası görünür bölümler için düğüm havuzu kullanıyor; teras sanatı, rota ve maskot ayrı UI katmanları.

Eski `journeyFrom/journeyTo` yalnız bellekteydi. `TakeMapJourney` harita girişinde bilgiyi siliyor; bölüm seçme, Sonraki ve yeniden başlatma da bekleyen geçişi temizliyordu. Kesilen/geciktirilen harita dönüşü ve uygulama yeniden açılışı bu nedenle görsel ilerlemeyle bağını kaybediyordu. Önceki ışık darbeleri gerçek pati şekli oluşturmuyordu.

Yeni `MapJourneyData` kayıt belgesinin `worldMap.journey` alanında saklanır. `displayedLevel`, `pendingFrom`, `pendingTo`, `lastShownLevel` ayrıdır; `CurrentUnlockedLevel` ve ardışık `CurrentCompletedLevel` kazanılmış bölüm kayıtlarından hesaplanır. Düğüm kimliği mevcut ada/slot kataloğundan gelir; ikinci bir kilit/ödül sistemi kurulmadı.

İlk kez ilerleme sınırındaki bölüm kazanılınca ödül ve bekleyen yolculuk aynı atomik kayda yazılır. Okuma bileti tüketmez. Doğal bitiş veya açık atlama hedefi onaylar, kedinin gösterilen konumunu günceller ve pending alanlarını aynı kayıtta temizler. Kayıt hatası bileti tüketmez; UI mevcut kayıt hata mesajını gösterir. Eski kayıtlar mevcut ilerleme noktasına sessizce taşınır; tarihsel kazanışlar tekrar oynatılmaz.

## Görsel ve hareket

- Mevcut onaylı şelale/teras, kristal, kale ve çiçek çizimleri korundu. Referans `Reference.png` olarak rapora alındı; oyunun arka planına düz ekran görüntüsü yapıştırılmadı.
- Düğümler yuvarlak kubbe biçimine, mor yüzeye ve altın kenara geçti. Açık fakat oynanmamış düğümlerde boş yıldızlar görünür. Mevcut bölüm mor-altın halka ve kısa büyüme/parıltı alır.
- Başlık ve alt navigasyonda mevcut mor-altın panel sanatı kullanıldı. Kristal haritanın son terasında 91–100 birlikte görünür; başlık görünen onlu aralığı takip eder.
- Kedi doğrudan oyunun `MageCat_FullBody_v001.png` ana maskotudur. Referanstaki kedi kullanılmadı. Son 100 düğümünde solda durur; bulunduğu dünyada hafif nefes hareketi ve temas gölgesi vardır. Başka adalar incelenirken yanlış bir oyuncu konumu gösterilmez.
- Varsayılan akış 2,2 saniye: eski konum → hafif sekmeli yürüyüş → sıralı altın patiler → hedefte kilit açılışı/altın ışık patlaması → idle. Inspector'da `Journey Duration` 1,5–3 saniye, `Arrival Fraction` ve `Journey Effect Budget` ayarlanabilir. Ada geçişinin perdesi ve yürüyüş zamanlaması süreye göre ölçeklenir.
- `JourneyPawTrail` tek UI mesh'idir. Ana yastık ve dört parmak oval geometriden oluşur; yumuşak dış ışık katmanı vardır. Rota teğeti yönü ve dönüşümlü sağ/sol offset kullanılır. 4–12 iz sırayla belirir, varışta söner; en fazla 1.296 vertex. Her adımda nesne, sprite, texture veya particle oluşturulmaz.
- Animasyon sırasında viewport dokunuşu atlar ve hedefe odaklanır. Scroll ataleti durdurulur; otomatik takip sırasında scroll girdisi geçici kapatılır, sonunda yeniden açılır. Geçişteki dokunuş aynı anda bölüme giriş yapmaz.
- Az hareket tercihi kısa hedef gösterimine geçer. Doğrudan LevelMap açılışında yerelleştirme hazır olmadan yürüyüş başlatılmaz.

## Ne zaman çalışır?

Yeni bir bölüm ilk kez açılınca çalışır. Haritaya daha sonra gelmek, uygulamayı yeniden açmak, Sonraki ile devam etmek veya eski bölümü tekrar oynamak bekleyen bileti silmez. Ardışık kazanışlar tek kısa yolculukta son hedefe birleştirilir. Bekleyen geçiş yoksa harita doğrudan güncel bölüme odaklanır.

Normal harita girişleri ve tamamlanmış bölüm tekrarları yeni animasyon oluşturmaz. Tamamlanan/atlanan yolculuk yeniden oynatılmaz. Gösterim bitmeden sahne/uygulama kapanırsa kayıt bekler ve sonraki uygun harita girişinde baştan bir kez tamamlanır. Arka plana geçiş gösterildi işareti yazmaz.

## Dosyalar

| Dosya | Değişiklik |
|---|---|
| `Assets/Scripts/Core/MapJourneyData.cs` | Kalıcı bilet, eski kayıt uyumluluğu, okuma ve atomik onay |
| `Assets/Scripts/Core/WorldMapData.cs` | Yeni durumun kayıt kopyasına ve başlangıç doğrulamasına eklenmesi |
| `Assets/Scripts/Core/BoosterActions.cs` | İlk kazanışta ödül ile birlikte yolculuk kaydı |
| `Assets/Scripts/Presentation/GameplayLevelCatalog.cs` | Select/Next sırasında pending silinmesinin kaldırılması |
| `Assets/Scripts/Presentation/PrototypeApp.cs` | Yeniden başlatmada pending korunması |
| `Assets/Scripts/Map/MapWorld.cs` | Başlangıç odağı, yerelleştirme sonrası başlangıç, pause davranışı |
| `Assets/Scripts/Map/MapWorldJourney.cs` | Panel stili, onlu aralık başlığı ve kısa ekran yerleşimi |
| `Assets/Scripts/Map/MapWorldJourneyMotion.cs` | Yürüyüş, idle, süre ayarları, kamera, dokunarak atlama ve ack |
| `Assets/Scripts/Map/JourneyPawTrail.cs` | Havuzlu, sıralı altın pati mesh'i |
| `Assets/Scripts/Map/JourneyRouteGraphic.cs` | Kubbe taş geometrisi |
| `Assets/Scripts/Map/MapWorldProgression.cs` | Altın durum renkleri ve boş yıldızlar |
| `Assets/Scripts/Tests/MapFlowProbe.cs` | İzole canlı Unity/gerçek oynanış testleri ve kareler |
| `Assets/Editor/MapFlowValidation.cs` | Model/save testleri ve Unity test menüsü |
| `Assets/Scripts/Tests/Part15DJourneyProbe.cs` | Eski testin yeni kalıcı durum ve süre beklentilerine uyarlanması |

## Unity'de deneme

1. `Assets/Scenes/LevelMap.unity` sahnesini açın. Normal Play mevcut kaydınızdaki bölüme gider; ilk kez kazanılmış bir sonraki bölüm varsa yürüyüşü gösterir.
2. Oyuncu kaydına dokunmadan tüm senaryolar için `Mewgic > Map Flow > Run Isolated Play Mode QA` menüsünü çalıştırın. Test kendisi Play Mode'a girer ve bitince çıkar; test nesnesini sahneye kaydetmeyin.
3. Yalnız kayıt kuralları için `Mewgic > Map Flow > Validate Save Model` menüsünü kullanın.
4. Gerçek akış: bölüm düğmesi → bulmacayı tamamla → Harita. Ayrıca Sonraki → pause/çıkış → Harita yolunu deneyin. Yürüyüşte haritanın içine dokunmak hedefe atlar.

## Doğrulama ve sınırlar

Son sonuç: **80 model/save kontrolü + 69 Unity Play Mode kontrolü geçti**. C# derlemesi ve son Console: **0 hata, 0 uyarı**. `COMPILE.json`, `CONSOLE.json`; gerçek Game View kareleri `GALLERY.html` altında.

`MODEL_TESTS.json`: eski kayıt, ilk kazanış, başarısız kazanış/save, JSON round-trip, gerçek PlayerPrefs/journal yeniden açma, replay, yanlış/idempotent ack, ardışık kazanış ve mevcut puzzle/booster model regresyonu. Gerçek oyuncunun kayıt anahtarının korunması doğrulanır; geçici test anahtarları temizlenir.

`UNITY_TESTS.json`: gerçek Play Mode yürüyüşü, viewport içinde pati geometrisi, varış kilidi, bitişte kalıcı durum, tekrar giriş/replay, arka plan, kesilip yeniden yükleme, gerçek UI raycast/atlama, 12 ada sınırı ve 601 devamı, 9:16 / 9:19,5 / 9:20 kareleri. Gerçek 51 bulmacası şişe butonlarıyla çözülür; Sonraki ile 52'ye girilip pause çıkışıyla haritaya dönülür ve bekleyen yolculuk doğrulanır. İzole test profili gerçek oyuncu oyun kaydını kullanmaz.

Yerelde kurulu `6000.3.27f1` ile Editor/Play Mode kullanıldı. Projenin `6000.3.27f2` sürüm dosyası doğrulamadan sonra byte düzeyinde geri yüklendi; paket ve oyuncu ayarları değiştirilmez. Fiziksel telefon, GPU/termal ölçümü, gerçek işlem sonlandırma ve yeni Android/iOS build'i bu testlere dahil değildir. Kaydın kalıcılığı gerçek save sistemiyle yeniden açılarak; kesilme ise sahne yıkımı/yeniden yükleme ile doğrulandı.

Referansın köprü/şelale/ışık/mor-altın ruhu yaklaştırıldı; onaylı sanatın köprü kıvrımları ve ana maskotun duruşu referansla birebir aynı değildir. Sonraki uygun iyileştirme: fiziksel düşük/orta seviye Android ve iPhone'da dokunma, Safe Area ve frame-time kabulü; ardından aynı maskot için özel yan yürüyüş sprite pozları.

Önceden var olan TMP fallback, iksir çalışmaları ve diğer kullanıcı dosyaları teslim kapsamı dışındadır. Sahne dosyaları ve onaylı sanat PNG'leri değiştirilmedi.
