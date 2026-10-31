# Mewgic — PART 12A Bölüm Üretici Tasarımı

## Kapsam ve mevcut kurallar

Unity 6000.3.27f1, mevcut geliştirme dalı. Oyun modeli `Puzzle` değiştirilmedi. Şişe kapasitesi **4**; katmanlar alttan üste saklanır. Kaynak üstündeki aynı renkli bitişik blok, hedef boşsa veya üst rengi aynıysa hedefin kalan kapasitesi kadar **azami miktarda** aktarılır. Bir geçerli aktarım bir hamledir. Kazanma: her şişe boş veya kapasitesi dolu tek renkli olmalı.

Mevcut görseller dört rengi ayırt eder: camgöbeği, pembe, altın ve mor. Üretici bu sürümde **2–4 renk**, **1–2 boş şişe**, **kapasite 4** kabul eder. Desteklenmeyen kapasite/renk/sürüm parametresi sessizce başka bir kurala çevrilmez; reddedilir. Görsel palet veya oyun kuralları büyütülmedi.

Authored `Level01.json` ve `Level02.json` ile kimlikleri aynen korunur. Prosedürel akış **Bölüm 3** ile başlar. Undo son gerçek aktarımı geri alır. Restart aynı başlangıcı geri yükler. Hint mevcut solver ile doğrulanmış hamle gösterir. Shuffle sıvı renklerini değiştirmez; şişe konumlarını döndürür ve çözülebilirliği kontrol eder. Yeni sistem bu işlevleri yeniden yazmaz.

## Bileşenler

- `Core/LevelGeneration.cs`: istek/çıktı, PRNG, kanonik imza, sertifikalı ters üretim ve ölçülmüş zorluk.
- `Core/PuzzleSolver.cs`: mevcut BFS'nin şişe permütasyonu eşdeğerlerini tek durumda birleştiren hali. Gerçek Pour kuralı kullanılır.
- `Core/LevelArchive.cs`: bellek/test arşivi, kalıcı değişmez bölüm dosyaları ve son 64 imza okuması.
- `Presentation/GameplayLevelCatalog.cs`: iki eski bölümü korur, yeni tanımı hazırlar, kaydeder ve Gameplay'e verir.
- `PrototypeApp`, `BootLoadingController`: hazırlığı coroutine/Task ile bekler; ana iş parçacığında arama döngüsü çalıştırmaz.

Yeni UI framework'ü, oyun mekaniği, renk, karakter veya tasarım değişikliği yoktur. Mevcut kurulu Newtonsoft JSON 3.2.2 runtime arşivinin doğrudan bağımlılığı olarak sabitlendi; paket sürümü değiştirilmedi.

## Determinizm ve iki seed kavramı

`GeneratorRequest`: bölüm kimliği, sürüm, ana istek seed'i, hedef kategori, kapasite, renk ve boş şişe sayısı. Tekrar filtresi ve sürümlü yedek havuzu da planlayıcının açık bağlamıdır.

PRNG **Xorshift32**: uint32 üzerinde sırayla `x ^= x << 13; x ^= x >> 17; x ^= x << 5`. Sıfır başlangıç durumu `0x6D2B79F5` ile eşlenir. Üst sınırla seçim modulo yanlılığını önleyen rejection sampling kullanır. Seed karıştırıcı sabitleri `0x7FEB352D`, `0x846CA68B`; aday türetme sabitleri `0x9E3779B9`, `0xA17C9E5D`. Tüm taşmalar açık unsigned 32-bit kurala dayanır. `System.Random`, tarih, platform hash'i veya kayan noktalı rastlantısallık kullanılmaz.

Seed=1 altın vektörü: **270369, 67634689, 2647435461**. Otomatik testte karşılaştırılır. Arama ve imza sıralaması `StringComparer.Ordinal`; hash koleksiyonları yalnız üyelik/sayı için kullanılır, gezinme sırasıyla rastgele karar alınmaz.

Tekrarı önlemek bazen başka adaya geçmeyi gerektirir. Bu nedenle:

- **requestSeed**: bölüm numarasından türetilen planlama seed'i.
- **seed**: gerçekten kabul edilen adayın PRNG seed'i. Yedek kullanılırsa yedeğin özgün kabul seed'i korunur.
- **recipeDifficulty**, renk, kapasite, boş sayısı ve sürüm: adayın üretim tarifidir.
- **target**: o bölüm için istenen zorluk; yedekte ölçülen kategoriyle farklı olabilir.

