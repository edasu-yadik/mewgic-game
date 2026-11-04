# PART 24 · Play kaydı, mağaza ve kapalı test hazırlığı

4 Kasım 2026 · MeWGic 0.8.0 RC.1 · Android kimliği `com.mewgic.game`

## Hesap ve uygulama

Bu oturumda Play Console kişisel geliştirici kaydının başlangıcına yönlendirdi. Kişisel akış seçildi, kullanıcının belirttiği Edasu Yadık geliştirici adı girildi ve ödeme profili adımına ulaşıldı. Geçici MeWGic adı sonrasında kullanıcı talebiyle değiştirildi. Kayıt tamamlanmadı. Henüz hata mesajı gözlenmedi. Bu oturumun aktif geliştirici hesabına erişimi doğrulanamadı. Başka Google hesaplarında mevcut geliştirici kaydı bulunup bulunmadığı belirlenmedi.

Mevcut bir Türkiye bireysel ödeme profili listelendi. Profilin gerçek hesap sahibine ait olduğu kullanıcı tarafından onaylanmadan bağlanmadı. Ekrandaki işlem “Kimliğinizi doğrulamak için ödeme profili bağlayın”. Bu bir hata mesajı değil, eksik kayıt adımıdır. Sonraki ekranlarda istenebilecek iletişim e-postası, telefon, gerçek kişi adı, adres ve deneyim beyanını hesap sahibi doğrulamalı. Oturum e-postası otomatik olarak herkese açık destek adresi kabul edilmemeli. Google ekranı bir defalık 25 USD kayıt ücreti ve kimlik doğrulaması gerektiğini belirtiyor. Ödeme, sözleşme kabulü, SMS/2FA ve kimlik adımlarını kullanıcı tamamlamalı.

Aktif panel açılınca önce uygulama listesinde MeWGic ve `com.mewgic.game` kaydını ara. Kayıt varsa devam et. Kayıt yoksa MeWGic, English (United States), Game ve Free seç. Önerilen kategori Puzzle. Developer Program Policy ve Play App Signing beyanları hesap sahibinin onayıyla tamamlanmalı. Paket adı ilk AAB ile bağlanacağından yükleme öncesinde kimliği tekrar doğrula.

## Mağaza dosyaları

| Alan | Hazır dosya / bekleyen bilgi |
| --- | --- |
| Ad | MeWGic |
| İngilizce kısa metin | Sort colorful potions with a wizard cat and explore 12 fantasy islands. |
| Uzun metin ve beş dil | `Store Assets/Metadata/en.md`, `tr.md`, `de.md`, `fr.md`, `es.md` |
| Uygulama ikonu | `Store Assets/GooglePlay/Icon-512.png`, 512×512 PNG |
| Özellik grafiği | `Store Assets/GooglePlay/Feature-1024x500.jpg`, 1024×500 JPEG |
| Telefon görselleri | `Store Assets/GooglePlay/Phone/`, altı 1080×1920 JPEG |
| Destek | Yayıncı tarafından doğrulanmış e-posta ve varsa destek sitesi eksik |
| Gizlilik | `Store Assets/Privacy Policy Draft.md` taslak. Yayıncı bilgileri ve çalışan herkese açık HTTPS sayfası eksik |
| Haklar | Projedeki sanatın ticari hak zinciri ve çeviri onayı açık. Ses üretim izi PART 18 envanterinde. Kesinleşmemiş haklar için yayın onayı verilmedi |

Format ve metin sınırları yeniden doğrulandı. Telefon kartları önceki gerçek Unity görüntülerinden oluşturulmuş editoryal görseller. Fiziksel Android çekimi olarak sunulmaz. Gerekli tablet grupları aktif Play Console ekranından belirlenecek. Yeni görsel veya özellik eklenmedi. [Google mağaza varlıkları](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en)

## Beyan taslağı

