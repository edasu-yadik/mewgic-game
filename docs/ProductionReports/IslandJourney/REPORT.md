# MeWGic ada rotası

3 Kasım 2026 · Unity 6000.3.27f1 · recovery/part09-completion

## Uygulama

LevelMap sahnesindeki ada kapsülü 50 ayrı bölüm taşından oluşan dikey bir rota açıyor. Rota kıvrımlı taş döşeme ve korkuluklu köprülerle birbirine bağlanıyor. Var olan Kristal Şelaleler çizimi, su ve aydınlatma materyali ve büyücü kedi dokusu ortak ada detayı dekoru olarak kullanıldı. Altın ok güncel bölümü, kilit simgesi erişilemeyen bölümü ve yıldızlar kaydedilmiş başarıyı gösteriyor. Bölüm kimliği ve seçilebilirlik mevcut katalog ve ilerleme defterinden geliyor.

Açılış adanın güncel bölümünü veya en yakın ucunu gösteriyor. Kaydırma sırasında taşlar yeniden bağlanıyor. Haritaya dönüş ve güncel bölüme kaydırma düğmeleri var. Klavye veya kontrolcü yönleri taşlar arasında ilerliyor. Dünya haritasının 12 ada sunumu, ilerleme, ödül, save şeması ve prosedürel üretim kodu aynı PART14A altyapısından geliyor.

## Doğrulama

- Unity Play Mode denetiminde **7.142 kontrol** geçti. Tüm 12 adada 1–600 ve devam adasında 601–650 numara, durum ve 50 taşa erişim kontrol edildi.
- Gerçek Game View 900×1600, 900×1950 ve 900×2000 boyutlarında raycast ve 48 px minimum bölüm dokunma hedefi doğrulandı. Çentik paylı 900×2000 ve kompakt 360×640 kamera çıktılarında taşma ve güncel taşın yeniden boyutlandırma sonrası görünmesi kontrol edildi.
- Scroll ve sürükleme olayları içeriği hareket ettirdi. Yukarı yön girdisi 100'den 99'a odaklandı. 101 kilit açıklamasını, 100 gerçek Gameplay sahnesini açtı. 99'un yıldızları test kaydından okundu.
- Gözlenen etkin taş sayısı en fazla 10. Rota tek mesh, dekor dokuları ortak ve görünmeyen dünya animasyonu ada detayı açıkken duruyor. Cihaz GPU, RAM, FPS ve ekran okuyucu kabulü yapılmadı.
- Ana oyuncu kaydı, yedek ve bozuk kayıt alanları test öncesi ve sonrası aynıydı. Son Play Mode turunda yakalanan hata veya uyarı yoktu. İlk turda özel mesh bileşenlerine eksik CanvasRenderer gereksinimi eklendi. Kompakt önizlemede tespit edilen yeniden boyutlandırma odak kayması da giderilip denetim tekrarlandı.

[Makine okunur test sonucu](UNITY_TESTS.json) · [Unity 900×1600](GameView.png) · [Kompakt 360×640](Compact_360x640.png) · [Çentik payı 900×2000](SafeArea_900x2000.png)

## Dosyalar ve sınırlar

Üretim sahnesi `Assets/Scenes/LevelMap.unity`. Rota ve havuz `Assets/Scripts/Map/MapWorldJourney.cs`, rota ve taş geometrisi `JourneyRouteGraphic.cs`, durum sunumu `MapWorldProgression.cs`, bağlantı ve yerleşim `MapWorld.cs`. Yeniden çalıştırılabilir Editor denetimi `Assets/Scripts/Tests/IslandJourneyProbe.cs` içindeki `Mewgic.IslandJourneyProbe.Begin()`. Önceki PART14A sayfa düğmelerini bekleyen denetimler o teslimin arşividir.

İstenen `/mnt/data/a_highly_detailed_colorful_fantasy_game_level_map.png` dosyasına erişilemedi. Bağlı sohbetin yerel eklerinde önceki mor arayüz ve PART14A özet görselleri vardı. Bu nedenle yeni rotanın görsel tasarımı yazılı tarif ve projedeki onaylı sanatla yapıldı. Yeni PNG referansıyla bire bir karşılaştırma bekliyor.
