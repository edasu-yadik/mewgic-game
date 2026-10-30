# PART 10G — Unity görsel teslimi ve regresyon

30 Ekim 2026. Proje: `C:\Dev\Mewgic`. Dal: `recovery/part09-completion`. İncelenen başlangıç commit'i: `3ea8d82fff3eb5f18420fa2bf6c4b23f8a2b873f`.

## Sonuç

PART 10A–10F güncel kaynak, gerçek Unity sahnesi ve yeni Play Mode testiyle doğrulandı. Yeni özellik veya üretim mekaniği değişikliği gerekmedi. Unity Editor **Gameplay / Play Mode** üzerinde, Game View büyütülmüş ve görüntü pencereye sığdırılmış halde açık bırakıldı. Kediye gerçek fare tıklamasıyla yardım menüsü açılıp kapatıldı; Yeniden butonu da gerçek fare girişiyle denendi.

Önizleme izole test profili kullanır; kullanıcı burada oynayabilir. Gerçek oyuncu kaydı testte aynı kaldı. Play Mode durdurulunca gerçek profil geri bağlanır. Önizleme koruyucusu sahneye kaydedilmedi; yalnızca bu Editor oturumundaki geçici nesnedir.

## A–F denetimi

| Part | Durum | Güncel kanıt |
| --- | --- | --- |
| 10A | TAMAMLANDI | Sabit “Önce kaynak...” metni görünür TMP bileşenlerinde yok. TutorialOverlay artık 10B rehberine ev sahipliği yapıyor. |
| 10B | TAMAMLANDI | Merhaba → kaynak → hedef → geçerli aktarım → kutlama → kapanış; yanlış hedef tamamlamıyor. Genel kısa yardım ilk geçerli hamlede kapanıyor. Kedi menüsü, Nasıl oynanır, mevcut Hint hakkı, tekrar giriş geçmişi doğrulandı. |
| 10C | TAMAMLANDI | PerformancePanel ve üç yıldız kutusu görünmüyor. Kazanma/ödül ekranı çalışıyor; ödül modelleri test edildi. |
| 10D | TAMAMLANDI | Ayrı Bölüm/Hamle panelleri ve Pause görünüyor. Gerçek hamle, Undo ve Restart kontrol edildi. Ortak hedef kutusu yok. |
| 10E | TAMAMLANDI | Güncel adaptif şişe yerleşimi kullanılıyor. Üç oranda şişeler birbirini, kediyi, HUD'ı veya booster alanını örtmüyor. Dokunma genişliği en az 44 ekran pikseli. Önceki 10E çok şişeli test kanıtları da incelendi; yeni birleşik görsel tur ilk bölümde yapıldı. |
| 10F | TAMAMLANDI | “Ücretsiz başlangıç hakları...” metni ve BoosterPolicy nesnesi yok. Yeniden/Ana Menü korunuyor. |

## Gerçek testler

- `UI_AUDIT.json`: **475 kontrol**, başarı; çalışma sırasında **0 hata / 0 uyarı**; `playerSaveIntact=true`.
- `MODELS.json`: Gameplay HUD, Daily Activities ve Reward Hub model grupları geçti. C# derleme hata/uyarı mesajı yok.
- Şişe seçimi, geçersiz hedef, gerçek sıvı aktarımı, Undo, Restart, Pause/Resume, çözüm hamleleriyle bölüm kazanma, ödül ekranı ve Gameplay → Home geçti.
- Pati Yardımı: tek rehber nesnesi, gerçek UI raycast, kaynak/hedef vurgusu, yanlış hamlede tamamlanmama, doğru hamlede geçmiş kaydı, kapanış, yardım menüsü, Hint bağlantısı, geri ile kapatma geçti.
- Save/Load: test profilinin JSON'a yazılıp okunması, rehberin yeniden oluşturulmasında öğrenilmiş öğreticinin açılmaması, eski kayıt uyumu ve hata halinde atomik tamamlama kontrol edildi. İşletim sistemi üzerinden uygulamayı kapatıp açma testi yapılmadı.
- 900×1600 (9:16), 900×1950 (9:19.5), 900×2000 (9:20) gerçek Unity kamera render'ları alındı. Son oranda üst/alt Safe Area inset simülasyonu kullanıldı. Balonun Safe Area içinde kalması, şişeleri örtmemesi ve ilgili TMP metinlerinin taşmaması doğrulandı.
- Otomatik UI adımları mevcut Button olaylarını çağırarak gerçek oyun kodunu çalıştırır; tüm senaryolar fiziksel dokunma testi değildir. Son Editor kontrolünde kedi, Kapat ve Yeniden fareyle çalıştırıldı.