`LevelGenerator.Reproduce(kabulEdilmişTanım)` geçmişten bağımsız olarak aynı başlangıcı tekrar üretir. Aynı kabul seed'i + aynı tarif + aynı sürüm = aynı renk dizileri. 1000 kabul edilen tanım için bu ayrıca sınanır. Ana istek seed'inin **farklı yakın geçmiş bağlamlarında** aynı adayı seçmesi şart değildir; çakışma nedeniyle başka kabul seed'ine geçilir. Aynı istek, aynı geçmiş, aynı havuz/bütçe ile aynı sonuç üretir. Bu ayrım, tekrar önleme ile seed tekrar üretimini birbiriyle çeliştirmeden sağlar.

Android/iOS cihazlarında ikili çalıştırma bu teslimde yapılmadı; platform bağımsız unsigned/ordinal algoritma, altın vektör ve tekrarlı Unity C# testleri doğrulandı.

## Çözülebilirlik: ters üretim + bağımsız BFS

### Neden sıradan geri karıştırma yeterli değil?

İleri hamle azami aynı renk bloğunu aktarır. Rastgele bir parçayı geri taşımak çoğu zaman bu hamlenin gerçek tersi değildir; yalnız ters gibi görünen bir animasyon çözülebilirlik kanıtı sayılmaz.

### Sertifikalı predecessor üretimi

Çözülmüş her renkten bir dolu şişe ve istenen boşlar ile başlanır. Şimdiki durum Q'da A'nın üst rengi c, uzunluğu L olsun. A'dan k adet c kaldırılıp B'ye eklenerek predecessor P denenir. A'nın kalan üstü boş veya c olmalı; B+k kapasiteyi aşmamalı. Ayrıca gerçek ileri miktar:

`min(k + B'nin önceki üst c bloğu, 4 - (L-k)) == k`

olmalıdır. Analitik süzme sonrası seçilen **her** P için gerçek `new Puzzle(P).Pour(B,A)` çağrılır. Aktarılan miktarın k ve çıkan bütün şişelerin Q ile birebir aynı olduğu kontrol edilir. Yalnız bu dönüşümler zincire alınır. Renk miktarları korunur. Son başlangıçta istenen sayıda boş şişe bulunması zorunludur. İç walk aynı şişe permütasyonu durumuna tekrar girmez.

Son adaydan, saklanan ters zincirin uygun prefix'i geri oynatılır; bütün hamleler geçerli ve son durum Won olmalı. Bu, A yönteminin somut sertifikasıdır.

### B yöntemi: bütçeli doğrulama ve en kısa çözüm

Bunun ardından mevcut gerçek kurallarla **BFS** çalışır. Bütün hamle maliyetleri 1 olduğundan ilk kazanma yolu, aranan bölüm için en az aktarım sayısıdır. Sadece kanonik şişe permütasyonu eşdeğerleri atlanır; renkler ve kazanma koşulu değişmez. Her node somut şişe sırasını ve önceki node/hamleyi tutar; bu nedenle geri çıkarılan kaynak/hedef indeksleri gerçekten oynatılabilir.

BFS; A*/IDA*'dan önce seçildi çünkü 2–4 renkli mevcut mobil kapsamda kesin en kısa aktarım sayısı ölçülebiliyor ve node bütçesi küçük tutulabiliyor. A* için admissible heuristic, IDA* için tekrar genişletme/CPU davranışı ek karmaşıklık getirirdi. Bu sürüm büyük renk sayıları için sınırsız BFS iddia etmez.

Sonuçlar ayrıdır:

- `Solved`: gerçek kazanma yolu bulundu ve tekrar oynatıldı.
- `Unsolvable`: kuyruk gerçekten tükendi.
- `LimitReached`: durum bütçesi doldu; **çözülemez demek değildir**. Aday kabul edilmez.

Ters yol olsa bile BFS bütçesini aşan aday bu sürümde oyuncuya gönderilmez; doğrulanmış yedek seçilir. Sertifikalı ters aday için BFS'nin Unsolvable demesi iç tutarsızlık kabul edilir, sessizce geçilmez.

## Zorluk ölçümü ve tempo

Ölçümler: en kısa çözüm uzunluğu, ilk farklı üretken seçenek sayısı, çözüm yolundaki seçeneklerin toplamı/ortalaması, karışık renk sınırları, renk sayısı, BFS genişletme/saklama/kuyruk sayıları ve görülen üretken çıkışı olmayan durum sayısı.

Tam sayı tasarım skoru:

`3 * optimalMoves + 2 * mixedBoundaries + 2 * colors + floor(branchSum/pathStates) + floor(log2(visited))`

Görülen dead-end durumları ayrı gözlem olarak raporlanır; bunlar **oyuncunun çıkmaza girme olasılığı değildir** ve skor için kesin insan risk tahmini gibi sunulmaz. Döngülü bir çıkmaz bileşenini yalnız çıkışsız node sayarak bütünüyle ölçtüğümüz iddia edilmez.

