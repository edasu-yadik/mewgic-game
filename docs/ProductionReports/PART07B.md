# PART07B — Etkinlikler ve ödül merkezi

25 Ekim2026 • Unity6000.3.27f1 • C:/Dev/Mewgic

## Tamamlananlar

Dört gerçek bölüm aynı dikey kaydırma merkezinde hazır:Daily Missions,Weekly Event,
Season Pass,Collection Rewards. Sekmeler bölümlere gider; etkin sekme vurgulanır.
Günlük özet gerçek alınabilir görev sayısını gösterir ve mevcut07A ekranını açar.
Günlük ekrandaki geri işlemi merkezden gelindiğinde merkeze döner. Günlük görev,
hediye ve ödül mantığı korunur.

Haftalık Kristal Şelalesi etkinliği3 gerçek kazanma ister; çubuk ve sandık gösterir.
Sandık+1 Karıştır ve mevcut yıldız görselinden Yıldız Rozeti kartını verir. Yerel
pazartesi00:00–sonraki pazartesi takvimidir; çevrimiçi takvim değildir.

Ücretsiz28 günlük sezonda2 kazanma:+1 Geri Al;4:+1 İpucu;6:+1 Ek Şişe ve mevcut
kedi görselinden Büyücü Kedi Kartı alınır. Numaralı basamaklar,bağlantı yolu ve
ilerleme çubuğu ayrı UI nesneleridir. Premium ayrı kilitli açıklama şerididir;
BİLGİ çalışır,ödeme alınmadığını belirtir. Fiyat/ürün/IAP/premium hak sistemi yok.

Koleksiyon yalnız bu iki gerçek görsel kartı içerir. Kilitliyken nasıl kazanılacağı,
sahip olunduğunda gerçek görsel önizlemesi açılır. Bunlar galeri kartlarıdır;
yeni kostüm/ekipman olarak sunulmaz. Bilinmeyen/üretilmemiş içerik kazanılamaz.

Home'da mevcut alt Events sekmesi merkeze bağlandı. Kaynaklarda yan Events ve
Collection bulunmadığından yalnız bu iki işlevsel yan giriş eklendi. Daily Gift ve
Missions korunur; görünür yeni düğmelerde boş callback yoktur.

## Gerçek olay ve kayıt

Kazanma makbuzları haftalık ve sezon listelerinde bağımsız sayılır. Günlük hedefler
dolsa da sezon ilerler. Aynı gerçek koşu bir kez sayılır; tekrar oynayıp yeniden
kazanmak yeni koşudur. Kazanma kaydı,günlük ve etkinlik sayaçları tek Commit'tedir.
Ödül hakkı,sahiplik ve alındı işareti de birlikte kaydedilir. Aynı dönem ödülü iki
kez alınamaz. Yeni dönemde güçlendirici yeniden alınabilir; kart sahipliği tek ve
kalıcıdır. Dönem bitince alınmamış ödüller sona erer. Günlük saat geri alma koruması
paylaşılır; gerçek sistem saati testlerde değiştirilmedi.

Mevcut Mewgic.Progress.v1 anahtarı korunur. Ek hub nesnesi bağımsız version=1 taşır;
eski07A kaydında yoksa boş durumla açılır. Günlük sayaçlar/hediye,mevcut haklar,
bölüm yıldızları ve harita sandıkları korunur. Yerel PlayerPrefs/sistem saati sunucu
ve anti-cheat güvenliği sağlamaz. Çevrimiçi takvim,rekabet veya ödeme eklenmedi.

## Sanat ve gerçek görüntüler

Yeni PNG veya Blender modeli üretilmedi. Önceden onaylı07A sandık/parşömen/mavi
buton,WizardCat prefabı,HUD yıldızı ve güçlendirici ikonları yeniden kullanıldı.
Haftalık çevre mevcut Crystal Falls illüstrasyonu;sandık/düğüm mevcut Map sanatıdır.
Kupa mevcut Home navigasyon ikonudur. Kedi kartı mevcut MageCat_FullBody_v001.png'dir.
Assets/Resources/Rewards/Art.asset gerçek Sprite bağlantılarını tutar; eski görseller
ve prefab kaynakları değiştirilmedi.

PART07B_OVERVIEW_GAMEVIEW, WEEKLY_READY_GAMEVIEW, SEASON_READY_GAMEVIEW,
COLLECTION_GAMEVIEW, OWNED_CARD_GAMEVIEW ve PREMIUM_INFO_GAMEVIEW gerçek Unity
ScreenCapture çıktılarıdır. Altı gerçek kazanmalı izole test profilini gösterir;
oyuncunun kaydı değiştirilmedi. PART07B_LAYOUT_* ve HOME_LAYOUT_* Unity kamera
RenderTexture oran çıktılarıdır. Referans karşılaştırması PART07B_COMPARISON_Dark.png.

