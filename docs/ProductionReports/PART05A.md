# PART05A — Gameplay görsel altyapısı

25 Ekim 2026. Yalnız Gameplay çevresi,katman/yerleşim altyapısı ve mevcut kedi prefabı entegrasyonu. Home.unity değiştirilmedi. Home'daki açık04D/04F görsel işleri bu görevde uygulanmadı.

## Oluşturulanlar ve değiştirilen dosyalar

- Assets/Art/Environment/Gameplay/Gameplay_Backdrop_v001.png:948×1660 opak kale/şelale/gece arka planı.
- Gameplay_Shelf_v001.png:2078×757 kaynak canvas,gerçek alpha;tek tekrar kullanılabilir taş raf. Saydam marj Sprite rect ile importta ayrıldı,kaynak pikseller korunur.
- Gameplay_Foreground_v001.png:941×1672 saydam kenar fener/çiçek/sarmaşık katmanı. Full-canvas düzeni korunur.
- Üç varlık referans02_Gameplay'den yerleşik image_gen ile üretildi;örtülü bölgeler tamamlandı. Blender kullanılmadı. İstemler PART05A_PROMPTS.md içinde.
- GameplayPresentation.cs:tam ekran GameplayBackdrop/CastleWaterfalls/ForegroundEdges;safe-area GameplayWorld/StoneShelves/CatStonePerch/GameplayWizardCat;mevcut Bottle board dinamik etkileşim alanıdır. Raf,kedi ve çevre Image/Graphic'leri raycast almaz. Mevcut opaque Bottle board paneli saydamlaştırılır.
- Gameplay.unity:PrototypeApp'ın bulunduğu nesneye GameplayPresentation ve üç sprite/onaylı WizardCat.prefab referansları kaydedildi.
- PrototypeApp.cs:yalnız Gameplay kurulum/Refresh ve dökme animasyonu başlangıç-bitişine sunum bağları eklendi. Puzzle modeli,Select/Puzzle.Pour kuralları,sıvı üretimi,Undo/Restart/Win/Replay ve bölüm verisi yeniden yazılmadı. Home metodu değişmedi.
- GameplayVisualTests.cs:yerleşim ve gerçek UI regresyonları.

## Dinamik yerleşim

Mevcut dört şişe iki raf/two-row düzeninde,kediye solda ayrılan alanın sağında yer alır. Slot hesabı şişe sayısından kolon/satır ve alan boyutundan genişlik/yükseklik türetir.1–24 şişede slot sınırları ve birbirleriyle örtüşmeme denetlendi. Bu24 gerçek oynanmış bölüm anlamına gelmez;yalnız mevcut dört şişelik bölüm oynandı. Daha kalabalık seviyelerde okunabilirlik ve fiziksel dokunma boyutu ayrıca değerlendirilmelidir.

Onaylı WizardCat prefabı değiştirilmeden instantiate edilir. Tüm karakter sol altta görünür,küçük bağımsız taş perç üzerinde durur. İlk görüntüdeki pivot kaynaklı kesilme aynı görevde düzeltildi. Şişeler Home dekoratif PNG'leri değildir;mevcut UI şişeleri ve dinamik sıvı katmanlarıdır.

Dökme sırasında IsAnimating bayrağı yerleşim kodunun şişe konumlarını sıfırlamasını engeller. Animasyon bittiğinde yeni durumla Bind/Layout çalışır. Safe Area ve ekran değişimlerinde düzen yeniden hesaplanır. Çevre tam ekran orantılı cover ölçeklenir;UI/şişe/kedi Safe Area içindedir. İlk safe-area ön plan kenar izi tam ekran dekor katmanına taşınarak düzeltildi.

## Görsel kalite ve referanstan kalan farklar

