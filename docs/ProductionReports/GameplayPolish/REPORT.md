# MeWGic · İlk dört bölüm, şişe kutlaması ve günlük görevler

6 Kasım 2026 · Başlangıç `d5d152f` · Dal `release/0.8.0-rc.1`.

## Uygulanan davranış

### 1. Kontrolü oyuncuda bırakan pati öğreticisi

Eski normal akış, yalnız 1. bölümde kontrolü kilitleyip oyuncu yerine bir hamle yapıyordu. Artık normal 1–4 bölümlerde yaklaşık **3 saniye etkileşimsizlik** sonrası pati belirir. Mevcut bulmacadan çözülebilir bir kaynak/hedef çifti seçilir; pati kaynağa ve hedefe gider, kısa basma hareketi yapar ve gerekirse tekrarlar. **Şişeye otomatik tıklamaz; sıvıyı değiştirmez; güçlendiricileri veya oyuncunun dokunmasını engellemez.**

Her şişe etkileşimi pati gösterimini kapatır ve beklemeyi yeniden başlatır. Kaynak zaten seçilmişse, bir sonraki beklemede yalnız uyumlu hedef gösterilir. Başarılı ilk aktarım o bölümün `paw.level.N.v1` kaydını tamamlar; aynı bölümde gereksiz tekrar olmaz. Tamamlanmış bölümlerde, öğrenilmiş derste ve 5+ bölümlerde yoğun öğretici başlamaz. 1. bölümün eski `paw.basic.v1` geçmişi korunur. Pause/restart yardımı güvenli kapatır.

`PawOnboarding.cs` ayrı modüldür. Süreler `PawGuide` alanlarında ayarlanabilir. Mevcut `PawTutorialDefinition`, `TryStartFeature` ve mekanik tamamlama anahtarları korunur. Ayarlar'dan açıkça istenen eski otomatik demo ayrı akışta çalışmaya devam eder; normal oyunda otomatik hamle yapılmaz.

### 2. Tek renk şişe kutlaması

`GameplayBottleView.Present`, kapasitesi dolu ve tamamı aynı renk olan şişenin **tamamlanmamış → tamamlanmış** geçişini izler. Bu geçişte `BottleCompletionGraphic` yaklaşık **1,05 saniyelik** altın çevre ışığı, genişleyen enerji halkası, taban halkası ve 12 küçük parıltı oynatır. Sıvının ortasını kapatmaz; efekt raycast almaz.

Sahne başında zaten tamamlanmış şişe ve aynı verinin tekrar sunulması efekti başlatmaz. Undo ile şişe bozulup gerçekten yeniden tamamlanırsa tekrar kutlanabilir. Her şişenin kendi efekt örneği vardır; birden fazla tamamlanma birbirini kesmez. Pause efekti dondurur; az hareket tercihinde kısa, daha sade gösterim kullanılır. Her karede particle/prefab/texture oluşturulmaz; şişe kurulurken bir UI mesh'i oluşturulur ve oynatma bitince kapanır. Bölüm sonu ödül ekranının mevcut akışı değiştirilmedi.

Bu istekte ayrıca erişilebilir bir şişe tamamlama referans dosyası bulunmadığı için efekt, mevcut oyunun altın/mor sihir diliyle hazırlandı; görülmeyen bir örnekle birebir eşleşme iddia edilmez.

### 3. Oyun içi maskot

`GameplayPresentation.mascotSize` varsayılanı **1,15**. Kedinin sol konumu ve platformu dengelendi; şişe tahtasına yaklaşınca genişlik sınırı uygulanır. Varsayılan tahta düzeninde görünür genişlik artışı yaklaşık **%13** olur. Ana sayfadaki veya haritadaki kedi ölçeği değiştirilmedi. 9:16, 9:19,5 ve 9:20 oranlarında maskotun Safe Area içinde kaldığı ve şişe dikdörtgenleriyle çakışmadığı doğrulandı.

### 4. Görev ikonu ve tıklama

