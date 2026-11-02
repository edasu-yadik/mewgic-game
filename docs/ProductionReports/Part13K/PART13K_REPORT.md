# PART13K — Unity son sürüm görsel inceleme

2 Kasım 2026 · başlangıç commit713ca5cc6442b05bf776a1cef38ad2c1a09f4986 · recovery/part09-completion

## Sonuç

Unity Editor açıldı ve oyun Play Mode'da çalışıyor. Doğru proje **C:/Dev/Mewgic**, kurulu/kullanılan sürüm **6000.3.27f1 (672d562a42a7)**. Açılışta Editor kapalıydı; aynı sürüm başlatıldı, import/derleme tamamlandı, MCP/Pipeline7800 bağlantısı doğrulandı. Paket, Unity sürümü, oyun kodu veya sanat değiştirilmedi. Yeni build yok. AGENTS.md ve PART13J son raporu okundu.

Editor şu anda **Game sekmesinde, LevelMap,900×1600**, ilk adada açık. Ayrı QA profili599 tamamlanmış bölümü temsil eder; tüm12 ada açık,600 güncel. Kullanıcının gerçek ilerlemesi değildir. Gerçek kayıt ana/yedek/bozuk değerleri oturum başlangıcıyla karşılaştırıldı: aynı. Test üretici arşivi ayrı geçici klasörde. Ada/bölüm/kayıt sıfırlanmadı.

## Hangi sahneyi açmalı, nasıl başlamalı?

- Normal başlangıç: Project penceresinde **Assets/Scenes/Boot.unity** açıp Play'e basın. Boot kaynakları/kaydı yükleyip **Home** ana menüsüne geçer. Build Settings sırası Boot,Home,Gameplay,LevelMap; SampleScene kapalı.
- Ana menü: **Assets/Scenes/Home.unity**. Oyna seçilen bölüme; alt çubuktaki Harita ise LevelMap'e gider.
- Harita: **Assets/Scenes/LevelMap.unity**. Şu an yeniden başlatmanız gerekmiyor: Game View'da hazır. Tam ekran Game görünümünden çıkmak için Shift+Space kullanabilirsiniz.
- Haritayı yukarı/aşağı sürükleyin veya Önceki Ada/Sonraki Ada düğmelerini kullanın. Bölümüme Git güncel bölgeye götürür. Bir10'lu grup düğmesine, ardından açılan seçicide bölüm numarasına basın. Oynanıştaki **HARİTA** düğmesi geri döndürür.

Tüm adaları gerçek ilerlemeyi değiştirmeden yeniden açmak için Play'i durdurup PowerShell'de mevcut güvenli önizleme aracını çalıştırabilirsiniz:

```powershell
unity command script eval 'Part12BEditor.Size(1600); Mewgic.Part13JProbe.Begin(true);' --project-path C:/Dev/Mewgic --json
```

Bu komut Play'i kendisi başlatır ve yeni, izole QA profili kullanır; normal oyuncu kaydını açıp adaları zorla açmaz. Normal Boot/Play gerçek kaydı kullanır ve yalnız gerçek ilerlemenize göre kilitleri gösterir. QA nesnesi eklenmiş sahneyi diske kaydetmeyin; sahne dosyası bu görevde değiştirilmedi. Play çıkışında test profilinin bağlantısı bırakılır.

## Bu aşamada gerçekten yapılan testler

|Kontrol|Sonuç|
|---|---|
|Boot→Home|İzole profil içinde Boot sahnesi yüklenip doğal Home geçişi gerçekleşti. Boot controller tamamlandığında kendisini yok eder; sonraki audit'te null olması hata değildir.|
|Home→LevelMap|Gerçek NavMap Button olayıyla geçti.|
|12 ada|12 artReady tanımı; tüm adalar açık. Her adaya odaklanıp güncel doğrudan Game View görüntüsü alındı.|
|Önceki/Sonraki|Gerçek düğmelerle0→1→0 bölgesi doğrulandı.|
|Haritadan bölüm|591–600 seçicisindeki gerçek600 Button olayı Gameplay600 açtı.|
|Bölümden haritaya|Gerçek HARİTA/Home düğmesiyle LevelMap'e dönüş; oyuncu ana/yedek kaydı aynı. UI_SELECTION.json.|
|601 üretimi|PrepareAsync(601), Generated.Validate geçti; fallback=false. Üretici gerçek çözüm sertifikasını oynatıp kazanan son durumu doğrular. Bu partta601 oynanıp kazanılmadı; o akış13J'de geçti.|
|Ekran oranı|Home/Gameplay900×1600; LevelMap900×1600,900×1950,900×2000. Canlı TMP taşması0 ve hata shader'ı0.|
|Kamera|Denetlenen üç sahnede Camera.main mevcut; gerçek ekran çıktıları alındı.|

Bu görsel incelemede13J'nin11930 ana/304 yeniden açma ve1599/1425 regresyon kontrolleri yeniden koşturulmadı. Önceki sonuçlar önceki rapora aittir. Fiziksel telefon, Safe Area cihaz kesiti, FPS/GPU/termal ve uygulama kill/resume testi yapılmadı.

## Sonsuz üretici nasıl çalışıyor?