Şelale,uçan ada,kale,sıcak altın fener,gece mavisi/mor,taş/çiçek/sarmaşık atmosferi yakındır. Mimari/perspektif ayrıntıları birebir değildir;bazı fenerler arka plan illüstrasyonuna gömülü,ön kenar dekoru ayrıdır. Tek raf asset'i her satırda yeniden kullanılır;referanstaki farklı taş basamakların birebir kopyası değildir. Kedi onaylı oturan pozdadır ve referanstan küçüktür.

**Şişe ve kontrol sanatı bu partta bitirilmedi:**dikdörtgen prototip şişeler,değişen renk/miktarlar ve mevcut mor Undo/Restart/Home butonları korunur. Referansın cam şişeleri,Goal/Moves kartları,yıldız çubuğu,Hint/Extra Bottle/Shuffle/Combo veya üst ekonomi UI'sı bu partın çıktısı değildir. Referansta daha fazla şişe varken başlangıç verisi dört şişedir ve değiştirilmedi. Görsel karşılaştırma tüm Gameplay ekranının final kalitede olduğunu iddia etmez;yalnız çevre altyapısı tamamlandı.

Sprite Single,shelf için Multiple içinde tek kırpılmış sprite;PPU100,pivot(.5,.5),FullRect,alphaIsTransparency,sRGB,Bilinear/Clamp,mip/readwrite kapalı,max2048,Android ASTC4x4. Raf/ön plan gerçek alpha kontrolünden geçti;arkaplan bilerek opaktır. Kaynak PNG çözünürlükleri korunur. Mobil cihaz bellek/FPS profili ölçülmedi.

## Gerçek testler

Son PART05A_TEST_RESULTS.json:2755 kontrol başarılı. Büyük kısmı1–24 şişelik slot sınırı/örtüşme invariants;gerçek dört şişelik seviyede source/target raycast tıklamaları,geçersiz hamlede sayaç0,başarılı aktarım,sunum kilidi,Undo,Restart,kazanma,Replay,Gameplay→Home→PLAY→Gameplay akışı geçti. Aynı karede Refresh ile yenilenen butona basan ilk test ayrı karelere ayrıldı;gerçek etkileşim doğrulandı.

900×1600,900×1950,900×2000 ve çentik/safe-area simülasyonu:şişeler/kedi görünür,sınır içinde,kedi/şişe/kontrol örtüşmesi yok. Dört şişe dokunulabilir.12 mevcut model kontrolü ve mevcut Gameplay smoke exitCode0 ile tekrar geçti. Son ön plan düzeltmesinden sonra2755 kontrol tekrar çalıştırıldı.

C# derlemesi ve son Console:0 hata/0 uyarı. Ara test dosyasındaki childCount derleme hatası giderildi;ilk kayıt PART05A_REPAIR_DIAGNOSTICS.json içinde saklanır. Fiziksel Android cihazı,yeni APK ve24 şişenin gerçek oynanış performansı test edilmedi.

182 önceki dosya SHA256 ile aynı:Home.unity,mevcut sanat/prefab/animasyonlar,Puzzle kaynakları ve Level01 verisi korunur. Geri dönüş:C:/Dev/Mewgic-Part05A-Backup. DevControl'e müdahale edilmedi.

## Kanıtlar ve sonraki aşama

PART05A_GAMEVIEW.png gerçek900×1600 Gameplay ScreenCapture Game View çıktısıdır. gameplay-05a-1600/1950/2000/2000-safe.png çalışan Canvas'ın kamera yakalamalarıdır. PART05A_COMPARISON_Dark/Light.png referans ve gerçek Gameplay'i tam ekran yan yana gösterir. IMPORT/ALPHA_AUDIT/TEST_RESULTS/SMOKE_CHECK/FINAL_CHECK/PRESERVATION.json ham kanıtlardır.

Bir sonraki görsel aşama için katman ve dinamik slot altyapısı hazırdır;şişe sanatı ve Gameplay UI ayrı görev gerektirir. Git commit/push doğrulaması teslim kaydına eklenir. Başka part başlatılmadı.
