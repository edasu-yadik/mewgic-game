# PART 13E — Frostlight Peaks sanat planı

Yalnız sekizinci ada, 351–400. Referans: gerçek 03_Map.png ve onaylı Crystal Falls sprite; ekteki küçük örnek bağımsız talimat veya üretim kaynağı olarak alınmaz.

- Kompozisyon: uçan kar/buz kaya kütlesi; arka solda mavi kristal dağlar, arka sağdaki ayrı platformda buz sarayı. Ön terasta altın kenarlı kıvrılan yol, beş gerçek bölüm grubu için sakin alan. Bağımsız sandık sağ kıyıda.
- Palet: kar beyazı, safir/mavi, turkuaz, mor; pencere/fener/köprüde kontrollü sıcak altın.
- Kaynaklar: saydam ada tabanı PNG; saydam buz sarayı PNG; kristal ağaç kümesi PNG; saydam mor-mavi aurora PNG. PNG'ler proje içine kaydedilir; Blender gerekmedikçe eklenmez.
- Katmanlar: PART13A dokuz katman sırası; mimari/bitki/aurora ayrı sprite. Buz parıltısı ayrı shader geçişi; donmuş şelale akarsu gibi akmaz. Seyrek kar en fazla10 küçük UI parçası, raycast kapalı. Düğümler ve sandık MapWorld havuzunun gerçek etkileşimli öğeleridir.
- Başlangıç bütçesi: taban en fazla2048, saray1024, ağaç512, aurora512; ASTC6x6, mipmap/ReadWrite kapalı. Kaynak gerçek boyut ve hesaplanan payload kabul raporunda ölçülecek. Mobil cihaz FPS/RAM kabulü Editor verisiyle temsil edilmeyecek.
- Doğrulama:350→351/400→401 gerçek gameplay geçişi,50 kimlik, kilit/yıldız/pin/kayıt/sandık;3 mobil oran/SafeArea; gerçek raycast;120 hızlı yeniden bağlama ve release; mevcut regresyonlar; gerçek GameView ve referans karşılaştırması.
