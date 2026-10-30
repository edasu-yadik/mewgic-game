# PART 11A — Gameplay teknik metinlerinin kalıcı kaldırılması

30 Ekim 2026. Proje `C:\Dev\Mewgic`; dal `recovery/part09-completion`. Başlangıç commit'i `a236f46a98cfbb05ba9ec06148c06eed9a6d0616`.

## Kaynak ve düzeltme

Sorun sahnede duran bir metin değildi; runtime kaynakları vardı:

| Kaynak | Tetikleyici | Önceki davranış | Sonuç |
| --- | --- | --- | --- |
| BoosterActions.Hint | İpucu ve Pati Yardımı / Hamle öner | `İpucu: 1 → 3` benzeri kaynak/hedef numaralı Message | Başarılı ipucu Message alanı boş. Çözüm ve hak tüketme mantığı korundu. |
| PrototypeApp.UseBooster | Başarılı Hint | Modele ait mesaja `Altın kaynak, mavi hedef` ekliyordu | Başarıda durum yazısı boş; görsel kaynak/hedef işaretleri kullanılıyor. |
| PrototypeApp.Refresh | İlk açılış, ek şişe, şişe sayısı değişimi, Restart | Her şişeye Number adlı TMP etiketi oluşturuyordu | Etiket oluşturma kodu kaldırıldı; sonraki UI yeniden üretiminde de oluşmuyor. |
| PrototypeApp.Select / AnimatePour | Seçme, seçim kaldırma, aktarım başlangıcı | Sabit hedef seçimi/işlem aşaması yönlendirmeleri | Bu aşamalarda durum yazısı boş. Geçersiz hamle için kısa `Bu hamle yapılamıyor.` bildirimi korunuyor. |
| BoosterActions bildirimleri | Hak bitmesi ve Karıştır | Satın alma sistemi / iç sıra kaydırma açıklaması | `Kullanım hakkı kalmadı.` ve `Şişeler yeniden düzenlendi.` şeklinde oyuncuya yönelik mesajlar. |

İpucu bulunamaması artık arama sınırı gibi iç ayrıntılar yerine `Şu anda ipucu bulunamadı; hakkın korunuyor.` bildirimi verir. Kayıt hatası, geri alma, boş şişe ve başarı gibi gerekli oyuncu bildirimleri korundu. Bu değişiklikler Editor koşuluna bağlı değildir: Android/iOS için aynı üretim kodunda teknik metin üretilmez. Debug metinlerini tekrar açan bir seçenek eklenmedi. Hiyerarşideki dahili `Bottle 1` gibi nesne adları oyun kodu için korunur; oyuncuya metin olarak gösterilmez.

## Görsel ipucu

Mevcut solver ve doğrulanmış hamle seçimi aynıdır. Kaynak otomatik seçilmeye devam eder; şişe verisi ipucu sırasında değişmez. Kaynakta yumuşak altın ışık ve küçük pati, hedefte mavi ışık ve aşağı ok gösterilir. HintCueGraphic yalnız Unity UI geometrisi çizer; font, şişe sprite'ı veya sıvı görseli değiştirilmez. Normal seçim ışığı korunur. Yeni işaretler dokunma yakalamaz, Rig'i takip eder; Restart/yeniden sunumda temizlenir. Pati Yardımı aynı mevcut Hint işlevine bağlı kalır; oyuncuya yönelik öğretici konuşmaları değiştirilmez.

## Gerçek test sonuçları

`TESTS.json`: **431 kontrol**, başarı, runtime **0 hata / 0 uyarı**, `playerSaveIntact=true`. Bu sayı ayrı senaryo sayısı değil, senaryolar boyunca metin/nesne ve işlev kontrollerinin toplamıdır.

- Bölüm açılışı, kaynak seçimi, ilk geçerli hamle, sonraki geçerli hamleler ve geçersiz hamle kontrol edildi.
- Hint, tekrar Hint ve Pati Yardımı / Hamle öner kontrol edildi. Öneri gerçek kurallara uygun; bir hak tüketiyor; aynı ipucu tekrar hak tüketmiyor; sıvı ve hamle verisi ipucunda değişmiyor.
- İki farklı metinsiz işaret ve ışığın zaman içinde değişmesi doğrulandı. Güçlendirici sayacı gerçek hak sayısıyla eşleşti.
- Undo, Restart, Ek Şişe sonrası UI yeniden üretimi, Karıştır, bölüm kazanma/ödül ekranı, Home'a dönüş ve tekrar PLAY geçti.
- Her aşamada görünür TMP metinleri tarandı; teknik ipucu numaraları, renk açıklamaları ve eski dahili yönlendirmeler yok. Şişe nesnelerinin altında gizli dahil hiçbir TMP numara etiketi oluşmuyor.
- 900×1600, 900×1950 ve 900×2000 gerçek Unity kamera render'ları alınıp görsel kontrol edildi. 2000 yüksekliğinde Safe Area inset simülasyonu kullanıldı.
- `MODELS.json`: mevcut GameplayBoosterTests.ModelTests, sessiz geçerli Hint ve ipucu bulunamadığında hakkın korunması geçti.
- `COMPILE.json`: C# derlemesi 0 hata / 0 uyarı. `CONSOLE.json`: test sonu Console 0 hata / 0 uyarı.

MCP bağlantısı doğrulandı; testler aynı açık Unity Editor'a Pipeline/CLI komutlarıyla gönderildi. Unity 6000.3.27f1 ve mevcut ayarlar korundu. Otomatik oyun testleri mevcut Button olaylarını çağırır. Son görsel kontrolde Pati Yardımı / Hamle öner gerçek fare girişiyle çalıştırıldı. Fiziksel iPhone/Android release testi veya yeni build yapılmadı.

## Ekran görüntüleri

- `Editor_Hint.png`: gerçek Unity Editor penceresinde Game View; fareyle Pati Yardımı üzerinden çalıştırılmış temiz Hint.
- `Gameplay_Hint-1600/1950/2000.png`: üç oranda gerçek çalışan Unity sahnesinin kamera çıktıları; temsili çizim değildir.
- `Gameplay_Normal.png`, `Gameplay_Reentry.png`, `Gameplay_Victory.png`: normal, tekrar giriş ve kazanma durumları.

Otomatik ScreenCapture denemesinin editör bağlamında farklı pencere boyutu üreten dosyası nihai görsel teslim kanıtı olarak kullanılmadı. Game View teslimi doğrudan doğrulanmış Editor penceresinden alındı. Görsel kontrolde kalan teknik metin veya işlevsel hata saptanmadı; cihaz kabulü ayrı aşamadır.

## Koruma ve dosyalar

`PRESERVATION.json` ve proje dışındaki `C:\Dev\Mewgic-Part11A-Baseline.json`, mevcut dosyalardan yalnız BoosterActions.cs, PrototypeApp.cs ve GameplayBottleView.cs değiştiğini gösterir. Sonrasında ilerleme belgesine bu görev kaydı eklenmiştir. Yeni HintCueGraphic.cs, Part11AEditor.cs ve yalnız Editor'de derlenen Part11AProbe.cs ile meta dosyaları eklendi. Fontlar, onaylı görseller, sahneler, Pati Yardımı kodu, şelaleler, oyun kuralları ve kayıt şeması değişmedi. Test profili gerçek oyuncu PlayerPrefs kaydını korudu. Önceki untracked çalışmalar commit dışında tutuldu.

Yalnız doğrulanmış PART11A dosyaları mevcut geliştirme dalına commit/push edilir; uzak SHA kontrol edilir. Master'a merge yapılmaz. TestFlight/Cloud Build/IPA oluşturulmadı.
