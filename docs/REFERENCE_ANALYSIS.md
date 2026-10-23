# MEWGIC — Referans ve mevcut durum analizi

23 Ekim 2026 / PART 01. İnceleme: gerçek PNG dosyaları, mevcut C# kaynakları,
sahneler, ProjectSettings, önceki test kayıtları ve prototip render görüntüleri.
Bu belge bir uygulama ve kalite farkı analizidir.

## Referans kayıtları
Kalıcı esas klasör: References/Visual/Approved; kaynak paket:
C:/Users/mehme/Downloads/Mewgic_References (1)/Mewgic_References.
Dosyalar byte düzeyinde korunur; manifest.json kaynak, boyut ve SHA256 içerir.
Önceki References/Visual kopyaları değiştirilmez. Altıncı sohbet ekran görüntüsü
ve kategori önizlemesindeki farklı oyunlar bu listeye alınmaz.

| Dosya | Boyut | Belirlediği karar | Doğrudan uygulanmayacak unsur |
| --- | --- | --- | --- |
| 00_Master_Cover.png | 1122×1402 | Beyaz-gri, mavi gözlü kedi; mor/altın yıldızlı şapka; mavi kristal; yıldızlı asa; Mewgic logosu; büyülü dünya | Kapağın bütününü arayüz veya esnetilmiş dikey arka plan yapmak |
| 01_Home.png | 941×1672 | Üst kaynak şeridi, iki yan kısayol grubu, merkezde dekoratif şişeler, sol altta kedi, büyük yeşil Play ve alt navigasyon | Sahte bakiye/can, işlevsiz satın alma veya sosyal akış eklemek |
| 02_Gameplay.png | 941×1672 | Üst hedef/hamle, yıldız şeridi, iki platformda şişeler, kedi, alt yardımcı kontroller | Level 28, 12 hamle, Combo x3, yıldız eşiği ve güçlendiricileri yeni kural saymak |
| 03_Map.png | 941×1672 | Uçan ada bölgeleri, okunur rota/düğümler, kilitler, bölüm başlığı, sabit HUD | Bu aşamada bölüm haritasını oluşturmak |
| 04_Events.png | 941×1672 | Mor/altın sekmeler, parşömen görev satırları, etkinlik afişleri, ödül vurgusu | Reklam, sezon bileti, ödül ekonomisi geliştirmek |

Referans görüntüleri düzleştirilmiştir; ayrı katman, alpha, font dosyası veya sprite
atlası sağlamaz. Örtülmüş çevreyi ve arkasında kalan karakteri basit kırpmayla eksiksiz
elde etmek mümkün değildir. Gereken parçalar yeniden üretilip ayrı onaylanmalıdır.

## Mevcut teknik envanter
- Unity: ProjectSettings/ProjectVersion.txt → 6000.3.27f1.
- Render pipeline: GraphicsSettings.asset'teki URP varlığı,
  Assets/Settings/UniversalRP.asset ve 2D Renderer; eski temel raporla uyumlu.
- Build Settings: Boot, Home, Gameplay, LevelMap etkin; Boot ilk sırada.
  SampleScene ve şablon sahnesi korunur, ana akışa dahil değildir.
- Boot, Home, Gameplay içinde Mewgic Prototype / PrototypeApp giriş bileşeni.
  Start() Boot'tan Home'a yükler; Home Play Gameplay'e, Home kontrolü menüye döner.
  LevelMap henüz işlevsel harita değildir.
- Assets/Scripts/Core/Puzzle.cs Unity namespace kullanmayan veri/kurallar modeli.
  Kapasite dört; katmanlar alttan üste; üst kesintisiz aynı renk grubu,
  hedefte boş yer ve renk koşuluyla aktarılır. Snapshot geçmişi Undo'yu sağlar.
  Moves yalnız başarılı aktarımda artar. Restart başlangıcı ve geçmişi sıfırlar.
  Won: her şişe boş veya tamamen dolu tek renk.
- Assets/Resources/Levels/Level01.json tek bölüm; iki renk, dört şişe.
  PrototypeApp.LoadLevel JSON'u modele çevirir.
- PrototypeApp.Select seçim ve geçersiz hamleyi yönetir.
  AnimatePour yaklaşık 0,25 saniye taşıma + 0,22 saniye akış gösterir; busy girişi kilitler.
  Model animasyondan önce güncellenir; görseller sonda Refresh ile yenilenir.
- UI: kodla üretilmiş Image, Text, Button, Outline, ScreenSpaceOverlay Canvas,
  CanvasScaler 900×1600 match 0.5, InputSystemUIInputModule ve Safe Area.
