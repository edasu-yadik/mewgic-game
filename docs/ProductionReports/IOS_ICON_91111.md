# iOS 91111 ikon düzeltmesi — 27 Ekim 2026

## Kapsam ve neden

Çalışma kopyası C:/Dev/Mewgic-iOS-Compat, branch ios/testflight-6000.3.26,
başlangıç commit 3cbaac4. Ana C:/Dev/Mewgic/master korunur.
Build 1'in kaynak PlayerSettings dosyasında iOS ikon yuvası yoktu.
Apple Transporter 91111 hatası, Any Appearance için 1024x1024 ikonun eksik
olduğunu bildirdi. Küçük cihaz ikonlarının IPA'da bulunması bu gereksinimi karşılamaz.

## Düzeltme

- Onaylı 00_Master_Cover ve MageCat_FullBody_v001 referanslarıyla büyücü kedi ikonu üretildi.
- Assets/Art/AppIcon/Mewgic_AppIcon_1024_v001.png: 1024x1024, 8-bit RGB,
  sRGB profilli, alpha/tRNS yok, tam kare. SHA256:
  bcb3f10d1dfbb6d58088b320da1f1780915aa6e154b7aab6e0f95879ae3e0410.
- Gerçek Unity Texture2D asset'i ve meta GUID'si kaydedildi. iOS Application,
  Spotlight, Settings, Notification ve Marketing dahil 19 yuva API ile atandı.
- Unity 6000.3.26f1 korunur. Sürüm 0.8.0 (2), com.edasuyadik.mewgic.
- PreExport ikon atamasını tekrar doğrular; postprocess gerçek Xcode AppIcon
  kataloğunu ve Any Appearance 1024 RGB dosyasını kontrol eder. Eksikse build hata verir.
- Bulut post-build script'i gerçek IPA Info.plist, CRC, cihaz ikonları ve Assets.car
  içindeki derlenmiş 1024 AppIcon kaydını macOS assetutil ile doğrulayacaktır.
  Kontrol geçmeden IPA başarılı teslim sayılmaz.

## Gerçek yerel doğrulama

- C# derlemesi başarılı; Preflight ve Xcode export işlemleri exit 0.
- Altı mevcut model test grubu ve Boot/Home/Gameplay/LevelMap referansları geçti.
- Eksik filename ve yalnız Dark Appearance kataloğu reddedildi.
- Gerçek iOS Xcode export: Builds/iOS/IconBuild2-Xcode.
- AppIcon.appiconset/Contents.json: 19 dolu dosya. Icon-Store-1024.png,
  ios-marketing, 1024x1024, 1x, appearances alanı yok (Any Appearance).
  Üretilen dosya RGB PNG, alpha kanalı yok. C# postprocess kontrolü geçti.
- Windows'taki kurulu Editor değiştirilmedi. Aynı 6000.3.26f1 Editor'ün
  C:/Dev/Unity-iOS/6000.3.26f1 kopyasına resmi imzalı iOS Build Support eklendi.
  Bu yerel export, Mac/Xcode imzalı IPA üretimi değildir.

Kanıtlar docs/iOSIcon/PLAYER_ICON_CHECK.json, PNG_CHECK.json,
MODELS_AND_SCENES.json, IDENTITY_PREFLIGHT.json, PREFLIGHT.json,
XCODE_CATALOG_CHECK.json ve AppIcon-Contents.json içindedir.

## Play Mode ve koruma

Play smoke exit 0: Boot → Home → Gameplay → Home; gerçek butonlar, geçersiz
hamle, aktarım/animasyon kilidi, Undo, Restart, kazanma ve Replay geçti.
900x1600 Home/Gameplay/Win ve 900x1950 Gameplay render'ları kaydedildi.
Mevcut ana menü/karakter görseli korundu; ikon oyun sahnesinde kullanılmaz.
Önceki uyumluluk raporunda da kayıtlı UnityEditor.Search.SearchDatabase
ArgumentOutOfRangeException tekrar görüldü. C# derleme/oyun testi hatası değil;
Console tamamen temiz olarak raporlanmaz. Lisans yenileme access-token uyarısı
da logda var; geçerli yerel lisansla test/export exit 0 tamamlandı.
Bu görevin kapsamı dışındaki Editor indeksleme ayarları değiştirilmedi.
Yerel export'un otomatik yazdığı URP/atlas/test dosyaları geri alındı.
Ana master SHA 4e3e32feaeab4521ceb2f7c06d8e99aced632a69 değişmedi,
ana checkout'ta tracked diff yok. Sahne/runtime/prefab/animasyon dosyaları aynı.

