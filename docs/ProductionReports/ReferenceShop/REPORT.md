# Onaylı mağaza referansları

11 Kasım 2026 · Unity 6000.3.27f2 · Home sahnesi

## Ekranlar

- **Özel Teklifler.** Başlangıç Paketi ve Oyun Gücü Paketi; ayrı içerik satırları, indirim rozeti, üzeri çizili eski fiyat, yeni fiyat ve yeşil CTA. Altta 100, 250 ve 1.000 altınlık üç kart.
- **Altın Paketleri.** %30 bonuslu 10.000 altın üst teklifi, dört küçük paket ve 10.000 altının alttaki yatay kartı. Her kartın katalog kimliği ve önizleme fiyatı ayrı.
- **Oyun Güçleri.** Altı yatay kart, ayrı ikon, açıklama, sahip olunan miktar, fiyat ve düğme. Ek Şişe 800, İpucu 1.000, Karıştır 1.200 ve Renk Değiştir 1.500 altın. Ekstra Hamle ve Şişe Çekici 500 ve 1.800 altınlık katalog önizlemesi açıyor. Mevcut bulmacada bu iki güç için uygulanabilir mekanik olmadığı için stok veya bakiye yazmıyor.
- **Kostümler.** Mevcut kostüm, yeşil kuşanma düğmesi ve altı kartlık grid. Prenses 1.200, Kaşif 900, Kristal 1.500, Gece Yıldızı 1.800, Orman Perisi 1.200 ve Kraliyet 1.800 elmas. Satın alma ile envanter kaydı aynı işlemde yazılıyor. Eski sahiplikler korunuyor. Önceden edinilmiş şişe ve efektler, kostüm sayfasının sonundaki Sahip Olduklarım ekranından kuşanılabiliyor. Efektler sekmesi yok.

Paket görselleri, başlık dekoru, maskot, fon, ikonlar ve kostüm portreleri onaylı referanslardan çıkarılan ayrı görsel katmanlar. Unity'de fiyat, metin, bakiye, sekme, satın alma ve kuşanma bağımsız ve canlı. Kart çerçeveleri, gradyan, parıltı, rozet konturu ve tipografi referansa göre yeniden kuruldu. Bu ayrıntılar piksel düzeyinde aynı görünmüyor. Kuşanılan yeni kostümler mevcut hareketli maskotta renk teması kullanıyor. Mağaza portrelerindeki kanat, taç veya aksesuarların ayrı animasyonlu sürümleri bulunmuyor. Yeni vitrin metinleri Türkçe.

Gerçek para fiyatları görsel katalog önizlemesi olarak gösteriliyor. CTA, bunun ödeme başlatmayan bir önizleme olduğunu açıklıyor. Kostüm ve mevcut güç satın alımı yalnızca oyun içi bakiye kullanıyor.

## Dosyalar

- UI ve giriş. `MagicShopPanel.cs`, `PrototypeApp.cs`.
- Ekonomi, kayıt ve kostüm. `MagicShop.cs`, `EconomyConfig.cs`, `Balance.json`, `ShopCosmetics.cs`.
- Kaynaklar. `Assets/Resources/ShopReference` altındaki `Art.png`, `ArtRegions.json`, `Offers.json`, `ShopPrice.asset` ve Unity metaları. Atlası yeniden oluşturan kaynak `tools/ShopReference/slice_art.py`.
- Test. `MagicShopProbe.cs` ve yeni sekme/fiyata uyarlanan `LevelProgressionProbe.cs`.
- Mağaza mevcut Home sahnesinde çalışma zamanında oluşturuluyor. Sahne veya prefab dosyası değişmedi.

## Unity doğrulaması

549 Play Mode kontrolü. Dört sekme 900 × 1600, 900 × 1950 ve 900 × 2000 boyutlarında açıldı. Metin taşması, tıklama alanları, gerçek kaydırma girdisi, teklif önizlemesinin ödeme başlatmaması, yetersiz altın/elmas, güç stok güncellemesi, tekrarlanan makbuz, elmasla kostüm alma, kuşanma ve kayıt yeniden açılışı denendi. Console hata sayısı sıfır ve test öncesi/sonrası oyuncu kaydı özeti eşit. Android/iOS cihaz testi ve mobil build yapılmadı.

- [Özel Teklifler](Offers.png)
- [Altın Paketleri](Gold.png)
- [Oyun Güçleri](Powers.png)
- [Kostümler](Costumes.png)
- [Play Mode çıktısı](PLAY_MODE.json)

Elle inceleme için Home → Mağaza. İzole otomatik test için `Mewgic → QA → Run Magic Shop`.

Mağaza fiyatı ve yeni sekme akışıyla birlikte [346 ilerleme regresyon kontrolü](PROGRESSION_REGRESSION.json) geçti. Beş yüksek eşik, eski bölüm tekrarı, yeni fiyatla Renk Değiştir satın alma ve gerçek oynanışta stok düşümü tekrar doğrulandı.
