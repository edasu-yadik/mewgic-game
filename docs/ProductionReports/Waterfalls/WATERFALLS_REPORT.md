# Home ve Gameplay şelale animasyonları

30 Ekim 2026 — kullanıcı başlığı: PART 10C / şelaleler. Önceki üç yıldız paneli PART10C raporu korunmuştur. Proje `C:\Dev\Mewgic`, geliştirme dalı `recovery/part09-completion`, başlangıç `f13d6e5362ef4bef83d01ddc9cb6d80de8e6ae1c`.

## İnceleme ve uygulama

Home şelaleleri üç boyalı mimari katmana gömülüydü. PART09B'de üretilmiş üç adet 256×512 su/pencere maskesi ve HomeWaterLight shader'ı zaten çalışıyordu. Bu maskeler, PNG'ler, mimari yerleşim ve pencere ışığı davranışı korundu; ikinci bir animasyon sistemi eklenmedi.

Gameplay şelaleleri tek `Gameplay_Backdrop_v001.png` içine gömülüydü ve animasyon bileşeni yoktu. Aynı shader yeniden kullanıldı. Kaynağın üst-sol normalize koordinatlarında 13 dar şelale zarfı ve mavi su renk ayrımıyla, yumuşak kenarlı 256×512 veri maskesi üretildi. Kaynak resim değiştirilmedi. `Gameplay_MaskOverlay.png` turuncu denetim kaplaması yalnız rapor görselidir; oyunda kullanılmaz.

Ortak `WaterfallAnimator`, Image'ın mevcut çiziminde tek kalıcı materyal kullanır. Maskenin kırmızı kanalında küçük UV dalgalanması ve aşağı hareket eden hafif su ışığı çalışır. Yeşil kanaldaki mevcut Home pencere ışıkları aynı kalır; Gameplay maskesinde yeşil kanal sıfırdır. Bütün resim veya mimari UV'si kaydırılmaz. Yeni sis/parçacık eklenmedi; Home'un mevcut sisi korundu.

| Ayar | Home | Gameplay |
| --- | --- | --- |
| Akış hızı (normalize UV/sn) | 0.022 | 0.012 |
| Işık genliği | 0.045 | 0.028 |
| Hareket opaklığı | 0.80 | 0.65 |

Kalıcı ayarlar HomeEnvironment ve GameplayPresentation Inspector'ındadır. Play Mode'da oluşturulan her WaterfallAnimator üzerinde de ayarlanabilir. Zaman monoton ilerler; shader'da animasyon karesi/UV sıfırlayan sert döngü yoktur. Gameplay efekti şişelerin ve bütün kontrol panellerinin arkasındaki arka plan Image'ındadır; raycast yakalamaz.

## Gerçek Unity testleri

Unity 6000.3.27f1 korundu. MCP durum çağrısı Editor bağlantısını doğruladı; Pipeline komutları Unity CLI üzerinden aynı açık Editor'a gönderildi. CLI sürüm güncellemesi yapılmadı.

`TESTS.json`: **39 kontrol geçti**, çalışma sırasında **0 hata / 0 uyarı**, gerçek oyuncu kaydı aynı kaldı. `COMPILE.json`: C# hata/uyarı yok. `CONSOLE.json`: test sonu Editor Console **0 hata, 0 uyarı**, derleme başarısızlığı yok.

- Home'da mevcut üç animatör, Gameplay'de tek animatör doğrulandı.
- Gerçek PLAY → Gameplay ve Gameplay → Home geçişleri geçti. Dönüşte katman çoğalmadı; Home materyalleri Gameplay geçişinde serbest bırakıldı.
- Gerçek oyun modeline bağlı şişe seçimi, sıvı aktarımı, Undo ve Restart geçti. Buton olayları otomatik testte mevcut oyun kodunu çağırdı.
- Home ve Gameplay için 900×1600, 900×1950 ve 900×2000 gerçek Unity kamera görüntüleri alındı, görsel olarak incelendi. Son oranda üst/alt Safe Area inset simülasyonu var. Arka planın cover ölçeklemesiyle maske de aynı UV'lerde kalıyor. Yeni taşma/okunurluk sorunu görülmedi.
- Her iki sahnede 32 zaman damgalı hareket karesi alındı. İlk/son kare aralığı Home'da 5.36 sn, Gameplay'de 5.35 sn. `frameTimes`, gerçek `Time.unscaledTime` değerlerini saklar.
- GPU yön testi sabit gri kaynak ve kırmızı kontrol maskesinde bir piksel aşağı kaymayı karşılaştırdı: aşağı yön hatası **0**, yukarı yön hatası **1.390.702**. Akış aşağı yönlüdür.
- Her katmanda 1000 WaterfallAnimator güncellemesi **0 managed byte** tahsis etti. Yaklaşık 5 saniyelik hareket yakalama boyunca sahne nesne sayısı sabit kaldı.

GPU'da 0 ve 2.8 saniye karşılaştırması:

| Katman | Değişen su pikseli | Maske dışı incelenen / aynı kalan piksel |
| --- | ---: | ---: |
| Home uzak adalar | 2467 | 125020 / 125020 |
| Home ana şato | 4994 | 121458 / 121458 |
| Home köy | 1043 | 125669 / 125669 |
| Gameplay arka planı | 3842 | 122093 / 122093 |

