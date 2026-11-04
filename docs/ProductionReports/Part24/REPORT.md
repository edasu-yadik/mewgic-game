# PART 24 · Google Play ve Android hazırlığı

4 Kasım 2026 · MeWGic 0.8.0 RC.1 · Başlangıç commit `50cbfbc` · Dal `release/0.8.0-rc.1`

| İşlem | Durum | Açıklama |
| --- | --- | --- |
| Google Play geliştirici hesabı | BLOCKED | Kişisel kayıt seçildi. Kullanıcının belirttiği Edasu Yadık geliştirici adı girildi. Ödeme profili bağlama adımı bekliyor. |
| Hesap doğrulaması | BLOCKED | Listelenen mevcut bireysel ödeme profilinin hesap sahibine uygunluğu onay bekliyor. Kimlik, iletişim doğrulaması, sözleşme ve ücret tamamlanmadı. |
| Android Unity modülleri | BLOCKED | Kaynak Unity 6000.3.27f2. Mevcut .27f1 araçlarıyla yerel paket doğrulandı. CLI beta.16 ile kurulum denendi. Windows yönetici onayı ELEVATION_CANCELLED sonucu verdi. Editor/modüller ve Android modül lisans kabulü açık. |
| Android AAB | BLOCKED | ReleaseAab girişi hazır. Güvenli ortamda bağlı upload keystore ve parolalar bulunmadı. Yerel QA APK üretildi. |
| MeWGic uygulama kaydı | BLOCKED | Aktif geliştirici paneli açılmadığı için mevcut uygulama listesi doğrulanamadı. Yeni uygulama oluşturulmadı. |
| Store Listing | BLOCKED | Beş dilde açıklama, ikon, özellik grafiği ve altı telefon kartının formatları geçti. Destek/gizlilik URL’leri, yayıncı hak onayı ve Console kaydı açık. |
| Data Safety | BLOCKED | Kaynak ve bağımlılık incelemesine göre taslak hazır. Son imzalı AAB, cihaz ağ davranışı ve hesap sahibi beyanı bekliyor. |
| Kapalı test | BLOCKED | Kanal, test kullanıcıları ve AAB yüklemesi yapılmadı. 12 kişi / 14 gün koşulu doğrulandı. |
| Gerçek Android testi | BLOCKED | ADB listesinde bağlı cihaz yok. Fiziksel kurulum, güncelleme, kayıt ve performans testleri bekliyor. |
| Yayın hazırlığı | BLOCKED | Hesap, imzalı AAB, kapalı test, fiziksel kabul ve son mağaza beyanları kapanmalı. |

Kaynak düzeltme commit kodu `4b3ae6c`, uzak release dalına gönderildi.

## Tamamlanan teknik çalışma

Android App Info eksikliği giderildi. `app.display_name` anahtarı sekiz dilde MeWGic. Android varsayılan launcher kaynağı da MeWGic. Smart String kabul etmeyen platform metadata’sı için kaynak şemasında düz metin işareti eklendi. Önceden düz metin olarak çalışan dokuz öğretici/hareket etiketi aynı özellikte işaretlendi. Görünen metinler değişmedi. Localization kurulum ve doğrulama betiği bu ayrımı koruyor.

Build ön kontrolü Android App Info ve dil tablosu bağlantısını doğruluyor. Eksik veya yanlış ada sahip paket derlemesini durduruyor. Uygulama adı dışındaki oynanış, ekonomi, ada algoritması, sahneler ve sanat varlıkları değiştirilmedi. iOS kimlikleri, sürüm yapılandırması ve mevcut TestFlight paketi değiştirilmedi. iOS Build 11 test edilebilirliği bu partta kullanıcının verdiği bilgi olarak kabul edildi, yeniden denetlenmedi.

