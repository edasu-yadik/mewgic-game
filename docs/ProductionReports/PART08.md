# PART08 — Entegrasyon, performans ve Android beta adayı

25 Ekim2026 • C:/Dev/Mewgic • Unity6000.3.27f1 • URP2D / Portrait.

## Teslim durumu

**Cihaz kabulü bekliyor. Kararlı Android beta teslimi henüz onaylanmadı.**
Son APK, test ve Git sonuçları bu raporun son teslim kaydında bulunur.
AGENTS.md, önceki part raporları ve beş Approved referans incelendi.
PART08_READ_AUDIT.json yolları ve referans hash'lerini içerir. PART04D raporu yoktur;
hiç uygulanmamış bölüm tamamlanmış sayılmadı. Başlangıç geri dönüş SHA'sı ve dosya
hash'leri C:/Dev/Mewgic-Part08-Backup altında korunur.

## Gerçek sahne ve ekranlar

| İçerik | Durum |
|---|---|
| Boot | Gerçek sahne; Home başlangıcı |
| Home | Gerçek sahne; onaylı sanat, karakter/şişe animasyonu ve UI |
| Gameplay | Gerçek sahne; iki doğrulanmış bölüm ve su sıralama |
| LevelMap | Gerçek sahne; dört çevre bölgesi, yalnız iki oynanabilir bölüm |
| Missions / Daily Gift | Home içinde gerçek çalışma zamanı panelleri, ayrı sahne değil |
| Events / Collection | Home içinde gerçek ödül merkezi, ayrı sahne değil |
| Settings | Home içinde minimal gerçek ses tercihi ve kapanabilen panel, ayrı sahne değil |
| SampleScene / WizardCatAnimationTest | Mevcut şablon/test sahneleri; üretim build'ine dahil değil |

Altı gerçek sahnede eksik script, kayıp prefab ve bozuk serialized referans sayısı0.
Build listesi yalnız Boot/Home/Gameplay/LevelMap. PART08_SCENE_AUDIT.json kanıttır.

## Tamamlanan onarımlar

- Seçili doğrulanmış bölüm kalıcı kayda bağlandı. Yeni oturum/Home açılışında okunur;
  kilitli veya üretilmemiş kimlik güvenle Bölüm1'e düşer. Harita seçimi ve Next kaydeder.
- Mevcut Mewgic.Progress.v1 korunur; bağımsız version1 PlayerPresentationData eklendi.
  Bölüm, para/elmas, enerji ve ses tercihi sürümlü çocuk kayıttır. Eski yıldız/hak,
  günlük, sandık, sezon ve koleksiyon alanları korunur. Yeni profil para/elmas0/0'dır;
  önceki12450/853 tasarım önizlemesi gerçek bakiye olarak taşınmadı.
- HomeResourceModel gerçek kayda bağlandı; SetValues kaydetme başarılıysa Changed
  ile UI günceller. Satın alma/para kazanma ekonomisi eklenmedi. Enerji tüketilmez.
- Doğrulanmış son commit `.backup` kopyasına yazılır. Bozuk ana belge yedekten açılır;
  özgün metin `.corrupt` olarak korunur. İki belge de bozuksa sıfırlamak yerine hak ve
  ödülleri kilitleyen korumalı profil kullanılır. Yeni/uyumsuz sürüm eski yedeğe sessizce
  düşürülmez. Bu yerel korumadır; sunucu/anti-cheat veya elektrik kesintisine mutlak
  işlem garantisi değildir. Kayıt aynı ödülün alındı işaretiyle bakiyesini birlikte tutar.
- İlk kazanma ödülünde int taşması artık exception ile akışı kesmez; işlem reddedilir.
- SceneNavigation.LoadOnce aynı karede birden fazla sahne yükleme isteğini engeller.
  Mevcut PLAY gecikme/tekrar giriş ve ödül idempotency korumaları korunur.
- Uygulama askıya alındığında PlayerPrefs flush edilir ve Gameplay Pause olur;
  geri dönünce kullanıcı Resume ile devam eder. Android yaşam döngüsü gerçek telefonda
  henüz doğrulanmadı; Editor'de callback ve durdurma davranışı sınandı.
- Ayarlar ses tercihini kaydeder ve AudioListener.volume'a uygular. Bu sürümde
  müzik/ses içerikleri yoktur; panel bunu açıkça söyler. Ses düğmesi küçük/güvenli
  ekranda da rahat hedef boyutuna göre düzenlendi.
- Haftalık etkinlik tarih/ödül metni ile ada görseli ayrıldı; tarih artık örtüşmüyor.
- Kapalı Daily/Reward panellerinin gereksiz her-kare Layout döngüsü durduruldu.
- Kullanılan dört WizardCat dokusuna Android ASTC4x4/max2048 override uygulandı.
  PNG pikselleri/çözünürlükleri, prefab, animasyonlar ve onaylı görseller değiştirilmedi.

## Gerçek testler ve kanıtlar

