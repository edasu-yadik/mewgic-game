# PART 13A — Harita mimarisi ve görsel üretim hazırlığı

2 Kasım 2026 · Unity 6000.3.27f1 · recovery/part09-completion

## Kapsam

Bu part yeni ada üretmez. PART12A–D raporları, AGENTS.md, WORLD_MAP_DESIGN ve gerçek 03_Map.png incelendi. İlk dört onaylı ada, bölüm üretimi, kayıt, sekiz dil ve Pati korunur. Yeni TestFlight/Player/iOS build'i yok. Başlangıçta font fallback değişikliği ve çok sayıda ilgisiz untracked sanat/rapor vardı; teslim kapsamına alınmaz.

## Mevcut yapı

- `Assets/Scenes/LevelMap.unity`: MapWorld doğrudan sahne bileşenidir; dört bağımsız ada prefabı yok. UI kökleri çalışma zamanında kurulur.
- `Assets/Art/Environment/Map/Map_{Village,Crystal,Moon,Castle}_v001.png`: dört onaylı, transparan, birleşik ada sprite'ı. Sky, Node, Chest aynı klasörde; MapProps.spriteatlas düğüm/sandık ortak atlasıdır. Mevcut importer/atlas değiştirilmez.
- `Assets/Scripts/Map/MapWorld.cs`: MapScrollRect, RectMask2D, Safe Area, parallax; 700 birim ada aralığı, 5 yeniden bağlanan IslandView. `UpdatePool` görünür bölgeyi ve kenar payını hesaplar, ekran dışı kökleri kapatır. Beş grup + sandık ×5, tek seçicide 10 gerçek bölüm: toplam 40 MapPoint. 600 GameObject oluşturulmaz.
- `MapWorldProgression.cs`, `MapProgressionCatalog.cs`, Core/MapProgression.cs: gerçek kayıttan kilit, yıldız, sandık ve bölüm kimliği. JSON 12 ardışık ada ×50 = 600; ilk dört artReady=true, diğer sekiz false. 601+ paketleri mevcut davranışı korur.
- Her adada 50 gerçek bölüm vardır. Görünüm beş adet 10'luk grup ve tek yeniden kullanılan seçici kullanır; 50 ayrı düğümü aynı anda adaya dizmek bu partın kapsamı değildir. Grubun numarası gerçek bölüm yıldızı gibi sunulmaz. Giriş mevcut PrepareAsync/pin/save yolundadır.

## Sabit sanat standardı

03_Map: yumuşak fantastik 3D illüstrasyon; doygun mor/mavi, sıcak altın ışık, serin gölge; uçan kaya tabanları, derin sis, ışıklı pencereler, küçük çiçek kümeleri, şelale ve ince köprüler. Ana yapı 3/4 üst açıyla; kaya altı ve negatif alan görünür. Arka uzak adalarda kontrast düşer. İlk dört sprite ışık ve perspektif referansıdır. Primitive geometriler nihai görsel değildir.

2048×2048 RGBA sRGB ortak ana tuval, pivot (0.5,0); tüm katmanlar aynı projeksiyon/tuval/ölçeği kullanır. PNG kenarlarında siyah/beyaz matte yok; alpha incelemesi açık/koyu zeminde ve gerçek UI kırpmasında yapılır. Unity Read/Write kapalı, mipmap UI için kapalı, bilinear/clamp; final platform sıkıştırması ada kabulünde belirlenir. İnce altın çizgiler ve bloom alfa kenarında kontrol edilir. Kaynaklar daha büyük olabilir, runtime çözünürlük ayrıca seçilir.

Düğüm, kilit, yıldız, metin, sandık etkileşimi sanatın içine basılmaz. Mevcut beş grup ankrajı x=.12/.31/.50/.69/.88, y=.46/.40 dönüşümlü; sandık (.90,.72). Bu alanlar ve ada altı isim plakası için sakin negatif alan bırakılır. 44 ekran pikseli dokunma hedefi ve üç mobil oran korunur. Büyü parçacıkları yazı/düğüm üstüne yoğunlaşmaz; ortak atmosfer 12 mote, yeni ada efektleri için önerilen üst sınır 12 görünür öğe/ada, düşük alpha ve sınırlı overdraw.

## Yeniden kullanılabilir katman sözleşmesi

`Assets/Prefabs/Map/IslandLayerTemplate.prefab` dokuz RectTransform içerir; sanat, collider, script veya oyun düğümü içermez. Sahneye otomatik bağlanmaz. `tools/Part13A/PrepareIslandLayers.cs` içindeki `Part13AEditor.Prepare()` Unity API ile üretir, mevcut şablonu üzerine yazmaz; `Validate()` sırayı ve katalog bütünlüğünü denetler.

