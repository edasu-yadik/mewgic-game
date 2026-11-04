# PART 19B · Gerçek cihaz hazırlığı

4 Kasım 2026 · Unity 6000.3.27f1 · Oyun kaynak tabanı `4b570f3`

## Android

| Alan | Değer |
| --- | --- |
| Paket | `com.mewgic.game` |
| Sürüm | 0.8.0 |
| Kaynak varsayılan build kodu | 8 |
| PART 19B test kodu | 10 |
| Minimum / derlemede sabitlenen hedef API | 26 / 36 |
| Mimari ve backend | ARM64 / IL2CPP |
| Yerel araçlar | Android SDK 36, Build Tools 36.0.0, NDK r27c |
| Dağıtım | Geliştirme APK'sı ve imzalı release AAB derleme girişi |

Google Play için yeni uygulama ve güncellemelerin hedefi API 36 veya üzeri. Proje ayarı ve yeni build girişi hedef API 36 değerini açıkça sabitler. AAB ve 16 KB doğrulamaları ayrı kapılardır. [Google Play hedef API politikası](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-gb) · [Android App Bundle](https://developer.android.com/guide/app-bundle) · [16 KB uyumluluğu](https://developer.android.com/guide/practices/page-sizes)

`MewgicAndroidDeviceBuild.TestApk` ve `MewgicAndroidDeviceBuild.ReleaseAab` Unity batch `-buildTarget Android -executeMethod` girişleridir. Test APK'sı geliştirme imzasını kullanır. Release girişinden önce Play Console'daki en yüksek koddan büyük `MEWGIC_ANDROID_BUILD_CODE` ve onaylı upload keystore için `MEWGIC_ANDROID_KEYSTORE`, `MEWGIC_ANDROID_KEYSTORE_PASSWORD`, `MEWGIC_ANDROID_KEY_ALIAS`, `MEWGIC_ANDROID_KEY_PASSWORD` değişkenlerini güvenli CI kasasından sağlayın. Değerleri terminal geçmişine, komut argümanına veya Git'e yazmayın. AAB çıktısını Play App Signing ve dahili test kanalında ayrıca doğrulayın. Bu oturumda release anahtarları kullanılmadı ve Play'e yükleme yapılmadı.

## Android APK doğrulaması

`Mewgic-0.8.0-10-qa.apk` üretildi. Dosya 104.248.368 bayt. Manifest, API 26/36 ve yalnız arm64-v8a doğrulandı. APK imzası geçerli ve önceki yerel beta APK'sıyla aynı geliştirme sertifikasını kullanıyor. APK ZIP hizalaması ve altı yerel kütüphanenin ELF yük segmentleri 16 KB denetimini geçti. Yerel Addressables kataloğu APK içinde. [Makine okunur APK doğrulaması](APK_VERIFICATION.json)

İlk denemede Localization cihaz dili seçicisinin Android JNI modülü eksikti. Yerleşik modül manifest ve kilide eklendi. İlk denemede ayrılan 9'dan sonra başarılı test paketi 10 koduyla çıktı. Yeniden üretimde build kodunu açıkça verin ve imzalı işleri temiz, ayrılmış CI kopyasında çalıştırın. Unity özetinde 0 hata ve 1 uyarı var. Uyarı Localization Android App Info meta verisinin eksik olmasıyla ilgili. Manifest uygulama etiketini Mewgic olarak gösteriyor. Android sistem düzeyindeki yerelleştirme ve oyun içi cihaz dili davranışı fiziksel testte ayrıca doğrulanmalı.

## iOS

Repo yapılandırmasında Bundle ID `com.edasuyadik.mewgic`, pazarlama sürümü 0.8.0, minimum iOS 15.0. Proje dosyasındaki iPhone build numarası 0 yerel başlangıç değeridir. Bulut pre-export girişi pozitif build numarasını `MEWGIC_IOS_BUILD_NUMBER` veya `BUILD_NUMBER` üzerinden alır. App Store Connect'te daha önce kullanılmamış numara seçilmelidir. Team eşleşmesi, geçerli dağıtım sertifikası ve App Store provisioning profili yetkili Mac/bulut işinde doğrulanmalıdır.

Mevcut proje belgelerinde TestFlight bulut hedefi için Xcode 26.5.0 ve geçmiş bir Unity 6000.3.27f1 erişilebilirlik reddi kayıtlı. Canlı bulut hedefi veya imza geçerliliği bu oturumda doğrulanmadı. Hedefin önceki kaydında `master` seçiliydi. Yetkili işte bu PART'ın doğrulanmış SHA'sını içeren kaynak dalı seçin. Mevcut `MewgicIosCloudBuild.PreExport` ve Fastlane yapılandırmasını kullanın. Apple yükleme eşiği Xcode 26 veya üzeri ve iOS 26 SDK. iOS 15 minimum kurulum hedefi ayrı ayardır. [Apple SDK yükleme gereksinimi](https://developer.apple.com/news/upcoming-requirements/?id=04282026a)

Yerel Windows kurulumunda iOS Build Support veya Xcode bulunmadı. Bu çalışma yeni iOS arşivi, IPA veya TestFlight yüklemesi üretmedi. Apple kimlik bilgisi, sertifika veya profil sırrı alınmadı.

## Fiziksel cihaz durumu

ADB cihaz listesi boştu. Windows'ta bağlı iPhone görünmedi. Bu nedenle her iki platformda cihaz modeli, OS sürümü, kurulum sonucu, FPS, bellek ve sıcaklık **bekliyor**. Cihaz üzerinde çalıştırılmış test sayısı sıfır. Fiziksel cihaz kabul testi her iki platform için yayın engeli.

| Platform | Cihaz / OS | Build | Senaryolar | Ölçüm / hata / düzeltme |
| --- | --- | --- | --- | --- |
| Android | Erişim yok | 0.8.0 / 10 QA | D01–D14 bekliyor | Cihazda ölçülmedi |
| iOS | Erişim yok | TestFlight numarası bekliyor | D01–D14 bekliyor | Cihazda ölçülmedi |

[Cihaz kabul rehberi ve doldurulabilir test tablosu](DEVICE_GUIDE.md)
