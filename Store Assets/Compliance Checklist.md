# MeWGic · Uyum ve mağaza veri beyanı

Bu çalışma yayın beyanı taslağıdır. İnceleme kapsamı kaynak, proje ayarları ve PART 19B Android geliştirme manifestidir.

| Alan | Bulgu / bekleyen doğrulama |
| --- | --- |
| Yerel veri | PlayerPrefs ana kayıt, günlük ve yedek yuvalarında ilerleme, yıldız, coin, güçler, öğretici ve tercih bilgisi tutuyor. Üretilen bölüm düzeni persistentDataPath altında. Cihaz dili ve saati yerel işlevlerde kullanılıyor. |
| Analitik / crash | UGS Analytics, reklam, IAP veya crash hizmeti entegrasyonu bulunmadı. UnityConnect ve Cloud Diagnostics kapalı. Analitik gönderim ve eski otomatik başlatma ayarları kapatıldı. |
| Sunucu / SDK | Oyun kaynaklarında sunucuya gönderim çağrısı bulunmadı. Addressables uzak katalog kapalı, gruplar yerel. Unity ve paketleri, Localization, Input System, Addressables, Newtonsoft ve URP kullanılıyor. Unity Pipeline geliştirme aracı. İmzalı paket trafiğini ayrıca ölçün. |
| Android izinleri | PART 19B geliştirme APK’sında INTERNET ve VIBRATE var. Release Gradle kuralı INTERNET’i kaldırır. Geliştirme bağlantıları korunur. VIBRATE isteğe bağlı dokunsal geri bildirim için kalır. Nihai AAB ve üretilen APK izinlerini kontrol edin. |
| iOS izinleri | Bildirim, takip, konum, kamera ve mikrofon istemi kodda görülmedi. IPA, Info.plist, entitlements, privacy manifest ve arşiv raporu hâlâ doğrulanmalı. |
| Yedekleme ve silme | Yerel kayıt silme ve cihaz yedekleme davranışı platform ayarına bağlı. Nihai Android yedek politikası ve iOS arşiv davranışını cihazda sınayın. Oyuncu hesabı yok. |
| Mağaza beyanı | App Privacy ve Play Data safety için kaynak bazlı aday, oyun verisinin geliştiriciye gönderilmediği yönünde. Son paket ve ağ incelemesi geçmeden formu kesinleştirmeyin. Mağaza ve destek kanalı verisini ayrı ele alın. |

- [ ] İmzalı Android AAB’den üretilen APK ve iOS arşivinde SDK, izin, trafiği ve depolama davranışını doğrula. Uygulama içi gizlilik bağlantısını ekle ve cihazda aç.
- [ ] HTTPS gizlilik sayfası, çalışan destek sayfası ve yayıncıya ulaşılabilen iletişim bilgileri hazırla. Apple Support URL ve Privacy Policy URL, Play gizlilik URL’si ve destek e-postasını doldur. Taslaktaki yer tutucuları kaldır.
- [ ] Apple App Privacy ve Play Data safety formunu son build’e göre doldur. Apple required-reason API’leri, özellikle uygulama kapsamındaki UserDefaults ve Unity/SDK manifestlerini Xcode arşivinde denetle.
- [ ] Apple yaş anketi ve Play IARC’yi doldur. Mevcut bulgular renk ayırma, fantastik çevre, kedi ve sanal ödüller. Çevrimiçi etkileşim, kullanıcı içeriği, açık şiddet, kumar veya gerçek para alışverişi gözlenmedi. Günlük sandık oyun içi ödül. Anket puanını mağaza belirler.
- [ ] Hedef kitleyi yayıncı doğrulasın. Sevimli kedi görsellerinin çocuklara hitap etme olasılığını Play hedef kitle ve Families değerlendirmesine dahil et. İçerik yaş derecesi ve hedef kitle ayrı kararlardır.
- [ ] Marka, görsel kaynak dosyaları, üretim kayıtları ve üçüncü taraf bildirimlerinin yayıncı onayını kaydet. Mağaza ekran görüntülerini son cihaz build’iyle karşılaştır.

## Lisans izi

- Kedi, logo, açılış, ada ve UI sanatları mevcut proje dosyaları. Mağaza kartları gerçek Unity çekimlerinden oluşturuldu. Yeni stok görsel alınmadı. Sanatın ticari hak zinciri ve varsa üretim şartları yayıncı tarafından doğrulanmalı.
- PART 18 seslerinin on klibi özgün sentez ve üretim betiğiyle kayıtlı. [Ses envanteri](../docs/ProductionReports/Part18/AUDIO.md)
- Noto Sans, Cormorant Garamond ve Liberation Sans için OFL metinleri depoda. Değiştirilmiş font adları ve dağıtılan paket içindeki bildirimler kontrol edilmeli. Unity ve diğer paketlerin sürüm bazlı bildirimleri final dağıtımda toplanmalı.
- Coin oyun oynayarak kazanılıyor ve yardımcı güçlere harcanıyor. Reklam ve ücretli satın alma entegrasyonu eklenmedi. İleride ücretli öğeler için mağaza satın alma katmanı, tekrar işleme güvenliği, makbuz doğrulama ve iade/geri yükleme gerekir. Reklam için ayrı tercih/izin ve veri beyanı değerlendirmesi gerekir.

[Apple App Privacy](https://developer.apple.com/app-store/app-privacy-details/) · [Apple privacy manifest](https://developer.apple.com/documentation/BundleResources/privacy-manifest-files) · [Apple yaş derecelendirmesi](https://developer.apple.com/help/app-store-connect/reference/app-information/age-ratings-values-and-definitions/) · [Google Data safety](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en) · [Google App content](https://support.google.com/googleplay/android-developer/answer/9859455?hl=en-en) · [IARC](https://support.google.com/googleplay/android-developer/answer/9898843?hl=en)
