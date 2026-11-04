# PART 21 · MeWGic 0.8.0 RC.1

4 Kasım 2026 · Dağıtım hedefi **0.8.0 (11)** · Dal `release/0.8.0-rc.1` · Başlangıç `9840c9e` · İlk yamalı derleme kaynak commit’i `ccf8c02f2edccc98cf59c89302a192601adec94f`

**Yayın kararı · BLOCKED.** Bu sürümün kaynak dalı hazır. iOS imzalı IPA hazır. Android imzalı AAB, fiziksel cihaz kabulü ve son mağaza beyanları açık. Mağazalarda canlı gönderim yapılmadı.

## Ön kontrol

| Alan | Durum | Kanıt |
| --- | --- | --- |
| 12 adalık dünya ve mobil harita | PASS · Editor | RC’de 12 × 50 doğrulandı. Önceki Game View kanıtı 12 ada, üç portre oranı, sayı taşmaları ve dekor dokunma ayrımını kapsıyor. [15E](../Part15E/REPORT.html) · [15F](../Part15F/REPORT.html) |
| Üretici ve oynanış kuralları | PASS · Otomasyon | RC dalında 10.000 farklı seed, 10.000 doğrulanmış sonuç, 0 başarısız. 2.060 fallback, 7.710 benzersiz düzen, 2.290 tekrar, 64 bölüm içinde 146 tekrar. Çözücü p95 2,329 ms, toplam çözümleme p95 5,767 ms. 600 ve 601 ayrıca gerçek sahnede doğrulandı. [16A](../Part16A/) |
| Pati öğreticisi ve harita yolculuğu | PASS · Editor | Pati gerçek döküş yaptı, öğretici durumu kaydedildi, ilerleme sonrasında kedi yürüdü. Önceki dokunmatik simülasyonda 259 kontrol. [16B](../Part16B/) |
| Günlük ödül, coin ve yıldız | PASS · Editor | Sandık 100 coin, ilk kazanış 50 coin, yıldız ve ilerleme kaydı doğrulandı. Ekonomi tekrar işleme kanıtı önceki 66 kontrole dayanıyor. [17](../Part17/) |
| Çoklu dil, ses ve ayarlar | PASS · Önceki Editor kanıtı | Sekiz dil, 42 Game View karesi, 132 kontrol ve özgün ses envanteri. RC’de ilk dil başlangıcı tekrar çalıştı. Fiziksel ses ve titreşim kabulü açık. [18](../Part18/) |
| Kayıt güvenliği | PASS · İzole Editor | 16 depolama kontrolü, sürümlü kayıt, yedek ve kurtarma. RC Play Mode’da yeniden yükleme ve korunmuş gerçek profil doğrulandı. Mağaza üstüne güncelleme açık. [Depolama kanıtı](STORAGE_TESTS.json) |
| Fiziksel cihaz ve mağaza dağıtımı | BLOCKED | Bu build için iOS/Android donanımında kurulum, dokunma, güncelleme ve performans ölçümü yapılmadı. |
| Mağaza varlıkları | PASS · Taslak ve format | İkon, 24 mağaza kartı ve beş dilde mağaza metni mevcut. [20](../Part20/REPORT.md) |
| Gizlilik, içerik ve son mağaza görselleri | BLOCKED | Son imzalı paket denetimi, gerçek iPad seti, varsa gerekli Play tablet seti, destek ve gizlilik URL’leri, uygulama içi gizlilik bağlantısı, yaş/veri formları ve hak onayı açık. [Uyum listesi](../../../Store%20Assets/Compliance%20Checklist.md) |

## Son regresyon

20 Play Mode kontrolü ve 16 depolama kontrolü geçti. Play Mode tekrarında 0 hata, 0 uyarı. İlk denemede oyun kontrolleri geçti ancak Unity Search başlatma istisnası raporlandı. Editor test girişine kısa bekleme eklendikten sonraki temiz koşu geçerli sonuçtur. Kontroller Windows Unity 6000.3.27f1 ve ayrı test profiliyle çalıştı. Yamalı araç zincirinde yeniden koşulması açık. [Play Mode](UNITY_REGRESSION.json) · [İlk deneme](UNITY_EDITOR_FIRST_ATTEMPT.json) · [Depolama](STORAGE_TESTS.json)

| # | Senaryo | Durum |
| --- | --- | --- |
| 1 | İlk açılış | PASS · Boot → Home ve yeni oyuncu. Cihazda soğuk açılış süresi açık. |
| 2 | İlk bölüm ve öğretici | PASS · Pati seçimi ve gerçek döküş. |
| 3 | Bulmaca kazanma | PASS · Çözücüyle doğrulanmış gerçek hamleler ve sonuç ekranı. |
| 4 | Coin ödülü | PASS · İlk kazanış 50 coin, animasyondan bağımsız kalıcı kayıt. |
| 5 | Günlük sandık | PASS · Ödül animasyon öncesinde yazıldı ve sandık tekrar kilitlendi. |
| 6 | Harita ilerlemesi | PASS · Yıldız, aktif bölüm ve kedi yolculuğu. |
| 7 | Yeni ada | PASS · 600 → 601 sınırı Editor. Ada giriş animasyonu için önceki 101 kanıtı. |
| 8 | 600 sonrası | PASS · 600 gerçek hamlelerle tamamlandı, 601 doğrulanmış tahta açtı. |
| 9 | Kaydetme ve geri yükleme | PASS · İzole profil yeniden yükleme ve bozuk kayıt kurtarma. |
| 10 | Eski uygulama sürümünden güncelleme | BLOCKED · Eski veri şeması testi geçti. Fiziksel cihazda mağaza üstüne güncelleme bekliyor. |
| 11 | İnternetsiz kullanım | BLOCKED · Android yerel katalog ve INTERNET izninin yokluğu doğrulandı. Uçak modu cihaz testi açık. |
| 12 | Arka plan | PASS · Editor arka plan simülasyonunda çift döküş oluşmadı. OS askıya alma ve ekran kilidi cihaz testi açık. |
| 13 | Tam kapatıp açma | BLOCKED · İzole kayıt yeniden açıldı. OS süreci sonlandırma ve yeniden açma cihazda açık. |
| 14 | Uzun süreli performans | BLOCKED · 30 dakika cihaz FPS, bellek, pil ve sıcaklık ölçümü açık. |
| 15 | iOS ve Android dokunmatik | BLOCKED · Önceki sanal dokunma kanıtı mevcut. RC probe UI düğmelerini çağırıyor. Fiziksel dokunma bekliyor. |