Referansın kedi/sandık,yeşil çubuk,parşömen,mor-altın ve parlak ödül dili korunur.
Farklar:haftalık tema Rainbow Reef yerine mevcut Crystal Falls; sezon3 ücretsiz
basamak; koleksiyon2 mevcut kart; kedi mevcut onaylı pozdadır. Merkez kaydırmalı,
günlük görev ayrıntısı mevcut ayrı07A panelindedir. Premium ödeme gibi sunulmaz.
Sandık alındı durumu düğme ile gösterilir;yeni kapak açılış animasyonu üretilmedi.
Birebir piksel eşleşmesi iddiası yoktur.

## Test ve düzeltmeler

RewardHubTests son çalıştırmada603 kontrolü geçti. Altı gerçek bölüm kazanma,
Home Events/Collection/alt Events,günlük ekrana gidip merkeze dönme,haftalık sandık,
üç ücretsiz basamak,premium açıklaması,koleksiyon önizlemesi ve Map dönüşü test edildi.
Gerçek EventSystem raycast/dokunma olayları kullanıldı. Günlük hedefler dolduktan
sonra kazanma ilerlemesinin sürmesi açıkça doğrulandı.

Model testleri:erken/çift ödül engeli,makbuz tekrar engeli,hedefte sınır,kayıt hatası,
sayaç taşması,bilinmeyen koleksiyon/bozuk sayaç reddi,eski07A kayıt uyumu,hafta/sezon
yenilenmesi,kartların kalıcılığı,saat geri alma/toparlanma. Benzersiz PlayerPrefs
anahtarına gerçek yazıp yeniden okuyarak sahiplik ve alındı işaretleri doğrulandı.
Tam uygulama sürecini kapatıp yeniden başlatma testi yapılmadı.

900x1600,900x1950,900x2000,Safe Area inset ve360x640 kontrolleri geçti. Düğmelerin
en az44px dokunma alanı ve çakışmama;viewport dışında tıklama engeli;gerçek pointer
sürükleme ve sekmeyle kaydırma doğrulandı. Yeni Home girişleri dört çözünürlükte
logo,kedi,PLAY,navigasyon ve sağ şişeyi kapatmıyor.

İlk kurulum scriptindeki yazım hatası düzeltildi; kayıt PART07B_INITIAL_COMPILE_ERROR.json.
Ek Home testi9:20'de logoya değen yeni giriş alanını yakaladı; yalnız yeni girişler
sağa alındı ve603 kontrol yeniden geçti. Bu hata PART07B_HOME_LAYOUT_FAILURE.json
ile korunur. Onaylı logo veya şişe sanatına müdahale edilmedi.

Mevcut DailyActivitiesTests yeniden çalıştırıldı:219 kontrol başarılı.07A görev,
hediye,gerçek iki kazanma ve tek ödül davranışları korunur. Güçlendirici/bölüm sonu723 kontrol,12 model ve smoke(exitCode0) başarılı. Console0 hata/0 uyarı; test profili kapalı,Home edit modunda.

## Dosyalar ve sınırlar

Yeni:Core/RewardHub.cs;Rewards/RewardHubArt.cs,RewardHubPanel.cs;
Editor/RewardHubSetup.cs,RewardHubTests.cs;Resources/Rewards/Art.asset ve meta'ları.
Güncellenen4 mevcut script:BoosterActions.cs(kayıt/kazanma bağlantısı),
DailyActivitiesPanel.cs(yalnız tek kullanımlık geri callback'i),HomeNavigation.cs
(Events bağlantısı),PrototypeApp.cs(merkez mount).300 mevcut dosya hash kontrolü
PART07B_PRESERVATION.json içindedir. Sahne,seviye verisi ve eski sanat kaynakları aynı.
Kurallar/katalog:docs/REWARD_CENTER.md.

Fiziksel Android cihaz/FPS/bellek testi ve APK üretimi yapılmadı. Sunucu güvenliği
ve premium satış sonraki görevlerin konusudur; bu partta eklenmedi. Sonraki part
başlatılmadı. Özellik commit:dabaa03cc19047ac39b79350e861d0de0f76ef9f. Private origin/master push başarılı; git ls-remote aynı SHA döndürdü. Teslim kaydı ayrı belge commitinde tutulur.
