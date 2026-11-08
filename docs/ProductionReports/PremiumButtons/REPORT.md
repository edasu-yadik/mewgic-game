# MeWGic premium düğme sistemi

8 Kasım 2026 · Unity 6000.3.27f2 · Başlangıç `15498e8` · Dal `release/0.8.0-rc.1`

Ana menü, ayarlar, yardımcı güç mağazası, günlük görev ve sandık, etkinlikler, harita, oynanış, duraklatma ve yükleme hata ekranındaki düğmeler ortak görsel aileye bağlandı. Önceki bölüm sonu ekranı `GameplayEndScreen.cs` ve `Assets/Resources/Gameplay/Victory/` ile birlikte korundu. Ana maskot, şişe seçimi, harita taşlarının onaylı çizimi ve görünmez geçiş dokunma alanları kendi biçiminde kaldı.

## Kategoriler ve ekranlar

| Ekran | Uygulanan düğmeler |
| --- | --- |
| Ana menü | Altın çerçevesi korunan ametist Oyna, özgün ikonlu Ana Menü, Harita ve Etkinlikler, kilit işaretli Takım ve Envanter, Görevler, Ayarlar, para birimi ve enerji eylemleri |
| Ayarlar ve mağaza | Ses, efekt, titreşim ve hareket seçenekleri, dil listesi, öğretici tekrarı, kapat ve geri eylemleri, dört ayrı yardımcı güç satın alma düğmesi |
| Görev ve ödül | Görev ve hediye sekmeleri, göreve git ve ödül al durumları, günlük sandık, ödül onayı |
| Etkinlikler | Etkinlik sekmeleri, haftalık Oyna ve sandık, sezon ödülleri, koleksiyon, bilgi ve geri eylemleri |
| Harita | Ada dolaşımı, mevcut bölümü bulma, haritaya dönme, hareket seçeneği, sandık alma ve bilgi pencereleri. Bölüm taşlarında mevcut kilit görseline dokunma tepkisi eklendi |
| Oynanış ve yükleme | Duraklat, devam et, yeniden başla, harita veya ana menüye dön, dört yardımcı güç, Pati açıklamasını kapatma ve yüklemeyi tekrar deneme |

Birincil yüzler geniş altın çerçeve, pembe iç ışık ve kristal işareti kullanıyor. İkincil yüzler daha sade. Ödül yüzleri sandık ikonu ve kısa altın yansıması kullanıyor. Seçili sekme daha aydınlık, pasif yüz mor-gri ve kilitli ödül işaretli. Takım ve Envanter mevcut bilgilendirmeyi açıyor. Üst altın sayacındaki mağaza ikonu mevcut yardımcı güç mağazasını açıyor.

Depodaki `origin/release/0.8.0-rc.1` dalında yer alan Navigation, PlayButton, HUD, Gameplay Tools ve bölüm sonu görsel referansları kullanıldı. Harita, Etkinlikler, Takım, Envanter, Görevler, Ayarlar, sandık ve yardımcı güç ikonları mevcut varlıklardan geliyor. Temel geri, yön, mağaza, tekrar ve hedef işaretleri çözünürlükten bağımsız UI geometrisi. Yeni onay bekleyen raster görsel eklenmedi.

## Prefab ve etkileşim

Oluşturulan ortak prefab `Assets/Resources/UI/PremiumButtonFace.prefab`. `PremiumButton` mevcut düğmenin altına prefab yüzünü yerleştiriyor ve mevcut tıklama hedefini, `onClick` bağlantılarını ve ekran yerleşimini koruyor. `PremiumButtonFace` mor-altın yüzleri ve yansımayı çiziyor. `PremiumButtonGlyph` temel işaretleri çiziyor. Ekranlar `SetRole` ile seçili sekme ve ödül durumunu değiştirebiliyor.

Basışta yüz ve içerik hafif küçülüyor ve bırakınca geri dönüyor. Etkin düğmede kısa ışık geçişi, ödül düğmelerinde düşük sıklıkta parıltı var. Pasif düğme işlevi çağırmadan kısa dokunma tepkisi veriyor. Fiziksel tıklama veya klavye onayı aynı açık düğmede **280 ms** içinde yinelenirse ikinci çağrı işlenmiyor. Kapanan bir pencerenin düğmesi yeniden açıldığında giriş kilidi sıfırlanıyor. Programatik oyun olayları kendi mevcut kurallarıyla çalışıyor.

