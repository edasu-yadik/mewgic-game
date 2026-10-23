# MEWGIC — Asset üretim planı

PART 01 / 23 Ekim 2026. Aşağıdaki dosyalar üretim talepleridir; henüz üretilmedi.
P0: ilk Home/Gameplay görsel dönüşümü; P1: kalite ve karakter hareketi;
P2: ileride Map/Events ve ek içerik. Boyutlar kaynak üretimi için başlangıç hedefidir.
Oyun içi import boyutu cihazdaki görünür ölçüye göre düşürülür.

## Üretim listesi
| Asset adı | Kullanım / tür ve başlangıç boyutu | Animasyon | Öncelik | Referans |
| --- | --- | --- | --- | --- |
| Logo_Mewgic | Ayrı şeffaf PNG, 1536×640; metin/marka çizimi | Hafif açılış parıltısı ayrı | P0 | 00 |
| BG_Sky | Metinsiz uzak gökyüzü PNG, 1440×2560 | Çok hafif kayma | P0 | 00,01,02 |
| BG_Islands | Şeffaf PNG ada/kale/şelale orta katman, 1440×2560 | Parallax; su efekti ayrı | P0 | 00,01,02 |
| BG_Foreground | Kenar bitki/taş/fener PNG, 1440×2560 | Fener ışığı ayrı | P0 | 01,02 |
| Platform_Stone | Şeffaf sprite, 1536×384 | Yok | P0 | 02 |
| Bottle_Back | Şeffaf cam gövde PNG, 384×768 | Transform taşıma/eğilme | P0 | 02 |
| Bottle_Front | Aynı tuval/pivotta yansıma PNG, 384×768 | Çok hafif highlight | P0 | 02 |
| Bottle_Mask | Aynı tuvalde gövde iç alan maskesi, 384×768 | Şişe ile dönüş; yüzey ayrı | P0 | 02 |
| Bottle_Rim | Şeffaf boyun/ağız PNG, 384×192 | Şişeyle birlikte | P0 | 02 |
| Liquid_Surface | Şeffaf menisküs sprite, 256×64 | Düşük genlik dalga | P0 | 02 |
| Liquid_UI | UI uyumlu maskeli unlit shader/material | Fill, yüzey ve eğim parametresi | P0 | 02 |
| Pour_Stream | Shader destekli mesh/sprite, küçük tile doku 64×128 | Akışa göre uzama/UV | P0 | 02 |
| Btn_Primary | Metinsiz yeşil/altın 9-slice PNG, 768×256 | Press scale/tint | P0 | 01 |
| Btn_Secondary | Mor/altın 9-slice PNG, 512×192 | Normal/pressed/disabled | P0 | 01,02 |
| Btn_Icon | Mor/altın yuvarlak çerçeve PNG, 256×256 | Seçili/pressed | P0 | 01,02 |
| Panel_Gold / Panel_Parchment | Merkez ve kenarları ölçeklenebilir PNG, 768×384 | Giriş/fade | P0 | 01,02,04 |
| Corner_Ornament | Şeffaf köşe sprite seti, 128×128 | Yok | P0 | 00,01,02 |
| Icon_Undo / Restart / Home / Play | Ayrı şeffaf PNG, ikon başına 256×256 | Buton ile | P0 | 01,02; Restart özgün uyarlama |
| Font_Title / Font_UI | Lisanslı font kaynağı, lisans ve TMP atlas üretimi | Yok | P0 | 00,01,02 |
| Cat_Idle | Beyaz-gri kedi transparan PNG, 1024×1536 | Hafif nefes; yüz/değnek ayrı gerekebilir | P0 | 00 kimlik,01/02 pozisyon |
| Cat_Blink / Cat_Celebrate | Uyumlu katman/sprite frame seti | Göz kırpma, kısa kazanma tepkisi | P1 | 00,02 |
| Wand_Glow / LanternGlow | Küçük şeffaf PNG 128×128, additive FX materyali | Yumuşak pulse | P1 | 00,01,02 |
| SelectionGlow / Sparkle / Bubble | PNG sprite atlas + havuzlu parçacık, 64–256 | Seçim, dökme, kazanma | P1 | 02 |
| Home_HeroPotions | Üç ayrı şişe PNG, şişe başına 512×1024 | Şişe başına hafif idle | P1 | 01 |
| Star_Filled / Star_Empty | Şeffaf PNG, 192×192 | Kazanma eşiği tanımlandıktan sonra | P1 | 02 |
| HUD_ResourcePill / Coin / Gem / Heart | 9-slice panel ve ayrı ikonlar, 128–512 | Veri güncelleme vurgusu | P2 | 01,02 |
| NavigationIconSet | Home/Map/Events/Team/Inventory ayrı PNG 256×256 | Aktif/pasif durum | P2 | 01,03,04 |
| Map_RegionLayers / Nodes | Bölge PNG ve düğüm/rota/lock sprite seti | Kaydırma; kısa seçim FX | P2 | 03 |
| Events_Header / Tabs / RewardIcons | Katmanlı afiş, 9-slice sekme ve ikon PNG | Açılma/ilerleme | P2 | 04 |