İlk yeni test denemesi, testin rehberi yeniden oluştururken host referansını güncellememesi nedeniyle durdu. Hata yalnız test kurulumundaydı; referans düzeltildikten sonra tüm tur yeniden geçti. Üretim kodu değiştirilmedi.

## Console ve araç sınırları

Oyun testi sırasındaki hata/uyarı sayısı sıfırdır. Bununla birlikte tüm Editor günlüğü tertemiz değildir: açılışta lisans erişim token yenileme mesajı, Editor OverlayCanvasesData/ScriptableSingleton yerleşim hatası ve importer Curl 23 ağ/yazma mesajı vardır. Model günlüğünde Curl 42 callback iptali de görülür. Bunlar `editor.log` ve `models.log` içinde saklandı; testin runtime sayısına dahil edilmedi. Oyun akışını durduran bir hata saptanmadı; Editor yerleşim/servis mesajları çözülmüş sayılmıyor.

Unity **6000.3.27f1** mevcut proje sürümüyle çalıştırıldı; sürüm değiştirilmedi. Unity MCP başlangıçta Pipeline bulunamadığını bildirdi; son durum çağrısı Editor'ın bağlandığını bildirdi, ancak kullanılabilir araç listesi hâlâ yalnız durum aracını gösterdi. Bu nedenle testler Editor içi C# araçlarıyla, son pencere doğrulaması computer-use ile yapıldı. CLI güncellemesi yapılmadı. Açık Editor'ın aktif hedefi Android; iOS derleme veya fiziksel iPhone testi yapılmadı.

## Görsel teslim

Tüm dosyalar bu klasördedir:

- `Gameplay_Normal.png`: temiz güncel Gameplay.
- `Gameplay_Tutorial.png`: ilk bölüm karşılama balonu.
- `Gameplay_Source.png`: kaynak şişe vurgusu.
- `Gameplay_Target.png`: hedef şişe vurgusu.
- `Gameplay_PawHelp.png`: kedinin yardım menüsü.
- `Gameplay_AfterFirstMove.png`: ilk geçerli hamleden sonra temiz görünüm.
- `Gameplay_Comparison.png`: solda arşiv PART09F Unity görüntüsü, sağda güncel A–F sonucu. Eski tasarım bu görevde yeniden kurulmadı.
- `Welcome/Source/Target/Menu-1600/1950/2000.png`: oran bazında kanıtlar.
- `Welcome-GameView.png`: Unity ScreenCapture ile doğrudan Game View çıktısı.
- `Editor_Gameplay.png`, `Editor_PawHelp.png`: açık gerçek Editor penceresinden teslim görüntüleri.

Normal ve senaryo PNG'leri gerçek çalışan Unity sahnesinin kamera render'larıdır; temsili çizim değildir. Karşılaştırma dosyası bu ekran görüntülerinin yan yana yerleştirilmesidir. Görsel kontrolde metin çakışması veya kritik UI regresyonu görülmedi. Pati vurgusu mevcut ince dikdörtgen altın çerçeve/pati işaretidir; bu QA aşamasında sanat yeniden tasarlanmadı.

## Koruma ve Git teslimi

Başlangıçtaki tracked dosyaların SHA-256 dökümü proje dışında `C:\Dev\Mewgic-Part10G-Baseline.json` olarak saklandı. `PRESERVATION.json`, test sonrası mevcut tracked dosyaların tümünün aynı olduğunu gösterir. Sonrasında yalnız üretim ilerleme belgesine bu QA kaydı eklendi. Önceden var olan untracked sanat/rapor dosyaları korunup bu commit dışında bırakıldı.

Eklenenler: Part10GEditor, Part10RegressionProbe, Part10PreviewGuard ve meta dosyaları; bu klasördeki görseller, test JSON'ları ve rapor. `docs/PRODUCTION_PROGRESS.md` güncellendi. Master değiştirilmedi. Yeni TestFlight/Cloud Build/IPA veya Apple yüklemesi yapılmadı.

QA commit'i `494e6294a7c91195bf8d1a09cca96a7317482207`, `recovery/part09-completion` dalına başarıyla push edildi; uzak dal SHA'sı aynı çıktı. Uzak master `f243d7a355fedf9cfbc65f8060f1d3bc14cae169` olarak korundu. Bu teslim kaydı ayrıca belge commit'iyle saklanır. Bu sonuç Editor QA kabulüdür; sonraki beta için fiziksel iPhone kabulü ayrı olarak gereklidir.
