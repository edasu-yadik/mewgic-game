# PART 12D — Üretici, harita ve dil entegrasyon kabulü

31 Ekim 2026 · Unity **6000.3.27f1** · recovery/part09-completion

## Sonuç

PART12A–12C kodları, raporları, resmî Localization ayarları, Git ve gerçek Unity sahneleri denetlendi. **Yeni özellik eklenmedi; üretim kodunda düzeltme gerektiren hata bulunmadı.** Yalnız bu entegrasyon için Editor/test araçları ve kanıtlar eklendi. 301 korunan dosya değişmedi; önceden bulunan ilgisiz untracked çalışmalar korundu.

## İstenen 14 kontrol

| Kontrol | Gerçek sonuç |
|---|---|
| Çözülebilir üretim | 1000/1000 doğrulandı, 0 sunulamayan sonuç; gerçek Pour sertifikaları oynatıldı |
| Aynı bölüm | Aynı oyuncunun açılmış pin'i / seed-sürüm / restart aynı. Farklı geçmişli profillerde aynı numara farklı aday seçebilir; küresel aynı-numara garantisi yok |
| Restart | Üretilmiş 3/8/20/601'de başlangıç düzeni ve hamle0 geri geldi |
| Sonraki bölüm | Gerçek kazanma ve Next ile 8→9, 600→601 |
| İlk dünya | 12 ada×50=600 veri yolu; yalnız 5 ada görünümü / 40 düğüm havuzu |
| 600 sonrası | 601 gerçek Gameplay açıldı; güncel bölüme odaklan ile 13. ada paketi erişildi |
| Kilitler | 50/51,100/101,599/600 sınırları; 601 açıkken602 kapalı; kilitli düğüm oyun açmadı |
| Kayıtlar | Gerçek kullanıcı ana/yedek/bozuk kayıt değerleri aynı; tam izole JSON ve pin yeniden açıldı |
| Pati | İlk bölüm gerçek tek örnek hamle+tamamlanma; üretilmiş bölümlerde kısa yardım;601'de otomatik ilk eğitim tekrarı yok |
| Cihaz dili | Resmî seçici kültür simülasyonları TR/EN/DE/pt-PT, desteklenmeyen→en; ilk Boot cihaz/fallback kullandı |
| Manuel dil | Almanca ayrı Boot/Play oturumunda korundu |
| Metin / glif / oran | TR/EN/DE,900×1600/1950/2000,Safe Area;0 taşma/eksik glif/anahtar |
| Hint / Undo / Shuffle | Gerçek üretilmiş şişeler, hak miktarları, geçerli çözüm, orijinal Restart korundu |
| Home / Gameplay / Map / Events | Gerçek runtime düğmeleriyle geçişler geçti; haritadan Gameplay ve geri dönüş korundu |

Harita geri dönüşü mevcut bookmark davranışını korur; yeni güncel bölgeye **Bölümüme Git / My Level** düğmesiyle hızlı odaklanıldı. Bunu otomatik bookmark sıfırlama özelliğine dönüştürmedik.

## Test kanıtları

- MODEL_TESTS.json: **9013 assertion**,1000 kombinasyon;845 yeni/155 mevcut doğrulanmış yedek;548 benzersiz,452 pencere dışı tekrar.
- UNITY_TESTS.json: **19102 assertion**, gerçek Play Mode birleşik akış. Sayımın büyük kısmı görünür metin/glif/yerleşim denetimidir; bu kadar farklı oyuncu oturumu demek değildir.
- RELOAD_TESTS.json: **302 assertion**, ayrı Boot/Play ve601.
- COMPILATION.json ve CONSOLE.json: derleme/çalışma zamanı **0 hata /0 uyarı**.
- PRESERVATION.json:301 korunan dosya; üretim değişikliği0; gerçek oyuncu kayıt/arşivi korundu.
- SOLVABILITY_REPORT.md, SAVE_LOAD_REPORT.md, PERFORMANCE.json: ayrı ayrıntılar.