Bu karşılaştırma bütün görüntünün farkını su hareketi saymaz; maske içi su ve maske dışı sabit pikseller ayrı ölçülür. Mevcut yeşil pencere ışığı maskesi mimari sabitlik ölçümünün dışındadır.

Kurulum aracının ilk rapor serileştirmesinde Unity Rect nesnesinin döngüsel özellikleri nedeniyle hata alındı; rapor yalnız x/y/width/height değerlerini yazacak şekilde düzeltildi. Son kurulum ve test turu yeniden geçti. İlk geliştirme sırasında eksik test sınıfı derleme hatası, test dosyası tamamlanınca giderildi. Sonuçlar bu temiz son tura aittir.

## Hareket ve görsel teslim

- `Home-Motion.gif`, `Gameplay-Motion.gif`: gerçek Unity karelerinden tam ekran hareket önizlemeleri.
- `Home-WaterDetail.gif`, `Gameplay-WaterDetail.gif`: aynı karelerden büyütülmüş su bölgesi; ince hareketi görmek içindir.
- `Home-TimeA/B.png`, `Gameplay-TimeA/B.png`: farklı zamanlarda alınmış gerçek kareler.
- `Home-1600/1950/2000.png`, `Gameplay-1600/1950/2000.png`: mobil oran kanıtları.
- `AspectRatios.png`: altı oran görüntüsünün temas sayfası.
- `Gameplay_Mask.png`, `Gameplay_MaskOverlay.png`, `MASK.json`: yeni maskenin denetim verileri.

GIF süreleri gerçek kare zamanlarına göre ayarlandı. Yakalama GPU readback/PNG maliyeti içerir ve cihaz FPS ölçümü değildir. GIF'in sonundan başına dönmesi kısa kaydın yeniden oynatılmasıdır; shader'ın kendi döngü sıfırlaması değildir. 64 ham `*-frame-*.png` yerelde tutulur, Git'e eklenmez; GIF'ler ve iki zaman karesi sürümlenir.

## Mobil performans ve sınırlar

Home'da önceden mevcut üç materyal/maskenin sayısı aynı kaldı. Gameplay tek kalıcı materyal ve **512 KiB GPU maske** ekler; maskenin okunabilir CPU kopyası yaklaşık 512 KiB daha tutar. Üç Home maskesi toplam 1.5 MiB GPU verisiyle aynen korundu. Materyaller sahne yok edilirken serbest bırakılır.

Shader tek geçişte üç texture örneklemesi yapar: kaynak, maske, hafif kaymış kaynak. Gameplay'in mevcut arka plan çiziminde iki ek örnekleme maliyeti vardır; ek tam ekran postprocess, RenderTexture, overlay çizimi veya ParticleSystem yoktur. Test yakalamasındaki RenderTexture/PNG tahsisleri üretim animasyonuna ait değildir. Sürekli nesne/materyal oluşturma yoktur.

Fiziksel iPhone/Metal GPU süresi, pil ve FPS ölçülmedi. Bu nedenle 60 FPS veya cihaz performansı garantisi verilmez. Hareket özellikle Gameplay'de düşük yoğunlukludur; bazı küçük/sisli su parçaları maskenin güvenli sınırları dışında statik kalır. Dinamik geometri/sıvı simülasyonu yoktur; mevcut boyalı tasarım korunur.

## Koruma, dosyalar ve Git

Proje dışı başlangıç dökümü: `C:\Dev\Mewgic-Waterfalls-Baseline.json`. `PRESERVATION.json` test sonrası yalnız dört mevcut dosyanın değiştiğini doğrular:

1. `Assets/Scripts/Presentation/HomeEnvironment.cs`: mevcut su materyali yönetimi ortak bileşene bağlandı; ayarlar açıldı.
2. `Assets/Scripts/Presentation/GameplayPresentation.cs`: yalnız arka plan su bileşeni ve ayarları.
3. `Assets/Shaders/HomeWaterLight.shader`: kontrollü aşağı akış parametreleri.
4. `Assets/Scenes/Gameplay.unity`: yalnız shader/maske ve üç su ayarı referansı.

Yeni varlıklar: WaterfallAnimator.cs, Gameplay_WaterMask.asset, WaterfallEditor.cs, yalnız Editor'de çalışan WaterfallProbe.cs ve meta dosyaları; bu rapor/kanıt klasörü. İlerleme belgesine görev kaydı eklendi. Unity'nin yeniden serileştirdiği TMP fallback asset'i başlangıç baytlarına geri getirildi. Onaylı görsel PNG'leri, şişeler, karakter, PLAY, HUD, Pati Yardımı, görev/ödül/kayıt kodları değiştirilmedi. Önceden mevcut untracked dosyalar commit dışında korundu.

Git teslimi yalnız doğrulanan şelale dosyalarıyla mevcut geliştirme dalına yapılır; uzak SHA ayrıca doğrulanır. Master'a merge yapılmaz. TestFlight/Cloud Build/IPA başlatılmadı. Fiziksel cihaz kabulü sonraki beta testine kalır.
