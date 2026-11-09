# Ödül animasyonları ve haritaya dönüş

## Akış

- Günlük hediye, aynı sandığın kapağını atlas içinde gövdeden ayırır. Kapak menteşe hareketiyle açılır ve mevcut açık sandık görseline geçer. Altın ve yıldızlar saçılır, ödül metni parlayarak gelir ve coinler üst bardaki bakiyeye uçar. Aynı sandık otomatik karşılamadan ve Günlük Hediye sekmesinden açılabilir.
- Bölüm sonunda biten bulmaca üzerinde kısa bir çift patlama görülür. Kazanılan yıldızlar soldan merkeze ve sağa açılır. Panel saydamlık ve hafif büyümeyle girer. Mor-altın plaka ve mevcut düğme kompozisyonu kullanılır. Reklam, sonraki bölüm ve alt düğmelerdeki kısa parlamalara arka planın küçük hareketli ışıkları eşlik eder.
- Günlük ve zafer ödülü aynı uçuş bileşenini kullanır. Yedi coin görselinin varışları bakiyeyi kademeli gösterir. Ekonomi ve reklam ödülü mevcut tek seferlik kayıt işlemiyle yazılır. Animasyon bakiye üretmez. Testte günlük ödül 550 → 650, gerçek 5. bölüm zaferi 550 → 600 oldu.
- Haritaya Dön düğmesi mor bir perdeyle `LevelMap` sahnesine geçer. Normal kazanışta ilgili ilerleme animasyonu oynar. Eski bölüm tekrarı o bölümün dünyasına ve terasına döner. 5. bölüm zaferinde Büyülü Köy 1–10, 91. bölüm tekrarında Kristal Şelaleler 91–100 açıldı. Kristal tekrarında güncel ilerleme 101 olarak kaldı.
- Ana menüdeki `MissionsButton`, simge ve geri sayacıyla birlikte yatay sürüklenir. Bırakıldığında yakın kenara yumuşakça oturur. Normal dokunuş Görevler'i açmaya devam eder. Kartın yeri o ana menü oturumu boyunca geçerlidir.
- Büyülü Köy bölüm seçicisi mevcut boyalı ada, altın çerçeve ve gerçek bölüm taşlarını kullanıyor. Geçici köprü grafiği kaldırıldı. Köy'ün nihai bölüm haritası tasarımı onay bekliyor. Kristal Şelaleler'in mevcut referansı ve rotası kullanılıyor.

## Değişen proje dosyaları

- `Assets/Scripts/Daily/DailyArrivalPopup.cs`, `DailyActivitiesPanel.cs`, `DailyArt.cs`, `MissionCardDrag.cs` ve yeni scriptin Unity meta dosyası.
- `Assets/Resources/Daily/Art.asset` mevcut coin sprite referansı.
- `Assets/Scripts/Presentation/GameplayEndScreen.cs`, `HomeTopBar.cs`, `PrototypeApp.cs`, `SceneNavigation.cs`, `RewardMotion.cs` ve yeni scriptin Unity meta dosyası.
- `Assets/Scripts/Map/MapWorld.cs`, `MapWorldJourney.cs`, `MapWorldCrystalJourney.cs`, `MapWorldJourneyLayers.cs`, `MapWorldJourneyMotion.cs`, `JourneyRouteGraphic.cs`.
- `Assets/Scripts/Tests/FullGamePreview.cs`, bu rapor, Game View kanıtları ve `docs/PRODUCTION_PROGRESS.md`.

## Unity Game View

- [Kapak hareketi](Chest-Lid.jpg), [sandık patlaması](Chest-Burst.jpg), [günlük bakiye artışı](Chest-CountUp.jpg)
- [Kutlama](Victory-Burst.jpg), [ilk yıldız](Victory-FirstStar.jpg), [zafer bakiye artışı](Victory-CountUp.jpg)
- [Geçiş perdesi](Map-Fade.jpg), [Büyülü Köy 1–10](Map-Village.jpg), [91 tekrarından Kristal 91–100](Map-CrystalReplay.jpg)
- [Sağa taşınan Görevler kartı](Drag.jpg)

## Doğrulama

Unity 6000.3.27f2 C# derlemesi başarılı. Play Mode görsel denetiminde 36 kontrol geçti ve hata listesi boş. 5. bölüm Game View'daki şişelere gerçek UI olayları gönderilerek kazanıldı. Hediye sekmesindeki sandık, coin sayacı ara değerleri, Görevler kartının sağa sürüklenmesi, normal dokunuşla açılması, mor geçiş perdesi ve iki doğru harita girişi doğrulandı. Sonuç paneli ve coin barı 900 × 1600, 1950 ve 2000 portre oranlarında kontrol edildi. Reklam regresyonu izole test sağlayıcısıyla geçti. Oyuncunun mevcut kayıt anahtarları aynı kaldı. Fiziksel cihaz testi ve mobil build yapılmadı. [Play Mode sonucu](PLAY_MODE.json)
