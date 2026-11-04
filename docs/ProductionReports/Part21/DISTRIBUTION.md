# MeWGic 0.8.0 RC.1 · Test dağıtımı

Kaynak dalı `release/0.8.0-rc.1`. Planlanan Android versionCode ve iOS build numarası 11. Her iki mağazada da 11'in daha önce kullanılmadığını ve son yüklenen numaradan büyük olduğunu yetkili kullanıcı doğrulamalı. Gerekirse daha büyük sayı verin. Paket kimliklerini koruyun.

## iOS ve TestFlight

1. Unity 6000.3.27f2 iOS Build Support yüklü yetkili Mac veya ayrı Mewgic iOS RC 0.8.0 11 bulut hedefini kullanın. Hedef Xcode 26.5 ile yapılandırıldı. Xcode 26 veya üzeri ve iOS 26 veya üzeri SDK kullanın. Minimum dağıtım hedefi iOS 15. [Apple SDK gereksinimi](https://developer.apple.com/news/upcoming-requirements/?id=04282026a)
2. RC dalını ve doğru SHA'yı seçin. `MewgicIosCloudBuild.PreExport` kancasını kullanın ve `MEWGIC_IOS_BUILD_NUMBER` değerini güvenli iş ayarından verin. `com.edasuyadik.mewgic` için dağıtım kimliği, profil ve imza ekibi eşleşmesini Xcode arşivinde kontrol edin.
3. Arşivde Release, ikon kataloğu, iPhone/iPad hedefi, Info.plist, entitlements ve PrivacyInfo.xcprivacy içeriğini doğrulayın. Yetkili yükleme ile arşivi TestFlight iç testine gönderin. İşlenme, export compliance ve tester erişimini kontrol edin. Dış test kullanılacaksa gerekli beta incelemesini tamamlayın. [Apple build yükleme](https://developer.apple.com/help/app-store-connect/manage-builds/upload-builds/) · [TestFlight](https://developer.apple.com/testflight/)
4. TestFlight’tan önceki aynı uygulama build’i üzerinden güncelleyin ve bir cihazda temiz kurulum yapın. Test kullanıcıları aşağıdaki kısa listeyi doldursun. Mağaza canlı yayını ayrı kullanıcı onayı gerektiriyor.

## Android ve Google Play

1. Play Console’da uygulama kimliğini, hesap türü ve açılış tarihini, Production access sayfasını, en yüksek versionCode değerini ve Play App Signing durumunu doğrulayın. Upload anahtarı ve Play’in cihaz paketini imzalayan anahtar farklı olabilir. [Play App Signing](https://support.google.com/googleplay/android-developer/answer/9842756?hl=en)
2. Unity 6000.3.27f2 Android Build Support ile batch `-buildTarget Android -executeMethod MewgicAndroidDeviceBuild.ReleaseAab` kullanın. `MEWGIC_ANDROID_BUILD_CODE`, `MEWGIC_ANDROID_KEYSTORE`, `MEWGIC_ANDROID_KEYSTORE_PASSWORD`, `MEWGIC_ANDROID_KEY_ALIAS` ve `MEWGIC_ANDROID_KEY_PASSWORD` değerlerini yetkili CI kasasından verin. Son AAB’yi bundletool ve Play Console ile doğrulayın. API 36, ARM64, 16 KB hizalaması, release imzası, debug bayrağı ve izinleri dağıtılacak APK’da kontrol edin. [Hedef API](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-gb) · [16 KB](https://developer.android.com/guide/practices/page-sizes)
3. İç testte kurulum ve güncelleme kontrolünü çalıştırın. Sonra gerekli kapalı test listesini oluşturup geri bildirimleri ve düzeltmeleri kaydedin. 13 Kasım 2023'ten sonra açılmış kişisel hesaplarda üretim erişimi başvurusunda son 14 gün boyunca kesintisiz katılmış en az 12 test kullanıcısı gerekir. Diğer hesapların durumu Console’dan doğrulanmalı. [Google kapalı test şartı](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en)
4. Yerel RC APK geliştirme sertifikasıyla imzalanır. Play üzerinden imzalanan uygulamanın üzerine kurmaya çalışmayın. İmza uyuşmazlığında kullanıcı verisini silmeyin.

## Test kullanıcısı kısa listesi

- Cihaz, işletim sistemi, build, dağıtım kanalı ve tarih yaz.
- Temiz kurulumda açılış, pati öğreticisi, ilk kazanış, coin ve sandığı dene.
- Önceki aynı kanal build’inde yıldız, coin ve ayarları kaydet. Üzerine güncelle ve aynı değerleri karşılaştır.
- Döküş sırasında arka plana geç, ekranı kilitle ve dön. Uygulamayı tamamen kapatıp aç. Çift hamle veya ödül oluşmamalı.
- Uçak modunda aç. Harita kaydırma, dokunma, ses, titreşim ve 30 dakikalık performansı kaydet.
- Erişilebilen QA profiliyle ada sınırı ve 601. bölümü dene. Gerçek oyuncu kaydını test verisiyle değiştirme.

[Ayrıntılı cihaz matrisi](../Part19B/DEVICE_GUIDE.md) · [Mağaza uyum kontrolü](../../../Store%20Assets/Compliance%20Checklist.md)
