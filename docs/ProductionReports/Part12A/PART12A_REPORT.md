# PART 12A — Prosedürel bölüm üretimi kabul raporu

31 Ekim 2026. **Tamamlandı**, `recovery/part09-completion` dalı; Unity 6000.3.27f1 korundu. Başlangıç `e2576d5efccf9650da1e41d7cc08792a3bd381e9`. AGENTS.md, son PART11F teslimi, gerçek oyun modeli/solver, iki authored bölüm, save, güçler ve Pati incelendi. MCP bağlantısı ve mevcut Editor Pipeline doğrulandı.

## Sonuç

Bölüm 1 ve 2 değiştirilmedi. Bölüm 3'ten itibaren talep üzerine, seed/sürüm tabanlı ve gerçek çözüm sertifikalı bölümler hazırlanıyor. Yeni bölüm tamamlanınca bir sonraki kimliğe devam ediliyor; geçmiş authored ID'ler, ödüller, oyuncu kaynakları ve tutorial kayıtları korunuyor.

Algoritma: **gerçek Pour ile geri doğrulanan ters üretim + kanonik, bütçeli BFS**. Ters hamle varsayımı yapılmadı: her seçilen predecessor gerçek ileri hamleyle önceki durumu birebir geri oluşturuyor; bütün ters yol ve BFS çözümü ayrıca oynatılıyor. BFS limit sonucu Unsolvable ile karıştırılmıyor. Limitli veya hedefe uygun olmayan aday gösterilmiyor; 192 doğrulanmış bölümden yedek seçiliyor.

PRNG sabit Xorshift32 ve belgelenmiş uint32 seed karıştırıcıdır. Platforma bağlı Random veya seed olarak GetHashCode yok. Ana planlama seed'i ile kabul seed'i ayrı saklanır. Kabul seed'i + sürüm/tarif, geçmiş olmadan aynı düzeni üretir; bu 1000 sonuçta sınandı. Aynı ana istek için geçmiş bağlamı da aynıysa aynı sonuç çıkar; tekrar filtresi farklı bağlamda başka kabul seed'i seçebilir.

Açılan tanım tam düzeni ve çözüm yoluyla kalıcı olarak sabitlenir. Sonradan yüklerken üretici çağrılmaz. Bozuk ana dosya için yedek; ikisi de bozuksa koruyarak reddetme ve tekrar deneme vardır. Mevcut PlayerPrefs schema version=1 değiştirilmedi.

Tam teknik belge: **`docs/LEVEL_GENERATOR_DESIGN.md`**.

## 1000 kombinasyonluk son kabul turu

| Ölçüm | Gerçek sonuç |
|---|---:|
| Farklı ana seed/bölüm isteği | 1000 |
| Doğrulanmış son bölüm | 1000 |
| Yeni adaydan kabul | 845 |
| Doğrulanmış havuzdan kabul | 155 |
| Sunulamayan/başarısız son bölüm | 0 |
| Yakın 64 içinde kanonik tekrar | 0 |
| Tüm turda farklı kanonik bulmaca | 567 |
| 64 penceresi dışında tekrar | 433 |
| Toplam denenmiş aday | 12457 |
| İstenen boş sayısına uymayan/uygun başlangıç vermeyen aday | 9302 |
| Doğrulanmış ama hedef zorluğa uymayan aday | 1953 |
| Yakın geçmişteki kopya aday | 357 |
| Normal turda BFS limitine takılan aday | 0 |

Normal turda limit oluşmaması limit yolunun sınanmadığı anlamına gelmez: ayrı **1 durumluk bütçe** testinde LimitReached doğrulandı, aday reddedilip havuzdan sertifikalı yedeğe geçildi. Gerçek çözülemez bir model ayrı Unsolvable verdi. Altın PRNG vektörü, sıfır/hatalı parametreler, iki renkli Tutorial, tek boş şişe, gerçek Hint/Shuffle/Undo/Restart, kanonik renk/şişe eşdeğerliği ve aynı kabul seed'inden yeniden üretim test edildi.

Model/algoritma **9013 assertion** geçti (`MODEL_TESTS.json`). Bu 9013 ayrı oyuncu oturumu demek değildir. Bütün kabul edilen çözüm yolları gerçek Puzzle.Pour üzerinden kazanmayla bitirildi. 1000 isteğin tamamında ölçülen kategori hedef kategorisiyle eşleşti; genel fallback API'si bunu her özel bağlam için garanti etmez.

