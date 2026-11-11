# MeWGic mağaza görsel eşleşme denetimi

11 Kasım 2026 · Unity 6000.3.27f2 · Home sahnesi · İzole oyuncu profili

Her yan yana karşılaştırmada sol sütun kullanıcının onayladığı görsel, sağ sütun son Unity Game View karesidir. Tasarım karşılaştırmaları 900 × 1600 ölçüsündedir.

## Özel Teklifler

### Referans görüntüsü

[Özel Teklifler referansı ve yan yana karşılaştırma](Offers-comparison.jpg)

### Gerçek Unity görüntüsü

[Özel Teklifler · 900 × 1600](Offers.png)

### Tespit edilen farklar

Altın kenar ışığı ve seçili sekmenin alt ucu daha sönüktü. Kartların ana boyutları, maskot, fon ve ürün sırası eşleşiyordu. Başlangıç Paketi ile Oyun Gücü Paketi başlık, içerik ve eski/yeni fiyatlarıyla yerindeydi. Ödeme önizlemesi bilgisi ancak düğmeye basınca görünüyordu.

### Yapılan düzeltmeler

Altın çerçeve içi yansımaları, sekme ucu ve başlık şeridi geçişi iyileştirildi. Önizleme durumu CTA üzerinde gösterildi. Paket içeriği, eski fiyat, yeni fiyat ve durum katalogdan karşılaştırıldı.

### Kalan farklar

Referanstaki satın alma çağrısı yerine Paketi Önizle ve Önizle gösteriliyor. Kıvrımlı rozet ve şerit konturları ile yazı ağırlığı yaklaşık.

## Altın Paketleri

### Referans görüntüsü

[Altın Paketleri referansı ve yan yana karşılaştırma](Gold-comparison.jpg)

### Gerçek Unity görüntüsü

[Altın Paketleri · 900 × 1600](Gold.png)

### Tespit edilen farklar

Küçük kartlarda bazı görseller bonus şeridine yaklaşıyordu. Açık fiyat zeminleri referanstan daha yukarıda başlıyordu. Uzun portrede bahçe ile son kart arasında sert geçişli geniş bir alan görünüyordu. 10.000 Altın üst ve alt kartları, %30 bonus ve fiyatlarıyla yerindeydi.

### Yapılan düzeltmeler

Görsel, bonus, fiyat ve CTA alanları ayrıldı. Fiyatlar tek satıra alındı. Uzun portrede bahçe görseli doğal oranını koruyor ve üstünde yumuşak bir ışık geçişi kullanılıyor. İki 10.000 Altın kartı aynı katalog girdisini okuyor ve önizleme açıyor.

### Kalan farklar

Bonus şeridinin kıvrımı ve 10.000 başlığının tipografisi yaklaşık. Referans 9:16 olduğundan daha uzun oranlarda kart boyutları korunarak alt dekor alanı uzatılıyor.

## Oyun Güçleri

### Referans görüntüsü

[Oyun Güçleri referansı ve yan yana karşılaştırma](Powers-comparison.jpg)

### Gerçek Unity görüntüsü

[Oyun Güçleri · 900 × 1600](Powers.png)

### Tespit edilen farklar

Açıklama ve stok rozeti bazı satırlarda birbirine yaklaşıyordu. Referanstaki örnek stoklar izole oyuncunun stoklarından farklı. İki desteklenmeyen ürün düğmeye basılana kadar satın alınabilir görünüyordu.

### Yapılan düzeltmeler

Açıklama ile stok rozeti ayrıldı, rozet genişletildi ve fiyatlar tek satıra alındı. Ekstra Hamle ve Şişe Çekici açıkça Önizle olarak gösteriliyor. Dört kullanılabilir gücün fiyatı ekonomi yapılandırmasından, iki önizleme fiyatı mağaza kataloğundan doğrulandı. Stok ve satın alma sonucu izole kayıtla kontrol edildi.

### Kalan farklar

Karıştır ve Renk Değiştir açıklamaları gerçek oyun mekaniklerini anlatıyor. Önizlemedeki güçler bakiye veya stok yazmıyor. Canlı stok değerleri örnek rakamlardan farklı olabilir.

## Kostümler

### Referans görüntüsü

[Kostümler referansı ve yan yana karşılaştırma](Costumes-comparison.jpg)

### Gerçek Unity görüntüsü

[Kostümler · 900 × 1600](Costumes.png)

### Tespit edilen farklar

Mevcut Kostüm etiketinin iki yanındaki altın yapraklar eksikti. Bazı portreler kart alanında referansa göre küçük görünüyordu. Referansın altın göstergesi ile çalışan mağazanın elmas bakiyesi farklı.

### Yapılan düzeltmeler

Onaylı görseldeki defne yaprakları kullanıldı ve portre alanları genişletildi. Prenses Kedi, Kaşif Kedi, Kristal Kedi, Gece Yıldızı, Orman Perisi ve Kraliyet Kedisi sırası doğrulandı. Altı fiyat ekonomi modeliyle karşılaştırıldı. Bir kostümün yalnızca elmasla alınması, kuşanılması ve kayıttan yeniden açılması doğrulandı.

### Kalan farklar

Üst bar elmas bakiyesini, kuşanma düğmesi gerçek sahiplik durumunu gösteriyor. Fiyat alanının renk geçişi ve yazı ağırlığı yaklaşık. Önceden edinilen ürünlere ait Sahip Olduklarım erişimi kostüm listesinin sonunda bulunuyor.

## Play Mode kanıtı

- [722 kontrolün sonucu](PLAY_MODE.json). Console hata sayısı sıfır. Test öncesi ve sonrası asıl oyuncu kaydı özeti aynı.
- [9:19.5 Game View](Portrait-1950.jpg) ve [9:20 Game View](Portrait-2000.jpg). 9:16, 9:19.5 ve 9:20 oranlarında dört sayfanın fiyat ve metinleri taşma açısından kontrol edildi. Fiyatlar tek satırda. Tekerlek kaydırması yalnızca içerik görünüm alanından uzunsa hareket etti.
- Sekme değişiminde başlık sabit kaldı. Kaydırılmış Kostümler sayfasından Özel Teklifler’e geçerken içerik konumu ve kaydırma hızı ilk karede sıfırlandı. Sonraki karede konum değişmedi. Efektler sekmesi üç oranda da bulunmadı.
- Gerçek para CTA’ları ve iki güç önizlemesi teslimat veya bakiye işlemi yapmadı. Mevcut oyun içi satın alma ve yetersiz bakiye kontrolleri geçti.
- Değişen üretim dosyaları mağaza paneli, mağaza atlası ve atlas bölgeleri. Atlas üretim aracı ve mağaza Play Mode testi güncellendi. Mobil cihaz testi ve yeni mobil build yapılmadı.
