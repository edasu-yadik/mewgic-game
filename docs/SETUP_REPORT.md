# Mewgic — Temel Kurulum Raporu

Tarih: 23 Ekim 2026 (Europe/Kiev).

## Kurulum

- Tek proje: `C:\Dev\Mewgic`. Unity Hub proje listesine eklendi.
- Önceden Unity Hub'a kayıtlı proje yoktu. Kullanıcı profilinde ProjectVersion.txt araması mevcut proje bulmadı. Masaüstündeki ve çalışma alanındaki Mewgic klasörleri boştu; korunmuştur.
- Unity Hub: 44.9.3.
- Önceden kurulu Editor: 6000.6.7f1. Bu sürüm LTS olmadığı ve Android modülü bulunmadığı için projede kullanılmadı; silinmedi.
- Proje için Unity **6000.3.27f1 / Unity 6.3 LTS** ve Android araçları kuruldu.
- Kaynak: bu Editor ile gelen **Universal 2D 6.1.6** resmi şablonu. Assets, Packages ve ProjectSettings kullanıldı; başka projenin önbelleği taşınmadı.
- Şablondaki Input System 1.12.0 ve Version Control 2.6.0 paketleri ilk derlemede Editor API uyumsuzluğu verdi. Yalnızca bu iki paket Editor'ün kendi paket listesindeki uyumlu sürümlere (1.20.1 ve 2.13.6) güncellendi. Başlangıç manifesti `template-manifest.original.json` içinde korunur. Version Control paketi şablondan gelir; herhangi bir servise bağlanılmadı.

## Android araçları

Editor içindeki `Editor/Data/PlaybackEngines/AndroidPlayer` dizininde:

- Android Build Support.
- OpenJDK: Temurin 17.0.18+8 (çalıştırılarak doğrulandı).
- NDK: r27c / 27.2.12479018.
- SDK Build Tools 36.0.0, Platform Tools 37.0.1, Command Line Tools 16.0.
- SDK platformları: API 34, 35, 36 ve 37 (Hub'ın bu sürüm için seçtiği bağımlılıklar).
- CMake 3.22.1.

## Yapılandırma

- Ürün adı Mewgic, Portrait; yatay otomatik döndürme kapalı.
- Android etkin hedef, IL2CPP, ARM64, minimum API 26, hedef API otomatik.
- Geçici uygulama kimliği `com.mewgic.game`; mağaza yayını öncesi kesinleştirilmeli.
- URP ve 2D Renderer kullanılır; varsayılan grafik ayarı aynı URP varlığına atanmıştır.
- İstenen sekiz Assets klasörü oluşturuldu.
- Boot, Home, Gameplay ve LevelMap sahneleri ortografik kamera ve Global Light 2D içerir.
- Derlemede dört sahne etkin; Boot ilk sırada. Şablonun SampleScene sahnesi silinmedi, derleme dışında bırakıldı.
- README.md ve docs/PROJECT_PLAN.md hazır.
- Yerel .gitignore yalnızca gelecekte kullanılmak üzere hazırlandı; Git deposu oluşturulmadı ve GitHub/DevControl entegrasyonu yapılmadı.

## Tamamlanan doğrulamalar

- Paket düzeltmesinden sonra C# derlemesi başarılı.
- Unity, Android hedefinde açıldı ve temel doğrulama kodunu hatasız tamamladı (çıkış kodu 0).
- Dört sahne açılarak ortografik kamera ve eksik script denetimi geçti.
- Portrait, etkin Android hedefi, Android desteği, URP ve Boot sırası denetlendi.
- Proje bütünlüğü: eksik/öksüz meta, yinelenen GUID, birleştirme işareti ve manifest kontrolleri; 0 hata, 0 uyarı.
- Başarılı kurulum günlüğü: unity-setup-verified.log. Önceki günlükler sorun giderme kaydı olarak korunur.

## Sınırlar

Henüz oyun mekanikleri, arayüz, sahne geçişleri veya bölüm verileri uygulanmadı. Bağlı Android cihaz bulunmadığından cihaz kurulumu ve dokunma/performans testleri yapılmadı. iOS kurulumu bu aşamaya dahil edilmedi.

## Android APK testi: başarılı

- `Builds/Android/Mewgic-Setup-Test.apk` oluşturuldu (41.296.630 bayt, yaklaşık 39,4 MiB).
- Unity doğrulama işareti: `MEWGIC_ANDROID_BUILD_OK`; Editor çıkış kodu 0.
- APK metaverisi: `com.mewgic.game`, arm64-v8a, minimum SDK 26, hedef SDK 36.
- APK imza doğrulaması başarılı (geliştirme/test imzası; yayın imzası değildir).
- Test günlüğü: `unity-android-build.log`.
- Unity/URP kaynaklı gölgelendirici tür dönüşüm uyarıları görüldü; derlemeyi engellemedi. Lisans günlüğünde erişim belirteci yenileme uyarısı da var; Unity Personal yetkisi çözümlendi ve derleme başarıyla tamamlandı.

## Normal Editor penceresi ve kalan manuel adım

Proje normal Editor ile başlatıldı. İlk açılışta **Unity Editor Software Terms** penceresi çıktı; kullanıcı koşulları inceleyip onaylamalı. Bu onay verilmeden normal proje arayüzünün tamamen açıldığını iddia etmiyoruz. Otomatik Editor açılışı, C# derlemesi, sahne yüklemeleri ve Android APK derlemesi başarıyla doğrulanmıştır. Pencere açık bırakıldı.

Onaydan sonra gerekirse Unity Hub → Projects → Mewgic üzerinden açın; Assets → Scenes → Boot sahnesini seçin. Boş mor arka plan bu aşamada beklenen görüntüdür. Telefon testi daha sonra USB hata ayıklaması açık bir Android cihaz bağlandığında yapılabilir; mevcut kurulum için zorunlu değildir.

GitHub Desktop için temel dosya düzeni ve .gitignore hazırdır. Git deposu/uzak depo oluşturulmadı, herhangi bir bağlantı yapılmadı.
