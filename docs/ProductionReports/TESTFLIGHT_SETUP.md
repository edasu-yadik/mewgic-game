# PART08 ek çalışma — TestFlight hazırlığı

Durum: KISMEN TAMAMLANDI. IPA üretilmedi, TestFlight yüklemesi ve iPhone testi yapılmadı.

## Doğrulananlar
- Unity Cloud Mewgic proje kaydı: 6b9ce85f-16e6-4b97-8f20-62dcd080e042.
- Genel kitle, özellikle çocuklara yönelik değil (kullanıcı beyanı).
- Apple Team VKW7GYV7B9, explicit App ID com.edasuyadik.mewgic.
- App Store Connect Mewgic kaydı 6826795393 oluşturuldu. TestFlight ekranı henüz build olmadığını gösteriyor.
- Apple Distribution sertifikası 5S972M9XK9 oluşturuldu. Şifreli özel anahtar ve p12 eşleşmesi doğrulandı.
- Mewgic TestFlight App Store profili oluşturuldu. Paket/ekip/sertifika/son tarih ve TestFlight yetkisi dahil 8 kontrol geçti. CMS imzası doğrulandı; yerel CA güven zinciri doğrulanmadı.
- Kullanıcının işlem anında onayıyla GitHub yalnız mewgic-game için Contents ve Metadata read-only, 24 Kasım 2026 son tarihli anahtar oluşturuldu. Unity Cloud yetkilendirildi, depo seçildi ve kaynak bağlantısı kaydedildi.
- Anahtar rapora/kaynak dosyalara yazılmadı. Apple özel imzalama malzemeleri kullanıcı profilindeki .codex/secrets/mewgic-testflight klasöründe, repo dışında.

## Dosyalar ve testler
- Assets/Editor/MewgicIosCloudBuild.cs: yalnız açıkça çağrılan iOS PreExport hook. Yerel Android hedefinde çalışmayı değişiklikten önce reddeder. Sürüm ve kayıtlı kimliği doğrular, bulut iOS ayarlarını uygular.
- BuildConfig/iOS-TestFlight.json: doğrulanmış açık uygulama kimlikleri, sürüm0.8.0 ve minimum iOS15.0.
- BuildConfig/Gymfile ve ucb_xcode_fastlane.json: App Store Connect dışa aktarma taslağı. Mac/Fastlane üzerinde henüz çalıştırılmadı; başarı iddiası yok.
- TESTFLIGHT_PREFLIGHT_TESTS.json: gerçek Editor üzerinde27 kontrol geçti; Android kimliği korundu.
- TESTFLIGHT_PRESERVATION.json:290 mevcut oyun/sahne/art/ProjectSettings dosyası hash kontrolü geçti.
- Son gerçek Unity MCP:0 Console hata,0 uyarı; aktif build yok; test kayıt profili kapalı.
- Önceki PART08'deki8735 kontrol bu ek çalışmada tekrar çalıştırılmadı.
- Yeni sanat veya oyun özelliği yok; Game View görsel değişikliği yok. Apple/Cloud kanıtları docs/Screenshots/TestFlight altında.

## Açık engeller ve kalan işler
- Unity Cloud manuel sürüm listesinde6000.3.27f1 bulunamadı; en yeni listelenen6.3 sürümü6000.3.26f1. Yerel proje düşürülmedi. Hedef taslağında Auto-detect seçildi, closest-version kapatıldı. Kesin sürüm bulutta kullanılabilir olmadan başarılı derleme varsayılmamalı.
- Kaydedilmiş iOS hedefi mewgic-ios-testflight: master, macOS Tahoe, Xcode26.5.0, Apple Silicon, Mac Standard, otomatik/scheduled build kapalı. PreExport hook ve custom Fastlane yolu girildi.
- Kullanıcının ayrı işlem anı onayıyla App Store profili, şifreli p12 ve parolası Unity Cloud Mewgic hedefine kaydedildi. Yeniden açılan Edit ekranında appstore tipi, paket kimliği ve Apple Distribution sertifikası doğrulandı.
- Usage allowance ekranında Mac0/100 dakika, Storage0/25GB ve Egress0/100GB doğrulandı. Ücretli yükseltme yapılmadı. aecea199fa5ac967131c5f29340f4c320e88eff3 commit için ilk derleme isteği gönderildi, servis Unity sürümü nedeniyle isteği reddetti; çalışan build oluşmadı.
- App Store Connect yükleme yetkilendirmesi/API anahtarı, imzalı IPA derleme, yükleme, Apple işlem sonucu ve dahili test dağıtımı henüz tamamlanmadı.
- Fiziksel Android testi kullanıcının talebiyle ertelendi. TestFlight cihaz testi de henüz yok.

## Güvenlik ve devam
GitHub read-only erişim yalnız kaynak klonlaması içindir. Apple p12/parola/profil ve gelecekteki p8/API sırları asla Git'e alınmaz. Kullanıcı giriş ve OTP işlemlerini kendi yapar. Diğer Unity projeleri, DevControl ve mağaza yayını değiştirilmedi. Rapor bir TestFlight teslimi değildir.


## Son doğrulama — 25 Ekim 2026
Unity Cloud hata metni: "Detected project using Unity Version 6000_3_27f1, but this version is not available currently. Please update your project to use a supported version of Unity, or manually select a version for Build Automation to build with."
Bu, yalnız listeden yapılan tahmin değildir; gerçek build submission reddidir.
Proje sürümü düşürülmedi, closest-version kapalı kaldı. Build başlamadı; IPA/TestFlight yüklemesi yok.
İmzalama kaydı adı: Mewgic TestFlight App Store 2026; profil tipi appstore; enterprise=false; cihaz sayısı0; sertifika Apple Distribution: EDASU YADIK (VKW7GYV7B9). Son tarih25 Ekim2027.
Kanıtlar CloudSigningSaved.jpg, FreeMacQuota.jpg ve UnsupportedEditorBuildError.jpg.
Devam seçenekleri: bulutta6000.3.27f1 desteği sağlanması veya ayrı bir bulut kopyasında desteklenen sürümle uyumluluk testlerinin planlanması. Çalışan ana projede sürüm değişikliği yapılmadı. App Store upload API yetkisi bu engel kalkmadan oluşturulmadı.
Hazırlık commit aecea199fa5ac967131c5f29340f4c320e88eff3 private master'a push edilmiş ve uzak SHA doğrulanmıştır.
