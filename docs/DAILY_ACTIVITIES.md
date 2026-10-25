# Günlük görev ve hediye kuralları

PART07A; yalnız yerel, çevrimdışı günlük sistem.

## Günlük görevler

- Kazanılan bölümlerde4 dolu/tek renkli şişe tamamla → +1 Geri Al.
- İki bölüm kazan → +1 İpucu.
- Kazanılan bölümlerde6 yıldız kazan → +1 Ek Şişe.
- Bir güçlendirici kullan → +1 Karıştır.

Şişe/hedef sayımı ancak gerçek kazanma sonrasında yapılır; kısmen tamamlanmış şişeyi Undo/Restart ile tekrar tekrar saydırma yoktur. Yıldızlar her gerçek koşunun HUD ödülüdür; en iyi yıldız farkı değildir. Yeni ve tamamlanmış seviyelerin gerçek tekrar kazanımları sayılır. Her denemenin GUID makbuzu vardır; aynı deneme bir günde yalnız bir kez sayılır. Kazanma ilerlemesi ve günlük sayaçları aynı ProgressLedger Commit belgesine yazılır. Başarılı güçlendirici harcaması ve görev ilerlemesi de aynı belgededir. Geçersiz işlem, satın alma veya UI'ya yalnız basma görev ilerletmez. Sayaçlar hedefte durur.

Görevler yerel gece yarısında yenilenir; önceki günün alınmamış görev ödülleri sona erer. Alınmış işareti ve gerçek güçlendirici artışı atomik olarak aynı kayda gider. Ödül almak güçlendirici kullanmak sayılmaz.

## Günlük hediye ve saat

Hediye+1 İpucu hakkıdır. İlk gün hemen alınabilir. Sonraki hediyede hem yeni yerel gün hem son alımdan en az20 saat UTC süresi gerekir; yerel gece yarısından birkaç dakika sonra tekrar alınamaz. Atlanan günler birikmiş hediye üretmez.

Son görülen en büyük yerel gün ve UTC zaman damgası saklanır. UTC saatinin5 dakikadan fazla geri gitmesi veya yerel günün geçmişe gitmesi günlük kazanım/ödülleri durdurur. Normal bulmaca ve güçlendirici kullanımı devam eder. Saat kayıtlı eşiğe yetişince günlük işlemler düzelir. Geçmiş güne dönen saat dilimi değişimi de bu tedbire takılabilir.

Bu bir sunucu güvenliği/anti-cheat sistemi değildir. PlayerPrefs düzenlenebilir ve sistem saati ileri alınabilir. Güvenilir sunucu saati, imzalı kayıt veya çevrimiçi ödül doğrulaması eklenmedi. İleri tarihe geçip sonra normal saate dönmek kayıtlı geleceğe yetişene kadar günlük işlemleri kilitleyebilir; kayıt silinerek otomatik sıfırlama yapılmaz.

## Kayıt uyumu

Mevcut Mewgic.Progress.v1 anahtarı korunur. Ek daily nesnesinin kendi version=1 alanı vardır. Eski kayıtta daily yoksa sıfır günlük durum eklenir; bölüm yıldızları, haklar ve claimedMilestones korunur. Bozuk/uyumsuz kayıt mevcut hata korumasıyla okunamaz kabul edilir; özgün anahtar üzerine yazılmaz. Yeni daily verisi geçersiz sayaç/ödül kombinasyonlarında reddedilir.

Home Daily Gift/Missions girişleri aynı gerçek ekranın ilgili sekmesini açar. GİT mevcut doğrulanmış başlangıç bölümünü açar. Hediye ve görev ekranları ayrı aktif UI katmanlarıdır; tek bir ekran resmi değildir. Onaylı WizardCat prefabı ve mevcut yıldız/güçlendirici ikonları yeniden kullanılır. Yeni parşömen, mavi düğme ve açık sandık ayrı Sprite varlıklarıdır. Sandığın içindeki mücevherler dekoratiftir; kazanılan ödüller satırdaki gerçek hak ikonları ve+1 değerleridir.

## Test

DailyActivitiesTests.Run(), model/saat/sınır koşulları, benzersiz test PlayerPrefs anahtarında yeniden açma ve gerçek Home→görev→Gameplay→kazanma→Home→ödül akışını izole profilde çalıştırır. Gerçek oyuncu kayıt anahtarının değişmediğini kontrol eder. Saat testlerinde enjeksiyon kullanılır; bilgisayarın sistem saati değiştirilmez.
