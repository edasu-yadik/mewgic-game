# PART 20 · Mağaza varlıkları ve yayın hazırlığı

4 Kasım 2026 · Başlangıç `0276a62` · Unity 6000.3.27f1

## Marka ve açılış

MeWGic yazımı mağaza materyallerinde kullanıldı. Mevcut oyun kedisinin şapka ve yüzüyle mor-altın kare ikon oluşturuldu. Unity Android Legacy, Round ve Adaptive için 18 yuvayı doğruladı. iOS Xcode export girişi 1024 RGB ikonu atıyor. iOS modülü ve imzalı arşiv olmadığı için son Asset Catalog doğrulaması açık. Android ve iOS paket kimlikleri korundu. Boot ilk sahne olarak özel kedi, logo ve gerçek yükleme göstergesini kullanıyor. Önceden kapalı Unity splash ve Unity logo ayarı Editor’da doğrulandı. Soğuk açılışın son cihaz build’inde kontrolü açık. [Unity 6.3 SplashScreen API](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/splashscreen/show) · [Önceki gerçek Boot Game View](../../Part09A/Opening-0-GameView.png)

## Mağaza görselleri ve metinleri

- 24 JPEG kartı RGB ve opak olarak üretildi. iPhone büyük 1320×2868 ve orta 1179×2556, Google Play 1080×1920 telefon seti. iPad 2064×2752 taslağı telefon Game View kaynaklarını kullanıyor ve iPad üzerinde çalışan UI kanıtı olarak kullanılmamalı. Universal iOS hedefi için son iPad seti gerekli. Desteklenen Android tablet ve diğer form faktörleri için Play Console'daki gerekli grupları son build’de doldurun. [Apple ekran görüntüsü şartları](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications) · [Apple gerçek kullanım ilkesi](https://developer.apple.com/app-store/review/guidelines/) · [Google Play görsel şartları](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en)
- Google Play ikonu 512×512 RGBA PNG ve 1 MB altında. Özellik grafiği 1024×500 RGB JPEG. Apple ikonu 1024×1024 RGB PNG. [Apple ikon kataloğu](https://developer.apple.com/documentation/xcode/configuring-your-app-icon) · [Google Play ikon ve özellik grafiği](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en)
- İngilizce, Türkçe, Almanca, Fransızca ve İspanyolca ad/alt başlık, kısa/uzun açıklama, özellikler, anahtar kelimeler ve sürüm notları hazırlandı. Ad ve Apple alt başlığı 30, Google kısa açıklama 80, uzun açıklama 4.000 karakter sınırında doğrulandı. Apple anahtar kelimeleri UTF-8 100 bayt altında. Yayıncı ve ana dil editörü onayı açık. [Apple uygulama bilgileri](https://developer.apple.com/help/app-store-connect/reference/app-information/app-information/) · [Apple sürüm alanları](https://developer.apple.com/help/app-store-connect/reference/app-information/platform-version-information) · [Google liste alanları](https://support.google.com/googleplay/android-developer/answer/9859152?hl=en)

## İzinler ve doğrulama

Unity Editor batch doğrulamasında 18 Android ikon yuvası, splash ayarı ve manifest kuralının tekrarlı çalışması doğrulandı. PART 19B Gradle export’unda launcher kuralı ile release manifest birleştirmesi çalıştırıldı. Sonuçta izin listesinde VIBRATE kaldı ve INTERNET çıkarıldı. İmzalı AAB/IPA bu aşamada üretilmedi. Gradle birleşimi ve Unity Editor sonucu cihaz doğrulaması yerine geçmez. Görsel ve metin kontrolleri yeniden çalıştırılabilir.

Oyunda mevcut ücretsiz coin sistemi kullanılıyor. Yeni reklam, ücretli ürün, analitik SDK veya sunucu entegrasyonu eklenmedi. Yerel veri akışı, font ve ses lisans izi, olası üçüncü taraf etkileri ve yaş anketi hazırlığı kontrol listesinde.

[Store Assets dizini](../../../Store%20Assets/README.md) · [Görsel ve metin doğrulaması](../../../Store%20Assets/IMAGE_VALIDATION.json) · [Gizlilik taslağı](../../../Store%20Assets/Privacy%20Policy%20Draft.md) · [Uyum kontrol listesi](../../../Store%20Assets/Compliance%20Checklist.md)

## Yayın kapısı

Fiziksel cihaz kabulü, imzalı release AAB ve iOS/TestFlight arşivi, gerçek iPad çekimleri, gerekli yerelleştirilmiş mağaza görselleri, destek ve gizlilik URL’leri, uygulama içi gizlilik bağlantısı, nihai veri/yaş beyanları ve sanat hak onayı açık. Mağazaya dosya veya beyan gönderilmedi.