- Font: LegacyRuntime.ttf. Mevcut sanat klasörlerinde üretim sprite'ı, özel shader,
  .spriteatlas veya .prefab yok. Referanslar Assets dışında tutulur.
- Gerçek zamanlı 2D ışık sahnelerde vardır; overlay UI otomatik olarak bundan
  sinematik aydınlatma almaz. Parlaklık sprite/materyal tasarımıyla sağlanmalıdır.
- Testler: MewgicPrototypeSetup.TestModel özel assertion tabanlı 12 kontrol ve BFS.
  MewgicPlaySmoke.Run Editor Play Mode callback senaryosudur; NUnit test keşfi değildir.
  Önceki Android APK derlemesi, metadata ve geliştirme imzası kontrolü başarılı.
  Fiziksel cihaz/dokunma/performance testi yok.

## Görsel farklar ve müdahale planı
| Alan | Mevcut durum ve açık | Uygulanacak teknik | Gerekli dosya |
| --- | --- | --- | --- |
| Arka plan/derinlik | Düz gece mavisi ve metin yıldızları; uçan dünya yok | Uzak/orta/ön çevre sprite'ları, sınırlı parallax, sıcak ışık vurgusu | BG_Sky, BG_Islands, BG_Foreground, LanternGlow |
| Cam şekli/parlaklığı | Dikdörtgen Image, boyun ve çizgi; referansın yuvarlak camı yok | Ortak siluet; cam arka, sıvı maske, ön yansıma ve rim; 2D sprite | Bottle_Back, Bottle_Front, Bottle_Mask, Bottle_Rim |
| Sıvı | Düz renk blokları ve aralık çizgileri; menisküs/kabarcık yok | Kesintisiz aynı renk yüzeyi, maske, hafif yüzey hareketi; kimlikler sabit | Liquid_Surface, Bubble, Liquid_UI.shader |
| Buton biçimi | Düz mor kutu, ince Outline; kabartma ve durum çizimi yok | 9-slice gövde, ayrı köşe süsleri, normal/pressed/disabled/focus | Btn_Primary, Btn_Secondary, Btn_Icon |
| Tipografi | Tek düz font ve auto-fit; referansın başlık hiyerarşisi yok | Lisanslı serif başlık/sans metin, Türkçe glyph, ayrı canlı yazı | Font kaynak/lisans, gelecekte TMP font asset'leri |
| Altın süsleme | İnce tek renk çizgi | Metal gradient çizimi, gölge/ışık, 9-slice panel ve corner overlay | Panel_Gold, Corner_Ornament, Star_Gold |
| Büyücü kedi | Yok | Master'a sadık ayrı transparan kedi; idle ve kutlama tepkisi | Cat_Idle, Cat_Blink, Cat_Celebrate, Wand_Glow |
| Ön/arka ayrımı | Board koyu tek blok | Platform, oyun arkası düşük kontrast ve çevre karartması; HUD en üstte | Platform_Stone, Board_Vignette, HUD_Panel |
| Animasyon/FX | Şişe doğrusal hareket ve düz akış | Kaynak boynundan hedef ağzına akış; yatay sıvı, easing, havuzlu yıldız | Pour_Stream, Sparkle, SelectionGlow, materyal |
| Mobil okunabilirlik | Büyük metinler var; görsel yoğunluk, uzun metin, çentik sınanmadı | Temiz oyun alanı, min hit alanı, ölçülü glow, Safe Area ve uzun metin testi | Theme tanımı, ikon durumları, erişilebilir renk/desen |

Mevcut prototip resimleri docs/prototype-home.png, prototype-gameplay.png,
prototype-gameplay-tall.png ve prototype-win.png'dir. Düz geometrik çizimler hedef
sanat kalitesiyle eşdeğer değildir. Ana görsel fark, birkaç renk değişikliğiyle kapanmaz:
ayrı üretim varlıkları, katmanlı kompozisyon ve sunum refaktörü gerekir.

## Korunacak davranış sınırı
Puzzle ve bölüm verisi korunur. Önce Home görünümü ayrılır; Gameplay görünümü sonra
modelin mevcut API'sine bağlanır. Skin değişirken kapasite, dökme kuralları, Undo,
Moves, Restart ve kazanma koşulları değişmez. Görsel yıldız, Combo, can ve booster
iş kuralları ayrıca tanımlanmadan eklenmez. Yeni sıvı shader'ı iş kurallarını yönetmez.
PART 01 çalışma sonucunun test kaydı PART01_VALIDATION.md içinde bulunur.
