# Haritada altın pati izleri ve varış ışığı

`LevelMap.unity` ve Kristal Şelaleler tema varlığından kedi atlası bağlantıları çıkarıldı. Kedi görünümünü oluşturan `MapWorldJourneyMotion.cs` kaldırıldı. Ana Menü ve oynanış kedileri kendi bileşenlerini kullanıyor.

`MapWorld.Continuous.cs` mevcut köprü ve geçiş eğrisini mesafeye göre örnekliyor. `JourneyPawTrail.cs` tek bir UI mesh içinde kıvrıma dönen, dolgun altın patiler çiziyor. Patiler yolun ortasında tek sıra halinde yaklaşık 160 ms aralıkla oluşuyor. Varıştan sonra son beş iz o harita oturumunda daha düşük parlaklıkla kalıyor. Az hareket ayarı yürüyüşü atlıyor.

`MapWorldProgression.cs` ve `JourneyRouteGraphic.cs` varış ışığını taşın arkasında çiziyor. Son adımdan sonra sıcak altın halka, ışınlar ve kısa ışıltılar açılıyor. Işınlar yaklaşık bir saniyede sönüyor, aktif taşın altın aurası kalıyor. Taşın numarası ve yıldızları ışığın önünde.

Unity 6000.3.27f2 Play Mode'da Ana Menü → Harita, Bölüm Sonu → Haritaya Dön, açık ve kilitli taşlar, 99 → 100, 50 → 51 ve üç portre oranı doğrulandı. Test gerçek oyuncu kayıtlarının değişmediğini doğruladı. Son turda Console hatası yoktu. Derlemede mevcut `PremiumButton.Start` CS0114 uyarısı görüldü. Bu görseller Unity Game View'dan alındı. Fiziksel cihaz testi yapılmadı.

Geniş harita regresyonu 75 kontrolle geçti. 50 → 51'den 600 → 601'e kadar 12 bölge sınırı, kayıt yeniden açma, atlama, az hareket tercihi ve gerçek bölüm kazanışı sonrasında haritaya dönüş doğrulandı. [Regresyon sonuçları](UNITY_TESTS.json)

Unity'de `Home.unity` ile Play Mode'u başlatıp **Harita** düğmesine bas. Efekti gerçek oyuncu kaydına dokunmadan görmek için **Mewgic → Visual QA → Verify Continuous Map** menüsünü kullan.

[Pati izleri](Golden-Paw-Trail.png) · [100. bölüme varış](Golden-Arrival.png) · [Play Mode sonucu](RESULT.json)
