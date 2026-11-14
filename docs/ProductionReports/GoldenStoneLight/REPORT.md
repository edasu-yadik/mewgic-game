# Bölüm taşı altın ışığı

Onaylı referanstaki ışık, mevcut bölüm taşının arkasına bağlı tek bir UI mesh içinde çiziliyor. Mesh, taşın mevcut dış çizgisini örnekliyor. Kenarlık alttan üste aydınlanıyor, yumuşak arka ışınlar açılıyor ve üstteki üç küçük elmas sırasıyla yanıyor. Tabanda kısa süre genişleyen altın ışık, iki yanda ince kıvrımlar ve az sayıda sabit konumlu ışıltı var. Patilerin varışı kutlamayı başlatıyor ve 1,8 saniyenin sonunda yalnız sakin aktif aura kalıyor. Az hareket ayarı doğrudan sakin aurayı gösteriyor.

Taş yüzü, bölüm numarası ve yıldızlar kendi mevcut katmanlarında. Işık dokunmayı yakalamıyor ve taş havuzuyla yeniden kullanılıyor. Her ışıltı için ayrı nesne veya parçacık sistemi oluşturulmuyor.

Unity Game View kareleri 99 → 100 geçişinden alındı. Karşılaştırmada referansın daha dokulu arka ışığı görülüyor. Oyundaki ışık mobil kadraja uyarlanmış mesh katmanlarıyla oluşturuldu. Kullanıcı referansındaki taş ve 10000 yazısı oyun taşına aktarılmadı.

Unity 6000.3.27f2 üzerinden **Mewgic → Visual QA → Verify Continuous Map** menüsü geçişi yeniden oynatır. [99 → 100 Game View](Golden-Arrival.png) · [Referans karşılaştırması](Comparison.jpg) · [Geçiş kaydı](Arrival.gif) · [Play Mode sonucu](RESULT.json)

Değişen oyun dosyaları `JourneyRouteGraphic.cs`, `MapWorldProgression.cs` ve `MapWorld.Continuous.cs`. `FullGamePreview.SingleMap.cs` başlangıç, zirve ve sakin aura karelerini topluyor. Fiziksel cihaz testi ve mobil performans ölçümü yapılmadı. C# derlemesinde mevcut `PremiumButton.Start` CS0114 uyarısı görüldü.
