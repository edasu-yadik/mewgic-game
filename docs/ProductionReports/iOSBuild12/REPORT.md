# MeWGic iOS Build 12 · Windows hazırlığı

8 Kasım 2026. Proje `C:\Dev\Mewgic`, Unity `6000.3.27f2`. Denetim başlangıcında yerel ve uzak `release/0.8.0-rc.1` SHA'sı `9456ed5` idi. Kristal Şelaleler ve son oynanış denetiminin uygulama commit'i `2d782fa` bu daldaydı.

## Sürüm ve ikon

- Kayıtlardaki son doğrulanmış TestFlight yayını 0.8.0 (2). 0.8.0 (11) için doğrulanmış çıktı imzalı bir bulut IPA'sı. Arşivde onun TestFlight'a yüklenmediği kayıtlı. Build 12 aday numarası olarak ayrıldı. App Store Connect'te canlı numara çakışması kullanıcı tarafından kontrol edilecek. [Önceki IPA doğrulaması](../Part21/IOS_IPA_VERIFICATION.json)
- Marketing Version `0.8.0`, kayıtlı iOS Bundle Identifier `com.edasuyadik.mewgic`, minimum iOS `15.0`. Üretim sahneleri sırasıyla Boot, Home, Gameplay ve LevelMap. SampleScene devre dışı.
- İlk doğrulanmış iOS ikonu, Build 2'de kullanılan `f434f8d` Git nesnesindeki `Mewgic_AppIcon_1024_v001.png`. Yerel dosyanın Git blob'u `09dc9f2bbdffc8044e1c857e31aa3e989a572f43` ile eşleşti. SHA-256 `bcb3f10d1dfbb6d58088b320da1f1780915aa6e154b7aab6e0f95879ae3e0410`. App Store kaynak kopyası da aynı SHA'ya sahip. [İkon referans zinciri](../IconRestore/REPORT.md)

## Unity denetimi

- Harita Play Mode 106, oynanış ve günlük görev Play Mode 114 kontrol. İki akış da oyuncu kayıtlarını ve kurtarma anahtarlarını byte düzeyinde korudu.
- 268 sahne, prefab ve varlık bağlantısı, 80 harita modeli, 16 günlük kayıt modeli ve 53 günlük regresyon kontrolü geçti.
- [Kristal kadrajı](Map/03-Crystal-100.png), [99 → 100 yürüyüşü](Map/07-Walk-99-100.png), [pati öğreticisi](Gameplay/02-Paw-Target.png), [şişe kutlaması](Gameplay/07-Real-Pour-Completion.png), [görev ikonu ve sayaç](Gameplay/06-Home-Quest-Icon.png). Kareler bu hazırlık turundaki Windows Unity Game View çıktılarıdır.
- [Harita testleri](Map/UNITY_TESTS.json), [oynanış testleri](Gameplay/UNITY_TESTS.json), [bağlantı denetimi](BINDINGS.json).

## Release export

- Windows'ta kurulu iOS Build Support ve Unity'nin varsayılan iOS platform profili kullanıldı. Release, IL2CPP, iPhone cihaz SDK'sı ve ARM64. Development Build, Script Debugging ve Auto Connect Profiler kapalı. Kayıtlı Apple ekibi Xcode projesine yazıldı. Xcode imzalama seçimi Mac'te yapılacak.
- İlk denemede Addressables derlemesi altı ada shader'ı için 12 Metal hassasiyet hatası verdi. `MapMetalPrecision.cginc` yalnız Metal yolunda maske hesaplarını tam hassasiyete çıkarıyor. Renkler, eşikler, animasyon formülleri ve diğer grafik API'lerinin hesapları aynı. Temiz export'ta bu hatalar tekrarlanmadı.
- Temiz export sonucu başarılı. Unity build raporunda **0 hata ve 1 uyarı** var. Unity Localization, Apple ana ekran adı için özel iOS App Info metadatasının tanımlı olmadığını bildiriyor. Uygulama mevcut `Mewgic` adını kullanır. Console son sayımı 0 hata ve 1 uyarı.
- Xcode projesi ve Info.plist, 0.8.0 (12), `com.edasuyadik.mewgic`, iOS 15.0, iPhone cihaz SDK'sı ve ARM64 hedefiyle doğrulandı. 19 Unity ikon yuvası doğru kaynağa bağlı. Xcode'un 1024 × 1024 RGB marketing ikonunun pikselleri tarihsel ikonla birebir eşleşiyor.
- 2.766 paketlenmiş varlık raporlandı. Kristal terası, su shader'ı, dinlenme/uyanma ve yürüyüş atlasları ile görev ikonu paket listesinde. Maskot, harita açılış kaydı, pati öğreticisi, şişe efekti, oynanış yerleşimi ve görev sayacı işlevleri üretilen IL2CPP kaynaklarında bulundu.
- 3.215 dosyalı çıktı yaklaşık 1,52 GB. Addressables kataloğu, IL2CPP verileri, ARM64 ve Intel Mac için IL2CPP araçları ve UnityFramework gizlilik bildirimi var. Xcode projesinde mutlak dosya referansı veya dosya bağlantısı bulunmadı.
- [Ön kontrol](PREFLIGHT.json), [export raporu](EXPORT.json), [Xcode denetimi](XCODE_VERIFICATION.json), [shader sonuçları](SHADERS.json), [Console](CONSOLE.json). Build çıktısı Git dışındadır.

## Mac'e aktarım

1. `C:\Dev\Mewgic\Builds\iOS\Build12` klasörünün tamamını aktar. Mac'te içindeki `Unity-iPhone.xcodeproj` dosyasını Xcode 26 veya üzeri ve iOS 26 SDK veya üzeriyle aç. [Apple SDK koşulları](https://developer.apple.com/news/upcoming-requirements/)
2. App Store Connect'te 0.8.0 için 12'nin kullanılmadığını doğrula. `Unity-iPhone` hedefinde ekip, Apple Distribution sertifikası ve mevcut uygulamanın App Store dağıtım profilini seç. `com.edasuyadik.mewgic` ile sürüm 0.8.0 (12) değerlerini kontrol et.
3. `Unity-iPhone` şemasını ve **Any iOS Device (arm64)** hedefini seç. Archive yapılandırması **Release** iken **Product → Archive** çalıştır. Organizer'da arşivi doğrula, ardından TestFlight dağıtımını veya IPA export'unu tamamla. [Apple build yükleme adımları](https://developer.apple.com/help/app-store-connect/manage-builds/upload-builds)

Xcode arşivi, IPA, Apple hesabındaki işlemler ve fiziksel cihaz doğrulaması Mac tesliminden sonraki adımlardır.