## Dağıtım ve açık hatalar

- **Android PASS · Yerel paket doğrulaması.** `Mewgic-0.8.0-11-rc-local.apk`, min API 26, hedef API 36, ARM64, 16 KB zip ve ELF hizalaması, geçerli yerel imza, debug bayrağı kapalı ve yalnız VIBRATE. Paket taramasında test varlık yolu bulunmadı. Editor test profilini açan girişler player derlemesinin dışında. Android ikon içe aktarma uyarıları düzeltildi. Son yerel derlemede 0 hata ve 1 uyarı. [Paket doğrulaması](ANDROID_APK_VERIFICATION.json)
- **Android BLOCKED · İmzalı AAB ve kapalı test.** Yerel APK Unity 6000.3.27f1 ile üretildi ve yerel geliştirme sertifikasıyla imzalı. Yamalı Windows Android araç zincirinde yeniden derleme ve yetkili Play upload anahtarı açık. Unity CLI Android modüllerini kurmak için lisans kabulü istedi. Yetkili ortamda modüller kurulmalı. Erişilen Play Console oturumu geliştirici hesabı oluşturma ekranında durdu. Hesap türü, uygulama kaydı, üretim erişimi ve kapalı test zorunluluğu bu oturumda belirlenemedi. Uygulanıyorsa 12 kullanıcının 14 gün kesintisiz katılımı gerekir. [Google kapalı test şartı](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en)
- **iOS PASS · İmzalı RC arşivi.** Ayrı bulut hedefi Unity 6000.3.27f2, macOS ve Xcode 26.5 ile oluşturuldu. Yamalı kaynak commit’iyle RC işi #2 başlatıldı. İş tamamlandı ve imzalı IPA ile derlenmiş 1024×1024 AppIcon doğrulandı. Kopyalanan bulut ortam değişkeni nedeniyle IPA build 2 çıktı ve RC kabulünde başarısız oldu. Dalın 44b5a3f commit kodu RC numarasını pre-export ve IPA denetiminde sabitliyor. Yanlış numaralı örnek IPA reddedildi. Build 11 için bulut işi #3 başarılı. App Store imzalı IPA, CRC, bundle kimliği, 0.8.0 (11) ve derlenmiş opak 1024×1024 AppIcon doğrulandı. [Paket kanıtı](IOS_IPA_VERIFICATION.json)
- **iOS BLOCKED · TestFlight ve cihaz kabulü.** Son entitlements ve PrivacyInfo.xcprivacy denetimi açık. Erişilen App Store Connect’te mevcut 0.8.0 (2) önceki TestFlight build’i. RC 11 için yükleme ve cihaz güncellemesi yapılmadı. App Store sürüm taslağı 1.0, paket sürümü 0.8.0. Mağaza sürümü ve paket eşleşmesi yayıncı tarafından çözülmeli. [Başarılı derleme, yanlış numaralı iş #2](https://cloud.unity.com/organizations/5774143930948/projects/6b9ce85f-16e6-4b97-8f20-62dcd080e042/cloud-build/buildtargets/mewgic-ios-rc-0-8-0-11/builds/2/log) · [Build 11 işi #3](https://cloud.unity.com/organizations/5774143930948/projects/6b9ce85f-16e6-4b97-8f20-62dcd080e042/cloud-build/buildtargets/mewgic-ios-rc-0-8-0-11/builds/3/log) · [Apple SDK koşulu](https://developer.apple.com/news/upcoming-requirements/?id=04282026a)
- **FAIL · İlk iOS paketinin numarası ve eski araç zinciriyle yayın paketi kabulü.** İlk IPA numara hatası iş #3 ile giderildi. Unity güvenlik duyurusu nedeniyle ilk iOS RC işi #1 iptal edildi. Kaynak ve iOS bulut hedefi 6000.3.27f2’ye sabitlendi. 6000.3.27f1 Android APK yerel QA kanıtı olarak tutuluyor. [Unity güvenlik duyurusu](https://gw-prd.hexagon.unity.com/security/unity-emudhra-compromise)
- **FAIL · Android yerelleştirilmiş sistem uygulama bilgisi.** Unity, Localization Settings altında Android App Info metadata uyarısı verdi. Oyun içi dil testleri geçti. Nihai launcher/uygulama adı çeviri kararı verilip paket yeniden doğrulanmalı.

[Performans raporu](PERFORMANCE.md) · [İmzalama, dağıtım ve test kullanıcı listesi](DISTRIBUTION.md)

Yayın kapısını açmak için Android yamalı AAB derlemesini ve iOS arşivinin son gizlilik/entitlement denetimini tamamlayın. Ardından yetkili TestFlight ve Play test kanallarında temiz kurulum, önceki build üzerine güncelleme ve kayıt korunumu dahil cihaz matrisini tamamlayın. Son cihaz görsellerini ve mağaza beyanlarını bitirin. Canlı gönderim için yayıncı onayı ayrı adım.
