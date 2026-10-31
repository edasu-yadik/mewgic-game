# Çözülebilirlik — PART 12D

Mevcut 12A üreticisi **12B WorldMapRules.Request** temposuyla tekrar sınandı. Yeni bölüm havuzu veya üretici algoritması oluşturulmadı.

- 1000/1000 son bölüm doğrulandı; sunulamayan sonuç: 0.
- Yeni aday: 845; mevcut 192 doğrulanmış yedekten: 155.
- 9013 model assertion geçti. Gerçek Puzzle.Pour ile bütün sertifika yolları kazanma sonucuna oynatıldı.
- Yakın 64 imzada tekrar: 0. Tüm turda 548 benzersiz / pencere dışında 452 tekrar. Sonsuza kadar sıfır tekrar iddia edilmez.
- Aynı istek+geçmiş deterministik; kabul seed/sürümünden tekrar üretim aynı. Açılan pin diskten aynı düzende yüklenir. Farklı profillerin farklı yakın geçmişleri aynı bölüm numarasında farklı aday seçebilir; PROFILE_VARIANTS.json bunu iki doğrulanmış Bölüm20 ile gösterir.
- BFS bütçe sınırı ayrı LimitReached olarak sınandı; çözülemezle karıştırılmadı. Hatalı parametreler reddedildi. Genel sınırsız üretim iddiası matematiksel sonsuzluk değildir; int32/depolama ve mevcut 4 kapasite/2–4 renk kapsamı sınırlıdır.
- Gerçek Unity'de 8 kazanma→9 ve 600 kazanma→601 geçişleri; üretilmiş 3/8/20/601'de Hint, Undo, Restart, Shuffle geçti.

Kanıt: MODEL_TESTS.json, SEEDS.json, UNITY_TESTS.json. QA sınır profillerinde önceki başarılar hazırlandı; 1–599'un insan gibi tek tek oynandığı iddia edilmez. 600. bölüm gerçek UI aktarımıyla kazanıldı.
