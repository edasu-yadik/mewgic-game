# MeWGic bölüm sonu ekranı

8 Kasım 2026 · Unity 6000.3.27f2 · Başlangıç `cc18023` · Teslim dalı `release/0.8.0-rc.1`

Ekli referanstan mor kadife ve altın çerçeve, taç, MeWGic plakası, kedi, şelaleler ve kristaller aktarıldı. Rozetler, ikonlar ve altın yansımaları referans plakasında duruyor. Düğmeler kendi tıklama alanlarına ve basış durumlarına sahip. Başlık, ödül tutarları ve etiketler sekiz dilde canlı Unity metni. 9:16, 9:19.5 ve 9:20 portre oranları için güvenli bölgeye göre yerleşim yapılıyor. Uzun ekranlarda artan alan, gökyüzü ve kristal bahçe katmanlarıyla devam ediyor.

## Yıldızlar ve reklam ışığı

- Sol yıldız **0,18 sn**, büyük orta yıldız **0,49 sn**, sağ yıldız **0,83 sn** sonra açılır. Yalnız kazanılan yıldızlar parlar. Diğer yuvalar mor ve sönük kalır.
- Her yıldız yaklaşık **0,32 sn** içinde büyür ve hafifçe hedef boyutu aşarak oturur. Orta yıldızın boyutu, büyümesi ve ışığı daha belirgin. Her kazanılan yıldızın çevresine dört kısa parıltı yayılır. Mevcut Coin ve Chest sesleri oynatıcının ses tercihlerine uyar.
- Reklam panelindeki film ikonu, coinler, altın çerçeve ve yansıma referans plakasındadır. Etkin panelde üst altın kenarı boyunca **1,2 sn** süren bir ışık geçişi yaklaşık **3,9 sn** aralıkla döner. Sağdaki coinlerin parıltısı geçişi izler. Hareketi azaltma tercihi açıksa sonuç doğrudan son durumunda görünür ve hareketli ışıklar durur.

## Altın ve reklam bağlantısı

Normal kazanış mevcut ekonomi tarafından hesaplanır ve sonuç ekranı kaydedilen miktarı gösterir. Örnek modelde 20 altına 80 eklenir. Gerçek üç yıldızlı ilk kazanış testinde 50 altına 50 eklenerek toplam 100 oldu. Reklam teklifi ilk kazanışa bağlandı. Böylece tekrar oynayarak reklam bonusu biriktirilemez.

Gerçek reklam sağlayıcısı `RewardedVictoryAds.Provider` alanına bir `IRewardedVictoryAd` adaptörü kaydederek bağlanır. `ShowAsync` reklamın tüm kapanış yollarında tamamlanmalı ve yalnız SDK'nın kazanılmış ödül bildirimi için `Rewarded` dönmelidir. Sağlayıcı yoksa veya reklam erken kapanırsa altın eklenmez. Onaydan sonra sadece `100 − kaydedilmiş kazanış ödülü` eklenir. Ek altın ile hakkın tüketilmesi tek kayıtta tamamlanır. Yazma hatasında aynı ekrandan reklamı tekrar izlemeden kayıt yeniden denenebilir.

Projede gerçek reklam SDK’sı ve reklam birimi yapılandırması bulunmadığı için başarı ve erken kapanış, yalnız Unity Editor’daki sahte sağlayıcı ile sınandı. Fiziksel cihaz ve yeni dağıtım build’i bu doğrulamaya dahil değildi.

## Unity’de kontrol

1. Normal oyuncu akışı için `Assets/Scenes/Boot.unity` sahnesini açıp Play’e bas. Ana menüden **Oyna**’yı seç ve bir bölüm kazan. Yeni kayıtta bölüm 1, şişe **1→3, 2→1, 3→2** döküşleriyle üç yıldız verir. Reklam teklifini görmek için ilk kez kazanılan bölümü kullan.
2. Tüm testleri yalıtılmış bir kayıtla çalıştırmak için Play Mode dışındayken **Mewgic → QA → Run Victory Reference** menüsünü çalıştır. Test `Assets/Scenes/Gameplay.unity` üzerinden gerçek döküşler yapar ve sonuçları bu klasöre yazar.

[Sonuç testleri](PLAY_MODE.json) · [Bölüm ilerleme regresyonu](PROGRESSION_REGRESSION.json) · [1 yıldız](01-OneStar.png) · [2 yıldız](02-TwoStars.png) · [3 yıldız 9:16](03-ThreeStars-900x1600.png) · [9:19.5](04-ThreeStars-900x1950.png) · [9:20 güvenli alan](04-ThreeStars-900x2000.png) · [Reklam onayı sonrası](05-AdRewardClaimed.png)

