# Mağaza animasyonları ve etkileşimleri

## Uygulama

- Özel Teklifler, Altın Paketleri, Oyun Güçleri ve Kostümler aynı yerleşim ve sanatla açılıyor. Önceki ve yeni sayfa 0,24 saniyede yatay kayarak birbirine karışıyor. Seçilen sekmede kısa altın yansıması var. Her sekme kaydırma konumunu oturum içinde saklıyor. Önceki sayfanın düğmeleri geçiş sırasında tıklanamıyor.
- Güç alımı kayıt başarılı olduğunda kart ışığını ve en çok sekiz parçacığı oynatıyor. Ürün simgesi stok etiketine kavisli bir yol izliyor. Altın bakiyesi 0,67 saniyelik akışta azalıyor ve stok etiketi varışta kısa süre büyüyor.
- Kostüm alımı elmas sayacını düşürüyor ve satın alınan kostümü gerçek maskot çizimiyle açıyor. Kuşanma ayrı bir işlem. Kuşan düğmesi kayıtlı sahipliğe göre hemen güncelleniyor. Kuşanma eski ve yeni portre arasında 0,72 saniyelik geçiş oynatıyor. Son kare mevcut maskot portresine oturuyor.
- **Atla** sayacı ve stok etiketini kayıtlı son değere getiriyor. Azaltılmış hareket ayarı geçişleri ve kutlamayı atlıyor. Başarısız veya iptal edilen alım kutlama başlatmıyor.
- Başlangıç Paketi ve öne çıkan 10.000 altın kartında hafif bir yansıma var. Sayaç yalnızca sağlayıcının doğruladığı bitiş zamanı geldiğinde gösteriliyor. Süre dolunca yeni alım açılmıyor. Mevcut tekliflerde doğrulanmış bitiş zamanı olmadığı için sayaç görünmüyor.
- Efektler mevcut mağaza görsellerini ve sekiz sabit parçacık nesnesini tekrar kullanıyor. Bakiye ve stok metni sayı değiştiğinde yeniden yazılıyor.

## Gerçek Unity Game View

Unity 6000.3.27f2 derlemesi 0 hata ve 0 uyarıyla tamamlandı. İzole Play Mode profilinde 822 mağaza ve ekonomi kontrolü geçti. 900×1600, 900×1950 ve 900×2000 portre boyutları kullanıldı. Animasyon örnekleri 900×1600 Game View karelerinden birleştirildi. Oyuncunun kayıt yuvaları test boyunca aynı kaldı.

| Akış | Kanıt |
|---|---|
| Sekme geçişi | [Game View GIF](Tab.gif) |
| Güç satın alma | [Game View GIF](Power.gif) |
| Kostüm açılması | [Game View GIF](Unlock.gif) |
| Kostüm kuşanma | [Game View GIF](Equip.gif) |
| Test ayrıntıları | [Play Mode raporu](PLAY_MODE.json) |

Tekrar alışta aynı efekt nesnelerinin kullanıldığı, düşük bakiyenin kutlama başlatmadığı, **Atla** ve azaltılmış hareket seçeneklerinin doğru bakiyeye döndüğü, test teklifinin süresi dolduğunda yeni alışın engellendiği kontrol edildi. Cihaz performansı ve gerçek mağaza süre verisi bu turda test edilmedi. Yeni mobil build alınmadı.

## Değişen uygulama dosyaları

- [MagicShopPanel.cs](../../../Assets/Scripts/Presentation/MagicShopPanel.cs)
- [MagicShopPanel.Motion.cs](../../../Assets/Scripts/Presentation/MagicShopPanel.Motion.cs) ve Unity meta dosyası
- [StorePayments.cs](../../../Assets/Scripts/Presentation/StorePayments.cs)
- [MagicShopProbe.cs](../../../Assets/Scripts/Tests/MagicShopProbe.cs)
