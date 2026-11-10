# Onaylanan bölüm sonu tasarımı

Yeni referansın logo, manzara, ince altın çerçeve, madalyon, para yığını, düğme süsleri ve maskot alanları ayrı Unity UI geometrilerine bağlandı. Kaynak, tam ekran bir UI görseli olarak çizilmiyor. Mor panel, başlık, yıldızlar, sayaç, etiketler, ışıklar ve dokunma alanları Unity tarafında ayrı. Referansın görüntüleyici kontrolleri oyun katmanlarına dahil edilmedi. 400 × 714 kaynak görsel, büyük Game View'da boyalı süsler için keskinliği sınırlıyor.

## Animasyon

| An | Görünen katman |
| --- | --- |
| 0,00 sn | Bulmaca üzerinde iki kısa kutlama patlaması |
| 0,49–1,45 sn | Kararma, manzara ve çerçeve ışığı |
| 0,99–1,70 sn | Mor panel ve başlığın kademeli açılışı |
| 1,61 / 1,91 / 2,23 sn | Sol, büyük orta ve sağ yıldız |
| 2,50 / 2,80 sn | Para yığını, gerçek tutar ve reklam kartı |
| 3,06 / 3,27 sn | Sonraki Bölüm, ardından alt düğmeler ve maskot tepkisi |
| 3,65 sn | Yerleşim tamamlanır. Altın barına uçuş devam edebilir. |

Hızlandır düğmesi sonucu son duruma getirir. Az hareket tercihi katmanları doğrudan yerleştirir. Kısa tekrar eden efektler kenar yansıması, altın yıldız ışığı, reklam parlaması, şelale boyunca kayan mavi ışık, maskot nefesi ve kısa göz hareketinden oluşuyor.

## Ekonomi

Sonuç alanı tamamlamanın kaydedilen gerçek ödülünü kullanır. Ek altın oluşturmayan bir tekrarda `Yeni altın ödülü yok` gösterilir. Geçerli makbuzun taban ödülü 5 ile çarpılır. Tamamlanan reklam yalnızca farkı ekler. Örneğin 20 + 80 = 100 ve 50 + 200 = 250. Reklam hakkı mevcut ilk kazanış kuralına bağlıdır. Ödül ve makbuz tüketimi aynı kayıt işleminde yapılır. Reklam hazır değilse kartta durum metni gösterilir.

## Değişen dosyalar

- `Assets/Scripts/Presentation/GameplayEndScreen.cs`, `VictoryArtwork.cs`, `RewardMotion.cs`
- `Assets/Resources/Gameplay/Victory/ApprovedVictoryAtlas.png` ve yeni Unity meta dosyaları
- `Assets/Scripts/Core/RewardedVictory.cs`, `EconomyConfig.cs`, `Assets/Resources/Economy/Balance.json`
- `Assets/Localization/Source/Mewgic.json`, `Mewgic Shared Data.asset` ve sekiz dilin `Mewgic_*.asset` tablosu
- `Assets/Scripts/Tests/FullGamePreview.cs`, `VictoryReferenceProbe.cs`, `MagicShopProbe.cs`
- Bu rapor, Unity kanıtları ve `docs/PRODUCTION_PROGRESS.md`

## Game View kanıtları

- [Gerçek üç yıldızlı kazanış, 50 altın ve 250 altın reklam teklifi](Unity-3-Yildiz.jpg)
- [Animasyon videosu](Unity-Kutlama.webm), [bulmaca kutlaması](Unity-Kutlama.jpg), [çerçeve oluşumu](Unity-Cerceve.jpg), [başlık girişi](Unity-Baslik.jpg)
- [Bir yıldız](Unity-1-Yildiz.jpg), [iki yıldız](Unity-2-Yildiz.jpg), [uzun portre](Unity-9x20.jpg)
- [Reklam kullanılamıyor](Unity-Reklam-Yok.jpg), [reklam ödülü alındı](Unity-Reklam-Odendi.jpg)

Video 30 gerçek Unity Game View karesinden oluşuyor. Ses kaydı içermiyor. Gerçek 5. bölüm Play Mode bulmacası kullanıldı.

## Doğrulama

Unity 6000.3.27f2 C# derlemesi başarılı. Bölüm sonu odaklı 384 Play Mode kontrolü ve 37 tam oyun görsel QA kontrolü geçti. Gerçek bulmaca ile 1, 2 ve 3 yıldız, 40, 45 ve 50 altın, 20 → 100 kayıt modeli, 50 → 250 test reklamı, kapanan ve çevrimdışı reklam, tekrarlanan makbuz ve sahne geçişinden sonra gelen doğrulama test edildi. Sonraki Bölüm, Tekrar Oyna, Haritaya Dön ve Animasyonu atla gerçek UI tıklamalarıyla doğrulandı. 9:16, 9:19.5, 9:20, güvenli alan ve sekiz dilde metin yerleşimi kontrol edildi. Oyuncu kaydı korundu. Reklam test sağlayıcısı kullanıldı. Mobil build ve fiziksel cihaz testi yapılmadı.

[Sonuç ekranı Play Mode raporu](VICTORY_PLAY_MODE.json) · [Tam oyun görsel QA raporu](FULL_GAME_QA.json)