Kaynaklar: Assets/Scripts/Core/LevelGeneration.cs, PuzzleSolver.cs, LevelArchive.cs, WorldMapData.cs; Assets/Scripts/Presentation/GameplayLevelCatalog.cs.

İlk iki bölüm Resources'daki onaylı düzendir.3 ve sonrasında bölüm kimliğinden kararlı seed elde edilir; **Xorshift32** PRNG kullanılır. Çözülmüş renk şişelerinden başlanıp tersine çevrilebilir dökme hamleleriyle aday karıştırılır. Her ters adımın geri oynatılabildiği kontrol edilir. Ardından **BFS (genişlik öncelikli arama)**, şişe sırası eşdeğerlerini kanonik anahtarla ayıklayarak çözümü/optimal hamle sayısını arar. Renk permütasyonu ve şişe sırası kanonik imzası son64 arşiv bölümündeki benzer düzenleri elemekte kullanılır.

Zorluk puanı optimal hamle, karışık renk sınırı, başlangıç seçenekleri ve dallanma/ziyaret edilen durum gibi ölçülerden hesaplanır. Varsayılan bütçe32 aday, aday başına6000 durum, toplam30000 durumdur. Uygun aday bulunmazsa son geçmişte olmayan, renk/boş şişe koşullarını karşılayan önceden doğrulanmış fallback seçilir. Fallback de yoksa başarısız hazırlık güvenli hata verir; çözülmemiş rastgele bölüm başlatılmaz. Üretim Task.Run altında seri worker kilidiyle yapılır; arşivde varsa bölüm yeniden üretilmez. Şişeler, seed/sürüm, imza ve çözüm sertifikası dosyada sabitlenir.

**600'de durma koşulu yoktur.**600 kazanıldığında601 hazırlanabilir; sonraki bölümler de aynı kimlik tabanlı üreticiyle devam eder. Her50 bölüm bir bölge;601 yeni bölgenin ilk bölümüdür. Ada girişlerinde ilk bölüm Easy, sonraki iki Normal/3 renk olarak yumuşatılır.601+ için yeni12'li harita paketi ve sanatı bekleyen prosedürel bölge tanımı kullanılır;13. adanın hazır resmi yoktur. “Sonsuz” sınırsız eşsiz sanat/bulmaca garantisi değildir: kimlikler Int32 ile sınırlı,3–4 renkli durum uzayı sonlu ve tekrar koruması son64 arşiv kaydıyla sınırlıdır.

## Görsel bulgular — mevcut hali

[Güncel galeri](GALLERY.html) içinde Home,Gameplay,12 ada ve uzun oranlar var. Her PNG doğrudan Unity Game View yakalamasıdır;13J görüntüleri tekrar kullanılıp yeni diye sunulmadı. Eksik büyük ada resmi, pembe hata materyali, siyah kamera veya aktif metin taşması saptanmadı. İlk/son adaların kadraj sınırı ve komşuların viewport'ta kesilmesi kaydırmanın normal sonucu. İstenen12 ada mevcut ve tema ayrımları korunuyor.

Açık görsel sınırlar: üç haneli grup aralıkları rozet kenarına yakın; küçük fiziksel ekranda okunurluk hâlâ denenmeli. Referans03_Map'e göre büyük mor paneller daha fazla alan kaplıyor, ada araları daha seyrek. Home yoğun/parlak bir kompozisyon; bu görevde yeniden tasarlanmadı. Komşu adanın başlığı uzun ekranda ana hedefe yakın görünebilir; otomatik taşma kontrolü estetik hiyerarşi kabulü değildir. Materyal denetimi hata shader'ını arar, tüm GPU platformlarını doğrulamaz. İnceleme sırasında gördüğümüz ekranlarda düzeltme gerektiren kritik görsel kusur bulunmadı; cihaz kalite kabulü iddia edilmiyor.

## Console ve koruma

CONSOLE.json güncel Editor groundTruth hata0/uyarı0, compiling=false/compilationFailed=false gösteriyor. Pipeline retained counts1 hata/1999 uyarı gösteriyor; bunlar güncel Editor sayısı değildir. CONSOLE_HISTORY.json örnekleri1 Kasım tarihli eski, artık bulunmayan13A yakalama dosyalarının import uyarılarını içerir. Yeni Editor başlangıç logunda OverlayCanvasesData ScriptableSingleton yerleşim hatası ve lisans access-token yenileme tanısı görüldü; mevcut Play Mode'u engellemedi. Bunlar oyun hatası diye düzeltilmiş gösterilmedi; proje/paket/lisans ayarları değiştirilmedi.

Başlangıç Git dalı recovery/part09-completion,713ca5c; önceden değişmiş TMP fallback ve ilgisiz untracked işler korundu. Fontun başlangıç byte'ları geri kondu. PRESERVATION.json:2799 başlangıç izlenen dosyada değişiklik0 (ilerleme raporu yazılmadan önce). Üretim sahne/kod/asset/kayıtları korunur. Bu part yalnız tools/Part13K denetimleri, ekranlar ve rapor ekler. Yeni özellik, reset, paket kurulumu, sürüm değişikliği veya build yok.
