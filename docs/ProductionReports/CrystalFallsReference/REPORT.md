# Kristal Şelaleler · 91–100 referans kadrajı

## Görsel düzen

- Kullanıcının referansından, etkileşimli öğelerin altındaki çevre çizimi çıkarıldı. Kristal saray sağ üstte, pembe çiçekli ağaç sağ yamaçta, sarı fenerler yol dönüşlerinde, mor kristaller alt ön planda. Sağ üst, sol yamaç, kemer altları ve sağ alttaki şelaleler ayrı konumlarında duruyor.
- Dört taş kemer ve geniş alt viraj, yolu üstten alta birbirine bağlar. On bölüm taşı yol yüzeyine ayrı ayrı yerleştirildi. 100 açıldığında mor taşın çevresinde altın hale ve taç süsü belirir. Numaralar, yıldızlar ve kilitler Unity tarafından çizilir.
- Sahne tam portre kadrajını kullanır. Başlık ve düğmeler sabit kalır. Kaydırma bırakılınca en yakın onlu teras kadraja oturur. 51–90 için mevcut boyanmış Kristal teraslar kullanıldı ve ilk/son düğümler sabit kontrollerin arasına alındı.
- Su shader'ı mavi ve turkuaz piksellerde hareketli düşey ışık çizgileri ve küçük UV yer değiştirmeleri kullanır. Sağ alt havzada akış yataya döner. Tek çizim ve tek ek doku örneğiyle çalışır. Az hareket ayarında çevre saati durur.
- Ana maskotun mevcut asasız poz atlası kullanıldı. Kedi çizilmiş taş yol üzerinde yumuşatılmış ara noktaları takip eder ve altın pati izleri bırakır. 99 → 100 sırasında özel taşa ulaşıp son kıvrımdan ön plandaki sol kayaya gelir. Orada patileri önde oturur.

## Değişen dosyalar

- `Crystal_Reference_91_100.png` ve içe aktarma ayarı, `crystal.asset`, `MapJourneyTheme.cs`.
- `MapWorldCrystalJourney.cs` Kristal'e özel kadraj, düğüm koordinatları, yürüyüş eğrisi ve teras hizalama. `MapWorldJourney.cs`, `MapWorldJourneyMotion.cs`, `MapWorldJourneyLayers.cs`, `MapWorld.cs` ve `MapPoint.cs` bu akışı mevcut haritaya bağlar.
- `JourneyRouteGraphic.cs` ve `MapWorldProgression.cs` 100 için altın hale ve taç. `MapWaterLight.shader` Kristal referans su varyantı.
- `MapFlowProbe.cs` ve `MapFlowValidation.cs` Unity doğrulaması. Sahne ve prefab dosyaları değişmedi.

## Unity'de kontrol

1. `Assets/Scenes/LevelMap.unity` açıkken **Mewgic → Map Flow → Run Crystal Falls QA** komutunu çalıştırın. Geçici profil 91 → 92 ve 99 → 100 akışlarını açar.
2. 91–100 kadrajında sağ üstteki ve köprü altlarındaki şelaleleri izleyin. Kaydırıp bıraktığınızda terasın kadraja oturuşunu kontrol edin.
3. 99 → 100 geçişinde pati izlerini, 100'ün altın vurgusunu ve kedinin sol ön plandaki oturuşunu inceleyin. Normal kayıtta bu akış, 99 ilk kez kazanılıp haritaya dönüldüğünde oynar.

## Game View

Unity 6000.3.27f2 üzerinde 98 Play Mode kontrolü geçti. 90 → 91 ve 99 → 100 geçişleri, teras hizalama, 12 dünya sınırı, gerçek oynanıştan haritaya dönüş ve üç portre oranı doğrulandı. Derleme ve Console sıfır hata, sıfır uyarı. Su shader'ı için derleyici mesajı yoktu.

- [91 → 92 yürüyüş](01-Walk.png), [99 → 100 yürüyüş](07-Walk-99-100.png), [100 ve bekleme pozu](03-Crystal-100.png).
- [9:16](04-Portrait-1600.png), [9:19.5](04-Portrait-1950.png), [9:20](04-Portrait-2000.png), [önceki teras](06-Scroll-Settled.png).
- 0,4 saniye arayla alınan aynı kadrajda sağ üst, sol yamaç, kemer altı ve sağ alt şelaleleri değişirken taş yol örneği sabit kaldı. [İkinci su karesi](08-Water-Later.png), [piksel karşılaştırması](WATER_CHECK.json).
- [Play Mode sonucu](UNITY_TESTS.json), [Console](CONSOLE.json), [görsel üretim istemi](PROMPT.md).
- Menü ve düğümler mevcut Unity arayüzü üzerinden çalışır. Bu yüzden referanstaki etiket ve süsler yerine oyunun işlevsel kontrolleri görünür. Fiziksel cihaz testi yapılmadı.

## Git teslimi

Uygulama commit'i `c1022f323b831c5b8f1a6eb8547ccd5658d9ba91` uzak `origin/release/0.8.0-rc.1` dalında SHA eşleşmesiyle doğrulandı.
