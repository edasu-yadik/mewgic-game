# PART 19A · Teknik QA ve optimizasyon

4 Kasım 2026 · Başlangıç `5cc3873` · Unity 6000.3.27f1

## Proje denetimi

Yedi sahne Editor önizlemesinde açıldı. Etkin dört build sahnesi Boot, Home, Gameplay ve LevelMap. Denetim sahne nesnelerini, bileşenlerdeki kırık serileştirilmiş referansları ve YAML dış varlık GUID’lerini kapsadı. Missing Script ve çözülemeyen referans sayısı sıfır. Proje taraması manifest ve paket kilidi sürümlerini, iki sprite atlası ve büyük dokuların platform ayarlarını kaydetti. Ana paketler URP 17.0.3, Input System 1.20.1, Localization 1.5.13 ve Test Framework 1.4.5.

- **Yüksek** — Kayıt aynası yarıda kalırsa eski nesle dönme ve eski oturumdan yeni kaydı ezme riski düzeltildi. Yeni model testleri geçti.
- **Orta** — Editor’da ana iş parçacığı GPU’dan daha uzun sürüyor. Döküşteki yönetilen tahsislerin ve harita kaydırma sırasındaki gereksiz çizim güncellemelerinin bir bölümü kaldırıldı.
- **Düşük / takip** — Unity Pipeline 0.9.1-exp.1 Editor bağlantısında kullanılıyor. Aseprite, Tilemap, SpriteShape, Timeline ve Visual Scripting için taranan oyun kodu ve sahnelerde doğrudan kullanım bulunmadı. Bunlar kaldırma adayları. Paket bağımlılığı, Editor kullanımı ve Addressables doğrulaması olmadan paket veya sanat silinmedi. Üç build dışı sahne geliştirme ve şablon içeriği.
- **Console** — Son ölçüm ve doğrulama oturumlarının logları ham raporlarda. Önceki Editor geçmişinde PART 18 harita başlatma hatası görüldü. Son sahne döngülerinde tekrarlanmadı.

[Proje, sahne, paket ve doku envanteri](PROJECT_AUDIT.json)

## Uygulanan iyileştirmeler

- Döküş geometrisinin şekil ve kırpma listeleri ile akış görseli yeniden kullanılıyor. Aktif döküş tam kare hızında, boşta sıvı çizimi en fazla 30 güncelleme/sn.
- Dünya haritası nesne havuzu yalnız görünür bölge aralığı değiştiğinde yeniden bağlanıyor. Kozmetik dünya ve kristal parçacığı güncellemeleri en fazla 30/sn. Şelale shader zamanı çalışmaya devam ediyor. Aynı malzeme parametresi tekrar yazılmıyor.
- Ana ekran arka planı yeniden yerleşimi boyut değişimine bağlı. Mobil çalışma zamanına 60 kare/sn hedefi verildi. Bu bir hedef ayarı.
- Navigation ve MapProps atlasları korundu. İncelenen büyük ada panelleri 1024×1536, Android ve iOS ASTC 5×5, mipmap ve CPU okunabilirliği kapalı. On bir Resources ada panelinin PNG kaynak toplamı 41,0 MB. Bu değer paket veya çalışma zamanı belleği değildir. Editor GPU sonucu ve mevcut sıkıştırma göz önüne alınarak sanat kalitesi veya sıkıştırma profili değiştirilmedi.

## Kayıt güvenliği

V1 oyuncu anahtarı ve şeması geriye uyumlu. İsteğe bağlı nesil numarası olmayan eski kayıt sıfırıncı nesil olarak açılır. Yeni kayıtta tam belge önce günlük yuvasına yazılır. Ardından ana kayıt ve ayna güncellenir, önceki doğrulanmış nesil saklanır. Açılışta dört yuvadan en yeni geçerli nesil seçilir. Kırık ana belge teşhis için saklanır. Gelecek şema, yanlış JSON türleri ve eski açık oturumun üzerine yazması engellenir. Coin, yıldız ve günlük ödül işaretleri tek belge halinde kalır.

Üretilmiş bölüm arşivinde geçici dosya diske boşaltılıp doğrulandıktan sonra yayımlanır. Kimliği yanlış veya içeriği bozuk ana bölüm dosyası için geçerli yedek veya doğrulanmış geçici kopya kullanılabilir. Yayımlanmış bölümün hiçbir geçerli kopyası kalmadıysa yeniden üretim durdurulur. Her iki son oyuncu kopyası da bozulmuşsa önceki nesle dönmek en son işlemi kaybettirebilir. PlayerPrefs yuvaları aynı platform depolamasını paylaşır. Tam depolama kaybı ve fiziksel güç kesintisi bu testlerle kapsanmadı.

[Edit Mode kayıt ve arşiv senaryoları](STORAGE_TESTS.json)

## Editor performansı