PART08_SAVE_TESTS:23 kontrol. Gerçek benzersiz PlayerPrefs anahtarına yazma/yeniden
okuma; bölüm, bakiye, enerji, ses, yıldız, haklar, günlük/haftalık sayaçlar, alınmış
hediye, tekrar ödül reddi; bozuk ana kayıt/yedek kurtarma, iki bozuk belgeyi koruma,
silinmiş ana kayıt/eksik temel alan kurtarma, gelecek sürümü reddetme, eski kayıt uyumu, yazma hatası ve taşma sınandı. Test anahtarı
ve yedekleri temizlendi; gerçek oyuncu kaydı değiştirilmedi.

Güncel regresyon sonuçları PART08_REGRESSIONS.json ve Part08Tests/*.json içindedir.
Aynı testin tekrarları toplam bağımsız kontrol sayısı gibi toplanmaz. Binlerce
kontrolün önemli bölümü slot sınırı/örtüşme invariants; binlerce oyun oturumu değildir.
Güncel testler gerçek EventSystem raycast/pointer, model/ödül ve navigasyon akışlarını
birlikte sınar. Tarihsel04A–04F testlerinin eski önizleme/boş Map varsayımları güncel
beta koşusuna dahil edilmedi; bunların yeniden geçtiği iddia edilmez.

İlk beta yürütücüsü Home hazır olmasını ve sceneLoaded'ın sonraki karede gelmesini
beklemiyordu; test zamanlaması düzeltildi ve başarısız kayıtlar korundu. Sonraki
tekrarda Windows1224 hatası eski PNG'nin üzerine yazmayı engelledi. Daily/Reward/
GameplayVisual/WizardCat testleri benzersiz çıktı klasörü kullanacak şekilde
onarıldı; eski görüntüler silinmedi. Hatalar PART08_*FAILURE.json içinde saklanır.

900x1600,900x1950,900x2000,Safe Area inset ve360x640 kontrolleri güncel günlük,
ödül merkezi, harita ve Gameplay testlerinde kapsandı. Yeni ayar kontrolü ayrıca
oran/güvenli alan testine alındı. Bunlar fiziksel dp/parmak ergonomisi veya Android
cihaz testinin yerine geçmez. Süreci tam kapatıp telefonda yeniden açma kabulü bekler;
PlayerPrefs'i yeniden açma testi bununla karıştırılmadı.

## Görsel kalite

Yeni sanat üretilmedi; Blender kullanılmadı. Gerçek900x1600 Game View ScreenCapture
çıktıları PART08_HOME/GAMEPLAY/MAP/MISSIONS/EVENTS/SETTINGS_GAMEVIEW.png'dir.
İzole test profili gösterilir; gerçek oyuncuya test ödülü verilmedi. Diğer oran
çıktıları kamera RenderTexture yakalamalarıdır. PART08_VISUAL_COMPARISON.html beş
onaylı referansı güncel gerçek ekranlarla yan yana gösterir.

- Master: logo/kedi kimliği korunur; oturan kedi pozu ve logo süs geometrisi farklı.
- Home: ana üçlü odak, sol kedi, alt PLAY ve sıcak/soğuk dünya ilişkisi yakın;
  mimari/ölçek, ek logo, ayrı alt kartlar ve dört yan giriş farklı. Shop/Starter Pack yok.
- Gameplay: cam/dinamik sıvı/raf/şelale dili var. Sıvı ışık/kırılma sanatı referanstan
  sade; dört şişeli gerçek Bölüm1 kullanılır. HUD ortak mor panel ailesidir; referans
  Level28/Moves12/Combo değerleri gerçek oyuncu verisi gibi kopyalanmadı.
- Map: dört renkli bölge korunur; düzen daha seyrek, yollar daha basit. Üst ekonomi,
  yan etkinlikler ve harita kedisi yoktur. Üç gelecek bölgede oynanabilir veri yok.
- Events: yerel Crystal Falls, üç ücretsiz basamak ve iki mevcut galeri kartı vardır;
  Rainbow Reef/premium ödeme/ileri koleksiyon vaat edilmez. Günlük panelde parşömen,
  yeşil çubuk, sandık/kedi/mavi buton dili korunur. Birebir final sanat eşleşmesi yok.

## Performans

PART08_EDITOR_PERFORMANCE: Home388→388, Map239→239, Events388→388,
Gameplay183→183 nesne; kısa sabit aralıkta havuz büyümesi görülmedi. Aktif gerçek
ParticleSystem sayısı0; kıvılcımlar sınırlı UI havuzlarıdır. ProfilerRecorder geçerli,
GC medyanı yaklaşık9.8–10KB örnekleri Editor/test/MCP maliyetini de içerir. Örnekler
Editor.update üzerinden toplandı; Android frame sayısı/FPS veya sıfır GC iddiası yok.
Editor native ayrılan bellek örnekleri JSON içinde bulunur; cihaz RAM'i değildir.

76 PNG incelendi: hepsi Read/Write kapalı, mipmap kapalı ve2048 altında.
Dört kullanılan WizardCat dokusunun ASTC blok tahmini toplam6,312,384byte≈6.02MiB;
aynı boyut RGBA32 yaklaşık24MiB olurdu. Bu GPU blok hesabıdır, gerçek ölçüm değildir.
Kullanılmayan kaynaklar dahil tüm doku tahminini aktif sahne belleği olarak sunmuyoruz.
PART08_TEXTURE_AUDIT ve gerçek build paket içeriği ayrı kayıtlardır. Saydam çevre
katmanları overdraw, dökme sıvı mesh'leri ve eşzamanlı UI animasyonları cihazda
CPU/GPU/GC/termal profil gerektirir. Gerçek cihaz yükleme süreleri ölçülmedi.

## Eksikler ve sonraki kabul

Yalnız iki bölüm, sınırlı yerel ödüller; ekonomi/enerji yenileme/Shop/Starter Pack/
Team/Inventory/gerçek premium/çevrimiçi hesap/ses içerikleri tamamlanmadı.
Yarım bulmacanın sıvı dizilimi kaydedilmez; seçili bölüm baştan açılır, kalıcı haklar
ve ödüller korunur. Yerel kayıt/saat sunucu güvenliği sağlamaz.
Gerçek telefon kurulum, yeniden açılış, dokunma, Safe Area, crash/ANR, bellek/FPS,
ısı ve uzun oturum kabulü gereklidir. Bu açıklar kapanmadan kararlı beta onayı yoktur.
Kurulum ve Google Play/iOS öncesi kalanlar ANDROID_BETA_GUIDE.md içindedir.
DevControl'a, başka projelere veya önceki sanat kaynaklarına müdahale edilmedi.

## Android teslim doğrulaması

Unity BuildPipeline son kaynaklarla Succeeded;0 hata/0 uyarı,73.1 saniyelik
incremental build. İlk tam build279.1s sürdü; eski APK'lar korunur. BuildReport'un
totalSize alanı debug sembolleri ve diğer çıktıları da sayar; APK boyutu değildir.
Incremental APK'daki yaklaşık19MB boşluk yalnız üretilmiş Gradle launcher hedefinin
temiz paketlenmesiyle giderildi.160 ZIP girdisinin açılmış içerik SHA'ları aynı kaldı.
C#/oyun verisi yeniden düzenlenmedi. Son paket yeniden imza/CRC/hizalama denetiminden geçti.

- APK: C:/Dev/Mewgic/Builds/Android/Mewgic-0.8.0-beta.apk
- Gerçek dosya:68,616,458byte (~65.44MiB); SHA256:
  e28b3b08239532fa4c9d2ea20cf6a8f97942c06ed752b6abc5077481f1e3a760
- Uygulama Mewgic;com.mewgic.game;0.8.0/code8;IL2CPP/ARM64;minSDK26,targetSDK36.
- Development/debug imza doğrulandı; önceki prototiple aynı debug sertifikası.
  CRC,ZIP16KB ve bütün native ELF LOAD segment hizalamaları geçti. Bu statik
  kontroller gerçek16KB cihaz çalıştırması veya mağaza onayı değildir.
- Kaynak sürümünde testin bıraktığı runInBackground=true eski false değerine alındı.
  Android build'in disk üstünde bıraktığı geçici InputSystem preloadedAssets satırı,
  canlı Editor API'sinde zaten0 olduğu doğrulanarak başlangıçtaki[] biçimine döndürüldü.
- ADB yetkili/bağlı cihaz listesi boş. APK telefona kurulmadı/başlatılmadı; gerçek
  uygulama kapat/aç,device logcat/crash/ANR,performans ve dokunma kabulü yapılmadı.

Kanıtlar:PART08_ANDROID_BUILD.json,ANDROID_PACKAGING.json,PACKED_ASSETS.json,
APK_VERIFICATION.json,APK_BADGING.txt,APK_SIGNATURE.txt ve APK_ZIPALIGN.txt.
APK ve debug sembolleri Git'e eklenmez. Kurulum adımları ANDROID_BETA_GUIDE.md'dedir.

## Son test özeti

23 kayıt+89 beta entegrasyon+603 ödül+219 günlük+200 ilerleme+723 güçlendirici+
336HUD+3183 şişe+551harita+2755yerleşim+41karakter+12model=8735 assertion.
Her güncel paketin son koşusu geçti; smoke exitCode0. Son kayıt düzeltmesi sonrası
ödül/günlük/güçlendirici model sınamaları da yeniden çağrıldı; bunlar sayıya ikinci
kez eklenmedi. PART08_VALIDATION_SUMMARY.json sayıları ve sınırları kaydeder.
Son Console0 hata/0 uyarı; profil testi kapalı,derleme bitmiş durumda.
**PART08 kısmen tamamlandı: doğrulanmış APK adayı hazır; fiziksel cihaz kabulü açık.**
