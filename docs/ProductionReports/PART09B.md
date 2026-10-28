# PART09B — Ana menü canlılık katmanı

29 Ekim 2026 • Unity 6000.3.27f1 • master başlangıcı 89b1056.

## Sonuç

Home tasarımı korunarak PLAY görseline yaklaşık 6 saniyelik ±%0,6 nefes ve
7 saniyelik hafif altın kenar geçişi eklendi. Dokunma alanı animasyona katılmaz.
Basma hareketi önceliklidir; bekleyen geçişte nefes ve kenar geçişi durur.
Mevcut pressed sprite, bırakma gecikmesi ve çift tıklama koruması korunur.

Şelaleler üç mimari katmana gömülüdür. Kaynak PNG'ler değiştirilmeden uzamsal
bölgeler ve kaynak renk/alpha bilgisiyle üç adet 256×512 veri maskesi üretildi.
Kırmızı kanal yalnız suda küçük örnekleme dalgalanması ve aşağı ilerleyen ışık
çizgileri, yeşil kanal seçili altın pencerelerde ±%1,8 ışık değişimi uygular.
Tüm katman veya mimari kaydırılmaz.

## Mevcut animasyon denetimi

- WizardCat Animator, nefes, otomatik göz kırpma, zamanlanmış değnek hareketi
  ve 18 kıvılcım havuzu zaten vardı; yeniden üretilmedi.
- Üç şişenin salınımı, mevcut glow/sparkle ve merkezdeki hazır spiral korundu.
  HomePotionDisplay ve sanat dosyaları değiştirilmedi.
- Sis ve 16 havuzlanmış yavaş/parlayan ortam ışığı aynı yoğunlukla korundu.
  Ek ParticleSystem veya her kare nesne üretimi eklenmedi.
- Eski su shader'ının renk tabanlı hafif ışığı, katmana özel maskelenmiş
  su hareketi ve pencere ışığıyla değiştirildi.

## Gerçek testler

HOME_MOTION_TESTS.json: gerçek Home Play Mode'da 38 kontrol geçti;
runtime Error/Exception/Assert kaydı yok. C# ve shader'lar Unity'de derlendi.
900×1600, 900×1950, 900×2000 (Safe Area iç boşluğuyla) kamera render'ları
incelendi. Kimlik, yerleşim, mevcut şişe ve kedi sunumu korundu.

- 63 dokunma noktası nefeste aynı sonucu verdi; hedef RectTransform aynı.
- Basma küçülmesi/sprite, dokunma engellenmemesi, çift tıklama koruması,
  gerçek Gameplay'e geçiş ve Home'a dönüş geçti.
- 6,5 saniyede nesne sayısı sabit; mevcut kedi/şişe/değnek ve ortam hareketleri
  sürdü. Dönüşte tek HomeEnvironment ve tek PLAY component'i var.
- Test profili kullanıldı; gerçek oyuncunun PlayerPrefs kaydı değişmedi.
- GPU render karşılaştırması (0/2,8sn): uzak adalar su/pencere değişen piksel
  2480/169; ana şato 5013/722; köy 1053/809. Maske dışı 125020, 121458,
  125669 pikselin tamamı birebir sabit: toplam 372147.
- Mevcut MewgicPlaySmoke.Run, Boot üzerinden oyun akışını çalıştırdı;
  Editor süreç çıkışı 0. Log ve gerçek render'lar aynı kanıt klasöründe.

İlk GPU testinde alpha-blended hedef ikinci ölçümden önce temizlenmediğinden
karşılaştırma başarısızdı. Test düzeneğinde her ölçümden önce hedef temizlendi;
aynı başlangıç koşullarıyla bütün kontroller geçti.

## Performans ve sınırlar

Üç maske toplam 1,5 MiB GPU verisidir; okunabilir CPU kopyası da tutulur.
Üç farklı kaynak texture için üç kalıcı materyal başlangıçta oluşturulur ve
Home yok edilirken serbest bırakılır. Shader toplam üç texture örneklemesi
kullanır (kaynak, maske, kaydırılmış kaynak). PLAY mevcut glow Image üzerinde
tek örneklemeli shader kullanır. Kare başına yeni materyal/nesne oluşturulmaz.

Windows Unity/D3D doğrulamasıdır; fiziksel iPhone/Metal FPS ve pil ölçümü
yapılmadı. Yeni iOS/TestFlight/Cloud Build başlatılmadı. Editor kapanışındaki
JobTempAlloc uyarıları loglarda saklandı; oyun sırasındaki hata kaydı boş.
39 gerçek Unity karesinden üretilen GIF cihaz performans ölçümü değildir.

## Koruma, dosyalar ve teslim

PRESERVATION.json: başlangıçtaki 915 tracked dosyadan yalnız Home sahnesindeki
dört referans, üç Home sunum script'i ve HomeWaterLight shader'ı değişti.
910 diğer tracked dosya ve önceden bulunan 133 untracked dosya byte düzeyinde
korundu. Gameplay, Boot, kayıt sistemi, karakter prefab/animasyonları,
Unity sürümü ve iOS yapılandırması aynı. Smoke'un eski rapor görselleri yeni
klasöre kopyalanıp önceki dosyalar geri korundu.

Yeni dosyalar: PlayFrameSweep shader'ı, üç native Texture2D maskesi,
Part09BEditor kurulum/test aracı ve yalnız Editor'de derlenen HomeMotionProbe.
Probe üretim sahnesine kaydedilmez. Kurulum menüsü:
Mewgic/PART09B/Install Home motion masks.

Kanıt klasörü: docs/Part09B. Home-Motion.gif, Home-1600/1950/2000.png,
Play-Pressed.png, Gameplay.png, Home-Return.png, MASK_AUDIT.json,
HOME_MOTION_TESTS.json, PRESERVATION.json ve Unity logları içerir.

İlgili doğrulanan dosyalar private origin/master'a commit/push edilir;
uzak SHA doğrulaması teslim mesajında verilir. Fiziksel cihaz kabulü açık;
bu partta yeni build üretilmedi.