| Sıra | Katman | Sorumluluk |
|---|---|---|
| 0 | Background | Ada çevresi uzak adalar/sis; ortak Sky ana arka plan olarak kalır |
| 1 | IslandBase | Uçan kaya kütlesi ve toprak; statik PNG |
| 2 | Architecture | Ana yapı, köprüler; boyanmış sanat veya Blender renderı |
| 3 | Vegetation | Ağaç, çiçek ve sarmaşık; statik çoğunluk |
| 4 | WaterOrSpecialEffects | Ayrı su/ışık maskeleri, düşük maliyetli UV/alpha animasyonu |
| 5 | Decorations | Fenerler, kristaller, küçük öykü öğeleri |
| 6 | LevelNodes | Mevcut havuz düğümlerinin gelecekteki bağlama ebeveyni |
| 7 | Rewards | Gerçek sandık ve ödül UI; PNG'ye gömülmez |
| 8 | ForegroundEffects | İnce sis/mote; raycast kapalı |

Statik katmanlar üretimde ayrı tutulur; teslimde sabit taban+mimari+bitki+dekor gerektiğinde tek sprite'a birleştirilerek draw call azaltılır. Dokuz katman dokuz tam boy texture zorunluluğu değildir. Blender yalnız gerekirse mimari/kaya/özel yapı için kullanılır; ışık bake edilir, şeffaf render boyanmış katmanlarla birleştirilir. Her öğenin gerçek zamanlı 3D olması gerekmez. Kaynak .blend: kamera, ışık, koleksiyonlar, materyaller ve render ayarlarıyla birlikte saklanır; bu partta yeni .blend/sanat üretilmez.

## Yükleme ve bellek

Mevcut düğüm/ada görünüm havuzu uygulanmış durumdadır ve korunur. GameObject kapatmak texture RAM'ini serbest bırakmaz: ilk dört sprite doğrudan sahne referansları nedeniyle resident kalır. Yeni sekiz adayı terrain dizisine topluca doğrudan eklemek üretim politikası olmayacak.

Sonraki sanat entegrasyonunda ada anahtarıyla ayrı yerel Addressables prefab/atlas grubu yüklenir (Localization grupları değiştirilmez). `UpdatePool` görünür kümesi tek otorite olur: görünür ada + kaydırma yönündeki bir komşu, en fazla beş bağlı sanat handle'ı. Hızlı kaydırmada istek nesli/ada kimliği kontrol edilir; geç gelen sonuç yanlış havuz slotuna bağlanmaz ve handle'ı bırakılır. Slot ayrılırken efekt durur, sprite/material referansı temizlenir, handle tam bir kez release edilir; ortak düğüm/arka plan tutulur. Eşzamanlı en fazla bir sanat yüklemesi, ana thread'de solver yok. Her kaydırmada Resources.UnloadUnusedAssets veya GC.Collect çağrılmaz. Düşük bellek olayında görünmeyen preload bırakılır. Eksik/başarısız sanat mevcut 'sanatı bekliyor' davranışını korur; artReady kabul öncesinde açılmaz. Bu loader yeni sanat olmadığından bu partta runtime'a bağlanmadı; ayrı ada entegrasyonunun açık kabul maddesidir.

Bütçe modeli (ölçüm değildir): ana birleşik 2048² + iki 1024² efekt/dekor atlası, mipmap yok. ASTC6×6: ceil(w/6)×ceil(h/6)×16 → yaklaşık 2.68 MiB/ada; RGBA32 fallback 24 MiB/ada. Ek 512² efekt 0.113 MiB ASTC /1 MiB RGBA. Beş ada için 13.4–14.5 MiB ASTC veya 120–130 MiB RGBA; ortak Sky/UI, eski dört resident sprite, render target ve Unity overhead hariçtir. Kaynak PNG disk boyutu GPU RAM değildir. Gelecek kabulde gerçek iOS/Android formatı, atlas padding, tepe eşzamanlı yükleme ve Memory Profiler ölçümü gerekir. Tüm 8 adayı tek dev atlasa paketlemek yasaktır; ada başına atlas yüklenebilirlik korunur.

## Sekiz ayrı sanat üretim kartı

Her kartın tabanı: `Assets/Art/Environment/Map/Islands/<key>/<katman>/`; manifest orada. Kaynak: `SourceArt/Map/Islands/<key>/{Blender,Renders,Paint,Masks}/`. Kaynaklar Resources dışında ve otomatik build'e dahil değildir. Ortak Node/Chest sprite'ları yeniden kullanılır. Aşağıdaki PNG adları üretilecek kaynaklardır, mevcut dosya değildir.

