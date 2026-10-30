# PART 11B — İlk bölüm etkileşimli Pati eğitimi

30 Ekim 2026. Proje `C:\Dev\Mewgic`; dal `recovery/part09-completion`; başlangıç `cb83ecbe20c64c71a0ba781596c5ed4dfb55e3b0`.

## Uygulama

Mevcut PART10B `PawGuide` genişletildi. İkinci TutorialManager oluşturulmadı. WizardCat prefabı, mevcut Wand Magic/18 kıvılcım havuzu, parşömen balon ve Masalsı TMP fontları aynen kullanılıyor.

Eğitim yalnız Bölüm 1'de, hem bölüm hem `paw.basic.v1` öğretici kaydı tamamlanmamışsa otomatik başlar. Mount sırasında hemen açılmak yerine sonraki karede hazır Gameplay, kedi, şişeler ve geçerli UI alanı beklenir. Home'da çalışmaz. Hazırlık başarısız olursa en geç üç saniyede bekleyen eğitim iptal edilir; normal oyun devam eder.

| Aşama | Mesaj / ilerleme |
| --- | --- |
| Karşılama | “Merhaba! Ben senin büyücü yardımcınım. Hadi ilk iksirimizi hazırlayalım!” — Devam veya Kapat. |
| Kaynak | “Önce parlayan şişeye dokun!” — yalnız işaretlenen kaynak aşamayı ilerletir. |
| Hedef | “Şimdi aynı renkli sıvıyı uygun şişeye aktar!” — yalnız önerilen gerçek aktarım tamamlanınca ilerler. |
| Kutlama | “Harika! Artık renkleri kendi şişelerinde toplayabilirsin.” — mevcut Wand Magic ve kısa bekleme sonrası kapanış. |

Kaynak/hedef önerisi mevcut solver'ın doğruladığı çözüm yolundan alınır. Eğitim ek bir Pour çağırmaz: mevcut PrototypeApp.Select → Puzzle.Pour → AnimatePour akışı kullanılır. Başlangıç hamle sayısına göre tam bir hamle artışı ve doğru kaynak/hedef çifti kontrol edilir; kayıt ancak gerçek aktarım animasyonu bitince yapılır. Seri dokunuşlarda mevcut busy kilidi korunur.

Karşılama sırasında Devam/Kapat beklenir. Kaynak/hedef aşamalarında işaretlenmeyen şişeler modele uygulanmaz; bu kısıtlama yalnız eğitim açıkken geçerlidir. Kapat ve geri her aşamada kullanılabilir. Kapat, raycast engelini hemen kaldırır; yaklaşık 0.18 saniyelik yumuşak çıkış sonrasında balon görünmez olur. Eğitim kapalıyken normal oyun kontrolleri aynıdır.

Eski keskin vurgunun yerine yuvarlatılmış, hafif altın ışık halkası ve sakin hareketli pati kullanıldı. Balon şişelerin altında, kedi yakınındaki mevcut boş alandadır. Açılış/kapanış geçen süreye bağlıdır; ağır sahne açılış karesinin deltaTime değeri yüzünden animasyonun atlanması önlenir. Balon solarken alttaki normal durum bildirimi üst üste görünmez; kapanınca bildirim geri gelir.

## Kayıt ve yarıda bırakma

Mevcut `completedTutorials` alanındaki **paw.basic.v1** kimliği kullanılır. Kayıt şeması/sürümü değiştirilmedi. Önceden aynı eğitimi bitirmiş oyuncu yeni mesajlar için tekrar eğitime alınmaz. Bölüm 1'i zaten tamamlamış eski oyunculara da zorunlu eğitim eklenmez.

Yarım bırakılan eğitim tamamlandı olarak kaydedilmez. Sonraki Bölüm 1 girişinde karşılama aşamasından yeniden başlar; önceki kaynak seçimi veya yarım aşama taşınmaz. Aynı ziyarette Kapat veya Restart sonrasında eğitim zorla tekrar açılmaz. Oyuncu normal oyuna devam edebilir. Kayıt hatasında dostça mesajla kutlama kapanır; yanlış tamamlanma kaydı oluşmaz. Eksik hedefte rehber kapanıp girdi kısıtlamasını bırakır.

## Testler ve sınırlar

Son turda **248 + 7 = 255 kontrol geçti**. Her iki Play Mode aşamasında runtime **0 hata / 0 uyarı**; C# derlemesi ve test sonu Console **0 hata / 0 uyarı**. Mevcut GameplayBoosterTests.ModelTests de çalıştırıldı. Bu sayı ayrı senaryo sayısı değil, işlev ve yerleşim kontrollerinin toplamıdır.

