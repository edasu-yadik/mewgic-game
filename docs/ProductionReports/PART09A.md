# PART 09A — Mewgic özel açılış ve gerçek yükleme

29 Ekim 2026 • C:/Dev/Mewgic • Unity 6000.3.27f1 • başlangıç master 4e3e32f.

## Sonuç ve kapsam

Boot artık Mewgic markalı açılış ekranını gösterir; Home gerçek asenkron
yükleme tamamlanınca açılır. Mevcut Home/Gameplay sahneleri, oyun kodu,
kayıt şeması, karakter prefabı ve onaylı sanat dosyaları değiştirilmez.
Yeni TestFlight/Cloud Build veya IPA üretilmez. DevControl'e müdahale edilmez.

Önceki Boot sahnesindeki PrototypeApp.Start doğrudan senkron Home geçişi
yapıyordu. Yalnız Boot içindeki bu component kaldırıldı, ayrı
BootLoadingController eklendi. PrototypeApp.cs dahil mevcut oyun kodu aynı.
Boot ilk etkin build sahnesi olmaya devam eder; sahne GUID'si ve liste sırası aynı.

## Unity sürümü, lisans ve splash

CLI self-update: en güncel kurulu CLI1.0.0-beta.14. `unity license list`
Unity Personal Assigned döndürdü. Editor sürümü6000.3.27f1 korundu.
Başlangıç PlayerSettings: Show Unity Splash Screen=true, Show Unity Logo=true.
Unity'nin güncel resmi lisans açıklamasında Unity6 Personal splash'i isteğe bağlıdır:
https://unity.com/products/pricing-updates
https://unity.com/releases/editor/whats-new/6000.0.0f1

Bu nedenle resmî PlayerSettings.SplashScreen API'siyle show=false ve
showUnityLogo=false uygulandı ve geri okundu. Lisans atlatma, motor patch'i veya
zorunlu ekranı çalışma zamanında gizleme yok. Lisans Unity6'da bunu desteklemeseydi
Unity ekranı korunup Mewgic ekranı ardından gösterilecekti. OS/engine ilk yüklenme
süresi bu özel ekranın ölçtüğü iş değildir; ilk Boot yüklenince sunum başlar.

## Görsel kimlik ve katmanlar

- Yeni arka plan: Assets/Art/Boot/Mewgic_Startup_Background_v001.png.
  00_Master_Cover referanslı imagegen built-in üretimi; mor-mavi gökyüzü,
  altın ışıklar, uçan kaleler, renkli büyülü şişeler. Rush Out markası/görseli yok.
- Logo: mevcut Assets/Art/UI/Logo/Mewgic_Logo_Main.png.
- Kedi/değnek: mevcut Assets/Art/Characters/MageCat/MageCat_FullBody_v001.png.
- Logo üst Safe Area'da, ayrı kedi orta bölümde, alt bölümde okunur mesaj,
  altın çerçeveli mor degradeli gerçek ilerleme çubuğu var. Katmanlar bitmap'e
  gömülü UI değildir; runtime ayrı gerçek Unity UI nesneleridir.
- Kedi 4 UI birimi genlikte hafif yüzer, çok küçük döner. Değnek yıldızı
  yumuşak parıldar.10 küçük yıldız/mote ve5 halkalı glow sabit mesh sayısıyla
  çizilir. ParticleSystem, dinamik ışık ve yeni shader/material yok.
- Arka plan mobil Android/iPhone ASTC6x6 olarak ayarlandı. Eski sprite importer,
  karakter animasyonu ve prefabı değiştirilmedi. Dokunma yalnız gerçek Retry
  düğmesinde açık; süs katmanları tıklamayı yakalamaz.

## Gerçek yükleme akışı

1. UI kurulur ve ilk frame'e izin verilir; GameplayProgress.Current ile mevcut
   kayıt açılır. LoadError varsa veri sıfırlanmaz, hata gösterilir.
2. Kaydedilmiş bölüm seçimi ResumeFromSave ile okunur. Kedi/logo/arka plan,
   Home'un build listesinde olması ve font doğrulanır. Gerekli bölüm TextAsset'i
   Resources.LoadAsync ile hazırlanır; mevcut katalog doğrulaması kullanılır.
3. SceneManager.LoadSceneAsync(Home) gerçek Unity AsyncOperation'ını başlatır.
   progress/0.9 normalize edilir; allowSceneActivation=false ile Boot korunur.
4. Hazırlık tamamlanınca Boot input'u kapatılır. Aktivasyon tamamlanmadan
   yüzde100 gösterilmez. Home aktif olup hazır olduğunda yüzde100 ve yalnız
   180ms görsel geçiş uygulanır; Boot UI/controller kaynakları kaldırılır.

