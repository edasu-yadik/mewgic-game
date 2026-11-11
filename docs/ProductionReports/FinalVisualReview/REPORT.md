# Mobil build öncesi son görsel inceleme

## Açık Unity oturumu

`C:\Dev\Mewgic` · Unity 6000.3.27f2 · Denetlenen oyun sürümü `4849608e3c5751f6129173757d1f7e7353c0a2f0`. Yerel başlangıç commit’i GitHub release dalıyla eşleşti. Çözülmemiş Git birleştirme kaydı bulunmadı. Önceden var olan yerel dosya değişiklikleri ve izlenmeyen varlıklar korundu.

**Home.unity → Game View → Özel Teklifler**, 900×1600, Play Mode. İzole profilde 3.400 altın ve 2.700 elmas var. Ek Şişe alındı ve Kaşif Kedi kuşanıldı. Gerçek oyuncu kayıt yuvaları test sonunda aynıydı. [Açık Game View](LeftOpen.jpg) · [Ek Şişe alımı](PowerPurchased.jpg) · [Kostüm kuşanma](CostumeEquipped.jpg)

**Mewgic → Visual QA → Open Full Game Preview** menüsü kontrol penceresini gösterir. Devam eden Play Mode oturumunda aynı önizleme kullanılır. Dört mağaza sekmesi, Büyülü Köy ve Kristal Şelaleler için doğrudan düğmeler var. Test bölümü kaydırıcısı, 1–3 yıldız, üç ekran oranı ve örnek profili sıfırlama seçenekleri pencerede. **Run Final Visual Review** otomatik incelemeyi yeniden çalıştırır ve Özel Teklifler’de bırakır.

Klan, lig ve etkinlik ekranları aynı bilgisayardaki geçici SQLite verisi ve test hesabıyla açıldı. Mevcut yerel sunucunun kullandığı DLL kilitliydi. QA API’si `Library/MewgicVisualQA/LocalBackend` klasörüne ayrı derlendi. Supabase ve Render üzerinde işlem yapılmadı.

## Game View ve sahneler

14 ekranın her biri üç boyutta açıldı. **42 gerçek Game View görüntüsü, 44 başarılı kontrol, 0 Console hatası ve 0 tespit edilen metin taşması**. Derleme 0 hata ve 0 uyarı. Uzun portre mağaza sayfalarında son ürünle alt dekor arasında daha fazla boşluk var. Görsel referans ve tema tasarımları korunuyor.

| Sahne | Açılan ekranlar |
|---|---|
| `Home.unity` | [Ana Menü](Home.jpg), [Özel Teklifler](Offers.jpg), [Altın Paketleri](Gold.jpg), [Oyun Güçleri](Powers.jpg), [Kostümler](Costumes.jpg), [Klan](Clans.jpg), [Lig](League.jpg), [Etkinlikler](Events.jpg), [Günlük Görevler](Daily.jpg) |
| `Gameplay.unity` | [Oyun](Gameplay.jpg), [Bölüm Sonu](Victory.jpg) |
| `LevelMap.unity` | [Dünya Haritası](World.jpg), [Kristal Şelaleler 91–100](Crystal.jpg), [Büyülü Köy 1–10](Village.jpg) |

`Boot.unity` dahil dört etkin build sahnesi referans denetiminden geçti. Bu görsel koşu yukarıdaki üç sahneyi Play Mode’da kullandı.

[9:16 tüm ekranlar](Portrait-1600.jpg) · [9:19,5 tüm ekranlar](Portrait-1950.jpg) · [9:20 tüm ekranlar](Portrait-2000.jpg) · [Play Mode sonuçları](RESULT.json)

## Performans ve asset denetimi

Unity Editör’de 900×1600 Game View’da ekran başına 45 kare örneklendi. İşlemci değeri ana iş parçacığının P95 süresi. Bellek sütunu Editor profilinin ölçtüğü toplam ayrılmış belleği, doku sütunu o anda yüklenmiş dokuları gösterir. Bu değerler aynı oturumda sahne geçişleriyle alınan anlık ölçümlerdir. Cihaz FPS veya bellek bütçesi fiziksel cihazda ölçülmeli.