| Ada / bölüm | Kompozisyon ve palet | Önemli yapılar ve çevre | Animasyon | Gereken kaynaklar | Tahmini ada dokusu |
|---|---|---|---|---|---|
| woods ·201–250 | Sola yükselen ağaç silueti, ortada açık dere; #35215B #5544A5 #289B91 #FFD27D | Sıcak pencereli ağaç evleri, kavisli köprü, mantar, menekşe, sarmaşık | Ateşböceği yavaş alpha, dere UV, hafif yaprak salınımı | Woods_Base2048.png, Woods_Flow1024.png, Woods_Glow1024.png; Treehouse.blend isteğe bağlı, boyalı bitki kaynağı ve su maskesi | 2.68 MiB ASTC /24 MiB RGBA |
| mirage ·251–300 | Sağ üst pavyon, alt orta turkuaz vaha, mor kaya altı; #62449A #DCA64C #39C6C4 #FFE19C | Kubbeli gözlemevi/pavyon, kemerli köprü, palmiye, çöl çiçeği, altın kum | Vaha parıltısı, ince kum motesi; yoğun sıcaklık distorsiyonu yok | Mirage_Base2048.png, Mirage_Water1024.png, Mirage_Glow1024.png; Pavilion.blend ve boyalı kum/bitki | 2.68 /24 MiB |
| coral ·301–350 | Katmanlı su terasları, sol üst inci sarayı, açık rota; #303D8B #25BEC7 #F48CB9 #FFE3A0 | Mercan kemerleri, şeffaf havuz, su köprüsü, deniz çiçekleri | Ayrı şelale maskesi, yavaş kabarcık ve caustic alpha | Coral_Base2048.png, Coral_Water1024.png, Coral_Glow1024.png, Coral_Bubbles512.png; PearlPalace.blend isteğe bağlı | 2.79 /25 MiB |
| frost ·351–400 | Sağ arka buz zirvesi, önde geniş rota, sıcak küçük kulübe; #405696 #9CE4F4 #8270CE #FFD18C | Kristal kule, buz köprüsü, kar çiçeği, donmuş şelale | Seyrek kar, buz glint; rota üzerinde beyaz patlama yok | Frost_Base2048.png, Frost_Ice1024.png, Frost_Glow1024.png; IceSpire.blend, kar boyası/maskeleri | 2.68 /24 MiB |
| clockwork ·401–450 | Sol üst saat kulesi, iki küçük yardımcı uçan kaya, ince köprüler; #493279 #B68048 #53B4C6 #FFD68A | Zarif saat, pirinç kemerler, sarmaşık, mekanik çiçek | İki küçük çark yavaş dönüş, saat ışık nabzı; tam 3D rig gerekmez | Clockwork_Base2048.png, Clockwork_Gears1024.png, Clockwork_Glow1024.png; ClockTower.blend, ayrık pivotlu çark renderları | 2.68 /24 MiB |
| starlight ·451–500 | Orta arka çiçek tacı, rota boyunca sakin açık bahçe; #3D286E #A36CE1 #5BAFCB #FFE3A7 | Işıklı çardak, yıldız çiçekleri, küçük gölet ve köprü | Çiçek emisyonu alpha, seyrek yıldız tozu; düğüm halo rengi baskılanmaz | Starlight_Base2048.png, Starlight_Pond1024.png, Starlight_Glow1024.png; Gazebo.blend isteğe bağlı ve çiçek paint katmanları | 2.68 /24 MiB |
| library ·501–550 | Sağ arka akademi, sol küçük kitap terası, alt rota boş; #39265F #6656B4 #388FA8 #FFCA79 | Kütüphane, ışıklı pencereler, kitap kemeri, sarmaşık/çiçek, köprü | Pencere nefesi, iki uçan kitap küçük salınım, rün alpha | Library_Base2048.png, Library_Books1024.png, Library_Glow1024.png, Library_Runes512.png; Academy.blend, kitap ayrı render/pivot, rune maskeleri | 2.79 /25 MiB |
| aurora ·551–600 | Arka merkezde yüksek kale, iki yanda ince aurora; alt kaya derinliği açık; #322B76 #647DDA #56D8AC #FFD985 | Gökyüzü kalesi, altın zirveler, ışıklı köprü, buz çiçeği, bulut terası | Düşük alpha aurora UV, sıcak pencere glint, seyrek yıldız | Aurora_Base2048.png, Aurora_Ribbon1024.png, Aurora_Glow1024.png, Aurora_Mist512.png; Citadel.blend ve ayrı aurora/mist paint/maskeleri | 2.79 /25 MiB |

## Sonraki üretimin kabul kapısı

