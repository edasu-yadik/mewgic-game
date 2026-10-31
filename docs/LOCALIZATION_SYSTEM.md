# Mewgic çoklu dil sistemi — PART 12C

## Resmî paket ve kapsam

Unity **6000.3.27f1** korunur. Resmî `com.unity.localization` **1.5.13**, onun Addressables bağımlılıkları ve TextMeshPro kullanılır. Unity bu paket sürümünü 6000.3 için yayımlamıştır: https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-safe/com-unity-localization . Unity sürümü değiştirilmedi.

Desteklenen Locale kodları: **tr, en, de, fr, es, pt-BR, it, pl**. Her biri gerçek Unity Locale asset'idir. `Assets/Localization/Tables/` altında **Mewgic** String Table Collection, ortak anahtar verisi ve sekiz String Table bulunur. Marka/karakter/arka planlar dile göre değişmediği için Asset Table gerekmez. Görsele gömülü PLAY yerine, aynı özgün çerçeve üzerinde çevrilebilir TMP başlığı kullanılır.

## Tek çeviri kaynağı

`Assets/Localization/Source/Mewgic.json` çeviri düzenleme/üretim kaynağıdır. 157 anahtar × 8 dil, toplam 1256 değer içerir. Dosya oyuncuda ayrı bir çeviri motoru olarak okunmaz: Editor üreticisi bundan **resmî Unity String Table asset'lerini** günceller. Runtime metinleri Unity StringDatabase ve Smart Formatter çözer.

Bütün çeviriler `review: draft` durumundadır. Ana dil konuşuru tarafından doğrulanmamış ilk taslaklardır. Özellikle fantastik ada adları, kısa arayüz ifadeleri ve Lehçe çekimler ana dil incelemesi gerektirir. Kesin profesyonel çeviri garantisi verilmez.

## Başlangıç dili sırası

LocalizationSettings içindeki üç gerçek IStartupLocaleSelector:

1. `SavedLocaleSelector`: mevcut `ProgressLedger.Presentation.manualLocale` tercihi varsa.
2. `DeviceLocaleSelector`: Unity'nin resmî `SystemLocaleSelector` uygulaması; cihazın dili ve uygun üst kültür eşleşmeleri. Portekizce için desteklenen `pt-BR` eşleştirmesi eklenir.
3. `SpecificLocaleSelector`: **en**.

Örnekler: tr-TR → tr, en-US → en, de-AT → de, fr-CA → fr; pt ve pt-PT → pt-BR. Desteklenmeyen dil İngilizceye gider. Başlangıçta otomatik seçilen dil manuel tercih diye kaydedilmez. Sonraki açılışta cihaz dili yeniden değerlendirilebilir; kayıtlı manuel tercih her zaman önceliklidir.

iOS tercih edilen dili ve Android cihaz dilini okuyan yerel kod resmî paketindir; yeni native eklenti veya ikinci bir platform dili yöneticisi eklenmedi. Fiziksel cihazdaki OS çağrıları bu görevde uygulama build'i alınarak sınanmadı. Editor kültür simülasyonları ile gerçek StartupLocaleSelector zinciri ve ayrı Boot/Play oturumları sınandı.

`L.Choose` eşleştirme kurallarının saf test yardımcısıdır; oyuncuda birincil seçici Unity ayarındaki zincirdir.

## Mevcut Settings ve kayıt

Var olan HomeTopBar Settings paneline Dil/Language düğmesi ve sekiz dilin kendi adlarıyla seçici eklendi. Seçim için `L.Select(code)` kullanılır. Bu yöntem önce mevcut ledger ile tercihi kaydeder; yalnız kayıt başarılıysa SelectedLocale değişir. Yazma hatasında eski dil ve oyuncu verileri korunur.

Yeni alan: `PlayerPresentationData.manualLocale`; eski JSON'da yoksa null. Mevcut schema version=1 korunur; MemberwiseClone alanı da taşır. Bölüm, yıldız, para, elmas, güçlendirici, ödül ve tutorial geçmişi sıfırlanmaz. Ayrı PlayerPrefs dil anahtarı veya ikinci bir SaveManager yoktur.

## Runtime metin bağlantısı

Mevcut UI runtime fabrikaları `LocalizedTMP`, yani TextMeshProUGUI alt sınıfı oluşturur. Bu küçük bağlantı resmî tablodaki anahtarı ve parametreleri tutar; locale değişince açık etiketleri, kapalı etiketler yeniden açılınca da güncel dili gösterir. Çevirileri kendisi saklamaz. Sahneler veya oyun nesneleri dil değişimi için yeniden oluşturulmaz.

```csharp
// label, mevcut UI fabrikasının oluşturduğu LocalizedTMP olmalı.
label.text = L.V("gameplay.level", activeLevel);
label.text = L.V("daily.task.bottles", bottleTarget);
```

`L.V` hiyerarşi, sahne, asset yolu veya oyun kimliği değildir; yalnız UI'ya verilen anahtar/argüman isteğidir. Dahili bir ayırıcı ve parametre JSON'u taşır; **oyuncuya doğrudan yazılmaz**. LocalizedTMP setter'ı bunu StringDatabase üzerinden çözer. Normal TMP bileşenine bu isteği doğrudan vermeyin; mevcut fabrikayı kullanın veya Unity'nin LocalizeStringEvent bileşenini bağlayın. Sayısal etiketler ve marka/endonym değerleri düz metin olabilir. Dil değişiminde oyun modelini yeniden başlatmayın.

L.Resolve tablodaki eksik anahtarı `L.Missing` listesine ekler. Locale girdisi eksikse İngilizce fallback kullanılır; İngilizcede de olmayan anahtar teslim doğrulamasından geçmez. Mevcut bütün ekranlarda key marker'larının görünmediği ayrıca test edilir.

