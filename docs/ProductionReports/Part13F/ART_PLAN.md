# PART13F — Clockwork Isles sanat planı

Yalnız9. ada401–450. Kaynak stil: gerçek03_Map.png ve onaylı Crystal Falls. Ekteki küçük örnek görsel üretim kaynağı veya ek talimat olarak kullanılmaz.

- Kompozisyon: taş/altın uçan kaya, arkada ayrı saat kulesi platformu; küçük atölye terası. Ön kıvrılan rota5 gerçek bölüm grubuna alan bırakır. Ön kayanın yüzünde3 ayrı dişli.
- Palet: sıcak krem taş/altın/pirinç, mor ve mavi kristal enerji, sıcak pencereler; küçük bitkiler masalsı karakteri korur.
- Kaynaklar: saydam taban PNG, bağımsız saat kulesi PNG, bağımsız küçük atölye kümesi PNG, merkez pivotlu önden görünüş dişli PNG. Dişli ortak sprite3 instance; gölge/arka plaka dönen sprite'a gömülmez.
- Katmanlar: PART13A dokuz sıra; düğüm/sandık MapWorld sahibi. Kristal enerji ayrı düşük alpha shader geçişi; birkaç çark yavaş ters yönlü görsel dönüş. Karmaşık fizik veya yeni mekanik yok.
- Başlangıç doku üst sınırları: taban2048, kule1024, atölye512, dişli512; ASTC6×6, ReadWrite/mipmap kapalı. Gerçek boyut/payload son kabulde raporlanır.
- Kabul:400→401/450→451 gerçek UI oyun, kayıt/kilit/yıldız/sandık, görev/navigasyon,3 oran/SafeArea, gerçek raycast,40 düğüm/5 görünüm havuzu/release, sessiz Editor kare ölçümü, alpha ve referans karşılaştırması. Cihaz performansı Editor sonucu olarak sunulmaz.