Test çıktıları `PHASE1.json`, `TESTS.json`, `COMPILE.json` ve `CONSOLE.json` dosyalarındadır. İki ayrı Play Mode çalıştırması kullanıldı. İlk aşama gerçek, izole PlayerPrefs test kayıtlarını oluşturdu; Play Mode durdurulup yeniden başlatıldıktan sonra ikinci aşama aynı kaydı diskteki veriden yeniden açtı. Gerçek oyuncunun ana kaydı, yedeği ve varsa bozuk-kayıt kopyası aynı kaldı; geçici test anahtarları temizlendi.

Doğrulananlar: Home'da açılmama, hazır Gameplay'de otomatik karşılama, dört aşama, yanlış kaynak, yanlış hedef, işaretlenmemiş diğer geçerli hedef, hızlı tekrar dokunuşları, tek gerçek aktarım, eğitimde Hint hakkı harcanmaması, Wand Magic ve görünür kıvılcımlar, yumuşak açılış/kapanış, Undo/Restart, Kapat, yarıda çıkış ve geri giriş, tamamlanmış eğitimin tekrarlanmaması, kayıt hatası, eksik hedef, manuel yardım ve PART11A metinsiz Hint.

900×1600, 900×1950 ve 900×2000 Unity kamera görüntüleri alındı. Son oranda Safe Area inset simülasyonu kullanıldı. Metin taşması, balonun Safe Area içinde kalması ve hiçbir şişeyi örtmemesi test edildi. Ayrıca gerçek Editor Game View'da fareyle Devam, kaynak, yanlış hedef ve doğru hedef denendi.

Bu, Unity Editor ve Play Mode yeniden giriş testidir; işletim sistemi üzerinden uygulama süreci öldürülmedi ve fiziksel iPhone/Android cihaz testi yapılmadı. Yeni build oluşturulmadı. Unity 6000.3.27f1 korundu. MCP/Pipeline bağlantısı kullanıldı; CLI sürümü güncellenmedi.

İlk test denemelerinde sahne hazır olmadan kontrol eden sabit bekleme ve giriş animasyonunun örnekleme zamanı test düzeneğinde düzeltildi. Son görsel kontrolde fark edilen durum yazısı/balon geçiş çakışması PawGuide içinde düzeltildi ve iki test aşaması yeniden çalıştırıldı. Son teslim yalnız son temiz tura dayanır.

## Görseller

- `Tutorial_Stages.png`: dört aşamanın gerçek Unity görüntüleri yan yana.
- `Welcome/Source/Target/Celebration-1600/1950/2000.png`: aşama ve oran bazında çıktılar.
- `SourceMotion.gif`: gerçek karelerden pati/halka önizlemesi.
- `CelebrationMotion.gif`: mevcut kedi büyüsü ve kapanış önizlemesi.
- `Normal-1600.png`, `Completed-Reentry.png`: kapanmış eğitim ve tamamlanmış kayıtla yeniden giriş.
- `Editor_Welcome.png`, `Editor_Target.png`: gerçek Editor Game View penceresinden teslim kanıtı.

GIF'ler gerçek Unity karelerinden oluşturulur; cihaz FPS ölçümü değildir. Ham hareket PNG'leri yerelde kalır, GIF'ler sürümlenir. Yeni karakter veya sabit ekran görseli üretilmedi.

## Koruma ve teslim

Başlangıç dökümü proje dışında `C:\Dev\Mewgic-Part11B-Baseline.json` olarak saklandı. `PRESERVATION.json`, mevcut dosyalardan yalnız `PawGuide.cs` ve `PrototypeApp.cs` değiştiğini gösterir. PrototypeApp değişikliği eğitim açıkken şişe girdisini rehbere danışan tek satırdır. Sonrasında üretim ilerleme belgesine kayıt eklendi. Yeni Part11BEditor ve yalnız Editor'de derlenen Part11BProbe test/izole önizleme araçlarıdır; üretim sahnesine kaydedilmez.

Sahneler, WizardCat prefabı ve animasyonları, fontlar, şişe/sıvı varlıkları, şelaleler, oyun kuralları, ödüller ve save kodu değişmedi. Önceki untracked çalışmalar korunup commit dışında bırakıldı. Yalnız doğrulanmış PART11B dosyaları mevcut geliştirme dalına gönderilir; master'a merge ve TestFlight/Cloud Build/IPA işlemi yapılmaz.

Uygulama/kanıt commit'i **b9d503f39f86468104e90e7ab392254623aca5a6**, `recovery/part09-completion` dalına başarıyla push edildi; uzak SHA eşleşti. Master `f243d7a355fedf9cfbc65f8060f1d3bc14cae169` olarak korundu. Bu teslim kaydı ayrı belge commit'iyle saklanır. Unity, gerçek kayda yazmayan izole yeni oyuncu önizlemesinde, Gameplay / Play Mode karşılama aşamasında açık bırakıldı.