Başlangıç `LocalizationBoot` ile asenkrondur; gerekli yerel tablolar hazır olunca Boot normal yüklemeye devam eder. Kaynakların hazır olması bir sahte süre sayacıyla taklit edilmez. Tablolar yerel Addressables gruplarında; uzak katalog kapalıdır. Gelecek normal Player build'inde Addressables içeriğinin de üretilmesi ayarı açıktır. **Bu görevde Player/iOS/TestFlight build'i alınmadı.**

## Parametreler ve dilbilgisi

Sayılar cümleye Türkçe string birleştirmeyle eklenmez. HUD, kazanma özeti, görev sayıları, sandık ödülü, ada adları/aralıkları ve tarihler anahtar + parametre kullanır. Tarihler ve büyük sayı biçimleri seçili Locale kültürüyle işlenir.

Daily görevlerinde resmî Smart Strings plural formatter kullanılır; İngilizce/Almanca vb. tekil-çoğul ve Lehçe one/few/many/other seçenekleri vardır. Türkçede sayıdan sonra isim çoğul eki almaz. 0,1,2,4,5,21,22 gibi değerler runtime'da sınanır. Kazanma özetinde “Hamle: 1 / Yıldız: 1” gibi etiket biçimi, yanlış “1 stars” ifadesini önler.

Pati balonu kısa tutuldu; mevcut gösterim süresi, otomatik gerçek hamle ve kedi davranışı korunur. Yeni mekanik tanımlarında `PawTutorialDefinition.messageKey` kullanılmalıdır. Boş anahtarlı eski/eksik tanım genel yerelleştirilmiş yardım balonuna düşer; mevcut oyunda yeni mekanik eklenmedi.

## Font ve görsel koruma

Masalsı başlık fontu **Cormorant Garamond**, okunabilir gövde/rakam fontu **Noto Sans** korunur. Mevcut MewgicTitle/MewgicBody dosyaları ve depodaki SIL OFL 1.1 lisansları kullanılır; yeni font indirilmedi. Atlaslar tüm çeviri korpusu ve dil adlarının karakterleriyle kontrol edilir; runtime'da yeni karakter üretmeye güvenilmez.

PLAY PNG'leri değişmedi. Küçük UI shader'ı, eski metin bölgesinde aynı sprite'ın boş yeşil yüzünü uzatır; özgün altın çerçeveye dokunmaz. Üstüne Masalsı TMP başlığı yerleştirilir. Basma, nefes alma, dokunma alanı ve sahne geçişi aynı mekanizmadadır. Bu nedenle başlık artık eski resimdeki harflerin piksel kopyası değildir; gerçek çevrilebilir metindir.

İtalyanca dar Home hediye etiketi kısaltıldı; Lehçe Ek Şişe etiketi kısaltıldı. Pati metinleri ve iç metin boşluğu uzun diller için düzenlendi. Settings ve dil seçicinin mevcut mor-altın çerçevesi 9-slice ile ölçeklenir.

## Yeni anahtar ve dil ekleme

1. JSON'a benzersiz anlamlı anahtar ve tüm dillerde değer ekleyin. Parametre numaralarını koruyun; çoğul kurallarını ana dil incelemesinden geçirin.
2. Yeni dil için L.Codes/L.Names dizilerine doğru BCP-47 kodunu ve endonym'i ekleyin. Gerekirse Match bölge eşleşmesini tanımlayın. Varsayılan fallback İngilizcedir.
3. Unity Edit Mode'da `Part12CLocalization.Install()` çalıştırın. Locale, fallback, ortak tablo/entry ID'leri, yerel Addressables, materyal ve font kapsamı güncellenir. Var olan anahtarları gereksiz yeniden adlandırmayın.
4. Non-Latin yeni dil gerekiyorsa mevcut fontun kapsamını ayrıca değerlendirin; lisansı doğrulanmış uygun fallback font olmadan kare kutuları kabul etmeyin.
5. `Part12CLocalization.Validate()` ve aşağıdaki bağımsız aracı çalıştırın.
6. Part12CEditor.Run() ile gerçek ekran/dinamik değer/oyun regresyonu; ardından Part12CEditor.Reload() ile Boot ve kayıt testi yapın. Testler izole oyuncu anahtarı kullanır. Yeni ana dil incelemesini çeviri satırlarının review durumuna işleyin.

## Doğrulama araçları

```text
python tools/validate_localization.py --self-test --output docs/ProductionReports/Part12C/SOURCE_VALIDATION.json
```

Bu araç eksik dil/anahtar, yinelenen anahtar, parametre numarası uyuşmazlığı ve C# içindeki doğrudan L.V anahtar referanslarını listeler. Dört negatif test eksik Fransızca, yanlış Lehçe parametre, yinelenen anahtar ve bilinmeyen kaynak anahtarını gerçekten yakalar. Dinamik ada anahtarları ve Smart Strings dilbilgisi gerçek Unity testinde ayrıca taranır.

Unity Validate, üretilen gerçek tabloların kaynakla eşitliğini ve her iki TMP fontunun bütün çeviri gliflerini kontrol eder. Runtime kabulü tüm görünür TMP etiketlerinde taşma, eksik glif, ham/sabit metin ve görünür anahtar ayırıcılarını denetler. Dil değişiminin oyuncu durumuna dokunmadığı, gerçek aktarım/Undo/Restart/Hint/Pause/kazanma, harita ve yardım akışları test edilir.

Gerçek test sonuçları ve ekranlar: `docs/ProductionReports/Part12C/`. Fiziksel iPhone/Android ve ana dil incelemesi sonraki beta öncesi ayrıca gereklidir.
