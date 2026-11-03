# PART 15D · Canlı harita ve büyülü yolculuk

4 Kasım 2026 · Unity 6000.3.27f1 · Başlangıç `201153b`

## Yolculuk

Başarılı ilk kazanış sıralı ilerlemenin sınırını açtığında kayıt defteri bir defalık oturum içi sunum hedefi hazırlar. Haritaya dönüşte bu hedef tüketilir. Eski bölüm tekrarında yeni hedef oluşmaz. Ödül, yıldız ve erişim animasyon başlamadan mevcut atomik kayıt üzerinden tamamlanır. Sunum ödül vermez ve kayıt şemasına yeni alan eklemez. Oyuncu doğrudan ada detayını açarsa kedi, o adadaki güncel taşın yakınında durur.

Yaklaşık 1,8 saniyelik akışta önceki taş parlar, ışıklar sırayla ilerler ve kedi onları izler. 91–100 eğrileri çizimdeki yol merkezinden ölçülen ek ara noktaları kullanır. Diğer rotalar mevcut taş merkezleri arasından geçer. Onaylı kedi dokusunun küçük bir mesh’i patiler ve kuyruk bölgesini deforme eder. Gövde eğimi, adım sekmesi ve varış sıçraması bu dokuya eklenir. Bu, mevcut oturan karakter sanatından üretilen stilize bir yürüyüş. Ayrı çizilmiş bir yürüyüş döngüsü bulunmuyor.

Hedef taş açılışa kadar kilitli görünür. Varışta geçici kilit kalkar, taş kısa süre büyür ve altın-mor halka ile yörünge yıldızları görünür. 100 daha büyük kilometre taşı ve daha geniş yıldız yörüngesi kullanır. Kamera aynı ScrollRect üzerinde kediyi sönümlü biçimde izler. Elle kaydırma sunumu bırakıp kontrolü kullanıcıya verir. Farklı adalarda kedi mevcut rotanın çıkışına gider, kısa bir perde geçişiyle hedef adanın girişinden devam eder. Adalar arasında yeni bir fiziksel köprü çizilmedi.

## Ortam ve hareket tercihi

Su akışı, kristaller ve fenerler var olan malzeme üzerinden çalışıyor. Çiçek ve yaprak renk bölgelerine küçük bir doku salınımı eklendi. Hareket seçeneği ada detayında, sekiz dilde yerelleştirilmiş. Az hareket açıkken hedef doğrudan görünür, kamera takibi ve yürüyüş atlanır. Tercih cihaz ayarı olarak saklanır. Düşük bellekli cihazlarda geçiş parçacıkları üç ile sınırlandırılır. Efekt bütçesi bileşenden ayarlanabilir. Varsayılan sekiz geçiş parçacığı bir kez oluşturulur. Doku pozu yalnızca geçiş sırasında, görüş alanına yakınken en çok 24 Hz güncellenir. Mevcut bölüm taşı havuzu kullanılır.

## Unity Play Mode

- [Yolculuk denetimi](UNITY_TESTS.json) 46 kontrol, hata veya uyarı yok. 91→92, 99→100 ve 100→101 gerçek bulmaca kazanışlarıyla doğrulandı. 91 tekrarında bekleyen hedef oluşmadı. Kamera takibi, elle kaydırma, 92 raycast’i, az hareket ve geçici kilit doğrulandı.
- Animasyon sürerken sahne kapatıldı ve ayrı PlayerPrefs test profili diskten açıldı. 101, doğru ada odağı ve coin bakiyesi korundu. Bu test, işletim sistemi seviyesinde uygulamayı sonlandırma testi değildir.
- 50→51 ve 600→601 mevcut kayıt defterinden ilk kazanış kaydıyla sınandı. 600 sonrası paket 12 ve hedef ada doğrulandı. Bu iki sınır testinde bulmaca oynanmadı.
- [Harita regresyonu](MAP_REGRESSION.json) 8.962 kontrol, en fazla 12 etkin taş, oyuncu kayıt alanları aynı. 1–650, portre yerleşimleri, kaydırma, dokunma, yıldızlar, 100 Gameplay ve sonuç ekranı tekrar denetlendi.
- İlk denemelerde sahne yenilenirken test yardımcısının tekrar kurulması ve aynı sahne için bekleme koşulu düzeltildi. Paylaşılan yerelleştirme tablosunun girintisi de düzeltilip canlı Türkçe etiket ve sekiz dilin kaynak doğrulaması tekrar kontrol edildi.
- Fiziksel cihazda işlem sonlandırma, dokunma gecikmesi, FPS, GPU ve termal performans ölçülmedi. Yeni build veya paket eklenmedi.

## Game View kareleri

[91→92 yürüyüş](Walk_91_92.png) · [100 açılışında altın halka ve yıldızlar](Unlock_100.png) · [101 hedef adaya giriş, varış öncesi](Island_101.png)

Bu kareler gerçek 900×1600 Game View render hedefinden, animasyon sürerken alındı. DX12 render hedefi yönü düzeltilerek PNG’ye yazıldı.

## Değişen dosyalar

- Harita ve animasyon · `MapWorldJourneyMotion.cs`, `MapWorld.cs`, `MapWorldJourney.cs`, `MapWorldCrystalJourney.cs`, `MapWorldProgression.cs`
- Kayıt ve oynanış bağlantısı · `BoosterActions.cs`, `GameplayLevelCatalog.cs`, `PrototypeApp.cs`
- Ortam · `MapWaterLight.shader`
- Yerelleştirme · `Mewgic.json`, Shared Data ve sekiz dil tablosu
- Editor denetimi · `Part15DJourneyProbe.cs`

Mevcut `LevelMap.unity` ve Gameplay sahneleri üzerinden çalışır. Sahne varlıkları ve mevcut teras çizimleri değişmedi.
