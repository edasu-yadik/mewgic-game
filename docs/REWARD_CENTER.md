# Çevrimdışı etkinlik ve ödül merkezi

PART07B. Ana menüde Events yan düğmesi haftalık bölüme, Collection koleksiyona;
alt Events sekmesi merkezin başına açılır. Daily Missions mevcut07A ekranını
korur; günlük ekranın geri düğmesi merkezden gelindiğinde merkeze döner.

## Gerçek ilerleme

Her gerçek kazanılmış koşunun GUID makbuzu hem haftalık hem sezon sayacına
ayrı yazılır. Günlük hedeflerin dolması bu sayaçları durdurmaz. Tekrar oynanan
bölümleri yeniden kazanmak da sayılır. Sadece düğmeye basmak veya bitmemiş
bir bulmaca ilerletmez. Normal bölüm kazanma kaydı ve bütün sayaçlar aynı
ProgressLedger Commit'inde kaydedilir.

Hafta yerel pazartesi00:00'da başlar, sonraki pazarteside yenilenir.3 kazanma
haftalık sandığı açar:+1 Karıştır ve Yıldız Rozeti koleksiyon kartı.

Ücretsiz sezon28 yerel takvim günüdür; dönemler05.01.2026 referans tarihine
hizalanır.2 kazanma:+1 Geri Al;4 kazanma:+1 İpucu;6 kazanma:+1 Ek Şişe ve
Büyücü Kedi Kartı. Her basamağın bağımsız alındı işareti vardır.

Dönem bitince alınmamış o dönem ödülleri sona erer. Yeni hafta yalnız haftalık
sayaç/işareti; yeni sezon yalnız sezon sayaç/işaretlerini sıfırlar. Kazanılmış
koleksiyon kartları kalıcıdır. Yeni dönem güçlendirici ödülü tekrar alınabilir,
ancak aynı koleksiyon kartı envantere iki kez eklenmez.

Bu takvim yerel ve çevrimdışıdır. Küresel yayın takvimi/yarışma/lider tablosu
ve sunucu yoktur. Ekranda açıkça ÇEVRİMDIŞI yazılır. Mevcut günlük saat geri
alma koruması paylaşılır; UTC5 dakikadan fazla veya yerel tarih geriye giderse
etkinlik kazanımı ve ödülü durur. Yerel saat ve PlayerPrefs güvenilir sunucu
ve anti-cheat güvenliği sağlamaz.

## Koleksiyon kataloğu

Yalnız iki mevcut, gerçek görsel kullanılır:

- star_badge.v1: Assets/Resources/Gameplay/HUD/HUD_Star_v001.png.
- wizard_card.v1: Assets/Art/Characters/WizardCat/MageCat_FullBody_v001.png.

Bunlar galeride görüntülenen kart/rozetlerdir; yeni karakter kostümü, ekipman
veya henüz üretilmemiş bir varlık değildir. Kilitli kart düğmesi kazanma yolunu,
sahip olunan kart düğmesi gerçek görsel önizlemesini açar. Bilinmeyen koleksiyon
kimliği kayıt doğrulamasında reddedilir. Assets/Resources/Rewards/Art.asset,
bu varlıklar ile mevcut haftalık ada/sandık/düğüm/kupa kaynaklarını referanslar;
kaynak PNG'leri değiştirilmez. Haftalık tema mevcut Crystal Falls sanatıdır.

## Premium sınırı

Premium ayrı bir açıklama şerididir. BİLGİ düğmesi, özelliğin henüz hazır
olmadığını ve ödeme alınmadığını açıklar. Ürün kimliği, fiyat, ödeme aracı,
premium hak veya satın alma/ödül API'si bulunmaz. Yalnız üç ücretsiz basamak
çalışır; premium basamağı istemek kayıt modeli tarafından kabul edilmez.

## Kayıt

Mevcut Mewgic.Progress.v1 anahtarı ve günlük/harita verileri korunur. Ek hub
nesnesinin bağımsız version=1 alanı vardır. Eski kayıtta hub yoksa boş durumla
açılır. Dönemler, sınırlı makbuz listeleri, ilerleme, alındı işaretleri ve iki
bilinen kartın sahipliği birlikte saklanır. Hak artışı ve sahiplik/işaret tek
Commit'tedir; yazma hatası veya sayaç taşmasında ikisi de değişmez.

## Test yürütücü

Editor Play kapalıyken RewardHubTests.Run() model, benzersiz PlayerPrefs anahtarı
ile yeniden yükleme, altı gerçek kazanma, ücretsiz/haftalık ödüller, premium
bilgi ve koleksiyon önizlemesini sınar. Oyuncu kaydı izole profille korunur.
Üç ekran oranı,Safe Area ve360x640 için kaydırma, en az44px hedefler,
çakışmayan ödül hit bölgeleri ve viewport dışında tıklama engeli kontrol edilir.
Fiziksel cihaz testi ve tam uygulama süreci yeniden başlatması ayrıca gerekir.
