# PART 16A · Prosedürel bölüm üreticisi

4 Kasım 2026 · Başlangıç `4c9be4f` · Unity 6000.3.27f1 · Üretici v2

## Bulmaca ve doğrulama

`Puzzle.cs` katmanları dipten ağza saklar. Bir döküş, kaynaktaki en üst aynı renk serisini hedefteki boşluk kadar taşır. Hedef boş olmalı veya üst rengi kaynakla eşleşmeli. Kapasite dört katmandır. Kazanışta her şişe boş veya dört katmanlı ve tek renklidir. Başarısız hamle sayılmaz. Üretim sertifikaları güçlendirici veya özel mekanik kullanmaz.

Önceki v1, çözülmüş tahtadan geçerli ters dönüşümler alıyordu. V2, dengeli renk katmanlarını belirlenimli takas veya Fisher–Yates karıştırmasıyla yerleştiriyor. Dolu renk şişeleri ve iki boş şişe standart üretim düzeni. Adayın çözülmüş başlangıç, tekrar dilimi, en kısa çözüm ve zorluk koşulları kontrol ediliyor. Adayların çözülebilir olması karıştırma aşamasında varsayılmıyor.

Çözücü, normal şişe ve renk aralığında durumları iki tamsayıya paketleyen BFS kullanıyor. Şişe sırası eşdeğerliği arama tablosunda birleştiriliyor, çözüm hareketleri özgün şişe indeksleriyle saklanıyor. Paketleme dışındaki girdiler mevcut referans çözücüyle çalışıyor. BFS çözdüğünde başarılı döküş sayısı en kısa çözümdür. 120 tahtada referans BFS ile aynı hareket dizisi, ziyaret, saklanan durum, kuyruk tepesi ve çıkmaz sayısı elde edildi. Kabul edilen her sertifika gerçek `Puzzle.Pour` üzerinden yeniden oynatılıp kazanışla bitiyor.

Aday araması varsayılan olarak en fazla 64 aday, aday başına 6.000 ve istek başına 30.000 durumla sınırlı. Durum sınırı ve çözümsüzlük ayrı sonuçlar. 4.905 örneğinde bir durumluk bütçe gerçekten arama sınırına ulaştı. Doğrulanmış yedek mevcutken sunuldu, yedeksiz çağrı bölüm vermedi. Havuzdaki 487 yeni ve 192 eski kayıt en kısa çözüm ve sertifika açısından yeniden denetlendi.

## Determinizm, tekrar ve uyumluluk

- Bölüm numarası, çakışmasız 32 bit istek seed’ine eşlenir. Seed, Xorshift32 ve aday sırası tamamen tamsayı işlemleridir. Aynı seed, üretici sürümü, istek kuralları, bütçe ve aynı yedek havuzu birebir aynı dizilimi verir. V2 arşiv geçmişini aday seçimine katmaz. 10.000 isteğin tamamı geçmiş ve havuz sırası değiştirilerek ikinci kez üretildi. Kayıttaki tarif seed’i ayrıca kabul edilen tahtayı yeniden üretir.
- Renk ve şişe adlarından bağımsız kanonik imza üç renkli rotayı sekiz, dört renkli rotayı 16 dilime ayırır. Seed dilimi bölüm numarasının aynı modunu taşır. Doğrudan ve yedek tahtalar aynı kuralı izler. V2 tahtaları ilgili dilim mesafesinden daha yakın tekrarlanamaz. Bu sınır eski v1 pinleriyle çapraz eşleşmeyi kapsamaz.
- Önce mevcut arşiv pini okunur. V1 dizilimi veya hamle sertifikası yeniden üretilip yazılmaz. Birincil pin bozuksa doğrulanmış yedek okunur. İkisi de bozuksa işlem kapanır. Daha önce bir tahta atanmamış bölüm yeni sürümden hazırlanabilir. İlerleme kayıt şeması, ilk iki el yapımı bölüm ve ada aralıkları değişmedi.
- Unity Play Mode’da eski v1 pinli 100 aynı tahta ile gerçek Gameplay’e girdi. 601, 1000 ve 5000 için Unity ile .NET arasında ham şişe sırası, katmanlar, seed, imza, yedek seçimi ve hamle sayısı birebir eşleşti. [Uyumluluk kanıtı](UNITY_COMPATIBILITY.json)

## Zorluk

Ölçüm en kısa çözüm, renk ve şişe sayıları, boş katmanlar, renk sınırları, başlangıç seçenekleri ve optimal yol üzerindeki üretken dalları kapsıyor. Puan `3×hamle + 2×karışık sınır + 2×renk + ortalama üretken dal + floor(log2(ziyaret)) + max(0,8−boş katman)`. Dal ortalaması tamsayı bölmesidir. Şişe sayısı ayrı saklanır ve boş katman hesabına girer. Özel mekanik alanı standart üretimde sıfırdır.