## Gerçek Unity Game View karşılaştırmaları

| Ekran | Önce | Sonra |
| --- | --- | --- |
| Günlük sandık | [Önce](Before/00-Gift.png) | [Sonra](After/00-Gift.png) |
| Ana menü ve navigasyon | [Önce](Before/01-Home.png) | [Sonra](After/01-Home.png) |
| Ayarlar | [Önce](Before/02-Settings.png) | [Sonra](After/02-Settings.png) |
| Yardımcı güç mağazası | [Önce](Before/03-Shop.png) | [Sonra](After/03-Shop.png) |
| Görevler | [Önce](Before/04-Missions.png) | [Sonra](After/04-Missions.png) |
| Günlük ödül | [Önce](Before/05-DailyReward.png) | [Sonra](After/05-DailyReward.png) |
| Etkinlik merkezi | [Önce](Before/06-Events.png) | [Sonra](After/06-Events.png) |
| Haftalık ve sezon ödülleri | [Önce](Before/07-Weekly.png) | [Sonra](After/07-Weekly.png) |
| Bölüm haritası | [Önce](Before/08-Map.png) | [Sonra](After/08-Map.png) |
| Oynanış ve yardımcı güçler | [Önce](Before/09-Gameplay.png) | [Sonra](After/09-Gameplay.png) |
| Duraklatma | [Önce](Before/10-Pause.png) | [Sonra](After/10-Pause.png) |

[Yan yana karşılaştırma](COMPARISON.png) · [9:19.5](After/01-Home-1950.png) · [9:20 güvenli alan](After/01-Home-2000.png)

## Doğrulama

Önceki akışta 50, yeni sürümde **972** odaklı Play Mode kontrolü geçti. Satın alma ve ayar değişikliği iki ayrı karedeki hızlı dokunuşlarla sınandı. Kilitli haftalık ödül ve harita taşı, gerçek sandık talebi, yardımcı güç kullanımı, Oyna, haritadan dönüş ve duraklatma akışları canlı sahnede çalıştı. 9:16, 9:19.5 ve 9:20 oranlarında taşma kontrol edildi. 9:20 için kenar ve çentik payı ayrıca simüle edildi. Ana menü sekiz dilde denetlendi. Son C# derlemesi temiz ve testlerde Unity Console hatası çıkmadı. İzole profil çalıştırmaları gerçek oyuncu kayıtlarını korudu.

[Önceki Play Mode](Before/PLAY_MODE.json) · [972 düğme kontrolü](After/PLAY_MODE.json) · [105 harita regresyonu](MAP_REGRESSION.json) · [294 bölüm sonu regresyonu](VICTORY_REGRESSION.json)

Unity’de yeniden çalıştırmak için Play Mode dışındayken **Mewgic → QA → Run Premium Buttons** menüsünü kullan. Test, `Home.unity` üzerinden yalıtılmış bir oyuncu profiliyle başlar ve normal bağlantılarla LevelMap ile Gameplay’e geçer. Fiziksel cihaz ve dağıtım build’i bu doğrulamaya dahil değildi.

## Kaynak dosyalar

- Ortak sistem `Assets/Scripts/UI/PremiumButton.cs`, `PremiumButtonFace.cs`, `PremiumButtonGlyph.cs` ve bunların Unity metaları.
- Prefab `Assets/Resources/UI/PremiumButtonFace.prefab` ve Unity metası. Oyna’nın mevcut dokusuna ametist renk uygulaması `Assets/Shaders/LocalizedPlayFace.shader` içinde.
- Ekran bağlantıları `BootLoadingView.cs`, `DailyActivitiesPanel.cs`, `DailyArrivalPopup.cs`, `MapPoint.cs`, `MapWorld.cs`, `GameplayHud.cs`, `GameplayTools.cs`, `HomeNavigation.cs`, `HomeTopBar.cs`, `PrototypeApp.cs`, `RewardHubPanel.cs` ve `PawGuide.cs`.
- `Assets/Scripts/Tests/ButtonSystemProbe.cs` ve Unity metası, bu klasördeki Game View kareleri ile test çıktıları, `docs/PRODUCTION_PROGRESS.md`.