Kalıcı dosya testleri: Save/Load, gelecekteki üretici sürümü numarası taşıyan tam tanımı yeniden üretmeden yükleme, yedek kurtarma ve iki bozuk tanımı rastgele yeni bölümle değiştirmeme geçti.

## Zorluk ve hız

Skor en kısa aktarım sayısı, karışık renk sınırları, renk sayısı, çözüm boyunca alternatif üretken seçenekler ve BFS ziyaret sayısını kullanır. Gözlenen çıkışsız durumlar ayrıca tutulur. **Kesin insan zorluğu veya oyuncunun çıkmaza girme olasılığı iddia edilmez.** Tutorial/Easy/Normal/Hard/Expert eşikleri ve dinlenme bölümlü tempo teknik belgede açıklanır.

| Windows Unity Editor üretim süresi | Son ölçüm |
|---|---:|
| Medyan | 33.39 ms |
| Ortalama | 68.54 ms |
| Yüzde 95 | 268.36 ms |
| En yüksek | 586.55 ms |

Ölçülen kabul edilmiş aramalarda en fazla **850 saklanan durum**, **176 kuyruk node'u** görüldü. Yapılandırılmış üst sınırlar aday başına 6000, istek toplamında 30000 durum ve 32 adaydır. Bu sayılar bütün Unity uygulamasının tepe RAM ölçümü değildir. Arama/üretim tek worker üzerinde; önceden hazırlama var. Unity testinde hazırlık sürerken 11 kare ilerledi; ana thread bir solver döngüsü içinde bekletilmedi. Fiziksel telefon performansı/FPS/IL2CPP kabul testi yapılmadı.

## Gerçek Unity entegrasyonu

`UNITY_TESTS.json`: **242 kontrol geçti**, runtime **0 hata / 0 uyarı**.

- Gerçek authored Bölüm 2 çözülüp Next ile üretilmiş Bölüm 3, sonra Bölüm 4 açıldı.
- Üretilmiş bölümde gerçek şişe aktarımı, Undo, aynı başlangıca Restart, doğrulanmış metinsiz Hint, Shuffle, Ek Şişe, Pause/Resume ve kazanma geçti.
- Açılmış pin yeniden yüklendiğinde aynı kaldı. Test ana/yedek bölüm dosyaları bilerek bozularak Home'da durma, anlaşılır tekrar deneme mesajı ve dosya onarılınca **aynı özgün pin** ile yeniden giriş sınandı.
- Bu hata testi sırasında bulunan eksik PLAY TMP erişimi ve eski tek-tıklama kilidinin hata sonrası kalması düzeltildi. Premium PLAY tasarımı/normal animasyonu değişmedi; yalnız başarısız hazırlıkta geri açma eklendi. Son tur bu yoldan hatasız geçti.
- Pati, yeni kanonik solver ile ilk bölümde bir gerçek örnek hamleyi tamamladı; temel eğitim kaydı ve mekanikler korundu.
- Normal/Zor örneklere erişim için **yalnız QA profilinde** önceki bölüm başarı işaretleri ayarlandı. 1–20'nin tamamının gerçek kullanıcı gibi oynandığı iddia edilmez.

`RELOAD_TESTS.json`: **5 kontrol geçti**. Ayrı Play Mode oturumunda **gerçek Boot → Home → PLAY → prosedürel Bölüm 20** akışı geçti. Kaydın bütün JSON'u ve açılmış bölüm imzası aynı; gereksiz otomatik öğretici yok.

Son derleme: **0 hata / 0 uyarı** (`COMPILE.json`). Son Console canlı tampon ve Editor groundTruth: **0 hata / 0 uyarı** (`CONSOLE.json`). Önizleme guard'ı Unity script/scene uyarısı üretmemesi için kendi dosyasına ayrıldı. Gerçek oyuncu ana kayıt/yedek/bozuk-belge anahtarları test öncesi ve sonrası aynı kaldı. Geçici PlayerPrefs ve test arşivleri temizlendi.

## Gerçek görsel teslim