| Form | Kaynak incelemesine göre cevap adayı | Kapanış için gereken |
| --- | --- | --- |
| App Access | Giriş hesabı veya üyelik gerekmiyor. Bölüm açma oyun ilerlemesine bağlı | Dağıtılacak build ile mağaza inceleyicisinin erişim akışını doğrula |
| Ads | Reklam SDK veya reklam gösterimi bulunmadı. Contains ads için No adayı | Son paket bağımlılıkları ve gerçek davranışı doğrula |
| IAP | Ücretsiz oyun içi coin. Ödeme SDK veya ücretli ürün bulunmadı | Son paket ve liste metnini karşılaştır |
| Data Safety / Collection | İlerleme, coin, yıldız, öğretici, tercih, zaman ve üretilen bölüm düzeni yerel tutuluyor. Kaynakta geliştirici sunucusuna aktarım bulunmadı | İmzalı dağıtım paketi, son manifest, SDK sağlayıcı açıklamaları ve cihaz ağ ölçümü sonrası hesap sahibi onayı |
| Third-party SDK | Unity, URP, Input System, Addressables, Localization, Newtonsoft. Pipeline, test ve IDE paketleri geliştirme araçları | Paket listesi, stripping sonucu ve son çalışma zamanı SDK envanterini doğrula |
| Content Rating | Renk sıralama bulmacası, fantastik kedi ve çevre, sanal coin ve günlük sandık. Kaynakta kullanıcı içeriği, sohbet, bahis veya gerçek para alışverişi bulunmadı | Hesap sahibi IARC anketini gerçek içerikle tamamlar. Yaş derecesi tahmin edilmedi |
| Target Audience | Kesin hedef yaş grubu belirtilmedi | Yayıncı tasarlanan hedef kitleyi onaylar. Yalnız daha kolay yayın için çocukları dışlayan cevap verilmez |
| Privacy Policy | Taslak mevcut, yer tutucular var | Yayıncı adı, destek süreci, veri saklama ve geçerli hak bilgileri tamamlanır. Web ve uygulama içi bağlantı kontrol edilir |

Unity Cloud geliştirme derlemelerinde sembol yüklenmesi, oyuncu uygulamasında analytics bulunduğunu kanıtlamaz. Bu ayrım son paketle doğrulanmalı. Data Safety'de cihazdan dışarı aktarılmayan yerel veri ile geliştiricinin topladığı veri ayrı değerlendirilir. Kapalı testte de Data Safety ve gizlilik sayfası gerekir. [Data Safety](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en) · [Uygulama beyanları](https://support.google.com/googleplay/android-developer/answer/9859455?hl=en) · [Hedef kitle](https://support.google.com/googleplay/android-developer/answer/9867159?hl=en)

## Android derleme ve imza

Kaynak hedefi Unity 6000.3.27f2, API 36, min API 26, IL2CPP ve ARM64. Bu makinede 6000.3.27f1 Android araçlarında JDK 17.0.18, NDK r27c ve API 36 doğrulandı. Resmî self-update ile CLI 1.0.0-beta.16 sürümüne yükseltildi. Yamalı Editor indirmesi doğrulandı, fakat kurulum Windows yönetici onayında ELEVATION_CANCELLED ile durdu. Kullanıcı Unity Hub veya yönetici terminalinde 6000.3.27f2 ile Android Build Support, SDK/NDK ve OpenJDK kurulumunu tamamlamalı. Android modül lisansları ayrıca kullanıcı onayına bırakıldı. Paket adı ve iOS build ayarları değiştirilmedi.

Release girişi `MewgicAndroidDeviceBuild.ReleaseAab`. `MEWGIC_ANDROID_BUILD_CODE`, `MEWGIC_ANDROID_KEYSTORE`, `MEWGIC_ANDROID_KEYSTORE_PASSWORD`, `MEWGIC_ANDROID_KEY_ALIAS`, `MEWGIC_ANDROID_KEY_PASSWORD` güvenli CI/oturum üzerinden sağlanmalı. Değerler rapora veya Git'e yazılmamalı. Şu oturumda bu değişkenler bulunmadı, proje özel keystore kullanmıyor. Eski Play kaydı açılırsa yeni anahtar üretmeden önce o uygulamanın upload key bilgisini doğrula.