## Bulut teslimi

Mevcut mewgic-ios-testflight hedefinde MEWGIC_IOS_BUILD_NUMBER=2 ve
BuildConfig/verify-ios-icon.sh post-build kontrolü kaydedildi. Script hata verirse
build'in başarısız sayılması açık. İmzalama, branch, Editor/Xcode ve Mac Standard
ayarları korundu.

### Nihai sonuç

- Kaynak commit f434f8dea86a0e8a5665b183b984fb563032212b private
  origin/ios/testflight-6000.3.26 üzerine push edildi; uzak SHA eşleşti.
- Cloud Build #2 **Success**. Gerçek kaynak checkout SHA aynı; Unity6000.3.26f1,
  Xcode26.5.0/macOS Tahoe/Apple Silicon/Mac Standard korundu.
- Build süresi8dk13sn, bekleme1dk23sn, billable6dk50sn; ücretli yükseltme yok.
- Yeni sertifika/profil oluşturulmadı. Mevcut Mewgic TestFlight App Store profili
  ve Apple Distribution EDASU YADIK (VKW7GYV7B9) kullanıldı.
- Mac post-build script gerçek imzalı IPA'yı açtı. ZIP CRC, Info.plist kimliği,
  cihaz ikonları ve assetutil --info ile Assets.car kontrolü **geçti**.
- Derlenmiş ikon kaydı: AssetType=Icon Image, Name=AppIcon,
  RenditionName=Icon-Store-1024.png, Idiom=marketing, Scale=1,
  PixelWidth=1024, PixelHeight=1024, ColorModel=RGB, Colorspace=srgb,
  BitsPerComponent=8, Opaque=true.
- IPA0.8.0/build2/com.edasuyadik.mewgic. Boyut182624948byte.
  SHA256:5f7d25f48acbdd05ae3107f5b8ddcea18616c4a00dc6eb6e2fcb8b3f96f22496.
- İndirilen gerçek dosya hash'i Mac'teki denetlenen IPA ile birebir eşleşti.
  Yerel CRC, cihaz ikon PNG'leri, Info.plist, App Store profilindeki app/team
  kimliği/get-task-allow=false ve imza dosyalarının varlığı da doğrulandı.
- Dosyalar: C:/Users/mehme/Downloads/Mewgic-0.8.0-build2.ipa ve
  C:/Dev/Mewgic-iOS-Compat/Builds/iOS/Mewgic-0.8.0-build2.ipa.
  Build1 ve orijinal indirme korunur. IPA Git'e eklenmedi, Drive'a yüklenmedi.
- Kaynak AppIcon Any Appearance ve derlenmiş IPA ikonu doğrulandığı için
  **MacBook Transporter ile yeniden yüklemeye hazır**.
  Apple'a yeniden yükleme/Apple işlem kabulü ve fiziksel iPhone testi yapılmadı.
  Windows üzerinde macOS codesign ile ek imza denetimi yapılmadı.

Kanıtlar: CLOUD_IPA_ICON_VERIFICATION.json, DOWNLOADED_IPA_CHECK.json,
EXPORTED_PNG_AUDIT.json, LOCAL_LOG_SUMMARY.json ve Cloud-Build2-Success.jpg.
Önceden bilinen yerel Editor Search indeksleme exception'ı dışında oyun
regresyonu bulunmadı. Master, menü, animasyon, mekanik ve DevControl korunur.

Cloud sonucu: https://cloud.unity.com/organizations/5774143930948/projects/6b9ce85f-16e6-4b97-8f20-62dcd080e042/cloud-build/buildtargets/mewgic-ios-testflight/builds/2

## Resmi kaynaklar

- https://developer.apple.com/documentation/xcode/configuring-your-app-icon
- https://developer.apple.com/design/human-interface-guidelines/app-icons
- https://docs.unity.com/en-us/build-automation/reference/available-environment-variables
- https://docs.unity.com/en-us/build-automation/advanced-build-configuration/run-custom-scripts-during-the-build-process
