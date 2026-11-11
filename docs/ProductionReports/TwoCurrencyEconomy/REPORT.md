# Altın ve elmas ekonomisi

## Denetim ve ayarlar

Kayıt zaten ayrı `long` altın ve elmas bakiyeleri tutuyordu. Mağaza güçleri altınla, kostümleri elmasla satıyordu. Satın alma fiyatı sunulan fiyatla karşılaştırılıyor, bakiye ve stok/sahiplik tek kayıtla değişiyordu. Ancak ilerlemede düzenli bir ücretsiz elmas kaynağı yoktu. Eski bakiye, stok ve kostüm kayıtları taşınmadan açılır. Önceden alınan ödüllere geriye dönük ödeme yapılmaz. Eski bir bölümün yıldızını artırmak yalnızca altın farkı kazandırır.

[Aktif ekonomi yapılandırması](../../../Assets/Resources/Economy/Balance.json) başlangıç stoğunu, ödülleri, reklam çarpanını ve altın/elmas fiyatlarını içerir. Yerel sezon, haftalık etkinlik ve günlük görev eşikleri mevcut kurallardan gelir. TL cinsinden paketler mevcut katalogdaki önizlemelerdir. Gerçek ödeme veya ürün teslimatı etkinleştirilmedi.

| Kaynak | Altın | Elmas | Kural |
|---|---:|---:|---|
| İlk bölüm galibiyeti | 20 + yıldız başına 5 + ilk denemede 10 + ipucu kullanılmadıysa 5 | — | 25–50 altın ve 1 ipucu |
| Eski bölümde yeni en iyi yıldız | Yeni yıldız başına 5 | — | Aynı yıldız sayısı yeni bölüm ödülü vermez |
| İsteğe bağlı reklam | Normal ödülün 5 katı toplam | — | Onaylı tamamlama sonrası yalnızca 4 kat fark eklenir |
| Günlük hediye | 100 | — | 24 saatte bir ve 1 ipucu |
| Günlük görevler | 15 / 25 / 30 / 10 | Yıldız görevinde 2 | 4 şişe, 2 galibiyet, 6 yıldız ve 1 güç kullanımı, her görevden 1 ilgili güç |
| Haftalık etkinlik | 60 | 12 | 3 galibiyet, haftada bir talep, ayrıca 1 Karıştır |
| 28 günlük yerel sezon | 30 / 45 / 75 | 15 / 25 / 40 | 2 / 4 / 6 galibiyet, her kademede bir talep ve 1 ilgili güç |
| İlk 20. bölüm | Normal bölüm ödülü | 40 | İlk kazanış |
| Her 25. bölüm | Normal bölüm ödülü | 20 | İlk kazanış |
| Her 100. bölüm | Normal bölüm ödülü | İlave 45 | İlk kazanış, 25. bölüm ödülüyle birlikte 65 |

Yeni profil 0 altın, 0 elmas, 3 Geri Al, 3 İpucu, 1 Ek Şişe ve 1 Karıştır ile başlar. Ayrıca her ilk galibiyet bir ipucu verdiği için ilk bölümlerde satın alma yapmadan da güç kullanılabilir. Renk Değiştir ve Şişe Çekici 101. bölümde açılır. Ekstra Hamle önizlemede kalır. Oyun güçlerinin ve kostümlerin mevcut fiyatları korundu.

## Örnek oyuncular

Rahat profil günde 4 galibiyet, ortalama 2,2 yıldız, %55 ilk deneme ve %70 ipucusuz oyun varsayar. Uygun günlük ve sezon ödüllerinin %75'i, haftalık ödüllerin %75'i ve güç kullanım görevinin %40'ı alınır. Düzenli profil günde 10 galibiyet, ortalama 2,7 yıldız, %80 ilk deneme ve %85 ipucusuz oyun varsayar. Uygun günlük, haftalık ve sezon ödüllerinin tümü, güç kullanım görevinin %70'i alınır. Dört şişe görevi iki galibiyetle tamamlanmış kabul edilir. Başlangıç ilk Pazartesi ve sezonun ilk günüdür. Dönemler mevcut 7/28 gün kurallarına göre sayılır.

Bunlar harcama öncesi tahminlerdir. Reklam, eski bölüm yıldız iyileştirmesi, gerçek para ve çevrimiçi ödüller hesap dışında. Gerçek oyuncu telemetrisi kullanılmadı. Oyun günü atlanırsa veya ödüller alınmazsa takvim süresi artar.