Yeni upload anahtarı gerekiyorsa `tools/New-AndroidUploadKey.ps1` kullanıcı tarafından etkileşimli çalıştırılmaya hazır. Resmî JDK keytool yolu ve depo dışında güvenli dosya yolu ister. Mevcut dosyaya yazmaz. Parola ve gerçek sertifika bilgileri keytool isteminde kullanıcı tarafından girilir. Anahtar oluşturma bu raporda tamamlandı sayılmadı. Anahtar ile parola ayrı güvenli yedeklere alınmalı. [Play App Signing](https://support.google.com/googleplay/android-developer/answer/9842756?hl=en)

Android uygulama adı sekiz dil tablosuna bağlı. Desteklenmeyen cihaz dilleri için varsayılan launcher adı da MeWGic olarak düzenleniyor. Branding anahtarı düz metin olarak tutuluyor çünkü Android App Info Smart String kabul etmiyor. Diğer metinlerin Smart String davranışı korunuyor. Build ön kontrolü eksik metadata veya yanlış adı reddediyor. Adaptive icon aynı mevcut kedi sanatı ve ayrı arka/ön katmanları kullanıyor.

## Kapalı test ve fiziksel cihaz listesi

Yeni kişisel hesap için en az 12 kullanıcının en az 14 gün kesintisiz kapalı test katılımı ve ardından üretim erişimi başvurusu gerekir. Bu bekleme süresi uygulamanın otomatik yayın onayı anlamına gelmez. [Google kapalı test şartı](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en) · [API 36 şartı](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en)

1. Aktif hesap ve uygulamada önce Internal testing aç. Son AAB kimliğini, sürüm kodunu, imzayı, debug bayrağını, izinlerini ve 16 KB hizalamasını doğrula. Play tarafından üretilen APK'yı da kontrol et.
2. Yetkili test e-posta listesini veya Google Group'u hesap sahibi sağlasın. Test kullanıcılarına mesaj gönderilmedi. Sonra Closed testing listesine ekle, geri bildirim kanalı ve opt-in bağlantısı oluştur.
3. Build sürüm notlarını beş dildeki mevcut dosyalardan al. Yalnız test kanalında sürüm taslağı oluştur. Review/rollout öncesindeki yasal beyanları kullanıcı onaylasın.
4. Katılımcıların aynı Google hesabıyla opt-in yapıp Play üzerinden kurduğunu kaydet. Testten çıkış kesintisiz süreyi etkiler. Başlangıç ve son tarih, geri bildirimler, hatalar ve düzeltmeler kayıtlı olmalı.
5. Her cihazda model, Android sürümü, build kodu, dağıtım kanalı ve sonuç yaz. Min API destekli bir cihaz ve güncel Android cihaz kullan. 16 KB sayfa boyutlu cihaz erişimi yoksa ayrı BLOCKED yaz.

| Test | Beklenen sonuç | Sonuç |
| --- | --- | --- |
| Temiz kurulum / ilk açılış | MeWGic adı ve ikon, portre Safe Area, ilk pati öğreticisi | Fiziksel test bekliyor |
| İlk on bölüm / hızlı dokunma | Tek döküş, geçersiz hamle geri bildirimi, Undo ve Hint | Fiziksel test bekliyor |
| Coin, yıldız ve sonuç | Ödül bir kez yazılır, tekrar kazanışta ilk ödül tekrarlanmaz | Fiziksel test bekliyor |
| Günlük sandık / kapatıp açma | Ödül bir kez, tekrar açış yok | Fiziksel test bekliyor |
| Harita / kedi / adalar | Kaydırma ile seçme ayrılır, kedi yol üstünde ilerler | Fiziksel test bekliyor |
| Güncelleme / kayıt | Önceki aynı imzalı test sürümünün üzerine yükle. Coin, yıldız, aktif bölüm, ayarlar ve öğreticiyi karşılaştır | Fiziksel test bekliyor |
| Uçak modu / ekran kilidi | Yeniden açılır, çift hamle/ödül yok, kayıt korunur | Fiziksel test bekliyor |
| 600 → 601 / 1000 | Yetkili test kaydıyla doğrulanır. Gerçek oyuncu kaydı zorla açılmaz | İleri cihaz kaydı bekliyor |
| 30 dakika oyun | FPS, kare p95, bellek, pil ve sıcaklık kaydedilir | Ölçüm bekliyor |

Yerel debug imzalı APK ile Play imzalı kurulumun güncelleme sertifikaları farklı olabilir. İmza uyumsuzluğunda uygulamayı silerek gerçek kayıtları kaybetmeyin. Önce aynı kanal ve aynı paket imzasını doğrulayın.
