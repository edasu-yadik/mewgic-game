# PART 12C — Çoklu dil ve cihaz dili

31 Ekim 2026 · Unity 6000.3.27f1 · recovery/part09-completion

## Sonuç

Resmî Unity Localization 1.5.13 ile **tr, en, de, fr, es, pt-BR, it, pl** tamamlandı. Mewgic String Table Collection: **157 anahtar / 1256 çeviri değeri**. Çeviriler ana dil konuşuru incelemesi yapılmamış ilk taslaklardır; kesin profesyonel çeviri olarak sunulmuyor.

Başlangıç sırası kayıtlı manuel tercih → Unity SystemLocaleSelector ile cihaz → İngilizce. Bölgesel eşleşmeler ve pt-PT → pt-BR desteklenir. Ayarlar/Dil seçimi mevcut panelden anında uygulanır; mevcut ledger içindeki opsiyonel manualLocale alanıyla kalıcıdır. Otomatik cihaz seçimi manuel tercih diye kaydedilmez. Kayıt yazılamazsa eski dil/veri korunur.

Home, Gameplay, harita/ada adları, Daily, Events, Season, Collection, Settings, Pause, kazanma, Pati balonu, güç açıklamaları, Boot ve oyuncu hata/uyarı metinleri resmî tablolara bağlandı. Sayı, tarih, görev ve ödül parametreleri Smart Strings kullanır. Yeni metinleri kaynak koda çeviri cümleleri halinde dağıtan ikinci bir localization motoru oluşturulmadı.

## Gerçek test sonuçları

| Kontrol | Sonuç / kanıt |
|---|---|
| C# derleme | Başarılı, 0 hata / 0 uyarı |
| String Table, kaynak ve glif doğrulaması | 157×8, eksik anahtar/glif: 0; VALIDATION.json |
| Bağımsız kaynak aracı | 185 anahtar çağrısı, 0 hata; 4 bozuk örneği yakaladı; SOURCE_VALIDATION.json |
| Unity Play Mode bütünleşik akış | 132157 assertion geçti; büyük kısmı görünür karakter/yerleşim denetimi; UNITY_TESTS.json |
| Ayrı Boot / Play Mode yeniden açılışı | 328 assertion geçti; RELOAD_TESTS.json |
| Eski JSON / yeni kullanıcı / dil ekleme | 3 kayıt uyumu kontrolü geçti; COMPATIBILITY.json |
| Son Unity Console | 0 hata / 0 uyarı, compilationFailed=false; CONSOLE.json |
| Gerçek oyuncu primary/backup/corrupt | Test öncesi/sonrası aynı; oyun testleri ayrı anahtar kullandı |

Test edilenler: Türkçe/İngilizce/Almanca sistem kültürleri; desteklenmeyen ja-JP → en; Portekizce bölge eşleşmesi; kayıtlı manuel dilin gerçek Boot sonrası korunması; başarısız yazmada geri dönüş; tüm sekiz dilin tüm 157 anahtarının gerçek Smart Formatter ile çözülmesi; 0/1/2/4/5/21/22 çoğulları; dinamik bölüm/hamle/para/ödül/tarih; aktif prefab kilit etiketi ve 3/5=%60; gerçek Play, aktarım, Undo, Restart, Hint, Pause, kazanma ve Gameplay → Home → harita geçişleri.

Tüm ilgili ekranlarda 900×1600 (9:16), 900×1950 (9:19.5), 900×2000 (9:20) ve daraltılmış Safe Area sınandı. Bütün görünür TMP etiketleri taşma, eksik glif, sabit metin ve dahili anahtar ayırıcıları açısından denetlendi. Saptanan uzun İtalyanca/Lehçe etiketler kısaltıldı; Pati balonunun metin boşluğu ayarlandı.

Son yeniden yükleme denetimi, Unity YAML boşluk normalizasyonunun SerializeReference girdilerini bozduğunu ortaya çıkardı. Başlangıç seçicileri ve Smart Formatter uzantıları Unity API'siyle yeniden oluşturulup Unity'nin kendi serileştiricisiyle kaydedildi. Seçici/fallback/Smart Formatter bütünlük kontrolleri doğrulama aracına eklendi. Yukarıdaki sonuçlar **bu onarımdan sonra tekrar alınmış** sonuçlardır. Unity asset YAML dosyalarına dışarıdan boşluk temizliği uygulanmamalıdır.

## Görsel teslim

- Home_8Languages.jpg — sekiz dil karşılaştırması.
- Gameplay_8Languages.jpg — sekiz dil karşılaştırması.
- Settings_8Languages.jpg — sekiz dil karşılaştırması.
- Home-{kod}.png, Gameplay-{kod}.png, Settings-{kod}.png — 24 tam çözünürlüklü Unity kamera çıktısı.
- Almanca 1950/2000 oran görüntüleri ve Daily/Events/Season/Collection/Map/Paw/Win ek kanıtları aynı klasörde.
- Editor_Home_TR.png, Editor_LanguagePicker.png, Editor_Settings_DE.png, Editor_Gameplay_DE.png — doğrudan Unity Editor Game View pencere yakalamaları. Dil düğmeleri gerçek fareyle de kontrol edildi.
- GALLERY.html — 24 ana görüntü için yerel galeri.