| İlk kazanılan bölüm sayısı | Rahat gün | Rahat altın / elmas | Düzenli gün | Düzenli altın / elmas |
|---:|---:|---:|---:|---:|
| 20 | 5 | 1.615 / 116 | 2 | 1.479 / 136 |
| 100 | 25 | 7.580 / 298 | 10 | 6.615 / 289 |
| 500 | 125 | 37.810 / 1.314 | 50 | 32.505 / 1.021 |
| 1.000 | 250 | 75.508 / 2.529 | 100 | 64.950 / 1.990 |

| Güç | Altın fiyatı | Görev ve hediyelerle, rahat / düzenli | Yalnız bölüm altını, rahat / düzenli |
|---|---:|---:|---:|
| Geri Al | 35 | 1 / 1 galibiyet | 1 / 1 galibiyet |
| Ek Şişe | 800 | 11 / 13 galibiyet | 20 / 18 galibiyet |
| İpucu | 1.000 | 14 / 16 galibiyet | 25 / 22 galibiyet |
| Karıştır | 1.200 | 16 / 19 galibiyet | 30 / 27 galibiyet |
| Renk Değiştir | 1.500 | 20 / 24 galibiyet | 38 / 33 galibiyet |
| Şişe Çekici | 1.800 | 24 / 28 galibiyet | 45 / 40 galibiyet |
| Ekstra Hamle | Satış kapalı | — | — |

Alım sıklığı, 1.000 bölümdeki ortalama brüt gelirin fiyata bölünmesinden türetilir. Diğer harcamalar, eldeki ücretsiz stok ve seviye kilidi bu aralığa dahil değildir.

| Kostüm | Elmas fiyatı | Rahat gün / bölüm | Düzenli gün / bölüm |
|---|---:|---:|---:|
| Büyücü Kedi | 0 | Başlangıç | Başlangıç |
| Kaşif Kedi | 900 | 86 / 342 | 45 / 450 |
| Prenses Kedi | 1.200 | 114 / 454 | 60 / 600 |
| Orman Perisi | 1.200 | 114 / 454 | 60 / 600 |
| Kristal Kedi | 1.500 | 148 / 591 | 78 / 775 |
| Gece Yıldızı | 1.800 | 175 / 700 | 90 / 900 |
| Kraliyet Kedisi | 1.800 | 175 / 700 | 90 / 900 |

Kostüm satırları sıfır bakiyeden tek kostüm için hesaplanır. Aynı bölüm sayısına daha yavaş ulaşan oyuncu, daha fazla günlük ve haftalık döngüye katılabilir. Önceki sürümden sahip olunan veya ödüle bağlı kozmetikler mevcut katalog kurallarıyla çalışır.

## Kayıt ve doğrulama

Bakiye değişiklikleri yerel işlem kimliği, kaynak ve iki para biriminin değişimiyle aynı ilerleme kaydına yazılır. Son 32 işlem teşhis ve gelecekteki entegrasyon için saklanır. Çift ödül kontrolü bölüm ve ödül haklarıyla, çift satın alma kontrolü mağaza istek numarası ve sahiplikle yürür. Sunucu doğrulaması ve tam mutabakat kuyruğu gelecekteki entegrasyona bağlıdır. Çevrimiçi ekonomi ve cihazlar arası senkronizasyon bu testin kapsamı dışındadır.

## Unity kanıtı

Unity 6000.3.27f2 derlemesinde 0 hata ve 0 uyarı. İzole profil [Play Mode raporunda](PLAY_MODE.json) 772 kontrol geçti ve çalışma sırasında gerçek oyuncu kaydının beş kayıt yuvası aynı kaldı. Mağaza ile günlük, haftalık ve sezon ödülleri 900×1600, 900×1950 ve 900×2000 Game View boyutlarında kontrol edildi. Bölüm 20 gerçek bulmaca kurallarıyla çözüldü ve zafer ekranında 50 altın ile 40 elmas göründü. Kayıt hatasında altın, elmas, stok, sahiplik ve işlem geçmişi korunuyor. Reklam doğrulaması kayıt tarafındaki tamamlanma bildirimiyle yapıldı. Gerçek reklam sağlayıcısı, gerçek ödeme, mobil cihaz ve canlı servis kapsam dışıydı.

- [Altın eksikliği](GoldShortfall.jpg)
- [Elmas eksikliği](GemShortfall.jpg)
- [Günlük yıldız görevi](DailyGemReward.jpg)
- [Haftalık etkinlik](WeeklyGemReward.jpg)
- [Sezon ödülleri](SeasonGemReward.jpg)
- [20. bölüm galibiyeti](VictoryGemReward.jpg)