| Kategori | Sürüm 1 ölçütü |
|---|---|
| Tutorial | 2 renk ve en fazla 3 optimal aktarım; gerçek Bölüm 1 authored olarak korunur. |
| Easy | Skor ≤40. |
| Normal | Skor 41–60. |
| Hard | Skor 61–78. |
| Expert | Skor ≥79. |

3–7 Easy; 8–17 Normal. Sonra `Normal, Normal, Hard, Easy, Normal, Hard, Normal, Expert, Easy, Normal` döngüsü kullanılır. Zorların ardından dinlenme bölümleri vardır. Renk/şişe sayısı tek başına etiket değildir: beş şişeli Easy ve Normal Unity örnekleri sırasıyla 5 ve 7 optimal hamle, 36 ve 44 skor taşır. Aynı altı şişe/dört renk ile havuzda Normal, Hard ve Expert örnekleri vardır.

Bu eşikler **ölçülmüş tasarım metriğidir**; oyuncu testleri/telemetrisi olmadan kesin insan zorluğu veya adaptif kişisel zorluk iddiası yoktur. Tüm parametre kombinasyonları her zorluk sınıfına ulaşamaz; bütçe içinde uygun aday bulunamazsa rastgele etiket basılmaz.

## Tekrar önleme ve matematiksel sınır

Bölüm imzasında bütün renk adlandırmaları (en fazla 4! = 24) denenir. Her adlandırmada şişe dizileri ordinal sıralanır; en küçük tam dizi imzadır. Renk adı veya şişe sırası değiştirilmiş kopyalar aynı imzayı verir. Kısa ham dizinin kendisi karşılaştırılır; hash çakışmasına dayalı eşitlik kullanılmaz.

Yeni bölüm, daha önce hazırlanmış en yakın **64 bölüm kimliğinin** imzalarıyla karşılaştırılır. Sınırsız geçmiş RAM'de tutulmaz. İmzalar mevcut arşivden en çok 64 kayıt okunarak oluşturulur. Solver kanoniği yalnız şişe sırasını indirger; bölüm tekrar kanoniği ayrıca renkleri de indirger.

Kapasite, renk ve şişe sınırları sabitken olası bulmacalar sonludur. Güvercin yuvası ilkesi gereği matematiksel sonsuz **sıfır tekrar** mümkün değildir. Garanti yakın 64 pencere içindir. Testte 1000 istekte 567 farklı kanonik bulmaca, pencere dışında 433 tekrar görüldü; bunlar saklanmaz veya benzersizmiş gibi sayılmaz. Uzun vadeli çeşitlilik ve gerçek insan zorluğu ileride kalibre edilmelidir.

Bölüm kimliği mevcut int32 sistemiyle 2.147.483.647'ye kadardır; taşmada sarmalanmak yerine sonraki bölüm durur. “Sınırsız”, bu sürümde tek tek authored içerik olmadan talep üzerine devam etme anlamındadır; gerçek matematiksel sonsuzluk değildir.

## Performans ve doğrulanmış yedekler

Varsayılan deterministik bütçe: en çok **32 aday**, aday başına **6000 saklanan durum**, istek başına **30000 durum**. Bütçe geçersiz/negatif olamaz; üst sınırlar da doğrulanır. Duvar saati, aday seçimini değiştirmez; aksi halde aynı istek farklı cihaz hızında başka bölüm seçebilirdi.

Saf C# arama/üretim/JSON arşiv işi `Task.Run` üzerinde, tek worker kilidiyle çalışır. Aktif profil için en çok iki hazırlık işi vardır; tamamlanan task kayıtları temizlenir. Resources metni ana iş parçacığında alınır, havuzun deserialize/sertifika kontrolü worker'da yapılır. Bir sonraki bölüm Gameplay sırasında önceden hazırlanır. PLAY/Next/Boot coroutine'leri sonuç hazır olmadan sahneye geçmez; kullanıcı arayüzü kare üretmeye devam eder. Hata/IO sorunu oyuncu kaydını değiştirmek yerine tekrar deneme yolunda durur.

192 benzersiz, önceden doğrulanmış bölüm paketlenir: üç renk için 96, dört renk için 96; hepsi iki boş şişelidir. Varsayılan üretim de bu parametreleri kullanır. Son 64 pencere en çok 64'ünü eleyeceğinden her renk grubu için en az 32 kullanılabilir yedek kalır. Böylece normal parametrelerde aday üretimi başarısız olsa da geçerli yeni pencere içi bölüm sunulabilir. Dosya/sertifika bozuksa bu garanti uygulanmaz; bozuk kayıt yerine rastgele bölüm gösterilmez.

