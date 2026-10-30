# PART 11C — Kedi maskotundan yardım alma

31 Ekim 2026. Proje `C:\Dev\Mewgic`, dal `recovery/part09-completion`, başlangıç commit'i `9fa3ecd51f04eafa7b67cf34640ad92ecfdde402`.

## Sonuç

Mevcut PART11B PawGuide genişletildi; yeni TutorialManager oluşturulmadı. Kediye dokunulduğunda dört seçenekli, krem parşömenli mor-altın çerçeveli Pati Yardımı menüsü açılır:

- Nasıl Oynanır?
- Hamle Öner (1 hak)
- Güçlendiricileri Tanı
- Kapat

Başlık mevcut Masalsı başlık fontunu, diğer metinler mevcut okunabilir TMP gövde fontunu kullanır. İki satırlı düğme düzeni, şişelerin altında ve kedinin yakınındadır. Aynı menü açıkken tekrar dokunmak yeni nesne oluşturmaz veya giriş animasyonunu sıfırlamaz. WizardCat prefabı, Idle/Blink/Wand Magic animasyonları ve kıvılcım havuzu değiştirilmedi; mevcut Wand Magic çağrısı korunur.

## Yardım ve oyun verileri

Nasıl Oynanır iki kısa balonla üst renk, boş/aynı renkli hedef ve renkleri bir araya toplama kurallarını açıklar; son Devam düğmesi menüye döner. Mevcut bölüm sıfırlanmaz; şişe seçimi, sıvılar, hamle ve tutorial kayıtları değiştirilmez. Temel eğitimi yeniden oynatma bu partta eklenmedi.

Hamle Öner, mevcut PrototypeApp.UseBooster(Hint) yolunu kullanır. Geçerli çözümden mevcut solver önerisi seçilir; başarıda bir İpucu hakkı tüketilir. Yeni ücretsiz hak veya ücretlendirme sistemi eklenmedi. Zaten gösterilen aynı ipucu tekrar hak tüketmez. Hak yoksa menü seçeneği devre dışıdır. Kaynak altın pati/ışık, hedef mavi ok/ışık ile gösterilir; teknik şişe numarası/debug metni üretilmez.

## PART11D bağımlılığı

Projede mevcut bir PART11D güçlendirici ekranı bulunamadı. Bu nedenle **gerçek PART11D ekranına geçiş tamamlanmış olarak raporlanmıyor**. Seçenek şimdilik aynı Pati balonunda Geri Al, İpucu, Ek Şişe ve Karıştır için dört kısa, salt okunur açıklama gösterir. Bu, ayrı PART11D ekranının yerine geçici işlevsel içeriktir; tam PART11D ekranı bu görevde geliştirilmedi.

`PawGuide.BoosterHelpRequested` tek bağlantı noktasıdır. Gelecekte PART11D bu olaya bağlandığında menü kapanır ve bu giriş çağrılır; kısa balon içeriği yerine gerçek ekran açılabilir. Bağlantı çağrısı test edildi; henüz olmayan ekranın görsel/akış testi yapıldığı iddia edilmiyor. Eksik bağımlılık için tercih sorusu sunuldu; yanıt gelmediğinden belirtilen kısa balon varsayımıyla devam edildi.

## Güvenli etkileşim

Menü ve manuel açıklamalar açıkken PawMenuShield adlı görünmez UI katmanı şişe dokunuşlarını yakalar. Buna ek olarak mevcut AllowsBottleTap kontrolü doğrudan oyun komutundan gelen şişe seçimlerini de reddeder. Güçlendiriciler arka plandan kullanılamaz; menüdeki bilinçli Hamle Öner seçimi önce menüyü kapatıp mevcut Hint işlemini çalıştırır.

Kapanışın yaklaşık 0.18 saniyesinde şişe girdisi korunur; çok hızlı aç/kapat sırasında dokunuş alttaki şişeye geçmez. Menü kapandığında normal oyun serbest kalır. Pause manuel yardımı kapatır; paused durumunda kedi menüsü açılmaz. Resume eski menüyü kendiliğinden açmaz. Seçili şişe korunur. Gerçek aktarım sürerken menü açılmaz. PART11B'nin kaynak/hedef eğitim girdisi bu modal menüden ayrı korunur.

## Gerçek testler

**TESTS.json: 191 kontrol geçti. Runtime ve test sonu Console: 0 hata / 0 uyarı. C# derlemesi: 0 hata / 0 uyarı.** Gerçek oyuncunun ana kaydı, yedeği ve varsa corrupt kopyası aynı kaldı. İzole test profilleri kullanıldı.