| Kontrol | Gerçek sonuç |
| --- | --- |
| C# derlemesi ve Android paket | Unity 6000.3.27f1 ayrı proje kopyasında Succeeded, 0 hata, 0 uyarı, 91,6 saniye |
| APK sürüm ve kimlik | 0.8.0 / 11, com.mewgic.game, min API 26, target API 36, ARM64 |
| Paket güvenliği | Debug bayrağı kapalı, yalnız VIBRATE, yerel Addressables, imza geçerli, test varlık yolu eşleşmesi yok |
| 16 KB | ZIP hizalaması ve altı yerel kütüphanenin ELF LOAD hizalaması geçti |
| Android görünen ad | aapt2, varsayılan ve sekiz dilde MeWGic doğruladı |
| Adaptive icon | Paket XML’inde ayrı foreground/background katmanları doğrulandı |
| İkon tutarlılığı | Android 1024 kaynak ikonu mevcut App Store 1024 ikonuyla piksel düzeyinde aynı |
| Platform yerelleştirme | Sekiz Android strings.xml üretildi. Metadata tekrarlı çalıştırmada aynı. Uyarı yok |
| Unity Localization | 188 anahtar, sekiz dil ve glif/bağlantı kontrolü geçti |
| Kaynak yerelleştirme | 188 anahtar, sekiz dil, 201 kaynak başvurusu, 0 hata |
| Mağaza varlıkları | Mevcut ölçüler ve beş dilde alan sınırları tekrar doğrulandı |
| Upload key hazırlığı | Depo dışı yol ve mevcut dosyayı koruyan etkileşimli keytool betiği hazır. PowerShell sözdizimi, depo içi yol reddi ve mevcut dosyayı koruma testleri geçti. Anahtar üretilmedi |

İlk metadata testinde Smart String kullanımına karşı paket assertion’ı yakalandı ve düz metin olarak düzeltildi. Geniş Localization kontrolünde eski dokuz düz metin anahtarının kaynak metadata’sıyla uyuşmadığı görüldü. Kaynak işaretleri mevcut çalışma zamanı verisiyle eşleştirildi ve kontrol temiz geçti.

Yerel APK `Builds/Android/Part24/Mewgic-0.8.0-11-rc-local.apk`. SHA-256 `75e1b3a4350d0db4c208ea16d3edea8b8b3770e34247f095c44c21e9f0efdec2`. Bu paket geliştirme sertifikasıyla imzalı ve .27f1 araç zincirinden geliyor. Play dağıtımı için yamalı araç zincirinde yetkili anahtarla AAB gerekli. [Araç zinciri](TOOLCHAIN.json) · [Anahtar hazırlık testleri](KEY_PREPARATION_TESTS.json) · [APK denetimi](ANDROID_APK_VERIFICATION.json) · [Android App Info](UNITY_APP_INFO.json) · [Unity Localization](UNITY_LOCALIZATION.json) · [Kaynak kontrolü](LOCALIZATION_SOURCE.json) · [Mağaza varlıkları](STORE_VALIDATION.json)

## Kullanıcı müdahalesi ve sıradaki işlemler

1. Play hesabının gerçek sahibine ait ödeme profili seçilmeli. Ekranda listelenen mevcut profil farklı kişi adına. Adres, kimlik ve ödeme bilgisi varsayılmadı. Kayıt ücreti ve geliştirici sözleşmesi kullanıcı tarafından onaylanmalı. Mevcut kayıt adımında teknik hata mesajı gözlenmedi.
2. Kullanıcı CLI güncellemesine onay verdi. Resmî self-update sonucu sürüm 1.0.0-beta.16 oldu. İlk beta.14 kurulumunda os error 123 oluştu. Güncel CLI bu aşamayı geçti, fakat Windows yönetici onayı iptal edildi veya zaman aşımına uğradı. Son hata ELEVATION_CANCELLED. Unity Hub veya yönetici terminalinde 6000.3.27f2 ve Android Build Support, SDK/NDK, OpenJDK kurulmalı. Android SDK/NDK lisans kabulü için ayrıca kullanıcı onayı bekleniyor.
3. Mevcut Play uygulaması varsa upload key durumu doğrulanmalı. Yeni anahtar gerekiyorsa kullanıcı etkileşimli betikle oluşturup güvenli yedeklemeli. Sırlar sohbete veya Git’e yazılmamalı.
4. Doğrulanmış destek e-postası, gizlilik sayfası, hedef kitle, IARC ve hak zinciri onayları sağlanmalı.
5. AAB, test kanalı ve cihaz matrisinden sonra üretim erişimi değerlendirilir. Canlı yayın onayı ayrıca gerekir.

[Ayrıntılı kayıt, beyan, build ve kapalı test rehberi](PLAY_PREPARATION.md) · [Önceki yayın raporu](../Part21/REPORT.md)

Git değişiklikleri yalnız bu partın Android marka metadata’sı, doğrulama/build betikleri ve raporlarıyla sınırlı. Başlangıçtaki TMP fallback değişikliği ve diğer kullanıcı dosyaları kapsam dışında tutuldu. Gerçek kullanıcı kaydı test profiline çevrilmedi. Testler ayrı proje kopyasında çalıştı. Google’a e-posta veya destek başvurusu gönderilmedi.
