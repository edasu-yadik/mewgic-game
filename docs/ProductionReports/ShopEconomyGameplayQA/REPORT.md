# MeWGic mağaza, ekonomi ve oynanış entegrasyon denetimi

Unity 6000.3.27f2. Denetlenen oyun kodu `ec07b9a`. Derleme 0 hata ve 0 uyarı. Oturumlar ayrı QA profilleri ve üretici arşivleri kullandı. Gerçek oyuncunun kayıt yuvaları her koşuda önce ve sonra aynıydı. Bu turda uygulama kodu, sahne ve sanat değiştirilmedi. Yeni mobil build alınmadı.

## Durumlar

| Sistem | Durum | Bulgular |
|---|---|---|
| İlk dört bölüm öğreticisi ve yeni oyuncu | Unity'de çalışıyor ve test edildi | Ana menü sıfır bakiyeli profille açıldı. Pati 1–4. bölümlerde bekleyince göründü. Önerdiği gerçek döküş çözüme açık kaldı, yardım tahtaya kendi başına hamle uygulamadı. [Game View](Tutorials.jpg) |
| Bölüm ilerlemesi ve sonsuz üretim | Unity'de çalışıyor ve test edildi | 1→5, 9→11 ve 99→101. 999→1.000→1.001, 9.999→10.000, 99.999→100.000, 999.999→1.000.000 geçişleri ve yüksek seviye haritası. Üretici denetimi aşağıda. |
| Bölüm sonu, yıldızlar ve coin barı | Unity'de çalışıyor ve test edildi | 1, 2 ve 3 yıldız, Sonraki Bölüm, Tekrar Oyna ve Haritaya Dön. İlk kazanışta gerçek coin uçuşu izlendi. Bar ödüle ara değerlerden ulaştı. [Game View](CoinFlight.jpg) |
| Mağaza, ekonomi ve satın alma hataları | Unity'de çalışıyor ve test edildi | Dört sekme, altın/elmas ayrımı, doğru eksik miktar, negatif bakiye engeli, başarısız kayıtta stok ve bakiye, tekrar istek, animasyon atlama ve azaltılmış hareket. |
| Oyun güçleri | Unity'de çalışıyor ve test edildi | Ek Şişe gerçek 5. bölümde kullanılıp stoktan düştü. 10.001 ve 10.005 seviyelerinde güçler ve şişe yerleşimi denetlendi. Çözücü, renk miktarlarının ve çözümün korunduğunu doğruladı. [Game View](PurchasedBooster.jpg) |
| Ekstra Hamle | Eksik | Hamle sınırı bulunmadığı için kart önizleme olarak sunuluyor. Aktif güç veya teslimat işlemi yok. |
| Günlük görev, altın ve elmas | Unity'de çalışıyor ve test edildi | Altı yıldız görevi gerçek kazanışlarla doldu, mağazadan bağımsız olarak 30 altın ve 2 elmas ödedi. Yeniden talep reddedildi. [Game View](DailyClaim.jpg) |
| Kostüm satın alma, kuşanma ve maskot | Unity'de çalışıyor ve test edildi | Kaşif Kedi yalnız elmasla alındı. Altı kostümün ayrı Play Mode koşusunda ana menü, dünya haritası, bölüm haritası pozları ve oyun içi/zafer görünümü doğrulandı. [Harita](CostumeMap.jpg) · [Oynanış](CostumeGameplay.jpg) |
| Kalıcılık ve bozuk kayıt | Unity'de çalışıyor ve test edildi | Play Mode durdurulup yeni oturumda Boot açıldı. Bakiye, stok, kostüm, görev, işlem ve ilerleme değerleri eşleşti. Bozuk ana dosya izole yedekten kurtarıldı. Eski açık kayıt yazıcısı daha yeni veriyi ezemedi. [Yeniden açılış Game View](RestartGameplay.jpg) |
| Mobil ekran oranları ve cihaz davranışı | Uygulandı ama gerçek cihaz testi bekliyor | Editör’de 900×1600, 900×1950 ve 900×2000 boyutlarında dört sekme, metin/fiyat taşması ve kaydırma kontrolü geçti. Fiziksel iOS/Android ve uygulama sürecini kapatıp açma cihaz testi bekliyor. |
| Gerçek ödeme ve reklam sağlayıcısı | Harici hesap veya sunucu gerekiyor | IAP için test sağlayıcısı ve ödüllü reklam için test geri bildirimi kullanıldı. Apple/Google cihaz sandbox’ı ve reklam ağına bağlı cihaz doğrulaması bu koşuya dahil değildi. |

Uçtan uca akış yeni profilde başladı. İlk dört bölüm kazanışı ve coin uçuşu normal oyunla izlendi. Uzun birikim süresini beklemeden alım akışını sınamak için ardından yalnız QA profilinin altını Ek Şişe fiyatına, elması Kaşif Kedi fiyatından iki elmas aşağıya ayarlandı. Günlük görev kalan iki elması gerçek oyun ödülüyle tamamladı. Oyun içi ve mağaza işlemleri Unity raycast üzerinden çalıştırıldı.

