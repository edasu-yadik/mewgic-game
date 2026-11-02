# PART13I — Aurora Citadel sanat ve devam planı

Yalnız12. ada551–600. Gerçek03_Map ve onaylı Sky Castle stil/kamera referansıdır; ekli küçük örnek üretim girdisi veya ek talimat değildir.

- Kompozisyon: bulut üstü daha görkemli çok teraslı göksel ada, dev mavi/lila kristal kuleler, uzun parlak şelaleler, ışıklı taş köprüler. Arkada ayrı altın/mor kale ve kristal kapı, önde sakin bölüm rotası. Ayrı aurora ve2 küçük yıldız adacığı derinlik verir.
- Kaynaklar: saydam taban PNG, bağımsız kale PNG, mavi/pembe/mor aurora PNG ve küçük yıldız adacığı PNG. Son ada önceki kaynakların tek renk varyantı değildir. Düğümler/sandık sanata gömülmez.
- Efekt: düşük alpha aurora salınımı, eş koordinatlı ayrı su akışı, kale ışığı;4 küçük yıldız ve2 adacık hafif süzülür. Gerçek zamanlı3D/fizik/yoğun bloom yok. PART13A dokuz katman ve5 görünüm/40 düğüm havuzu korunur.
- Bütçe üst sınırı: taban2048, kale1024, aurora512, adacık512; ASTC6×6, mipmap/ReadWrite kapalı. Gerçek boyut/payload son raporda.
- Kritik kabul:550→551 kilit açılması; gerçek599→600→601 ve601→602;601 sonrası yeni harita paketi/sanatı hazır değil davranışı, pin/yıldız/save reload ve yeniden giriş.600'de bitiş ekranı veya yeni sınır eklenmez.
- Diğer kabul:551–600 gerçek50 kimlik, sandık tek ödül/reload,3 oran/SafeArea/raycast, efekt okunurluğu, görünürlük/release, mevcut regresyon, sessiz Editor performansı ve gerçek Unity Game View karşılaştırması. Yeni build yok.
