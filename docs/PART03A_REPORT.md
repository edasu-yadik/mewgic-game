# PART 03A — Fantastik Home çevresi

24 Ekim 2026. C:\Dev\Mewgic — Unity 6000.3.27f1 / URP / Android Portrait.

## Teslim

Altı ayrı raster katman üretildi ve gerçek Home sahnesine bağlandı. Karakter,
şişeler, OYNA ve diğer UI tasarımları korunur. Blender kullanılmadı; yerleşik
image_gen ile ayrıntılı 2D illüstrasyonlar üretildi. Üretim promptları ve kaynak
referansları PART03A_GENERATION_PROMPTS.md içinde kayıtlıdır.

Assets/Art/Environment/Home altında:

| Dosya | İşlev | Kaynak boyutu |
|---|---|---|
| Layer01_Sky_v001.png | Opak koyu mavi/mor gök, bulut ve yıldızlar | 841×1870 |
| Layer02_DistantIslands_v001.png | Şeffaf uzak adalar, küçük kaleler, şelale ve sis | 941×1672 |
| Layer03_MainCastle_v001.png | Şeffaf ana kale, köprü, sıcak pencereler, büyük şelaleler | 841×1870 |
| Layer04_Village_v001.png | Şeffaf köy, taş duvarlar, çiçek ve bitkiler | 841×1870 |
| Layer05_Foreground_v001.png | Şeffaf taş platform, merdivenler, fener ve çiçekler | 941×1672 |
| Layer06_Atmosphere_v001.png | Yumuşak alpha sis ve küçük ışıklar | 841×1870 |

Kaynak PNG'ler değiştirilmeden projeye kopyalandı; eski varlıklar ezilmedi.
Beş şeffaf katmanda alpha=0 doğrulandı. Geri dönüş: Git etiketi
`part03a-before-background`, commit `49264a3`.

## Unity yapısı ve hareket

Home.unity içindeki mevcut Home köküne HomeEnvironment eklenir. Sprite ve shader
bağlantıları sahnede serileştirilmiştir. Mevcut HomeCharacterPresenter yalnızca
Mount sırasında bu bileşeni başlatır. Background altında Sky; WorldDecorations
altında EnvironmentComposition ve kalan beş katman oluşur. Eski Night görseli
Home'da kapatılır. Her katmanın ayrı RectTransform'u vardır; bağımsız hareket
denetlendi. Katmanlar gerçek UI ve karakterin arkasındadır; raycast almaz.

Gökyüzü oranı korunarak ekranı kaplar. Mimari görseller enlerine göre aynı oranla
yerleştirilir; ana kaleye küçük kenar payı bırakılır. Foreground ekranın altını
izler. Farklı ekran oranlarında görseller x/y yönünde ayrı esnetilmez.

- HomeWaterLight shader'ı parlak turkuaz su üzerinde aşağı kayan çok hafif ışık
  bantları üretir. Bu, gerçek 3D su simülasyonu veya ayrı parçacıklı sıvı değildir.
- Aynı shader sıcak pencere/fener bölgelerinde yaklaşık %1.4 ışık titreşimi uygular.
- Sis yavaşça kayar ve alpha değeri hafifçe değişir. Kamera içinde sis kenarı kalmaz.
- 16 sabit ortam ışığı yavaşça süzülür/parlar. Havuz her kare yeniden oluşturulmaz.
- Gökyüzündeki boyalı yıldızlar sabittir; küçük hareketli parlamalar atmosfer
  ışıklarıyla sağlanır. Mevcut kedinin ayrı 18 parçacık havuzu değiştirilmedi.

## Test sonuçları

- Unity MCP üzerinden gerçek proje ve sahne okundu; sahne açma, Play, render ve
  Console kontrolleri yapıldı. C# derlemesi başarılı; son Console **0 hata / 0 uyarı**.
- HomeCharacterTests.Run() son sürümle tekrar çalıştı: **49 kontrol başarılı**.
  Idle, Blink, periyodik/elle Wand Magic, OYNA raycast'i, Gameplay geçişi ve menüye
  dönüş doğrulandı. Safe Area simülasyonunda karakter/PLAY çakışması yok.
- Gerçek Game View 900×1600, 900×1950, 900×2000 olarak değiştirildi. Her birinde
  **34 çevre kontrolü** geçti: mimarinin opak sınırları viewport içinde, görsellerin
  oranları doğru, katman hareketi bağımsız ve ortam öğeleri giriş engellemiyor.
  Mimari sınır taraması alpha>128 ve üç piksel örnekleme kullanır; sis saçağı dahil
  tek tek her düşük-alpha piksel için geometrik iddia yapılmaz.
