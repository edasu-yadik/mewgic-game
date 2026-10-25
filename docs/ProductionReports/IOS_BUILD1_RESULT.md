# iOS Build #1 — Sonuç ve TestFlight hazırlığı

## Doğrulanmış sonuç
- Unity Cloud proje6b9ce85f-16e6-4b97-8f20-62dcd080e042,hedef mewgic-ios-testflight,build1: **Success**.
- Branch ios/testflight-6000.3.26,derlenen commit c6496d5d1906362b6b8410497fdb9837414b977e.
- Unity6000.3.26f1,macOS Tahoe,Xcode26.5.0,Apple Silicon/Mac Standard.
- Build süresi11dk36sn; bekleme2dk35sn; Cloud billable süre9dk01sn. Bu süre ücretli fatura oluştuğu anlamına gelmez; başlangıçta100 ücretsiz Mac dakikası vardı. Yeni/tekrar build başlatılmadı.
- Gerçek logda postbuildsteps ve postbuildstatus başarı satırları okundu; Overview Success ve artefakt170.58MB ile çapraz doğrulandı.

## IPA doğrulaması
Dosya: C:/Dev/Mewgic-iOS-Compat/Builds/iOS/Mewgic-0.8.0-build1.ipa
Boyut178864686byte. SHA256 a982747982bd9450f4938b2e271d902283f8bee546b81eb2a1334b85393e6b67.
Bundle ID com.edasuyadik.mewgic,marketing0.8.0,build1,minimumiOS15.0,iphoneos26.5SDK.
ZIP CRC başarılı. Gömülü App Store provisioning profilinin CMS imzası doğrulandı; app/team/sertifika/debugging=false/TestFlight/son tarih/distribution kontrolleri geçti. Ana Mach-O LC_CODE_SIGNATURE ve CodeResources mevcut.
Windows üzerinde macOS codesign ile tam uygulama imza zinciri/codepage doğrulaması yapılmadı. İmza yapısı varlığını tam kriptografik codesign kontrolü olarak sunmuyoruz. Apple yükleme doğrulaması henüz yok.
IPA ve gömülü profil Builds altında Git ignore kapsamındadır; özel imzalama malzemesi commit edilmez.

## App Store Connect ve TestFlight
Mewgic uygulama kaydı6826795393 doğrulandı. TestFlight ekranı hâlâ “Submit a build to start testing” gösteriyor.
Users and Access→Integrations ekranı “Permission is required to access the App Store Connect API” ve “Request Access” gösteriyor. Kullanılabilir mevcut API anahtarı yok. Oturum çerezleri çıkarılmadı; gizli web endpoint'leri kullanılmadı.
Apple'a IPA yüklenmedi. Apple işlemesi tamamlanmadı. TestFlight kullanıma hazır değil.
Kullanıcıya API erişimi ve Developer rollü ekip anahtarı için işlem anı onayı soruldu; yanıt bekleniyor. Ekip anahtarının tüm ekip uygulamalarına uygulanacağı açıklandı. Henüz erişim isteği/anahtar oluşturma yapılmadı.
Apple'ın yeni Build Uploads API'si Windows dahil farklı platformlardan yüklemeyi destekliyor; yetkilendirme sonrası resmî API ile ilerlenebilir. Yeni Mac build gerekmiyor.

## Koruma ve teslim
Ana C:/Dev/Mewgic/master değiştirilmedi. DevControl'e müdahale edilmedi. Bu görevde Unity/oyun kaynak değişikliği yok. Kanıtlar docs/iOSCompatibility/Cloud/Build1Success.jpg ve AppleAPIRequestAccess.jpg; ayrıntılar IPA_BUILD1_VERIFICATION.json.

Kaynak: https://developer.apple.com/videos/play/wwdc2025/324/ ve https://developer.apple.com/documentation/appstoreconnectapi/build-uploads