Görseller çizim veya UI maketi değil, çalışan Unity UI çıktılarıdır. Kamera çıktıları aynı gerçek sahne/UI üzerinde belirtilen çözünürlükte render edilmiştir. Kontakt sayfaları yalnız bu çıktıları küçültüp yan yana getirir.

## Fontlar ve korunanlar

Mevcut Masalsı **Cormorant Garamond** başlık ve **Noto Sans** gövde fontları korundu. Depodaki SIL OFL 1.1 lisansları kullanıldı; yeni font indirilmedi. Türkçe, Almanca, Fransızca, İspanyolca, Portekizce, İtalyanca ve Lehçe korpusu iki atlasta kontrol edildi. Yalnız ’ ‹ › karakterleri mevcut atlaslara eklendi; eski karakter silinmedi.

Mewgic logosu, kedi, şişeler, arka planlar, oyun kuralları, solver/üretici, ödül verileri ve mevcut bölüm pinleri korundu. PRESERVATION.json içindeki 191 korunan dosya değişmedi. PLAY görselleri değiştirilmedi; metin bölgesi küçük UI materyaliyle boş yeşil yüz olarak gösterilip üzerine yerelleşen TMP başlığı eklendi. Bu başlık eski bitmap harflerinin piksel kopyası değildir; dokunma/nefes/basma davranışı korunur.

## Dosyalar

- Assets/Localization/: sekiz Locale, ortak tablo, sekiz String Table, settings ve tek JSON düzenleme kaynağı.
- Assets/AddressableAssetsData/: resmî yerel Localization içerik grupları; uzak katalog kapalı.
- Assets/Scripts/Localization/: seçiciler, resmi StringDatabase erişimi ve LocalizedTMP bağlantısı.
- Mevcut Boot/Presentation/Daily/Rewards/Map/UI fabrikaları: anahtar/parametre bağlantısı.
- Core/PlayerPresentationData: opsiyonel dil tercihi; BoosterActions: yalnız oyuncu mesajları.
- Prefabs/UI/MagicalProgressBar ve iki mevcut TMP font asset'i.
- Shaders/LocalizedPlayFace ve Resources/Localization/PlayFace.mat.
- Assets/Editor/Part12CEditor.cs, Part12CLocalization.cs; Scripts/Tests/Part12CProbe.cs.
- Packages manifest/lock ve EditorBuildSettings: resmî paket ve settings kaydı.
- tools/validate_localization.py, docs/LOCALIZATION_SYSTEM.md, bu rapor ve kanıtlar.

## Sınırlar / sonraki kabul

- Fiziksel iPhone/Android OS dili çağrıları ve Player/IL2CPP build'i çalıştırılmadı. Editor kültür simülasyonu ve ayrı Boot oturumları geçti. Gelecek cihaz kabulü ayrıca yapılmalı.
- Özellikle fantastik ada adları, kısa arayüz ifadeleri ve Lehçe çekimler ana dil incelemesi bekliyor. Her çeviri review=draft.
- Mevcut font ailesi ve mor-altın tasarım korunur; dil uzunluklarına göre kısa etiket/autosize uygulanır. Otomatik taşma testi ana dil okunurluk incelemesinin yerini tutmaz.
- PART12B'nin bekleyen sekiz ada sanatı bu görevde üretilmedi; onların adları çevrildi.
- Addressables normal gelecek Player build'ine dahil olacak şekilde ayarlandı; bu görevde içerik/Player/iOS/TestFlight build'i başlatılmadı.

## Git teslimi

Yalnız PART12C dosyaları recovery/part09-completion dalına commit/push edildi. Uygulama commit: **0daf1095fd7ca81ce027df07e71d3e6aa2e77857**. Git ls-remote ile uzak SHA eşleşti. Master **f243d7a355fedf9cfbc65f8060f1d3bc14cae169** değişmedi; merge yapılmadı. Önceden var olan ilgisiz untracked sanat/rapor dosyaları korunmuştur.

İlk push GH007 e-posta gizliliği kuralıyla reddedildi. Hesap gizliliği değiştirilmeden, depodaki önceki commit'lerdeki GitHub noreply adresi yerel repo için kullanıldı; henüz yayımlanmamış commit yazarı düzeltildi ve ikinci push başarılı oldu. Bu teslim kaydı ayrı belge commit'iyle eklenir.

**Yeni build alınmadı.** Unity Editor izole test profiliyle Türkçe Gameplay Play Mode'da bırakıldı; bu önizleme gerçek oyuncu verisini değiştirmez.