Doğrulananlar:

- Tek dokunuşla açılma; sekiz hızlı tekrar dokunuşunda tek menü ve sabit açılış animasyonu.
- Kedi UI raycast hedefi; menü katmanının şişe raycast'ini yakalaması; doğrudan Bottle/Booster komutlarında veri değişmemesi.
- Kapatma, yeniden açma, kapanışta dokunuş sızmaması ve sonrasında normal şişe seçimi/aktarım.
- Şişe seçimi sırasında yardım; seçimin korunması; oyun ortasında yardım; aktarım sırasında yardım açılmaması.
- Pause/Resume çakışması ve paused durumda yardım/şişe girişlerinin reddedilmesi.
- İki kural balonu ve menüye dönüş; dört kısa güçlendirici açıklamasında hak/veri değişmemesi.
- Mevcut Hint hakkının tek tüketimi, aynı ipucunda ikinci tüketim olmaması, sıfır hakta yeni ipucu üretilmemesi; iki metinsiz görsel işaret.
- PART11B yeni oyuncu karşılama → kaynak → hedef → tek gerçek hamle → tamamlama/kutlama/kapanış regresyonu.
- 900×1600, 900×1950 ve 900×2000 gerçek Unity kamera render'ları. Son oranda Safe Area inset simülasyonu var. Balonun Safe Area içinde kalması, şişeleri örtmemesi, metin taşmaması, düğmelerin birbirine çarpmaması ve en az 44 px dokunma boyutu doğrulandı.
- Mevcut GameplayBoosterTests.ModelTests çalıştırıldı. Son Editor kontrolünde kediye ve menü arkasındaki kaynak/hedef şişelere gerçek fareyle dokunuldu; Hamle 0 kaldı.

UI raycast testi, Editor'ın geçici pencere/Scene View boyutundan etkilenmemesi için kontrollü 900×1600 kamera/UI bağlamında yapılır. Otomatik eylemler mevcut Button olaylarını da doğrudan çağırır; tüm kontroller fiziksel dokunma testi değildir. Çok hızlı kapanışta bulunan giriş sızıntısı düzeltildikten ve menü başlığı için boşluk artırıldıktan sonra son tur yeniden geçti.

Unity MCP/Pipeline bağlantısı doğrulandı; komutlar aynı açık Editor'a CLI üzerinden gönderildi. Unity 6000.3.27f1 ve CLI sürümü korundu. Fiziksel iPhone/Android cihaz testi yapılmadı; yeni build yok.

## Görseller ve değişiklikler

- PawMenu-1600/1950/2000.png: gerçek Unity menü görüntüleri.
- HowToPlay-1600.png, BoosterIntro-1600.png, Hint-1600.png, NoHintCharges-1600.png: çalışan yardım durumları.
- Editor_Menu.png: gerçek Editor Game View penceresinden, ilgisiz masaüstü bildirimini dışarıda bırakan kırpılmış teslim görüntüsü.
- COMPILE.json, CONSOLE.json, TESTS.json ve PRESERVATION.json: test/koruma kanıtları.

Mevcut kaynaklardan yalnız PawGuide.cs ve PrototypeApp.cs değişti. PawGuide menü, kısa açıklamalar ve giriş katmanını yönetir. PrototypeApp'te menü sırasında booster işlemini reddeden kontrol ve Pause'da manuel yardımı kapatan çağrı eklendi. Yeni Part11CEditor/Part11CProbe yalnız test/önizleme araçlarıdır; sahneye kaydedilmez. Üretim ilerleme belgesi güncellendi.

Proje dışı başlangıç dökümü `C:\Dev\Mewgic-Part11C-Baseline.json` ve PRESERVATION.json ile sahneler, fontlar, kedi prefabı/animasyonları, görseller, şelaleler, çekirdek oyun kuralları ve save kodunun aynı kaldığı doğrulandı. Önceki untracked çalışmalar commit dışında korundu. Yeni TestFlight/Cloud Build/IPA veya master merge yapılmadı.

Uygulama/kanıt commit'i `cf47546eeb56b7d84ccbee886583a89de8d05de3`, mevcut `recovery/part09-completion` dalına başarıyla push edildi; uzak SHA eşleşti. Master `f243d7a355fedf9cfbc65f8060f1d3bc14cae169` olarak korundu. Bu teslim kaydı ayrı belge commit'iyle saklanır. Unity, gerçek kayda yazmayan test profilinde Gameplay / Play Mode üzerinde Pati Yardımı menüsü açık bırakıldı.