Yedekler en yakın ölçülmüş zorluk kategorisine göre seçilir. Eş kategori kalmadığında hedef ve ölçülen kategori farklı olabilir; metadata bunu açıkça taşır. 1000 kabul testinde bu fark oluşmadı. İki renk veya tek boş şişe gibi özel API isteklerinde aynı büyüklükte yedek havuzu garantisi yoktur; doğrulanamayan istek başarısız dönebilir.

CPU süreleri, node sayıları ve gerçek sonuçlar `ProductionReports/Part12A/MODEL_TESTS.json` ve `SEEDS.json` içindedir. Bunlar Windows Unity Editor ölçümleridir; fiziksel iOS/Android performansı veya FPS garantisi değildir. Zaman sınırı yerine deterministik iş bütçesi kullanıldığından cihazda süre farklı olabilir. Hint/Shuffle mevcut senkron bütçeli solver yolundadır; gerçek mobil profil ayrıca gereklidir.

## Kalıcı kimlik ve sürüm uyumluluğu

Mevcut PlayerPrefs belgesi, schema version=1, kaynaklar, tutorial ID'leri, yıldızlar ve ödül işaretleri değiştirilmedi. Yeni dosyalar yalnız **talep üzerine** `Application.persistentDataPath/GeneratedLevels/level-N.json` altında oluşur; sonsuz liste önceden yazılmaz. Prewarm en fazla sıradaki tanımı da sabitler.

Pin; başlangıç şişeleri, tam çözüm sertifikası, seed/requestSeed, üretici sürümü, tarif, hedef/ölçülen zorluk ve imzayı taşır. Dosya önce temp'e yazılıp doğrulanır, sonra ana isme taşınır; doğrulanmış yedek tutulur. Var olan açılmış pin farklı düzenle değiştirilemez. Ana dosya bozuksa geçerli yedek okunur. İkisi de bozuksa veri korunur ve yükleme reddedilir; yeni rastgele bölümle eski kimlik değiştirilmez.

Açılmış bölüm yüklenirken üretici **çağrılmaz**; saklı tam düzen/sertifika yüklenir. Üretici sürümü gelecekte farklı olsa da aynı pin kullanılır. Testte gelecekteki sürüm numarası taşıyan format-1 tam tanım yeniden üretim olmadan yüklendi. Arşiv formatı ile üretici sürümü ayrıdır; desteklenmeyen arşiv/rule formatı güvenle reddedilir. Sürüm-1 tarifini değiştirmek gelecekte yeni üretici sürümü gerektirir.

Bölüm 1–2 authored içerikleri ve tamamlanma kimlikleri korunur. Sonraki bölüm açılmadan pin yazılır; ardından mevcut `SetCurrentLevel` ile ilerleme işlenir. Restart modelin aynı başlangıcını kullanır. Hint/Undo/Shuffle gerçek modele bağlı kalır. Pati yalnız authored ilk bölümde mevcut doğrulanmış hamleyi kullanmaya devam eder.

Disk tanımları ve mevcut tamamlanma listesi ziyaret edilen bölüm sayısıyla büyür; bunlar sonsuz geçmişi RAM'e yüklemez. Çok uzun süreli oyun için depolama sınırı/indeksleme ayrı kapasite işidir. Mevcut sanatlı harita 11 numaraya kadar düğüm içerir; bu görev haritayı yeniden çizmez. Normal Gameplay Next/Resume akışı 11'in ötesinde de prosedürel kimlikleri destekler.

## Test ve teslim kanıtı

- En az 1000 farklı istek seed/bölüm kombinasyonu; aynı bağlamda tekrar üretim, kabul seed'inden bağımsız yeniden üretim ve renk/şişe permütasyon eşitliği.
- Bütün kabul edilen yollar gerçek Puzzle.Pour ile kazanmayla sonlandırılır; Undo/Restart/Hint/Shuffle, hatalı indeks ve parametreler sınanır.
- Gerçek Unsolvable ile LimitReached ayrımı ve zorlanmış limitte doğrulanmış yedeğe geçiş.
- Dosya Save/Load, yedek kurtarma, iki bozuk dosyada yeniden üretmeme ve sürüm pin'i.
- Gerçek Unity Bölüm 2→3→4, kolay/normal/zor örnekler, üç oran, kayıt/yedek koruması ve ayrı Play Mode Boot→Home→Gameplay yüklemesi.

Sayısal kabul raporu: `ProductionReports/Part12A/PART12A_REPORT.md`. Gerçek Unity görselleri aynı klasördedir. Yeni iOS/TestFlight/Cloud Build yok; master'a merge yok.