Yıldız simgesinin yerine üç onay işareti, mor kurdele ve altın uçlar taşıyan şeffaf parşömen ikonu eklendi: `Assets/Resources/Daily/QuestIcon_v001.png`. Yerleşim mevcut ana sayfanın sol görev alanında kalır. Yeni ikon yerleşik imagegen aracıyla üretildi; son istem ve üretim bilgisi `GENERATION_PROMPT.md`, alfa/hash kontrolü `ASSET_QA.json` içindedir. Kaynak RGBA 1254×1254; Unity import üst sınırı 512, mipmap/read-write kapalı, Android ASTC 4×4.

**İkonun kendisi, “Görevler” yazısı ve altındaki sayaç aynı `MissionsButton` tıklama alanına bağlıdır.** Görsel çocuklar raycast almaz; ana buton alır ve `Open(false)` ile doğrudan Görevler sekmesini açar. İkon merkezi ve yazı üzerinde gerçek EventSystem raycast kontrolü geçti. Ana sayfa sayacına okunur mor-altın zemin eklendi.

### 5. 24 saat yenileme ve sayaç

Yerel gece yarısında sıfırlama kaldırıldı. `DailyData.taskCycleUtc`, ilk günlük senkronizasyonda UTC tick olarak kaydedilir. 24 saat dolduğunda görev ilerlemeleri, alınan görev ödülleri ve o döngünün kazanış makbuzları tek atomik kayıtta yeni döngüye geçer. Uzun çevrimdışı sürede doğrudan güncel döngüye ilerlenir; birikmiş görev ödülü verilmez. Gece yarısı ve saat dilimi ileri değişimi tek başına 24 saati doldurmaz. Mevcut saat geri alma koruması korunur.

Eski kayıtta yeni damga yoksa mevcut ilerleme ve alınmış ödüller korunarak ilk 24 saatlik döngü başlatılır. `Copy`, doğrulama ve mevcut primary/pending/backup/previous save akışı yeni alanı taşır. Save hatasında eski döngü ve ödül işaretleri korunur.

**Sayaç ana sayfada ikonun/yazının altında `HH:mm:ss`; görev panelinin altında “Yenilenmesine 23:59:59” biçiminde görünür.** Metin saniyede bir hesaplanır; her saniye save yapılmaz. Yenileme gerektiğinde veya kayıt değiştiğinde senkronizasyon yapılır. Panel açıkken mevcut periyodik saat gözlemi sürer. Uygulamaya dönüldüğünde sayaç yeniden hesaplanır. Görev ödül ikonları ve `+1` miktarı biraz büyütüldü; mevcut ödül türleri ve miktarları korunur. Günlük hediye ekonomisi ve 24 saatlik hediye hakkı değiştirilmedi.

## Dosya listesi

| Dosya | İşlev |
|---|---|
| `Assets/Scripts/UI/PawGuide.cs` | Normal rehber ile açıkça istenen demo ayrımı, etkileşim bağlantıları |
| `Assets/Scripts/UI/PawOnboarding.cs` | Yeni, kontrolü engellemeyen 1–4 bölüm yardımı |
| `Assets/Scripts/Presentation/PrototypeApp.cs` | Şişe dokunuşunu rehbere bildirme |
| `Assets/Scripts/Presentation/GameplayBottleView.cs` | Tamamlama geçişi ve bağımsız efekt kurulumu |
| `Assets/Scripts/Presentation/BottleCompletionGraphic.cs` | Yeni yeniden kullanılabilir kutlama mesh'i |
| `Assets/Scripts/Presentation/GameplayPresentation.cs` | Oyun kedisinin güvenli ölçeği/platformu |
| `Assets/Scripts/Core/DailyActivities.cs` | Kalıcı 24 saat görev döngüsü |
| `Assets/Scripts/Daily/DailyActivitiesPanel.cs` | İkon alanı, sayaçlar, ödül sunumu |
| `Assets/Resources/Daily/QuestIcon_v001.png` ve meta | Yeni görev ikonu |
| `Assets/Localization/Source/Mewgic.json` ve 8 dil tablosu | Yalnız `daily.reset` geri sayım metni |
| `Assets/Editor/GameplayPolishValidation.cs` | Model/save testleri, import ve QA menüsü |
| `Assets/Scripts/Tests/GameplayPolishProbe.cs` | İzole gerçek Unity testleri ve ekran görüntüleri |
| `Assets/Editor/DailyActivitiesTests.cs` | Eski gece yarısı beklentisini 24 saate uyarlama ve geçici kayıt temizliği |