İlerleme aşama ağırlıklıdır: kayıt15%, kaynak/bağımlılık15%, Home hazırlığı65%,
aktivasyon5%. Bu bir kalan süre veya disk byte yüzdesi değildir. Görsel dolgu
yalnız doğrulanmış hedefe yaklaşır; sahte süre sayacı veya minimum logo bekleme
süresi yok. Hata durumunda anlaşılır mesaj ve yeniden denenebilir gerçek işlem var.

## Gerçek testler ve kanıtlar

- C# derlemesi gerçek6000.3.27f1 Editor üzerinde çalıştırıldı.
- Soğuk/domain reset başlangıcı900x1600;900x1950 ve900x2000 Safe Area/notch
  benzetimleri; önceden yüklenmiş kaynaklarla hızlı başlangıç; gerçek eksik
  Home sahnesi hatası + Retry:5 senaryo,79 kontrol geçti, runtimeErrors=[].
- Home hazır değilken aktivasyon yok; çizilen ilerleme gerçek değeri aşmıyor;
  Home/Play gerçekten açılıyor; her aşamada tek EventSystem; Boot kapanınca
  kalıcı overlay yok; gerçek oyuncu PlayerPrefs ana kayıt değeri değişmedi.
- Altı mevcut model grubu (bulmaca, kayıt/yedek, harita ilerlemesi,
  güçlendirici, günlük görev, ödül merkezi) ve dört sahnenin referans kontrolü geçti.
- Açılış ve hata görüntüleri çalışan Unity UI/kamerasından900x1600,
  900x1950 ve900x2000 RenderTexture'a gerçek render ile kaydedildi.
  Bunlar çizim mockup'ı veya sonradan yeniden boyutlandırılmış fotoğraf değildir.
- Ekran görüntülerinde95% aşamasını yakalamak için yalnız Editor testinde
  aktivasyon kapısı tutulur. Bu bekleme build'e dahil değildir; test toplam süreleri
  görüntü yakalama beklemelerini de içerir ve iPhone performans ölçümü sayılmaz.

Kanıtlar: docs/Part09A/SETUP.json, scenario-0..4.json, MODELS.json,
Opening-0..4-GameView.png, Error-GameView.png, Home-After-Boot.png.
Görseller onaylı00_Master_Cover kimliğiyle gerçek render üzerinden karşılaştırıldı.
Kaynak üretim prompt'u ve yöntem PART09A_ART_PROMPT.md içindedir.

Editor loglarında lisans access-token yenileme uyarısı, Pipeline paketinin
kapanış ObjectDisposedException ve Editor kapanış JobTempAlloc uyarıları görüldü.
Play senaryolarında yakalanan beklenmeyen uygulama hatası0. Enjekte edilen
MEWGIC_BOOT_ERROR uyarısı testin beklenen hata senaryosudur. Console'un tüm
Editor/teardown logları tamamen temizmiş gibi sunulmaz. Fiziksel iPhone üzerinde
bu yeni ekran henüz çalıştırılmadı; yeni TestFlight build'i özellikle başlatılmadı.

## Koruma ve Git teslimi

Başlangıçtaki875 tracked dosya ve133 önceden mevcut untracked dosyanın SHA256
karşılaştırması geçti. Kod/sahne/asset değişikliği olarak mevcut dosyalardan yalnız
Boot.unity ve ProjectSettings.asset değişti; sonrasında PRODUCTION_PROGRESS.md
bu aşamanın teslim kaydıyla güncellendi. Tüm eski Home/Gameplay/LevelMap,
runtime C# kodu, kayıt sistemi, sprite/prefab/animasyonlar ve133 untracked dosya aynı.
Yeni dosyalar yalnız Boot sunumu, yeni arka plan, Editor test araçları ve raporlardır.

Mevcut gerçek oyun smoke testi yeni Boot üzerinden çalıştı; exit0. Boot→Home→
Gameplay→Home, geçersiz hamle, aktarım/animasyon kilidi, Undo,Restart,kazanma,
Replay geçti. MODEL ve Save regresyon sonuçları Part09A altında kopyalandı;
eski test screenshot dosyaları başlangıç hallerine alındı. Son özet:
VALIDATION_SUMMARY.json:79 başlangıç kontrolü,6 model grubu,4 sahne,0 C# derleme
hatası,0 beklenmeyen Play runtime hatası, gameplay smoke exit0.

Önceden mevcut untracked çalışmalar commit'e alınmadı. Git commit/push ve
uzak SHA sonucu son teslim kaydında belirtilecektir. Yeni TestFlight build'i yok.

### Git teslim doğrulaması
Uygulama/test/kanıt commit'i4cdaab01dd85cde5e1e54f1b25188912f54364bb private origin/master'a push edildi. git ls-remote refs/heads/master aynı SHA'yı döndürdü. Önceden var olan133 untracked dosya korunur ve commit kapsamında değildir. Yeni TestFlight veya Unity Cloud build'i başlatılmadı. Bu kayıt ayrı belge commit'inde gönderilir.