Her ada ayrı görevde: kaynak/PNG teslimi → alpha ve ortak perspektif kontrolü → 03_Map ve ilk dört sprite ile yan yana kalite → yükleme handle testi → gerçek veriden 50 bölüm/sandık/kilit → üç oran ve Safe Area → gerçek Game View → cihaz formatı/bellek. Düğümler okunaklı, altın ışık kontrollü, köprü/şelale silueti anlaşılır olmalı. Bu sekiz ada henüz tamamlanmış sanat değildir; bu rapor PART13B veya başka partı başlatmaz.

## Doğrulama ve Git

Bu bölüm gerçek test sonuçları ve uzak teslim doğrulamasıyla tamamlanır. Önceki PART12 sonuçları bu partta yeniden çalıştırılmış gibi sunulmaz.

Hazırlama aracı projeye otomatik Editor kodu eklemeden çalışır: `unity command script run --file tools/Part13A/PrepareIslandLayers.cs --entry Part13AEditor.Prepare --project-path C:/Dev/Mewgic`. Kaynak Unity içinde derlenip çalıştırıldı; yeniden çalıştırma mevcut prefabı doğrular.

## Gerçek kabul sonuçları

- MAP_MODEL_TESTS.json: mevcut Part12B 1599 kontrol geçti.
- GENERATOR_MODEL_TESTS.json: mevcut Part12D 9013 kontrol, 1000/1000 doğrulanmış bölüm; 845 yeni/155 yedek, başarısız 0.
- UNITY_TESTS.json: mevcut Part12D gerçek Play Mode 19102 kontrol; gerçek oyun, güçler/Pati/kayıt, üç dil/üç oran ve havuz regresyonu geçti. Ana/yedek/bozuk oyuncu kayıtları aynı.
- RELOAD_TESTS.json: ayrı Boot/Play oturumunda 302 kontrol geçti.
- STRUCTURE_TESTS.json: Unity içinde derlenip çalıştırılan hazırlama aracı, 9 sıralı katman, 12×50 katalog, sıfır yer tutucu renderer; ikinci çalıştırma mevcut prefabı koruyarak geçti.
- LOCALIZATION_SOURCE_TESTS.json: 157 anahtar, 8 dil, 185 literal kaynak çağrısı; hata yok.
- COMPILATION.json ve CONSOLE.json: son durum derleme/runtime 0 hata ve 0 uyarı.
- PRESERVATION.json: başlangıçtaki 853 takipli üretim dosyası byte olarak aynı; başlangıçtaki ilgisiz font değişikliği de korunur.

İlk model denemesi Editor derleme/assembly yenilenmesinde ThreadAbortException ile kesildi (MODEL_INTERRUPTED.json); kabul sayılmadı. Ardından tam tur RanToCompletion ve success=true ile yeniden alındı. Açılış/yeniden derleme sırasında görülen Editor singleton mesajları son çalışma turunda tekrarlanmadı; temiz Console ve gerçek test çıktıları yukarıdadır. Eski PART12 rapor/görselleri test öncesi yedeklerinden geri getirildi; yeni sonuçlar yalnız PART13 altında tutulur.

Unity MCP başlangıçta kapalıydı; Unity6000.3.27f1 açılıp Pipeline7800 bağlantısı doğrulandı. Statik MCP araç listesi yenilenmediği için işlemler bağlı Pipeline'a Unity CLI üzerinden yapıldı. Blender5.2.3 MCP127.0.0.1:9876 geçici ayrı sahnede doğrulandı; mevcut sanat dosyaları açılmadı/değiştirilmedi. CLI beta.15 güncellemesi bildirildi, kullanıcı onayı olmadan araç güncellenmedi.

LevelMap_GameView-1600.png ve -1950.png gerçek Game penceresinin screen kaydıdır; görünür pencere ölçeği/kenar boşluğu içerebilir. LevelMap_Render-1600/1950/2000.png aynı çalışan sahnenin tam çözünürlüklü kamera/UI renderlarıdır; 2000 örneğinde daraltılmış Safe Area uygulanır. Yeni sanat veya arayüz maketi değildir. 03_Map ile karşılaştırmada yumuşak 3D ada sanatı, şelale, ışıklı yapı ve mor-altın kimlik korunur. Mevcut grup düğümleri/yatay grup yerleşimi, geniş ada aralığı ve alt kenarda kısmen kırpılan plaka referansın piksel eşleşmesi değildir; bu part görsel düzeni yeniden tasarlamaz. Fiziksel cihaz FPS/RAM/termal veya dokunma testi yapılmadı. Editör Türkçe izole LevelMap önizlemesinde bırakıldı.