İlk test aracındaki null yerine JSON boş string kabulü düzeltildi; otomatik dilin manuel seçim olmadığı null/boş eşdeğeriyle doğrulandı. Görsel test profili değiştirirken aktif dil de açıkça seçildi. Bunlar QA düzeltmeleridir; oyun davranışı değiştirilmedi. Son sonuçlar düzeltilmiş tam turdan alındı.

## Performans — yalnız Windows Editor

- 1000 üretimde medyan **22.29ms**,p95 **209.06ms**,maksimum **469.09ms**. Model süresi; telefon performansı değil.
- Gerçek asenkron hazırlık 18.7–139.2ms; ana thread çağrısı 0.12–3.20ms. Worker beklenirken kareler ilerledi; bu bir FPS ölçümü değildir.
- 120 zorlanmış harita yeniden bağlaması **25.28ms**; aynı40 instance / en fazla30 etkin nokta. Dünya bölüm sayısına göre600 nesne oluşturulmadı.
- Dil değişimi ve mevcut tercihin kaydı **0.18–6.20ms**.
- Editor Play Mode Boot→Home **2.82sn**. Editor varlık önbelleği/yerel yükleme içerir; gerçek soğuk telefon açılışı değildir.
- Allocation API 4096byte kontrol tahsisinde de0 bildirdi. Bu nedenle harita için ham0 sonucu “tahsis yok” garantisi sayılmadı. Editor managed heap bütün Editor oturumunu içerir; mobil tepe RAM olarak sunulmaz.
- **Fiziksel iPhone/Android FPS, GPU, RAM, termal ve OS dili testi yapılmadı.** Yeni build alınmadı.

## Gerçek görüntüler

Generated_Comparison.jpg: Bölüm3 Kolay(5 aktarım/skor36),8 Orta(7/skor44),20 Zor(9/skor63). Bunlar ölçülen tasarım kategorileri; gerçek insan zorluğu kalibrasyonu değildir.

Languages_Comparison.jpg: aynı Bölüm8 düzeni Türkçe/İngilizce/Almanca. WorldMap_Comparison.jpg: aynı haritanın üç dil örneği; kısa yeni ada kutlaması geçici olarak görünür. WorldMap_Clean-tr.png kutlama kapandıktan sonraki temiz güncel haritadır. Ham PNG'ler ve Almanca1950/2000 görüntüleri aynı klasörde. Home/Settings/Events/PawHelp,600 sonrası Gameplay/Map ve Reload kareleri de bulunur. Editor_Gameplay.png doğrudan Unity Editor penceresidir.

Görüntüler gerçek çalışan Unity sahnesinin kamera/UI render'larıdır; yapay maket değildir. Karşılaştırmalar yalnız bu kareleri yan yana getirir. Editör, Türkçe Bölüm8 üzerinde **Game sekmesi büyütülmüş / Play Mode / izole test profili** halinde bırakıldı; kullanıcı oyunu oynayabilir.

## Kalan sınırlar

- Yeni sekiz adanın nihai sanatı bekliyor;601+ paketleri yeni ufuklar/yer tutucu yol sunar. Tamamlanmış sanat olarak kabul edilmedi.
- Sekiz dil çevirileri ana dil konuşuru incelemesi bekleyen taslaklar. Bu partta üç dil birleşik kabulü yapıldı;12C'nin sekiz dil testleri ayrıca depoda.
- Matematiksel sonsuz/sıfır tekrar veya farklı geçmişlerde küresel aynı bölüm garantisi yok. Sonlu kurallar,64 imza penceresi,int32 ve depolama sınırları var.
- Cihaz performansı ve fiziksel dokunma/OS yerelleştirme kabulü sonraki mobil beta öncesinde yapılmalı.

## Git

Yalnız PART12D QA/dokümanları recovery/part09-completion dalına commit/push edildi. Kabul commit'i **124af59826e7eccbcf53b4999d5df785c31bcf32**; git ls-remote ile uzak SHA eşleşti. Master **f243d7a355fedf9cfbc65f8060f1d3bc14cae169** değişmedi. Bu teslim kaydı ayrı belge commit'inde gönderilir. Yeni iOS/IPA/Unity Cloud/TestFlight build'i alınmadı.
