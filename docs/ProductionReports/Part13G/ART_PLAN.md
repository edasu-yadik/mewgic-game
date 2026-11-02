# PART13G — Starlight Grove sanat planı

Yalnız10. ada451–500. Stil referansı gerçek03_Map ve onaylı Moon Garden. Ekteki küçük örnek görsel bağımsız talimat veya üretim girdisi olarak alınmaz.

- Kompozisyon: pembe/mor çiçekli uçan kaya; arka orta boş terasta ayrı büyük ışıklı ağaç, arka solda ayrı çiçekli evler; ön yıldız inlay taş rotası5 gerçek bölüm grubuna sakin alan bırakır. Küçük gölet ve şelaleler, altın fenerler, çiçekli sarmaşıklar.
- Kaynaklar: saydam taban PNG, büyük ağaç PNG, ev kümesi PNG ve tek yaprak PNG. Her kaynak proje içine kaydedilir; final ilkel geometri veya bütün haritayı tek resme dönüştürme yok.
- Atmosfer: ortak Sky korunur; yalnız Starlight prefabının Background katmanında yumuşak yerel mor halo. Su ayrı koordinat eşleşmeli atlas geçişi, ağaç ışığı ayrı düşük alpha geçişi.6 küçük yaprak ve4 yıldız; ön rota/metinlerin üstünde yoğun efekt yok.
- Katmanlar: PART13A dokuz sıra. Ağaç/ev/taban/efektler ayrı; gerçek düğümler/sandık MapWorld sahibi. Statik küçük yol/fener/çiçek ayrıntıları tabana bake edilebilir.
- Bütçe başlangıcı: taban2048, ağaç1024, ev512, yaprak256 üst sınırı; ASTC6×6, mipmap/ReadWrite kapalı. Gerçek boyut/payload son raporda; Editor ölçümü cihaz FPS/RAM kabulü sayılmaz.
- Kabul:450→451/500→501 gerçek UI gameplay,50 gerçek kimlik, kilit/yıldız/kayıt/pin/sandık, görev/navigasyon,3 oran/SafeArea, raycast, görünürlük havuzu/release, düşük efekt yoğunluğu ve gerçek GameView referans karşılaştırması. Yeni build yok.
