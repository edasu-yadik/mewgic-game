# Bölüm ilerleme düzeltmesi · 8 Kasım 2026

Başlangıç: `63d0f16` · Unity 6000.3.27f2 · Dal: `release/0.8.0-rc.1`

## Kök neden

Kazanış, bölümün tamamlandığını, en iyi yıldızları ve ödülü kaydediyordu. Ancak ana menünün kullandığı `presentation.currentLevel` yalnız bölüm seçilince veya **Sonraki Bölüm** çalışınca değişiyordu. Sonuç ekranındayken çıkış yapıldığında açık bölüm 2 olsa da devam noktası 1 kalıyordu. Haritadan eski bölüm seçmek ve oradan ilerlemek de devam noktasını geri alıyordu: 101 açıldıktan sonra 99 tekrar oynanıp 100'e geçildiğinde yeniden açılan kayıt 100'ü seçti. Görev ve haftalık ödül panelindeki **Oyna** düğmeleri ayrıca doğrudan bölüm 1 seçimini yapıyordu.

Düzeltme öncesi izole Play Mode testinde raycast ile basılan **Sonraki Bölüm** düğmesi üç kesintisiz zincirde doğru kimlikleri yükledi. [Önceki Unity ölçümü](BEFORE.json) doğrudan tıklama için başarıyı ve iki kayıt gerilemesini ayrı gösterir. Kullanıcının cihazındaki koşullar elde olmadığından aynı cihaz semptomunun tek nedeni olarak sahne veya üretici gösterilemez. Bir hazırlık hatasının gizli oyun etiketine yazıldığı ve sahne isteği reddedildiğinde geçiş kilidinin açık kaldığı kodda ayrıca tespit edildi.

## Uygulama

- Sıradaki yeni bölüm, yıldızlar ve kazanış ödülüyle aynı kayıt işleminde yazılır. Boot ve Oyna akışları devam noktasını ardışık tamamlanan bölümlerden bulur; böylece eski kayıt imleci de doğru bölüme döner.
- Haritadaki seçim oturumda oynanan bölümdür. Eski bölümde **Sonraki Bölüm** o bölümün bir sonrasını açar; kalıcı devam noktası geriye gitmez. **Tekrar Oyna** aynı şişe düzenini sıfırlar. **Harita** gerçek LevelMap sahnesine döner.
- Sonraki tahtanın kimliği doğrulanır ve seçim sahne isteği kabul edilince uygulanır. Hazırlık sırasında sonuç düğmeleri kilitlenir; bir hata olursa görünür mesajla yeniden deneme açılır.
- Var olan yıldız eşikleri, ilk kazanış ve yalnız yıldız farkına verilen tekrar ödülü korunur. Mevcut v2 üretici ve dosyaya sabitlenmiş tahtalar korunur. Bölüm 101'in daha kolay olması, yeni ada başlangıcındaki mevcut zorluk kuralıdır.

## Gerçek doğrulama

- [437 odaklı Unity Play Mode kontrolü](PLAY_MODE.json): **1→2→3→4→5**, **9→10→11**, **99→100→101**. Bulmacalar gerçek şişe düğmelerinden dökülerek kazanıldı. Sonuç düğmeleri UI raycast üzerinden çalıştırıldı. HUD kimlikleri, hamle sayacı, yıldızlar, kilit ve tahta imzaları denetlendi.
- Eski bölüm tekrarı +0 coin ve +0 ipucu verdi. İzole PlayerPrefs günlüğü ve bölüm arşivi yeniden açıldı. Gerçek Boot → Home → Oyna akışı 101'i aynı düzenle açtı. Görev ve ödül merkezi Oyna girişleri de 101'i yükledi. Enjekte edilen hazırlık hatası sonuçta göründü; tekrar tıklayınca 3 açıldı ve ikinci ödül yazılmadı. Üretim oyuncu kayıt anahtarları ve kurtarma kopyaları byte düzeyinde aynı kaldı.
- [80 kayıt ve model kontrolü](MODEL_TESTS.json) geçti. [Unity Test Runner](TEST_RUNNER.json) sıfır kayıtlı PlayMode testi buldu; canlı sahne kontrolleri projenin Play Mode QA bileşenleriyle çalıştırıldı.
- [100. bölüm tekrar sonucu](Result100.png) ve [Boot sonrası 101. bölüm](Level101.png) gerçek Unity Game View kareleri.
- Son C# derlemesinde tanı çıkmadı. Odak testi ve harita regresyonu Unity Console hata, exception ve assert kaydetmedi.
- [105 mevcut harita Play Mode regresyon kontrolü](MAP_REGRESSION.json) geçti; gerçek harita düğmesi → 51 → Sonraki 52 → harita akışı ve bekleyen yolculuğun korunması doğrulandı.
- [10.000 seed üretici denetimi](GENERATOR_AUDIT.json) güncel kaynakla yeniden çalıştırıldı. 10.000 doğrulanmış ve yeniden üretilebilir tahta, sıfır başarısız üretim, sıfır ardışık tekrar. En yakın v2 tahta tekrarı 16 bölüm arayla görüldü. Önceden sabitlenmiş v1 tahtalar korunur; bu v2 tekrar sınırı karma v1/v2 eşleşmelerini kapsamaz.

Cihazda veya bağımsız oyuncu uygulamasında kapat-aç testi yapılmadı. Kapat-aç davranışı Unity'de gerçek kayıt ve arşiv yeniden açılışı ile Boot sahnesinden doğrulandı. Yeni APK, IPA veya TestFlight sürümü üretilmedi.

## Değişen dosyalar

- `Assets/Scripts/Core/BoosterActions.cs`
- `Assets/Scripts/Daily/DailyActivitiesPanel.cs`
- `Assets/Scripts/Presentation/GameplayEndScreen.cs`
- `Assets/Scripts/Presentation/GameplayLevelCatalog.cs`
- `Assets/Scripts/Presentation/PrototypeApp.cs`
- `Assets/Scripts/Presentation/SceneNavigation.cs`
- `Assets/Scripts/Rewards/RewardHubPanel.cs`
- `Assets/Editor/BetaPlayTests.cs`
- `Assets/Scripts/Tests/LevelProgressionProbe.cs` ve `.cs.meta`
- `docs/PRODUCTION_PROGRESS.md` ve bu klasördeki rapor, test çıktıları ve Game View kareleri.

Oyun görselleri, Crystal Falls haritası, kedi animasyonları, sahne ve prefab dosyaları teslim kapsamına alınmadı. Dalda önceden bulunan ilgisiz çalışma dosyaları commit'e alınmadı.