| Sahne | Kare p95 ms | Ana iş parçacığı p95 ms | GPU p95 ms | GC medyan bayt/kare | Draw çağrısı |
| --- | ---: | ---: | ---: | ---: | ---: |
| Ana menü | 4,424 → 4,742 | 4,361 → 4,598 | 0,292 → 0,27 | 18.892 → 18.892 | 78 → 80 |
| Dünya haritası | 4,186 → 4,906 | 4,135 → 4,846 | 0,294 → 0,267 | 18.804 → 18.804 | 50 → 50 |
| Kristal Şelaleler | 4,273 → 4,698 | 4,289 → 4,664 | 0,325 → 0,325 | 18.804 → 18.804 | 88 → 88 |
| Oynanış boşta | 5,876 → 6,92 | 5,734 → 6,863 | 0,324 → 0,292 | 18.804 → 18.804 | 46 → 46 |
| Döküş başlangıcı | 6,319 → 5,717 | 4,909 → 5,378 | 0,325 → 0,287 | 20.884 → 18.804 | 48 → 48 |

Döküş başlangıcında medyan GC tahsisi yaklaşık %10 azaldı. Döküş p95 GC tahsisi 27.434 → 25.162 bayt. Oynanışın boşta ve haritanın p95 CPU örneği bu koşuda yükseldi. Tüm sahneler için genel CPU hızlanması sonucu çıkarılmadı. Home başlangıcındaki büyük GC p95 farkı oturumlar arası değişkenlik içeriyor ve tek başına yerleşim optimizasyonuna bağlanmadı. Ham veriler ayrıca medyan, maksimum, GPU örnek sayısı, batch, bellek ve nesne sayısını içeriyor.

Sekiz Home → Map → Gameplay döngüsünde kullanılmayan varlıklar boşaltılıp GC sonrası aynı sahne örneklendi. Gameplay 86 etkin Transform ile sabit kaldı. Toplam kullanılan Editor belleği 1.973.176.264–1.973.650.608 bayt aralığındaydı ve döngüler boyunca tek yönlü artmadı. 45 saniyelik ek Home beklemesi kısa bir soak kontrolüydü. Uzun oturum sızıntısı veya cihaz belleği için kesin sonuç çıkarılamaz.

Edit Mode kayıt/arşiv denetiminde 16, Play Mode teknik akışında 9 kontrol geçti. Gerçek döküşte aynı akış nesnesinin yeniden kullanılması, arka plan dönüşü, döküş ortasında sahne çıkışı, üç portre oranı, 1.000.000 bölüm odağı ve son tam 50’lik harita bölgesi sınandı. Üretici testinde int.MaxValue kimliği çözülebilir doğrulanmış bölüm olarak arşivlendi. Harita 50’lik tam adalarla sınırlandığı için int.MaxValue civarındaki son kısmi ada gösterilmiyor. Ana oyuncu kaydı ve kurtarma alanları değişmedi. Ek olarak PART 17 ekonomi Play Mode paketi 66 kontrolle yeniden geçti. Sandık animasyonunda diskteki ödül ve zaman işareti yeniden okundu, aynı hak ikinci kez verilmedi. [Ekonomi regresyonu](ECONOMY_REGRESSION.json)

## Test sınırları

Windows Unity Editor, Ryzen 7 6800H, RTX 3060 Laptop GPU ve Direct3D 12. İlgili sahnelerde 900×1600 sabit Game View ile önce/sonra ölçümleri alındı. Etkileşim kontrollerinde ayrıca 900×1950 ve 900×2000 kullanıldı. Ölçüm genel Editor ve test yükünü içeriyor. Draw/batch değerleri örnek sonu sayacı, GPU örneği kullanılabilir kareler üzerinden hesaplandı. Render Thread sayacı Editor yapılandırmasında veri vermedi.

Uygulama kapanması, uyku ve çevrimdışı senaryoları için yaşam döngüsü çağrıları, sahne kapatma ve kayıt yuvası kesinti benzetimleri kullanıldı. Telefon uykusu, bağlantının fiziksel olarak kesilmesi, işletim sisteminin işlemi zorla sonlandırması, termal düşüş ve saatler süren kullanım cihazda test edilmedi.

[Önce](BEFORE.json) · [Sonra](AFTER.json) · [Kristal Şelaleler Game View](Kristal.jpg) · [Oynanış Game View](Oynanis.jpg)

Değişiklikler `GameplayProgress.cs`, `BoosterActions.cs`, `LevelArchive.cs`, `PrototypeApp.cs`, `GameplayBottleView.cs`, `HomeEnvironment.cs`, `WaterfallAnimator.cs`, `MapWorld.cs` ve `MapWorldCrystalJourney.cs` içinde. Denetim ve test girişleri `Part19AEditor.cs` ve `Part19ATechnicalProbe.cs` içinde.

Git teslim dalı recovery/part09-completion.
