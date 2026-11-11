# Sonsuz bölüm sistemi ve oyun güçleri

11 Kasım 2026 · Unity 6000.3.27f2 · Windows Editör, 900 × 1600 Game View

## Bölüm ve harita

1.024 sayısı önceki üretim denetiminin örneklemiydi. Oyun 1.000 veya 1.024'te bitmiyordu. 1 ve 2 el yapımı, 3'ten itibaren bölümler prosedürel. Önceki katalog son pozitif `int` kimliğine kadar ilerleme deniyordu. Harita 50'lik adaları daha erken bitiriyordu. İlerleme kaydı da büyüyen JSON'u PlayerPrefs'te tutuyordu.

Katalog, ilerleme ve harita aynı son kimliği kullanıyor: **2.147.483.600**, 50'nin pozitif `int` aralığındaki en büyük katı. 1.000.000. bölüm oyun ve haritada açıldı. Üst sınırdaki bulmaca üretim denetiminde çözüldü. Üst sınırda oynanış veya bu noktaya kadar gerçek bir oyuncu geçmişinin depolanması denenmedi. Kayıt boyutu ve arşiv sayısı oynanan bölüm sayısıyla büyür.

Mevcut bölüm arşivleri öncelikli. 10.000'e kadar mevcut tarif, 10.001'den itibaren arşivlenmemiş kimlikler için v3 kullanılıyor. Kayıt seçili oynanış bölümünü, en yüksek tamamlananı ve kesintisiz açık son bölümü ayrı ele alıyor. İlk kazanış, yıldızlar, ödül ve ilerleme birlikte yazılıyor. Eski bölüm aynı yıldızla tekrarlandığında ek altın verilmedi. Yeni yıldızların mevcut artış ödülü korunuyor. Büyük profiller eski PlayerPrefs kopyalarını tutarak aynı geçici kayıt, ana kayıt ve yedek düzenini dosyaya taşıyor.

Harita 50 bölümden oluşan ada gruplarını gerektiğinde hesaplıyor. Uzak bölgeler mevcut arka plan ve ada görsellerini kullanıyor. 1.000.000. bölümün haritasında beş havuz adası ve 17 etkin nokta ölçüldü. Onaylanmış dünya, maskot ve sonuç ekranı sanat dosyalarında değişiklik yok.

## Üretim ve zorluk

Prosedürel adayın çözümü gerçek maksimum döküş kurallarıyla sınırlı genişlik öncelikli aramada bulunuyor. Kabul edilen hamle dizisi yeniden oynatılıyor ve kazanış doğrulanıyor. Eski ters hamleli tarif, her ters adımı gerçek ileri döküşle doğrular. Süre iptalinde tamamlanmamış bir arama başarılı sayılmıyor. Aday veya durum bütçesi yetmediğinde ilgili tarife, zorluk ve imza dilimine uygun önceden doğrulanmış yedek kullanılabiliyor. 126 tarif ve dilim bileşimi yedek aramaya zorlanarak geçti.

Zorlukta en kısa çözüm uzunluğu, karışık renk sınırları, çözüm yolundaki farklı hamleler, ziyaret edilen durum sayısı, renk ve boş kapasite etkili. v3'te üç/dört renk, iki boş şişe, ada başında rahatlama ve kolay/orta/zor/uzman döngüsü var. Zorluk oyuncu verisi toplamıyor. Aynı tarife ve kimlik aynı düzeni verir. İmza dilimleri yakın bölüm tekrarlarını azaltır. Sonlu şişe ve renk seçenekleri nedeniyle tüm kimliklerin yapısal düzenleri benzersiz olamaz. Örneklemde 7.422 benzersiz düzen vardı, en yakın tekrar 16 bölüm araylaydı.

## Güçler

