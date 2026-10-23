# MEWGIC — Görsel tasarım sistemi / PART 01

Durum: 23 Ekim 2026. Bu belge uygulanacak mimariyi tanımlar; yeni ekran, prefab,
shader veya üretilmiş sanat varlığı teslim edildiği anlamına gelmez.
Çalışan prototipin korunma kaydı: 9ebcae0.

## Referans sözleşmesi
References/Visual/Approved/00_Master_Cover.png marka ve karakter kimliğinde esas alınır.
01_Home.png ana menü, 02_Gameplay.png oynanış yerleşimi için ana kaynaktır.
03_Map.png ve 04_Events.png ilerideki ekranların tutarlılığını sağlar.
Approved/manifest.json kaynak, boyut ve SHA256 içerir. Orijinaller değiştirilmez.
Eski References/Visual/Cover.png vb. kopyalar geçmiş olarak korunur.
Sohbetteki küçük kategori önizlemesinin turuncu kedisi, insan büyücüsü ve reklam
kompozisyonu tasarım kararı değildir; gerçek PNG dosyaları üstündür.
Kapak 1122×1402, ekran referansları 941×1672'dir. Kapak 9:16 arka plan olarak esnetilmez.

## Tasarım ölçüleri ve yerleşim
Referans çalışma alanı: 900×1600 (9:16). Mevcut CanvasScaler da bu ölçüdedir:
Scale With Screen Size, Match Width Or Height = 0.5 başlangıç değeri korunur.
Pikseller mantıksal tasarım birimidir; farklı telefonlarda gerçek piksel değildir.
Arka plan ekranı taşarak doldurur; etkileşimli HUD ve kontroller SafeAreaRoot içinde kalır.
Screen.safeArea normalleştirilir, çözünürlük veya güvenli alan değişince yenilenir.
Çentik boşluğu iki kez uygulanmaz. Kaydırılan içerik, sabit alt navigasyonun arkasına kaçmaz.

| Bölge | Home için önerilen aralık (üstten) | Gameplay için önerilen aralık |
| --- | --- | --- |
| Üst HUD | 24–112 | 24–112 |
| Yan kısayollar / bölüm hedefi | 144–680 | 136–300 |
| Kahraman / oyun alanı | 400–1080 | 360–1240 |
| Ana eylem / alt kontroller | 1110–1290 | 1320–1480 |
| Alt navigasyon | 1380–1560 | Ayrı navigasyon yerine gerekli oyun kontrolleri |

Bunlar referanstan türetilen başlangıç bölgeleridir; gerçek cihazda içerik sığması
önceliklidir. Çakışma varsa dekor kırpılır, kontrol küçültülmez. Gameplay'deki
şişe alanı bölümün şişe sayısına göre satır/sütunla hesaplanır; kodun mevcut
iki sütun varsayımı görsel katmanda daha sonra kaldırılır, Puzzle modeli değişmez.
4 şişelik ilk bölüm 2×2 kalabilir; referanstaki 8 şişe için gelecekte 4×2 düzen gerekir.

Dokunma alanı hedefi en az 48 mantıksal birim; yoğun ekranlarda fiziksel
48 dp karşılığı cihazda ayrıca doğrulanır. Görünür ikon küçükse hit alanı büyütülür.
İlk sürümde çalışmayan ekonomi, güçlendirici ve sosyal kısayollar gösterilmez.
Referanslardaki sayılar, yıldız eşiği ve Combo x3 yeni oyun kuralı sayılmaz.

## Renk ve malzeme dili
Aşağıdakiler ekranlardan yön alınarak seçilmiş başlangıç token'larıdır; ölçülmüş
renk örneklemesi veya tamamlanmış erişilebilirlik onayı değildir.

| Token | Değer | Rol |
| --- | --- | --- |
| NightBase | #10162E | Arka plan ve karartma |
| RoyalPurple | #39245E | Panel yüzeyi |
| VioletAccent | #8D36D6 | Büyü ve seçili durum |
| GoldLight | #F4CA72 | Çerçeve ışığı / vurgu |
| GoldShadow | #8A4B20 | Metal derinliği |
| Parchment | #FFF0CE | Açık hedef ve görev paneli |
| Ink | #352039 | Açık yüzey üstü metin |
| TextLight | #FFF6E5 | Koyu yüzey üstü metin |
| ActionGreen | #31CE48 | Home ana eylemi |
| LiquidCyan / Pink | #23D2F2 / #F041A4 | Mevcut sıvı kimlikleri 0 / 1 |
| LiquidYellow / Violet | #FFD33B / #9758F4 | Mevcut sıvı kimlikleri 2 / 3 |