- Boot → Home ayrıca doğrulandı. Test sonunda önceki Game View seçimi geri alındı.
- Sis ve altın ışıkların gerçek zaman içindeki konum değişimi ölçüldü.
- Diğer dönüşümler sabit tutularak gerçek Home iki shader zamanıyla render edildi:
  1087 turkuaz su pikseli ve 9187 sıcak ışık pikseli değişti. Bu sayılar 450×800
  test render'ına aittir. Shader derleme mesajı yok.
- Boot, Gameplay, LevelMap ve WizardCatAnimationTest sahneleri, mevcut WizardCat
  prefabı/PNG'leri/Animator klipleri, PrototypeApp ve oyun modeli Git'te değişmedi.
- APK ve fiziksel Android cihaz testi bu aşamada yapılmadı. Gerçek cihazda FPS,
  GPU overdraw, termal durum ve ASTC görüntü kalitesi ayrıca doğrulanmalıdır.

Ham kanıtlar PART03A_CHARACTER_TESTS.json, PART03A_GAMEVIEW_*.json,
PART03A_MOTION_TEST.json, PART03A_SHADER_MOTION.txt ve PART03A_FINAL_CHECK.json.

## Mobil varlık bütçesi

Altı kaynak PNG toplam **8.36 MB**. Android ASTC 6×6 override, max 2048,
mipmap kapalı, Read/Write kapalı, sRGB, Clamp/Bilinear kullanılır. Kaynaklar
2048 sınırının altındadır; boyut kaybı uygulanmadı.

Altı çevre dokusunun ASTC blok hesabı **4,217,184 byte ≈ 4.02 MiB**'dir.
Bu tüm uygulamanın RAM'i değildir; karakter, UI, framebuffer ve sürücü yükü hariçtir.
Editor runtime doku raporu **43,544,160 byte ≈ 41.53 MiB** verir. Editor dokuları
sıkıştırılmamış kullanır; fiziksel Android bellek ölçümü yapılmadı. Saydam katmanların
overdraw maliyeti cihazda profillenmelidir. Detay: PART03A_ASSET_AUDIT.json.

## Referansla görsel karşılaştırma

Yan yana karşılaştırma: PART03A_COMPARISON.html. Sağdaki görsel gerçek Home render'ıdır.

| Alan | Değerlendirme / kalan fark |
|---|---|
| Kompozisyon | Sağda ana kale, solda köy ve adalar, altta fenerli taş teras korunur. Kale kütlesi referanstan daha baskındır. |
| Palet | Kobalt/gece mavisi, mor ve sıcak altın ilişkisi yakın; gökyüzü daha yıldızlıdır. |
| Derinlik | Ayrı uzak ada rengi/alpha'sı, sis ve öndeki büyük taş/fener ölçeğiyle kurulur; gerçek 3D parallax değildir. |
| Işıklandırma | Sıcak pencereler, soğuk şelaleler ve zemindeki ışık havuzları var. Dinamik fiziksel ışık/karakter gölgesi yok. |
| Mimari | Ayrıntılı kuleler, evler ve köprü var; referans mimarisinin birebir geometrik kopyası değildir. |
| Şelaleler | Birden fazla katmanda var, hafif ışık akışı çalışır. Geçici mor şişe paneli merkez şelalelerin bir bölümünü örter. |
| Bitkiler | Sarmaşık, yosun ve mor/pembe çiçekler üç ayrı düzlemde var. |
| Atmosfer | Sis, küçük altın ışıklar, sıcak fenerler ve uçan adalar mevcut. |
| Karakter uyumu | Mevcut karakter önde, renk ve oranları korunur; çevre onu kapatmaz. Ayrı temas gölgesi henüz yok. |
| Mobil okunabilirlik | OYNA ve etkileşimler erişilebilir. Mevcut logo/alt yazı parlak mimari önünde yer yer daha düşük kontrastlı; nihai UI aşamasında ele alınmalıdır. |

Bu teslim referansla aynı görseli birebir yeniden üretme iddiası taşımaz. Çevre
varlıkları ayrıntılı nihai illüstrasyon niteliğindedir; geçici şişe/UI panelleri
aynı kaldığından tüm menünün referansla görsel eşleşmesi henüz tamamlanmış değildir.

## Tekrar çalıştırma

Home.unity açılıp Play'e girildiğinde çevre ve mevcut karakter otomatik başlar.
Kurulum menüsü Mewgic/Home/Install environment references yalnızca Home referanslarını
yeniden bağlar; Play kapalıyken kullanılır. Testler: HomeCharacterTests.Run(),
HomeEnvironmentChecks.Current(), MotionBaseline()/MotionResult(), ShaderMotion().
Yeni dünya, harita, mağaza veya UI sistemi bu aşamada eklenmedi.
