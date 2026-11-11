# MeWGic · Tek parça bölüm haritası

## Akış

Ana Menü’deki Harita düğmesi eskiden RequestOverview ile LevelMap’i ada seçimi modunda açıyordu. Bölüm sonu aynı sahneye ayrı bir bölüm paneli isteği gönderiyordu. MapWorld iki görünüm de oluşturuyor ve görünürlüklerini değiştiriyordu. Artık iki giriş de aynı bölüm yolunu açıyor. Ana menü güncel bölüme odaklanıyor. Bölüm sonundan dönüş oynanan bölümü veya bekleyen ilerleme yürüyüşünü gösteriyor.

## Katmanlar ve ilerleme

Kale, uzak şelaleler ve dış nehir kıyıları sabit katmanda. Köprüler, köprü bağlantıları, dinamik bölüm taşları, maskot ve pati izleri tek kaydırma içeriğine bağlı. Boyanmış köprü katmanı onaylı Crystal_Reference_91_100 sanatını örnekleyen Unity mesh’leri kullanıyor. Bölüm taşları gerçek UI düğmeleri. Beş adet on taşlık grup yeniden bağlanıyor. Eski gruplar kaydırma konumuna göre yeniden çiziliyor.

Assets/Resources/Map/ContinuousThemes.json başlangıç seviyelerini ve renkleri tanımlıyor. 1–50 Büyülü Köy, 51 ve sonrası Kristal Şelaleler. Sınır çevresindeki 14 seviyelik renk karışımı dış çevrede kaydırma konumuna, yol yüzeyinde rota konumuna göre hesaplanıyor. Erken tema aynı köprü kompozisyonunun daha sıcak bir varyasyonu. Büyülü Köy’e ait ayrı nihai çevre çizimi bu aşamada yok.

Mevcut MapProgression ve ilerleme defteri bölüm açma, yıldız, konum ve sandık haklarını yönetmeye devam ediyor. Tamamlanan taşlar tekrar oynanabiliyor. Yeni varış kaydı maskot yürüyüşü bitince onaylanıyor. Hareket azaltma tercihi aynı ilerleme defterine bağlı.

## Unity kanıtı

- [Game View ve kullanıcı referansı](Referans-Karsilastirma.jpg)
- [Ana Menü girişi ve 50 → 51 kaydırma](Harita-Akisi.jpg)
- [9:16, 9:19.5 ve 9:20](Portre-Oranlari.jpg)
- [Maskot uyku, uyanma ve esneme](Kedi-Uyanma.jpg)
- [Ayağa kalkma, altın patili yürüyüş ve oturma](Kedi-Yuruyus.jpg)
- [İzole Play Mode kontrol sonucu](RESULT.json)

Unity 6000.3.27f2 üzerinde Mewgic → Visual QA → Verify Continuous Map çalıştırıldı. Oyun içi düğmeler Unity EventSystem üzerinden tetiklendi. Bölüm sonu örneği Visual QA ile hazırlandı ve gerçek Haritaya Dön düğmesiyle geçildi. Oyuncu kaydının asıl, geçici ve yedek anahtarları oturum öncesi ve sonrasında eşleşti. Son turda 10 kontrol tamamlandı ve çalışma zamanı hatası kaydedilmedi. Cihaz testi yapılmadı. Köprü bağlantısının doku geçişi kaydırma ortasında görülebilir. Köprü resmine bitişik su ve kaya ayrıntıları rota katmanıyla birlikte hareket eder. Aynı köprü sanatı ileride yeni tema varlığı eklenene kadar tekrar kullanılır.

## Değiştirilen dosyalar

| Alan | Dosyalar |
|---|---|
| Sahne | Assets/Scenes/LevelMap.unity |
| Harita çalışma zamanı | Assets/Scripts/Map/MapWorld.cs, MapWorld.Continuous.cs, MapPlateGraphic.cs, MapRoadJoinGraphic.cs, MapWorldProgression.cs, MapWorldCrystalJourney.cs, MapWorldJourneyMotion.cs, MapWorldThemedJourney.cs |
| Kaldırılan eski harita sınıfları | Assets/Scripts/Map/MapWorldJourney.cs, MapWorldJourneyLayers.cs |
| Tema verisi | Assets/Resources/Map/ContinuousThemes.json |
| Girişler | Assets/Scripts/Presentation/HomeNavigation.cs, PrototypeApp.cs |
| Visual QA | Assets/Editor/FullGamePreviewWindow.cs, Assets/Scripts/Tests/FullGamePreview.cs, FullGamePreview.Review.cs, FullGamePreview.SingleMap.cs |

Unity yeni ve kaldırılan dosyalara ait .meta dosyalarını aynı değişiklikte izler. Harita tek bir sahne kullanır ve prefab değişikliği yoktur.