El yapımı 1 ve 2, 3 ve 4 hamleyle çözülüyor. Yeni 3–8 örnekleri 3–5 hamle, 9–20 örnekleri 6–9 hamle arasında. Erken üretim için ayrıca üst sınırlar var. Aşama ortalamaları 6,44 → 8,81 → 9,78 → 10,42 → 11,44 → 12,04 hamleye çıktı. Her yeni adanın ilk üç bölümü rahatlama sağlar. Bu yüzden tek tek bölüm puanlarının her adımda yükselmesi beklenmez.

| Bölüm | Ölçülen | En kısa | Renk / şişe / boş katman | Karışık sınır | Başlangıç dalı | Puan | Kaynak |
| --- | --- | ---: | --- | ---: | ---: | ---: | --- |
| 100 | Hard | 11 | 4 / 6 / 8 | 10 | 4 | 72 | Aday |
| 500 | Hard | 12 | 4 / 6 / 8 | 10 | 4 | 74 | Aday |
| 1000 | Expert | 13 | 4 / 6 / 8 | 11 | 4 | 81 | Yedek |
| 5000 | Expert | 13 | 4 / 6 / 8 | 12 | 4 | 84 | Aday |

Tablo, daha önce pinlenmemiş v2 örneklerini gösteriyor. 601, yeni ada girişi olarak üç renk ve dört optimal hamlede doğrulandı.

## 10.000 seed ve Unity sonuçları

- 3–10.002 aralığındaki 10.000 farklı seed’in tamamı teslim edildi. 7.940 doğrudan, 2.060 yedek. Geçersiz veya çözümsüz teslim sıfır. Hedef katmandan daha zor teslim sıfır. En uzun optimal çözüm 15 hamle.
- 7.710 benzersiz tahta, 2.290 yeniden karşılaşılan imza, önceki aynı imzaya en fazla 64 bölüm uzaklıkta 146 tekrar. Ölçülen en yakın tekrar 16 bölüm. İlk 1.000 istekte 948 benzersiz tahta vardı. Eski PART12A denetiminde bu sayı 567 idi.
- 307.421 adaydan 416’sı şekil, 287.290’ı imza dilimi, 11.775’i zorluk koşulunda reddedildi. Bu örneklemde arama sınırı veya çözümsüz aday çıkmadı. İkisi ayrıca zorlanmış testlerle sınandı.
- Masaüstü .NET 10 denetiminde bölüm başına aday çözme süreleri toplamının medyanı 0,87 ms, p95 2,83 ms, p99 3,95 ms. Toplam üretim ve yedek seçiminin medyanı 2,02 ms, p95 6,62 ms. Bu ölçümler paralel masaüstü denetiminden alındı, fiziksel telefon ölçümü yapılmadı.
- Unity’de 4.215 kontrol, 12 ada sınırı, gerçek 601 hazırlama, 1000 Gameplay seçimi ve oyuncu kayıt bütünlüğü geçti. [Unity testi](UNITY_TESTS.json) · [1000 Gameplay](Oynanis_1000.png)
- [Özet ölçüm](AUDIT.json) · [Seed başına sonuç](SEEDS.ndjson). Yeniden çalıştırma komutu `dotnet run --project tools/Part16A/GeneratorAudit.csproj -c Release -- 10000`. `--build-fallbacks` yalnız yedek bankasını yeniden üretir.

## Oyuncu testi gerektiren noktalar

Optimal hamle sayısı, oyuncunun ilk bakışta doğru dalı bulmasını ölçmez. Özellikle 8→9 geçişinin hissi, ada başlarındaki rahatlama, uzman bölümlerde ipucu/tekrar oranı ve uzun oturumlarda benzer tahtaların algılanması gerçek oyuncu oturumlarında değerlendirilmelidir. 10.000 seed denetimi oynanabilirlik ve ölçülen eğri için kanıt sağlar, eğlence veya tüm olası seed’lerde sıfır hata garantisi vermez.

Sahne ve bulmaca kuralları değişmedi. Üretim `LevelGeneration.cs`, arama `PuzzleSolver.cs` ve `PackedPuzzleSearch.cs`, canlı katalog bağlantısı `GameplayLevelCatalog.cs` içinde. Tarihsel denetimler açık v1 tarifini kullanıyor. Yeni bankanın konumu `Assets/Resources/Levels/GeneratedFallbacksV2.json`.
