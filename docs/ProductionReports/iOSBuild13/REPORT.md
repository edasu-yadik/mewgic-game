# MeWGic 0.8.0 (13) · TestFlight hazırlığı

13 Kasım 2026. Windows üzerinde Unity **6000.3.27f2** ile Xcode projesi üretildi. Kaynak commit `6b9f8f77590e08d4282b18671622876cc20f26ea`, dal `release/0.8.0-rc.1`. Export son onaylı commit'in ayrı kopyasından alındı. Bu kopyada yalnız iOS aday numarası uygulandı ve build raporu üreten geçici yerel araç kullanıldı. Ana çalışma ağacındaki yerel değişiklikler pakete alınmadı. Oyuncu kaydına dokunulmadı.

| Alan | Sonuç |
|---|---|
| Sürüm | 0.8.0 (13). Apple'ın TestFlight e-postası 11'in testçilere açıldığını doğruluyor; 12 yerel export olarak mevcut. App Store Connect'e yetkili oturum olmadığından 13 için canlı çakışma kontrolü yüklemeden önce yapılmalı. |
| Kimlik | `com.edasuyadik.mewgic`, Apple ekip kimliği `VKW7GYV7B9`; iPhone cihaz SDK, ARM64, IL2CPP, Release, minimum iOS 15.0. Development Build, debugger ve profiler bağlantısı kapalı. |
| Oyun | Boot, Home, Gameplay ve LevelMap; son onaylı mağaza, tek harita, prosedürel bölüm üretimi, maskot, günlük görev ve bölüm sonu efektleri. SampleScene dağıtıma girmedi. |
| İkon | Onaylı 1024 kaynak SHA-256 `bcb3f10d1dfbb6d58088b320da1f1780915aa6e154b7aab6e0f95879ae3e0410`. 19 iOS yuvası doğrulandı. Xcode çıktısındaki RGB marketing ikonunun pikselleri kaynakla aynı. |
| IAP | Registry 5.5.0, yedi Consumable ve StoreKit kaynakları pakette. Ücretli paketler önizleme; cihazda canlı veya Sandbox satın alma başlatılamıyor. Apple doğrulama ve test cüzdanı eksik. 883 izole Unity Editor kontrolü önceki aynı kaynak commit'inde geçti. [IAP raporu](../IosSandboxReadiness/REPORT.md) |
| Reklam | Native AdMob köprüsü ve Podfile'da Google Mobile Ads SDK ile UMP bulunuyor. Release'te reklamlar kapalı; gerçek yayın kimlikleri ve HTTPS ödül doğrulaması ayarlı değil. Mac'te bağımlılıklar yine de linklenmeli. |
| Klan, lig, etkinlik | Oyun ekranları ve istemci kodu pakette. Üretim ve cihaz test sunucusu adresleri tanımlı değil. Geçici veritabanlı yerel test modu Unity Editor içindir; iPhone'da çevrimiçi test sunulmaz. Çevrimdışı günlük görevler çalışmaya devam eder. |

Unity build raporu: **0 hata**, 3 uyarı: Unity Services proje bağlantısı, iOS yerelleştirilmiş ana ekran metadatası ve Windows'ta CocoaPods kurulumunun yapılmaması. C# derleme çıktısında ayrıca mevcut `PremiumButton.Start` CS0114 uyarısı var. Addressables kataloğu, IL2CPP verisi, UnityFramework gizlilik bildirimi ve Mac host araçları kontrol edildi. [Export özeti](EXPORT.json) · [Xcode ve ZIP doğrulaması](XCODE_VERIFICATION.json)

Bu hazırlık turunda yeni fiziksel cihaz testi yapılmadı. Kaynak commit'indeki [mağaza Game View](../IosSandboxReadiness/IapTestSuccess.png) ve [tek harita Game View](../SingleMap/Harita-Akisi.jpg) önceki Windows Editor çıktılarıdır.

## Mac'te kalan işlemler

1. `C:\Dev\Mewgic\Builds\iOS\MeWGic_iOS_0.8.0_Build13_Xcode.zip` dosyasını aç. Çıktı `Build13` klasörüdür; Windows'taki orijinal Xcode projesi `C:\Dev\Mewgic\Builds\iOS\Build13\Unity-iPhone.xcodeproj`. ZIP, Burst `DoNotShip` hata ayıklama klasörünü içermez. Unity proje kaynak kopyası, oyuncu kayıtları ve imzalama dosyaları ZIP'e eklenmedi.
2. Mac'te `Build13` içinden `pod install` çalıştır; oluşan **Unity-iPhone.xcworkspace** dosyasını aç. Xcode 26+ ve iOS 26 SDK+ kullan. [Apple SDK koşulları](https://developer.apple.com/news/upcoming-requirements/)
3. [App Store Connect → TestFlight → Build Uploads](https://developer.apple.com/help/app-store-connect/manage-builds/view-builds-and-metadata/) altında 13'ü doğrula. Çakışma varsa arşivden önce daha yüksek uygun numarayı kullan. Kayıtlı Apple ekibini, Apple Distribution sertifikasını ve mevcut bundle için App Store profilini seç.
4. **Unity-iPhone → Any iOS Device (arm64) → Product → Archive**. Organizer'da arşivi doğrula. IPA gerekiyorsa imzalı paketi yerel olarak dışa aktar. iPhone'da açılış, önceki sürümden yükseltme, harita, bölüm sonu, maskot, günlük görev ve mağaza önizlemelerini kontrol et.

Mac'e dosya gönderilmedi, IPA imzalanmadı, App Store Connect'e yükleme yapılmadı. Canlıya geçilmedi. Xcode çıktısı ve ZIP Git dışında; sürüm numarası ve doğrulama belgeleri release dalına aittir.