Unity Play Mode'da 294 sonuç ekranı ve 437 bölüm ilerleme kontrolü geçti. Döküşlerle 1, 2 ve 3 yıldız kazanıldı. Sonraki, Tekrar Oyna, Haritaya Dön ve Reklam İzle gerçek UI raycast ile tetiklendi. Son C# derlemesi temiz. Sonuç testinde Console hatası ve oyuncu kayıtlarında değişiklik yok. Üç portre oranında Safe Area, sekiz dilde metin taşması ve etkileşim alanları denetlendi. Görsel karşılaştırmada canlı yazı tipi ve uzun oranlardaki dış manzara uzantıları referans rasterinden ayrılan küçük farklardır.

## Değişen kaynaklar

- `Assets/Scripts/Presentation/GameplayEndScreen.cs` ekran, ışıklar, güvenli alan ve düğme durumları.
- `Assets/Scripts/Presentation/PrototypeApp.cs` tamamlanan bölümün kaydını ve kazanış kimliğini ekrana iletir. Reklam gösterimi sırasındaki uygulama duraklamasını ele alır.
- `Assets/Scripts/Core/BoosterActions.cs` ve `Assets/Scripts/Core/RewardedVictory.cs` ilk kazanışa ait reklam hakkı ve tek seferlik kalıcı fark ödemesi.
- `Assets/Scripts/Presentation/RewardedVictoryAds.cs` sağlayıcı sözleşmesi.
- `Assets/Resources/Gameplay/Victory/` altında `ReferencePlate.png`, `ReferenceStar.png`, `ReferenceSkyEdge.png`, `ReferenceGardenEdge.png` ve Unity import metaları.
- `Assets/Localization/Source/Mewgic.json`, `Assets/Localization/Tables/Mewgic Shared Data.asset` ve `Mewgic_{tr,en,de,fr,es,pt-BR,it,pl}.asset` tabloları.
- `Assets/Scripts/Tests/VictoryReferenceProbe.cs` ve Unity metası. Ekran çalışma anında mevcut Gameplay sahnesine eklenir. Sahne ve prefab düzenlemesi gerektirmez.
- `docs/PRODUCTION_PROGRESS.md` ve bu rapor klasöründeki Game View kareleri ile test çıktıları.

## Referans görsel işlemleri

Codex’in yerleşik imagegen düzenleme aracı kullanıldı. Kaydedilen oyun varlıklarının konumu `Assets/Resources/Gameplay/Victory/`. Gökyüzü ve bahçe katmanları, genişletilen Game View kenarlarından Unity dokusu olarak kırpıldı.

1. “Precisely inpaint this supplied portrait game UI screenshot to create a CLEAN UI PLATE. This is an edit, absolutely no redesign, no shift or change in scaling or crop. Preserve the exact original composition and relative positions, same 9:16 aspect, illustrated cat, environment, top MEWGIC logo, crown, garlands, purple velvet, frames, coin piles, film icon, arrow and little bottom button icons. Preserve all button shapes and gold glows. Remove ONLY the three gold five-pointed stars under the crown (fill with matching purple velvet and subtle ambient purple light, keep laurels), and the text from the gold reward capsule, the watch ad panel, and the three bottom buttons. Keep the two-line large Bölüm Tamamlandı! title and golden divider EXACTLY as original, keep all surrounding art. Do not add anything, use purple fabric and matching lighting where the removed content was. Exact original reference pixel registration is critical so the altered portions can be used in the original image as inpaint patches. Return the full portrait plate.”
2. “Transparent sprite extraction for Unity. From supplied portrait UI reference, isolate only the big center gold five-pointed star under the crown. Keep exactly its beveled thick bright-gold edge, elegant five triangular faceted gold faces with creamy highlights and amber shadows, straight upright orientation and shape. Center it, full star visible and fills almost all of the square, generous transparent area on all sides. No ring, no particles, no rays, no text, no backdrop, no crown, no other stars. Crisp cutout, true transparent background. This is the existing star from the reference, not a redesigned icon.”
3. “Make exactly one tiny edit to this existing clean UI plate. Erase the two-line typography Bölüm Tamamlandı! in the purple velvet (around x=25%-76%, y=27%-36%), filling it with the continuous adjacent purple velvet texture. Keep the gold divider immediately below intact. No changes anywhere else; exact registration, no new content. Preserve every surrounding object, including the MeWGic logo, cat, crown, button shapes, icons, glow and background. The title must be completely blank purple velvet.”
4. “Precise cleanup for a 900 by 2000 portrait fantasy game UI screenshot, keep the supplied tall aspect and exact pixel layout. This is a targeted edge inpaint, do not redesign or reposition the existing composition or UI. Only repaint the topmost 6.5 percent (the artificial vertical streaks above the row of blossoms and above the MEWGIC title). Replace those streaks with natural continuous deep midnight indigo/purple starry sky, a few pink blossoms and branches in the top-left as a natural continuation of the flowers at the boundary, and subtle natural violet ambience on the right, matching the exact colors and boundaries immediately below. No text, no major objects in this top strip. Also only in the bottommost 6.5 percent replace the mirrored flowers, rock and repeated glowing paws with the natural continuation of the purple crystal garden rocks and water. Keep the central 87% absolutely unchanged. Preserve the 900:2000 framing and the horizontal registration. This output will only be used for cropping those two edge strips to patch the source.”
