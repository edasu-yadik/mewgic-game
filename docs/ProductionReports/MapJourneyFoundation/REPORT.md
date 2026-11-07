# Dünya haritası · ortak altyapı ve Kristal Şelaleler pilotu

## Görsel yön ve hareket

- Kristal Şelaleler için boyanmış vadi, uzaktaki kristal saray, kenar şelaleleri ve su havzası kullanıldı. Taş köprü dokusu, yuvarlak düğüm cepleri ve mor-altın kenarlar ortak rotaya uygulandı. Referanstaki el yapımı terasların kompozisyonu ileride dünya bazlı değerlendirmeye açık.
- Uzak manzara, orta sis ve yakın kristaller harita penceresine bağlı. Kaydırma sırasında yol, düğümler, kedi ve pati izleri birlikte ilerliyor. Çevre katmanlarındaki küçük parallax ofsetleri yolun hareketinden bağımsız ve sınırlı.
- Su shader'ı pilot görselin mavi ve camgöbeği bölgelerinde kaydırmadan bağımsız çalışan, döngüsel düşey ve yatay ışık hareketi kullanıyor. Düşey hareket şelalede, yatay hareket havzanın alt kısmında. Az hareket ayarı çevre animasyonunu durduruyor.
- Ana maskotun harita atlasında asasız oturma ve iki yürüyüş pozu var. Pati teması ve gölge yol koordinatını kullanıyor. Kedi adımları yol boyunca değiştiriyor, dönüş yönünü yol eğrisinden alıyor ve ardışık altın pati izleri bırakıyor. Varışta oturuyor ve açılan bölüm kısa altın vurgu alıyor. Dünya sınırlarında kaynak ve hedef harita arasında kısa bir geçiş kullanılıyor.
- Numaralar ve yıldızlar yolun yan ceplerinde. İlk bölümde şapkanın üst kenarla kesişmesini önlemek için kediye yol üzerinde ek pay bırakıldı.

## Değişen dosyalar

- `Assets/Scenes/LevelMap.unity` ortak görsel referansları bağlar. Prefab değişikliği yok.
- `Assets/Scripts/Map/MapWorld.cs`, `MapWorldJourney.cs`, `MapWorldJourneyLayers.cs`, `MapWorldJourneyMotion.cs` ve `JourneyRouteGraphic.cs` katmanları, rotayı, düğüm yerleşimini ve maskot hareketini yönetir.
- `Assets/Scripts/Map/MapJourneyTheme.cs` dünya bazlı görünüm tanımıdır. `Assets/Resources/Map/JourneyThemes/crystal.asset` pilot tanımdır.
- `Assets/Shaders/MapWaterLight.shader` su ve taş yüzeyini, şeffaf atlas kenarlarını işler. Dört görsel ve Unity içe aktarma ayarları `Assets/Art/Environment/Map/JourneyFoundation/` altında.
- `Assets/Editor/MapFlowValidation.cs` ve `Assets/Scripts/Tests/MapFlowProbe.cs` izole Unity doğrulamasını yürütür.

## Tema onayı

Her dünya için `Resources/Map/JourneyThemes/<dünya-anahtarı>.asset` ile arka plan, isteğe bağlı ön plan, taş dokusu, yüzey ve kenar renkleri ile su hareketi ayrı seçilebilir. Pilot dışındaki dünyalar ortak gökyüzü ve köprüyü kullanıyor. Önceki teras varlıkları sahnede karşılaştırma için `Use Legacy Journey Art` seçeneğiyle erişilebilir. Dünya bazlı çizim ve onay Kristal pilotu dahil sonraki turlara açık.

## Unity'de inceleme

1. `Assets/Scenes/LevelMap.unity` açıkken **Mewgic → Map Flow → Run Journey Foundation QA** komutunu çalıştırın. İzole test, gerçek oyuncu profili yerine geçici kayıt kullanır.
2. İlk örnek 91 → 92. Pati izlerini, zemine basan kediyi, altın açılışı ve oturma pozunu izleyin. Ardından rota kayarken şelaleler ve kenar kristallerinin ekrana bağlı kaldığını kontrol edin.
3. 100 → 101 geçişinde ilk bölümün ve kedinin kadrajını karşılaştırın. Game View'da 9:16, 9:19.5 ve 9:20 portre görüntülerini inceleyin.
4. Normal akışta ilk kez bir bölüm kazanıp haritaya dönün. Bir sonraki bölüm açılırken dokunarak geçin ve haritayı yeniden açın.

## Kanıtlar

Unity 6000.3.27f2 üzerinde 87 Play Mode kontrolü geçti. 12 dünya sınırı, gerçek bölüm kazanışı ve haritaya dönüş, animasyonun kesilip kayıt üzerinden yeniden açılması, dokunarak atlama ve üç portre oranı doğrulandı. C# derlemesi ile Console sıfır hata ve sıfır uyarı verdi. Su shader'ı için derleyici mesajı yoktu.

- [Yürüyüş](01-Walk.png), [açılış](02-Unlock.png), [kaydırılmış kadraj](06-Scroll-Anchored.png) ve [dünya geçişi](05-Island-101.png).
- [9:16](04-Portrait-1600.png), [9:19.5](04-Portrait-1950.png), [9:20](04-Portrait-2000.png).
- [Play Mode sonucu](UNITY_TESTS.json), [Unity Console özeti](CONSOLE.json), [görsel üretim girdileri](PROMPTS.md).
- Fiziksel cihaz ve dağıtım build'i bu turda denenmedi.