Sahne veya prefab dosyası değiştirilmedi; mevcut runtime UI kurulumu kullanıldı. Önceki harita akışı, mağaza/ikon ayarları, onaylı karakter/sahne sanatı ve kullanıcı iksir çalışmaları korunur. Başlangıçta değiştirilmiş TMP fallback dosyası teslim kapsamına alınmaz.

## Doğrulama

- `MODEL_TESTS.json`: **16** yeni model/save kontrolü; gece yarısı, 23:59:59/24:00:00 sınırı, çevrimdışı süre, eski kayıt, ödül tekrarı, save hatası ve gerçek PlayerPrefs/journal yeniden açma. Mevcut puzzle/booster model regresyonu da çağrıldı.
- `DAILY_REGRESSION.json`: **53** mevcut günlük sistem kontrolü; hediye, görev, kayıt, ödül tekrar koruması ve saat geri alma senaryoları geçti.
- `UNITY_TESTS.json`: **74** Play Mode kontrolü; 1–4 gerçek bölüm, bekleme, kontrolün oyuncuda olması, doğru çift/gerçek şişe raycast'i, ilk hamle kaydı, seçili kaynak, 5+ kapsam dışı, şişelerin bağımsız kutlaması/tekrar/başlangıç/pause/az hareket, gerçek döküş, üç portre oranı, doğrudan görev ikonu/yazı raycast'i, canlı sayaç, 25 saat çevrimdışı yenileme ve Ayarlar demosu.
- Oyuncunun primary/pending/backup/previous/corrupt anahtarları test öncesine göre byte düzeyinde aynı kaldı. Testler izole profilde çalıştı.
- C# derlemesi ve son Console: **0 hata, 0 uyarı**. Kanıtlar `COMPILE.json`, `CONSOLE.json`. Görseller `GALLERY.html` altında. `07-Real-Pour-Completion.png` özgün bölüm tasarımı değil, gerçek döküş entegrasyonunu doğrulayan izole QA bulmacasıdır.
- Yerelde kurulu Unity **6000.3.27f1** kullanıldı; ana projenin **6000.3.27f2** sürüm dosyası byte düzeyinde geri yüklendi. Fiziksel telefon, GPU/termal ölçümü, gerçek işlem sonlandırma veya yeni Android/iOS dağıtım build'i yapılmadı.

## Unity'de deneme

1. `Mewgic > Gameplay Polish > Run Isolated Play Mode QA`: oyuncu kaydını kullanmadan tüm akışı ve ekran görüntülerini üretir. Test nesnesini sahneye kaydetmeyin.
2. `Mewgic > Gameplay Polish > Validate Daily Save Rules`: 24 saat ve kayıt kurallarını doğrular.
3. `Home.unity`: sol parşömen ikonuna veya “Görevler” alanına basın. Görevler sekmesi açılmalı; ilerleme/ödüller ve panel altındaki canlı sayaç görünmeli.
4. `Gameplay.unity`: normal oyun üzerinden ilk dört bölüme girip yaklaşık 3 saniye bekleyin. Patiyi izleyin; şişeye dokununca kaybolduğunu ve başarılı aktarım sonrası tekrar etmediğini kontrol edin. 5. bölümde yoğun yardım çıkmaz. Tamamlanmış veya öğrenilmiş bölümde yardımın çıkmaması beklenen davranıştır; temiz test için izole QA menüsünü kullanın.
5. Bir şişeyi dört aynı renkle doldurun; kısa altın kutlamayı, tekrar sunumda yeniden başlamamasını ve Pause davranışını kontrol edin. 24 saat bekleme gerektiren uç durumlar QA'daki kontrollü saatle sınanır; gerçek oyuncu kaydını/sistem saatini değiştirmeyin.

Sonraki öneri: aynı akışın düşük/orta sınıf Android ve iPhone üzerinde dokunma, sayaç okunurluğu ve frame-time kabulü.