| Güç | Altın | Kullanım |
| --- | ---: | --- |
| Geri Al | 35 | Geçerli son döküşü geri alır. Döküş yoksa stok harcanmaz. |
| Akıllı İpucu | 40 | Sınırlı çözücüden kazanış yolundaki ilk döküşü alır. İki şişe işaretlenir. |
| Ek Şişe | 60 | Denemeye bir boş şişe ekler. Başlangıçtaki şişe sayısının en fazla bir üstüne çıkar. |
| Karıştır | 50 | Çözümü doğrulanabilen düzende şişelerin konumu döner. Katmanlar ve çözüm uzunluğu korunur. |
| Renk Dönüşümü | 60 | 101. bölümden itibaren seçilen şişenin üst rengini uygun ikinci şişeyle takas eder. Bir denemede bir kez uygulanır. |

Fiyat ve açılma bölümü `Assets/Resources/Economy/Balance.json` üzerinden ayarlanır. Her satın alma bir stok verir. Renk Dönüşümü toplam renk miktarını korur. Takas, daha kısa çözüm kanıtlandığında veya çözümsüz durumdan çözüme dönüş kanıtlandığında uygulanır. Anında kazanış ve süre sınırına takılan adaylar elenir. Geçersiz kullanım stok harcamaz. Başarıda iki şişe kısa parlar ve Geri Al geçmişi yeni döküşlerden başlar. Döküş kaydıyla doğrulanan rekabet akışına Renk Dönüşümü kullanılmış deneme gönderilmez. Sihirli Çekiç uygulanmadı çünkü şişe içeriğini silmek sabit renk miktarı kuralını bozar. Kristal Görüş için gizli renk mekaniği gerekir.

## Kanıt ve tekrar

- [Üretim denetimi](GENERATOR.json) · 10.000 ayrı prosedürel bölüm, 8.482 doğrudan ve 1.518 yedek. İki el yapımı bölüm ayrıca çözüldü. 120 örnekte referans ve hızlı çözücü karşılaştırıldı.
- [Unity Play Mode](PLAY_MODE.json) · 999 → 1.000, 1.000 → 1.001, 9.999 → 10.000, 99.999 → 100.000 ve 999.999 → 1.000.000, yıldızlı sonuç ekranının gerçek Sonraki Bölüm düğmesiyle test edildi. 106. bölümde Renk Dönüşümü 12 hamlelik en kısa çözümü 11'e indirdi. Satın alma, yeniden açılan kayıt, yinelenen satın alma, başarısız kullanım ve kayıt kurtarma kontrol edildi.
- [Eski akışın regresyonu](REGRESSION.json) · 1 → 2 → 3 → 4 → 5, 9 → 10 → 11, 99 → 100 → 101, tekrar oynama, haritaya dönme, hazırlığı yeniden deneme ve kayıttan devam.
- [1.000.000. bölüm](Level1000000.png), [bölüm haritası](Map1000000.png), [mağaza](BoosterShop.png), [Renk Dönüşümü](ColorSwap.png).
- Otomatik Unity tekrarı için `Mewgic → QA → Run Infinite Levels and Boosters`. Elle denemek için `Mewgic → Visual QA → Open Full Game Preview`, Test bölümü **106**, **Oyun**. Üst sırada soldan ikinci şişeyi seç ve **Renk Dönüşümü**'ne bas. İzole önizleme bir deneme stoğu verir.
- .NET tekrarı için `dotnet run --project tools/Part16A/GeneratorAudit.csproj -c Release -- 10000 --frontier --output=Library/InfiniteAudit`.

Cihaz testi, bir milyon bölümün sıfırdan oynanması ve üretim sunucusuyla yeni tarifin uyumu bu denetimin kapsamında doğrulanmadı. 1.000.000 geçişi izole, önceden doldurulmuş profille çalıştırıldı. Android/iOS paketlemesi ve bulut dağıtımı yapılmadı.

Unity Play Mode toplamı 344/344 odak kontrolü ve 437/437 ilerleme regresyon kontrolü. İki çalıştırmada da Console hata sayısı 0 ve asıl PlayerPrefs kayıt özeti aynı kaldı. 106. bölümde elle inceleme penceresi ayrıca etkin düğme ve bir stokla açıldı.