3D model ilk hedef için gerekli değildir. Gerçek zamanlı sıvı fiziği yerine model
katmanlarından sürülen 2D görünüm kullanılır. Şişe ve sıvı aynı raster görüntüye birleştirilmez.
Listede referans numaraları Approved klasöründeki dosya önekleridir.

## Dosya teslim standardı
İsim: Cat_Idle_v001.png gibi sürümlü üretim kaynağı; onaylı Unity adı sürümsüz.
PNG alpha temiz olmalı; şeffaf alan dama veya beyaz renge boyanmamalı.
Cam arka/ön/maske aynı tuval, alt merkez pivot, aynı boyun ağzı koordinatıyla teslim edilir.
Liquid katmanları, metin ve kontrol ikonları bağımsızdır; işlevsel etiketler resme gömülmez.
Kedinin tüy kenarı, göz rengi, şapka yıldızları, mavi kolyesi ve asa kimliği Master'a
göre kontrol edilir. Yüz/pençe değişimi veya farklı turuncu kedi kabul edilmez.
9-slice parçalarında sınır payı dosyayla birlikte belirtilir; köşe süsleri esnetilmez.

Kaynak/versiyonlar: References/ProductionSources.
Onaylı PNG: Assets/Art altındaki ilgili klasör.
Atlaslar: Assets/Art/Atlases; prefab'lar Assets/UI/Prefabs veya Assets/Prefabs/Gameplay/Bottles.
Şeffaf kaynak birleştirme dosyaları, üretim notu, model/prompt ve kullanılan referans
adı kayıt altına alınır. Orijinal Approved dosyaları üzerine yazılmaz.

## Üretim sırası ve araçlar
Bu oturumda ImageGen aracı kullanılabilir; referanslı üretim ve transparan PNG üretimi
yapabilir. PART 01 yalnız analiz ve plan istediği için görsel üretimi çağrılmadı.
İleride önce tek bir şişe katman seti ve kedi kimlik testi üretilir; onay sonrası
diğer durumlar/assetler türetilir. Her ekranı bağımsız üreterek karakter tutarsızlığı yaratılmaz.

Araç, referansı piksel piksel kopyalamayı, katmanlı PSD'yi, tutarlı sprite animasyonunu,
cam/maske hizasını veya temiz UI yazısını otomatik garanti etmez.
Şeffaflık ve hizalama gerçek dosyada doğrulanır; font, logo yazımı ve 9-slice kenarlar
gerekirse grafik editöründe elle tamamlanır. Düşük kaliteli otomatik çizim kabul yerine geçmez.
Kaynak ekranı kırpmak örtülmüş bölgeleri geri getirmez; katmanlar ayrı üretilir.
Shader, prefab, atlas ve animasyon bağlantıları Unity içinde ayrıca uygulanır.
Bu aşamada yeni bitmap, shader veya 3D model üretilmedi.

## Onay kapıları
1. Master ile yan yana karakter/renk/ışık kontrolü; aynı ölçekte alpha kenarı incelemesi.
2. Şişede boş, bir katman, dört farklı renk, tek renk dolu, seçili ve dökülür durum.
3. Home/Gameplay içinde yazı ve hit alanı gerçek etkileşimli UI olarak kontrol.
4. Android'de filtreleme, sıkıştırma, örtüşme, GPU bellek ve kare süresi ölçümü.
5. Model ve Play Mode testleri geçmeden önceki görünüme geçiş kaldırılmaz.

Sonraki aşama için ilk üretim paketi: BG katmanları, Cat_Idle, dört şişe katmanı,
Primary/Secondary buton, Panel_Gold ve temel kontrol ikonları.