## Son koşular

| Test | Geçen kontrol | Başarısız kontrol | Ham sonuç |
|---|---:|---:|---|
| Mağaza, portre oranları, ekonomi ve yerel IAP | 822 | 0 | [JSON](Results/Shop.json) |
| Zafer ekranı ve reklam geri bildirimi | 384 | 0 | [JSON](Results/Victory.json) |
| Yüksek bölümler ve dosya kurtarma | 348 | 0 | [JSON](Results/Frontier.json) |
| Bölüm ilerleme regresyonu | 437 | 0 | [JSON](Results/Progression.json) |
| Güçleri oynanışta kullanma | 140 | 0 | [JSON](Results/Booster.json) |
| Kostüm ve maskot pozları | 345 | 0 | [JSON](Results/Costumes.json) |
| Yeni oyuncudan kostüm kuşanmaya | 293 | 0 | [JSON](Results/Journey.json) |
| Yeni Play Mode oturumunda açılış | 13 | 0 | [JSON](Results/Restart.json) |

Son koşularda **2.782 kontrol geçti**. Kontroller raycast, kayıt ve durum doğrulamalarını da içerir. Ayrı üretici örnekleminde 2.048 bölümün çözüm sertifikası gerçek döküş kurallarıyla doğrulandı. 1.881 kanonik düzen, 304 yedek üretim ve sıfır ardışık tekrar görüldü. 71 bölüm arşivsiz yeniden üretildi ve birebir eşleşti. [Üretici raporu](Results/Generator.json)

### İlk koşulardaki başarısızlıklar

Beş ilk koşu tamamlanamadı. Üç yeni oyuncu koşusunda test sırasıyla sonuç düğmesinin etkileşim katmanı, bölüm haritasının geri düğmesi ve Gameplay araçlarının görünmesi için beklemiyordu. Bir güç koşusunda test sekme geçişi sırasında alış düğmesine bastı. Yeniden açılış koşusunun 12 durum kontrolü geçti, ancak ekran görüntüsü çıktı klasörü oluşturulmadan istendi. Test kodu gerçek dokunma sırasına ve çıktı doğrulamasına göre düzeltildi. Son koşularda aynı akışlar tekrar geçti.

[İlk yeni oyuncu koşusu](Results/First-Journey.json) · [İkinci koşu](Results/Second-Journey.json) · [Üçüncü koşu](Results/Third-Journey.json) · [İlk güç koşusu](Results/First-Booster.json) · [İlk yeniden açılış](Results/First-Restart.json)

## Görsel karşılaştırma

Solda kullanıcının 11 Kasım'da onayladığı referans, sağda güncel Unity Game View var. Görüntüler 900×1600. Metin, fiyat, CTA ve kaydırma ayrıca 900×1950 ve 900×2000 boyutlarında kontrol edildi.

| Ekran | Güncel Game View | Yan yana | Gözlenen fark |
|---|---|---|---|
| Özel Teklifler | [Unity](Offers-GameView.jpg) | [Karşılaştırma](Offers-Comparison.jpg) | İki ana paket ve üç küçük kart aynı sırada. Gerçek fiyatı olmayan kartlar Önizleme gösteriyor. Örnek fiyat ve indirim etiketleri kaldırılmış. Paket içeriği mevcut güçlere göre güncel. |
| Altın Paketleri | [Unity](Gold-GameView.jpg) | [Karşılaştırma](Gold-Comparison.jpg) | 10.000 altın üst ve alt kartta mevcut. Bonus ve 2×2 kart dizilimi korunuyor. TL fiyatları yerine önizleme durumu var. |
| Oyun Güçleri | [Unity](Powers-GameView.jpg) | [Karşılaştırma](Powers-Comparison.jpg) | Altı satır, ikon ve fiyatlar doğru sırada. Stoklar izole profilin gerçek değerleri. Ekstra Hamle önizlemede, açıklamalar çalışan bulmaca kurallarına göre. |
| Kostümler | [Unity](Costumes-GameView.jpg) | [Karşılaştırma](Costumes-Comparison.jpg) | Üstte kuşanılan kostüm ve altta altı kart 3×2 diziliyor. Sağ üstte referanstaki örnek altın yerine test profilinin elması gösteriliyor. Ürünler elmasla fiyatlanıyor. |

Ortak farklar yazı karakteri, parlama yoğunluğu ve bazı çerçeve boşlukları. Game View altın çerçeveleri referanstan daha yumuşak. Rapor görsel farkları kaydediyor. Bu turda üretim UI veya sanat dosyaları değiştirilmedi. Efektler sekmesi görünmüyor.

## Tekrar çalıştırma

Unity menüsünden **Mewgic → QA → Run Shop Economy Gameplay Journey** seçilir. İlk test durduktan sonra **Resume Shop Economy Gameplay Journey** aynı izole profili ayrı Play Mode oturumunda açar. Testler Game View ve JSON çıktısını `Library/ShopEconomyGameplayQA` altında yazar. Kod değişiklikleri QA senaryosu, yardımcı raycast bekleyicisi ve test çıktı yollarıyla sınırlıdır.
