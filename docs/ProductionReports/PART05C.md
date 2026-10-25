# PART 05C — Gameplay HUD ve Pause

25 Ekim 2026 • C:/Dev/Mewgic • Unity 6000.3.27f1

## Tamamlananlar

GameplayHud bileşeni gerçek bölüm numarası, dinamik hedef ilerlemesi, kullanılan hamle sayacı, üç yıldızlı performans alanı ve tıklanabilir Pause düğmesi oluşturur. Eski düz Chapter/Goal/Moves metinleri kaldırıldı. Panel, yıldız ve Pause için üç ayrı şeffaf PNG üretildi; sayılar ve metinler canlı Unity UI öğeleridir. Panel 9-slice kullanır; ikonlar orantılarını korur. Kaynak/prompt kaydı PART05C_PROMPTS.md içindedir.

Varlıklar: Assets/Resources/Gameplay/HUD/HUD_Panel_v001.png, HUD_Star_v001.png, HUD_Pause_v001.png. Kaynak çözünürlükleri 2172×724 ve 1254×1254; gerçek alpha 0–255 doğrulandı. Unity panel max2048, ikonlar max512; Android ASTC4×4, mipmap kapalı. Kaynak PNG'ler kesilmedi; import rect saydam boşlukları dışarıda bırakır.

GameplayHudState, sunumu gerçek Puzzle durumundan hesaplar. Level01.json içine id=1, goal ve yıldız eşikleri eklendi; mevcut şişe dizilimi aynı. Bölüm numarası yüklenen dosyanın id alanından gelir; referansın Level28/Moves12 değerleri kullanılmaz. Hedef, tam dolu tek renkli şişeler / bölümün renk sayısı olarak güncellenir. Hamle sayısı başarılı model hamlelerinin sayısıdır.

Bölüm1 için doğrulanmış 3 hamlelik çözüm temel alınarak ≤3 hamle 3 yıldız, ≤5 hamle 2 yıldız, daha fazlası 1 yıldız olarak tanımlandı. Eşikler bölüm verisindedir. Oyun sürerken yıldızlar HEDEF olarak gösterilir; kazanılmadan ödül verilmiş gibi gösterilmez. Kazanıldığında gerçek sonuç gösterilir. Undo model sayacını geri aldığı için yıldız hedefi de geri döner. Kalıcı yıldız kaydı/ekonomi veya ödül sistemi eklenmedi.

Gerçek Pause modalı Devam Et, Yeniden Başlat ve Ana Menü seçeneklerini sunar. Tam ekran raycast engeli ve kod tarafındaki paused koruması birlikte çalışır. Dökme coroutine'i ile şişe seçim/sıvı görsel saati durur; Resume aynı noktadan sürer. Pause Restart/Home devam eden aktarımı iptal eder, akışı temizler ve kilit bırakmaz. Time.timeScale değiştirilmez; Home'a taşan global saat yan etkisi oluşmaz. Kedi/çevre sanatına veya animasyon sistemine müdahale edilmedi.

## Dosyalar ve koruma

Yeni: GameplayHud.cs, GameplayHudState.cs, GameplayHudTests.cs ve ilgili meta/PNG'ler.
Değişen: PrototypeApp.cs Gameplay HUD/Pause bağlantıları; GameplayBottleView.cs yalnız Pause görsel saati/koruması; Level01.json yalnız HUD metadata alanları.

189 önceki sanat/prefab/animasyon/sahne/core dosyası SHA256 ile aynı. Home sahnesi, Gameplay sahnesi, kedi, şişe kabuğu, çevre ve Puzzle.cs değişmedi. Şişe diziliminin anlamsal eşitliği ayrıca doğrulandı. Geri dönüş kopyası C:/Dev/Mewgic-Part05C-Backup. DevControl'e dokunulmadı.

## Gerçek testler

- GameplayHudTests: 336 kontrol başarılı. Yüklenen level/goal, gerçek model hamleleri, hedef ilerlemesi, 3/2/1 yıldız eşikleri, kazanılmadan ödül verilmemesi, 3 ve 5 hamlelik kazanma sonuçları doğrulandı.
- Gerçek EventSystem raycast/pointer tıklamaları: Pause/Resume, modalın alttaki şişelere dokunuşu engellemesi, paused halde doğrudan callback çağrılarının da modeli değiştirememesi, Undo, Pause Restart, Pause Home ve tekrar Gameplay girişi geçti.
- Gerçek sıvı akışı sırasında Pause açıldı; şişe konumu, açısı ve sıvı miktarı bekleme boyunca sabit kaldı. Resume aktarımı tek mantıksal hamleyle tamamladı. Pause Restart/Home hareket sürerken de denendi; kilit veya akış nesnesi kalmadı.
- Mevcut GameplayBottleTests: 3183 kontrol başarılı; çoğunluğu slot geometrisi, ayrıca renk/hacim/model eşleşmesi ve gerçek oyun akışı kontrolleridir. 3183 ayrı oyun oturumu değildir.
- Mevcut 12 model kontrolü ve Boot → Home → Gameplay → Home smoke testi başarılı; smoke exitCode0.
- 9:16, 9:19.5, 9:20 ve Safe Area simülasyonu: HUD sınır içinde, Pause dokunma alanı ≥44px, şişelerle örtüşme yok. Uzun ekran metin aralıkları görsel kontrol sonrası iyileştirilip test tekrarlandı.
- Son C# derlemesi / Unity Console: 0 hata, 0 uyarı.
- İlk testte hatalı bir ek hamle senaryosu, ardından aynı karede açılan modal butonunu tıklama testi düzeltildi. İlk başarısız çıktılar TEST_REPAIR / TOUCH_TEST_REPAIR.json içinde korunur; oyun mantığı bu testleri geçsin diye değiştirilmedi.
- Fiziksel Android cihazı ve yeni APK testi yapılmadı.

## Görsel denetim ve kanıtlar

PART05C_GAMEVIEW.png gerçek Unity Game View ScreenCapture 900×1600 çıktısıdır. PART05C_PAUSE.png gerçek akış sırasında açık Pause panelidir. gameplay-05c-* görüntüleri çalışan Canvas/kamera ile üç oran, safe area ve kazanma yakalamalarıdır. PART05C_COMPARISON_Dark/Light.png referans ile gerçek ekranı yan yana gösterir.

Mor-altın kabartma, ayrı altın yıldızlar ve yuvarlak Pause dili uygulandı. Referanstaki ahşap/parşömen alt çeşitleri yerine ortak mor panel ailesi kullanıldı; yazı tipi, çerçeve süsleri ve yıldız alanı birebir aynı değildir. Yıldız alanı bir dolum çubuğu yerine net bir hedef/sonuç panelidir. Kaynak çubuğu, alt Gameplay kontrolleri ve önceki partın sıvı sanatı bu partta yeniden tasarlanmadı. Teknik HUD tamamlandı; bütün Gameplay ekranına referansla birebir final sanat onayı verilmez.

## Teslim

İlgili değişiklikler ve kanıtlar doğrulandı. Commit/push bilgisi teslim kaydına eklenecek. Başka part başlatılmadı; sonraki görev kullanıcı talimatıyla ele alınacak.