Sıvı kimlikleri mevcut JSON ve modelle aynı kalır. Renk sıralaması sprite değişince
değişmez. Renk körlüğü için ek desen/simge seçenekleri sonraki kullanıcı testine alınır.
Parlak metal çerçeve, koyu mor gövde, sıcak ışık ve soğuk mavi derinlik birlikte kullanılır.
Buton metni ayrı Text bileşenidir; çizimin içine gömülmez.

## Tipografi
Prototip LegacyRuntime.ttf ve UnityEngine.UI.Text kullanıyor. Sonraki aşamada mevcut
TMP altyapısı ve lisanslı font dosyası doğrulandıktan sonra TMP'ye kontrollü geçiş yapılır;
şimdi paket kurulmaz veya yazı bileşenleri değiştirilmez.
Başlık: okunaklı, yumuşak fantastik serif; buton/sayı/gövde: açık sans serif.
Font dosyası seçimi henüz yapılmadı. Türkçe İ ı Ş ş Ğ ğ Ç ç Ö ö Ü ü ve Latin rakamlar
atlas kapsamına alınır; lisans belgesi teslimin parçası olur.
Önerilen ölçek: büyük başlık 56–72, panel başlığı 36–44, buton 32–40,
gövde 28–32, ikincil açıklama 24. Sürekli auto-shrink ile küçük metne inmek yerine
satır kırma ve alan büyütme kullanılır. Hedef kontrast: gövdede 4.5:1,
büyük metinde 3:1; bu aşamada gerçek assetlerle ölçülmedi.

## Katmanlar ve sahiplik
Önerilen görünüm hiyerarşisi:
- BackgroundCanvas / order 0: uzak gökyüzü, orta ada/kaleler, öndeki bitki/fener.
- GameplayCanvas / order 10: platform, BottleBoard, BottleView'lar.
- CharacterCanvas / order 15: ayrı MageCatView; şişe hit alanını kapatmaz.
- HUDCanvas / order 20: SafeAreaRoot, üst bilgi, hedef, sayaç, alt kontroller.
- OverlayCanvas / order 40: seçili şişe / dökme görünümü.
- ModalCanvas / order 100: karartma, kazanma veya duraklatma paneli.
- TransitionCanvas / order 200: kısa geçiş ve giriş engelleme.

Bu ayrım mevcut sahnelerde henüz kurulmadı. Statik arka plan ile sık değişen HUD
ayrılır; her ikon için Canvas açılmaz. Dekoratif Image'larda raycastTarget kapalıdır.
Modal açıkken arka panel girdisi CanvasGroup ile engellenir. Safe Area tek sahibidir.
Kedi, şişeler, butonlar ve dökme efektleri arka planın içine boyanmaz.
Yalnızca birlikte sabit kalan uzak çevre detayları aynı resimde birleşebilir.

## Prefab ve sunum bağlantıları
| Önerilen prefab/bileşen | Sorumluluk | Değişmez bağlantı |
| --- | --- | --- |
| ButtonPrimary / ButtonSecondary | 9-slice yüzey, ayrı metin, hit alanı, ses/animasyon | Unity Button.onClick |
| PanelOrnate / ModalPanel | Köşe süsü, merkez, kenar parçaları; içerik alanı | CanvasGroup |
| BottleView | CamBack, LiquidMask, katmanlar, GlassFront, SelectionGlow | Şişe indeksi |
| BottleBoardView | Görünüm havuzu, düzen ve animasyon katmanı | Puzzle.Bottle |
| GameplayPresenter | Seçim, kilit, Undo/Restart/Home, model → görünüm | Puzzle.Pour/Undo/Restart/Won |
| ResourcePill / NavigationItem | İleride gerçek veri geldiğinde gösterim | Sahte ekonomi verisi bağlanmaz |
| MageCatView | Idle/tepki, ayrı değnek ve parıltı bağlantısı | Oyun sonucu olayı |
| SafeAreaRoot / Theme | Ekran uyumu ve renk/font/sprite referansları | Tek ortak tanım |