- `Generated_Easy.png`: Bölüm 3, 3 renk/5 şişe, en kısa 5 aktarım, skor 36.
- `Generated_Normal.png`: Bölüm 8, 3 renk/5 şişe, en kısa 7 aktarım, skor 44.
- `Generated_Hard.png`: Bölüm 20, 4 renk/6 şişe, en kısa 9 aktarım, skor 63.
- Her örneğin `-1950.png` ve `-2000.png` sürümleri: 9:16, 9:19.5 ve 9:20; son oranda Safe Area simülasyonu.
- `Generated_Comparison.jpg`: bu gerçek Unity kareleri yan yana, tasarım metriği başlıklarıyla.
- `EXAMPLES.json`: bu üç gerçek üretilmiş tanım, seed'leri ve sertifikaları.
- `UnityEditor_GeneratedNormal.jpg`: doğrudan Unity Editor Game View penceresi.

Şişeler Safe Area içinde; etkin HUD metinlerinde taşma yok. Beş şişeli Easy/Normal arasındaki farklılık, zorluğun yalnız şişe sayısı olmadığını gösterir. Logo, kedi, fontlar, arka planlar, HUD ve cam/sıvı görselleri değiştirilmedi. Editor gerçek oyuncu kaydına yazmayan Normal bölüm önizlemesinde açık bırakıldı.

## Değişen dosyalar ve koruma

Yeni runtime: `Core/LevelGeneration.cs`, `Core/LevelArchive.cs`, `Resources/Levels/GeneratedFallbacks.json` ve meta dosyaları. Solver şişe permütasyon kanoniği/metriklerle genişletildi. GameplayLevelCatalog, PrototypeApp ve BootLoadingController asenkron hazırlık/kalıcı tanım için bağlandı. MapProgressionCatalog hazırlanmamış yeni kimlikleri kalıcı olarak yanlış önbelleğe almıyor. PremiumPlayButton yalnız güvenli hata sonrası tekrar açılabiliyor.

Yeni QA: Part12AModelTests, Part12AEditor, Part12AProbe, Part12APreviewGuard ve meta dosyaları. Test guard'ları oyuncu derlemesinden hariçtir. Mevcut kurulu Newtonsoft JSON **3.2.2**, runtime arşivi için manifestte doğrudan bağımlılık yapıldı; sürümü yükseltilmedi. Paket kilidinde yalnız dependency depth 1→0 değişti.

Authored Level01/Level02, Puzzle.Pour/Undo/Restart modeli, BoosterActions, oyuncu kayıt şeması/ödüller, Pati UI kodu, karakter prefabı ve animasyonları, onaylı sanat/sahneler/ProjectSettings ve DevControl korunuyor. Hash karşılaştırması `PRESERVATION.json` içindedir. Önceden bulunan başka untracked çalışmalar commit'e alınmadı.

## Bilinen sınırlar

- Mevcut görsel/oyun kuralı kapsamında kapasite 4, 2–4 renk ve 1–2 boş şişe. Varsayılan sürekli akış 3/4 renk ve iki boş kullanır.
- **Matematiksel sonsuz sıfır tekrar yoktur.** Pencere 64; 1000 testte pencere dışında 433 tekrar oldu. Kalıcı tanımlar/başarı listesi ziyaretle büyür; int32 ve cihaz depolaması sonludur.
- İnsan zorluğu kalibrasyonu ve gerçek iPhone/Android performansı henüz yoktur.
- Özel API parametreleri için hedef zorluk her zaman elde edilemez; 192'lik güçlü yedek garantisi varsayılan 3/4 renk–iki boş kapsamındadır.
- Sanatlı harita mevcut 11 numaralı düğüm düzeninde kaldı; Gameplay Next/Resume bunun ötesinde üretimi destekler.

**Yeni iOS/TestFlight/IPA/Cloud Build oluşturulmadı. Master'a merge yapılmadı.**

## Git

Uygulama/test/tasarım/görsel commit’i `ab70130e8243ae11d346dc2a04c00c0183d7aab5`, private origin `recovery/part09-completion` dalına başarıyla push edildi. `git ls-remote` uzak SHA eşleşmesini doğruladı. Master `f243d7a355fedf9cfbc65f8060f1d3bc14cae169` olarak aynı kaldı. Bu teslim kaydı ayrıca dokümantasyon commit’iyle gönderilir.
