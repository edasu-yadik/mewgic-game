# Whispering Woods — kompozisyon ve asset listesi

Yalnız PART13B, 201–250. Referans: gerçek 03_Map.png ve ilk dört onaylı ada sprite'ı. Ekteki küçük konsept resmi üretim referansı yerine kullanılmaz.

Kompozisyon: üst solda büyük mor ağaç, üst sağda kobalt/turkuaz ağaç, aralarında sıcak altın pencereli ahşap ağaç evleri ve ince asma köprü; ön orta açık ve geniş S biçimli yürüyüş terası. Alt kütle sivri mor kaya ve sarmaşıklarla havada asılı. İki küçük turkuaz şelale kaya yüzünden akar; pembe/mor çiçek ve ışıklı altın mantarlar rota kenarında. Ada tek başına transparan, gökyüzü/UI/düğüm/sandık içermez.

Palet: koyu mor #35215B; menekşe #7545CA; kobalt #3E6FC6; turkuaz #38D9D0; altın #FFD27D; çiçek #F396DB. Serin gölge/sıcak pencere dengesi, yumuşak 3D illüstrasyon, referansla aynı ayrıntı seviyesi.

Asset listesi:
- Woods_Island_v001.png: transparan yüksek kaliteli statik taban, mimari, bitki ve dekor birleşimi. Kaynak render/paint korunacak.
- Woods_Mist_v001.png: transparan, hafif ince sis; hareketli ForegroundEffects katmanı.
- Su efekti: ana sprite'ın turkuaz su renklerini seçen ayrı WaterOrSpecialEffects UI bileşeni/materyali, akış ve parıltı shader'ı; çevrenin tamamını dalgalandırmaz.
- Altın ateşböcekleri: düşük adet ortak küçük glow grafiği; kontrollü konum/alpha hareketi.
- Mevcut Node/Chest/Star sprite'ları: ayrı gerçek MapPoint ve Button bileşenleri; yeniden çizilmez.
- WhisperingWoods prefabı: PART13A dokuz katman sözleşmesi, ada başına yerel Addressables yükleme ve serbest bırakma.
- Ada görünür olduğunda kısa opacity/ölçek açılışı; yeni mekanik/tutorial eklenmez.

Önce görüntü üretimi ve alpha incelemesi, sonra yalnız beşinci ada entegrasyonu, gerçek testler ve üç mobil oran karşılaştırması yapılır. Son kalite kabulü gerçek Unity görüntüsüne bağlıdır.
