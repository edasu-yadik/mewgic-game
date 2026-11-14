# Kesintisiz MeWGic bölüm yolu

Eski harita, on bölümlük Kristal Şelaleler resmini tekrarlıyor ve aradaki köprünün opaklığını kamera konumuna bağlıyordu. Tam kadrajın yakınında bağlantı sıfıra iniyordu. Kaydırma penceresinin merkezi değişince görünür parçalar da havuza geri dönüyordu. Rota hesabı ile köprü mesh'i farklı kaynaklardan beslendiği için pati izi ve bağlantı geometrisinin aynı yolu izlemesi garanti değildi.

| Önceki davranış | Yeni davranış |
|---|---|
| Kamera yer değiştirdikçe bağlantı beliriyor ve kayboluyordu. | Her köprü bağlantısı yol koordinatlarında kalıyor. Yalnız birleşimin küçük bölgesinde, komşu resimle örtüşen bir doku geçişi var. |
| Çalışma aralığı değişince bağlı parçalar temizleniyordu. | Görünür parçalar aynı nesnelerle kalıyor. Yalnız ekran dışındaki parçalar yeni kimliklere bağlanıyor. |
| Köy ve Kristal Şelaleler aynı resmin iki rengiydi. | Onaylı Büyülü Köy resminden alınan uzak manzara, sabit katmanda görüntülenen konuma göre geçiş yapıyor. Mevcut şelale shader'ı kendi katmanında akıyor. |
| Uzak odak tek karede değişiyordu. | Yakın hedefe yavaşlayarak kaydırıyor. Çok uzak hedefte kısa bir örtü geçişinden sonra son yaklaşımı kaydırıyor ve aktif taşı aydınlatıyor. Az hareket tercihinde doğrudan odaklanıyor. |

`ContinuousMapRoute.cs` çizilmiş yolun S dönüşlerini kübik Hermite eğrileriyle izliyor. Bağlantı mesh'i ve patiler aynı eğriyi örnekliyor. Bağlantının uçları komşu teraslardaki yolun konumuna ve teğetine uyuyor. Bölüm taşları aynı rotanın işaretlerine bağlı. İlk 100 bölümün taş konumları korundu. Sonraki parçaların yatay varyasyonu, parça kimliği ve geometri sürümünden hesaplanıyor. Aynı deformasyon köprü resmine, taşlara ve pati konumlarına uygulanıyor, uçlarda sıfırlanıyor. Eski kayıtlar bölüm kimliklerini kullanmaya devam ediyor. Varyasyon sürümünü değiştirmek, yeni bir yerleşim geçişi gerektirir.

Varsayılan havuz altı on bölümlük parça ayırıyor, en çok beşini bağlı tutuyor. Görünen alanın öncesi ve sonrası önceden bağlanıyor. Inspector'daki `preloadChunks` aralığı 1–3. Dünya kimlikleri tamsayı, görünür konumlar küçük bir yerel pencere içinde hesaplanıyor. Pencere merkezi değiştiğinde ScrollRect'in parmak başlangıç noktası aynı miktarda kaydırılıyor. Bu sayede parmak ekrandayken aynı hareket devam ediyor. Mevcut sınırlı pati mesh'i ve varış aurası yeni rota üzerinde çalışıyor.

Kale, gökyüzü, uzak şelaleler ve köy manzarası sabit Canvas katmanında. Köprü, korkuluklar, yakın çiçekler, destekler ve yol resmine bağlı küçük şelaleler taşlar, patiler ve aura ile birlikte hareket ediyor. Kristal Şelaleler resmi katmanlı bir kaynak olmadığı için bazı yakın kaya ve su detayları aynı doku içinde kalıyor. Geçiş aralığı `ContinuousThemes.json` içinde 45–58 olarak ayarlı. Köy manzarası geri kaydırıldığında yeniden beliriyor. Son onaylı temadan sonra aynı varlıklar belirlenimci yol ve ton varyasyonlarıyla kullanılıyor. Bu yapılandırma yeni tema aralıkları kabul ediyor, ancak farklı temalar için bağımsız bitki/kaya/partikül paketleri bu teslimde eklenmedi.

Haritada 100, 1.000 veya 10.000 için bir bitiş yok. Mevcut bölüm kimliği ve 50'lik ödül aralığının ortak üst sınırı 2.147.483.600. Harita nesnelerinin sayısı ilerlemeyle büyümüyor. Bölüm tamamlama kayıtları ise oyuncu ilerledikçe büyüyor ve mevcut kayıt kodu bunları okuyor. RAM, disk, bölüm arşivi ve kayıt serileştirme maliyetleri pratik sınırlar. Harita kaydırması bulmaca üretmiyor, oyuncu bir taşı seçtiğinde mevcut katalog o kimliğin bulmacasını hazırlıyor.

## Unity doğrulaması

Unity 6000.3.27f2 ile **Mewgic → Visual QA → Verify Continuous Map** çalıştırıldı. Ana Menü ve Bölüm Sonu aynı `LevelMap.unity` sahnesini açtı. 1 → 2, 9 → 10 → 11, 49 → 50 → 51 → 52, 99 → 100 → 101, 999 → 1.000 → 1.001, 9.999 → 10.000 → 10.001, 99.999 → 100.000 → 100.001 ve 999.999 → 1.000.000 → 1.000.001 izole kazanış kayıtlarıyla yürütüldü. 1.000.001 bulmacası ayrıca üretildi. Uzun sürükleme, tek dokunuşta yön değişimi, uzak dönüş, havuz sınırı ve yeniden bağlanan taşın konumu kontrol edildi. 9:16, 9:19.5 ve 9:20 Game View çekimleri, Console ve gerçek oyuncu kayıt anahtarları kontrol edildi.

İlk testlerden birinde sahne yüklenirken denetim eski taşı tuttu. Yeni sahnenin doğru ilerleme kimliğiyle açılması beklenerek düzeltildi. Uzak odak testinde de konum, kamera tam durmadan ölçülüyordu. Kamera tamamlandıktan sonra ölçülüyor. Fiziksel dokunmatik cihaz, işletim sistemi düzeyinde kapatma ve mobil performans bu çalışmada ölçülmedi.

Son görsel denetim 11 kontrol grubu ve geniş harita regresyonu 75 kontrol ile geçti. Regresyon, kesilen yürüyüşü ayrı test günlüğünden yeniden açmayı ve gerçek kazanıştan sonra haritaya dönüşü doğruladı. Son iki testte Console hatası yoktu. Geçiş sırasında eski resimden örneklenen taş kaplamasının ek yeri bazı açılarda hâlâ seçiliyor. Referans resmin ayrı köprü, bitki, şelale ve kaya katmanları olmadığından bu noktada görsel eşleşme tam değil. Mobil GPU, bellek ve FPS ölçümleri açık. [Geniş regresyon](UNITY_TESTS.json)

Başlıca dosyalar `ContinuousMapRoute.cs`, `MapWorld.Continuous.cs`, `MapPoint.cs`, `MapPlateGraphic.cs`, `MapRoadJoinGraphic.cs`, `MapJourneyTheme.cs`, `ContinuousThemes.json`, `crystal.asset`, `MapWaterLight.shader` ve `FullGamePreview.SingleMap.cs`.

[Unity sonucu](RESULT.json) · [Köy](Theme-Village.png) · [Kaydırma ve tema karışımı](Theme-In-Scroll.png) · [Kristal Şelaleler](Theme-51.png) · [Patiler](Golden-Paw-Trail.png) · [Varış ışığı](Golden-Arrival.png) · [1.000.001](Level-1000001.png)