PrototypeApp bugün hem sahne akışını hem UI üretimini hem seçimi yönetiyor.
Geçişte önce görünüm arayüzü eklenir, sonra Home ve Gameplay tek tek prefab'a alınır.
Puzzle.cs ve Level01.json değiştirilmez. Seçim, geçersiz hamle, hareket sayısı ve
animasyon kilidinin davranışı korunur. Refresh() her hamlede nesneleri silip oluşturuyor;
yeni BottleView'lar bir kez oluşturulup güncellenir. Bu optimizasyon henüz uygulanmadı.

## Cam, sıvı, animasyon ve mobil bütçe
İlk hedef 2D sprite + UI uyumlu basit sıvı materyali; 3D fizik veya gerçek zamanlı
kırılma zorunlu değildir. Cam ön/arka, sıvı maskesi ve parıltı ayrı dosyalardır.
Şişe eğilince sıvı yüzeyi ekran uzayında yatay kalmalı, maske şişeyle dönmelidir.
Üst renk grubu miktarı modelden gelir; görüntüde animasyon tamamlanırken girdi kilitlenir.
Shader gerekli UI stencil/clip/RectMask2D davranışlarını desteklemeli; önce hedef
cihazda doğrulanır. Mevcut URP 2D Renderer korunur.
Dökme akışı, havuzlanan küçük sprite veya çizgi mesh ile; kabarcık ve yıldızlar
havuzlanan parçacıklarla yapılır. Her şişede sürekli büyük blur/refraction yok.
Hareketsiz kullanıcı için yoğun titreşim azaltılabilir; seçili durumu yalnızca ışıkla anlatılmaz.

Başlangıç performans hedefi (ölçülmüş sonuç değildir): orta Android cihazda 60 FPS,
düşük kalite profilinde 30 FPS; görünür yumuşak parçacık sayısı toplam 64–96,
UI/şişe dokuları için yaklaşık 64 MiB yerleşik GPU bellek bütçesi.
Profiling ile aşılırsa arka plan katman sayısı, alpha örtüşmesi ve atlas boyutu azaltılır.
Hamle animasyonunda yönetilen bellek tahsisi hedefi sıfıra yakın; ölçüm sonra yapılır.

## Sprite içe aktarma ve atlas planı
PNG RGBA, transparan çevre, temiz alpha kenarı; beyaz zeminli kesit kabul edilmez.
Sprites: Sprite (2D and UI), sabit PPU=100 başlangıç standardı, bilinear,
Clamp, UI mipmap kapalı. Normal renk sprite'ları sRGB; maske verileri linear.
Kaynak PSD/katmanlı çalışma References/ProductionSources altında, oyun PNG'leri Assets içinde.
Packing öncesi pivot (şişe alt merkez; kedi alt merkez), şişe boyun çıkış noktası
ve güvenli transparan pay ortak şablonla tanımlanır.
UI atlasında rotation kapalı, padding 4; 9-slice parçalarında tight packing kapalı.
Önerilen atlaslar: UI_Common (2048), Bottles_FX (1024–2048), MageCat (2048),
Home_Decor (2048). Büyük arka planlar ayrı; Map/Events ekranla birlikte yüklenir.
Tek dev atlasla bütün ekranların belleğe alınması önlenir. Addressables bu aşamada eklenmez.
Android için ASTC 6×6 başlangıç adayı; cam kenarı/metinde 4×4 veya daha yüksek kalite
yalnızca cihaz karşılaştırmasıyla seçilir. Desteklenmeyen cihaz için fallback ayrıca sınanır.
Atlas sayısı tek başına draw call garantisi değildir: materyal, maske ve sıralama da ölçülür.

## Kalite kabulü
900×1600, 900×1950 ve 1080×2400 ekranlarda; çentikli/çentiksiz güvenli alan kontrolü.
Metin ve hit alanlarında çakışma yok; logo/kedi esnetilmez; taşan dekor kontrollü kırpılır.
Çalışan mekanikler aynı model ve Play Mode senaryolarını geçer.
Asset onayı için kenar/alpha kontrolü, gerçek boyutta tipografi, referansla yan yana
görüntü ve Android profiler kaydı gerekir. Bu aşama nihai görsel kalite onayı değildir.