| Ekran | Ana iş parçacığı P95 | Ortanca çizim çağrısı | Ayrılmış bellek | Doku belleği |
|---|---:|---:|---:|---:|
| Ana Menü | 2,90 ms | 38 | 587 MiB | 219 MiB |
| Oyun | 3,11 ms | 24 | 470 MiB | 102 MiB |
| Bölüm Sonu | 4,11 ms | 48 | 470 MiB | 102 MiB |
| Dünya Haritası | 3,13 ms | 33 | 468 MiB | 101 MiB |
| Kristal Şelaleler | 4,89 ms | 42 | 471 MiB | 101 MiB |
| Özel Teklifler | 2,84 ms | 47 | 606 MiB | 235 MiB |
| Altın Paketleri | 2,94 ms | 48 | 606 MiB | 235 MiB |
| Oyun Güçleri | 2,91 ms | 49 | 607 MiB | 235 MiB |
| Kostümler | 3,05 ms | 51 | 607 MiB | 235 MiB |

- Ölçülen en yüksek ana iş parçacığı P95 değeri Kristal Şelaleler’de **4,89 ms**, en yüksek çizim çağrısı Kostümler’de **51**. Mağazada dokular yaklaşık **235 MiB** ile bu koşunun en yüksek doku ölçümüne ulaştı.
- Etkin sahnelerde çözülemeyen referans görülmedi. Google Mobile Ads’in yalnızca Editor altındaki 15 önizleme prefabı, iki çözülemeyen GUID içeriyor. Bunlar örnek reklam ve onay formu varlıklarına ait. Test reklamı bu koşuda izole QA sağlayıcısından geldi. SDK’nın kendi Editor örnekleri ve gerçek reklam/onay akışı ayrıca doğrulanmalı.
- Sahne, Resources, StreamingAssets, Addressables ve ön yüklenen varlıklar üzerinden yapılan statik tarama 1 MB üzerinde 11 doku adayı verdi. AppIcon ayarlarda referanslı olduğu için listeden çıkarıldı. Geriye kalanlar ağırlıkla 1,36–1,92 MB aralığındaki yerel Home iksir kaynakları ve Editor karakter üreticisinde kullanılan MageCat kaynağı. Bunlar bağımlılık denetimi adayı olarak tutuldu. 3,94 MB `Lit2DSceneTemplate.scenetemplate` ayrı bir Editor şablonu.
- Mağazanın 2048×2048 atlası ve Resources altındaki harita arka planları kullanılıyor. Kaynak PNG boyutu ve Editor doku belleği ayrı ölçümlerdir. [Ham varlık raporu](PROJECT_AUDIT.json)

## iOS ve Android build öncesi açık işler

1. **iOS.** Projede iOS destek modülü kurulu. Son build öncesi yetkili macOS ortamında Xcode 26 ve iOS 26 SDK veya güncel üst sürümü, dağıtım sertifikasını, provisioning profilini ve kullanılmamış build numarasını doğrula. Yerel ayar iOS build 12 ve minimum iOS 15.0. [Apple SDK gereksinimleri](https://developer.apple.com/news/upcoming-requirements/?id=04282026a)
2. **Android.** Android Build Support kurulu. Yerel hedef API 36, min API 26, ARM64 ve versionCode 11. Yetkili imza, sonraki versionCode, AAB ve native kitaplıkların 16 KB hizalaması doğrulanmalı. [Google Play hedef API](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en-MZ) · [Android 16 KB denetimi](https://developer.android.com/guide/practices/page-sizes)
3. **Fiziksel cihaz.** Uzun ekran ve çentik Safe Area, dokunma, animasyon sırasında kare süresi, soğuk açılış, arka plana alma ve kayıt dönüşü iOS ve Android’de denetlenmeli. Mağaza ile harita geçişlerinde bellek ve GPU ölçülmeli.
4. **Servisler.** Gerçek IAP ve reklam için yetkili cihaz sandbox testi gerekiyor. Klan, lig ve etkinlik bu turda yerel API ile doğrulandı. Yerelde kullanılan Unity IAP arşivi ve kayıt deposu çözümlemesi CI ve temiz checkout üzerinde doğrulanmalı. [IAP hazırlık raporu](../StoreIap/REPORT.md)
5. **Görsel onay.** Büyülü Köy mevcut geçici terasıyla gösteriliyor. Kullanıcı onayı bekleyen harita temalarına dokunulmadı.

Yeni mobil build alınmadı. Uygulama ve sanat içeriği yerine Visual QA kontrolleri ile denetim aracı güncellendi.
