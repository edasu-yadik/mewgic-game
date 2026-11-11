# Gerçek maskot kostümleri ve kuşanma

11 Kasım 2026 · Unity 6000.3.27f2 · İzole oyuncu profili

## Kostümler ve Game View

| Kostüm | Fiyat | Unity Game View |
|---|---:|---|
| Büyücü Kedi | Ücretsiz | [Oyun kareleri](ClassicCat.jpg) |
| Prenses Kedi | 1.200 elmas | [Oyun kareleri](PrincessCat.jpg) |
| Kaşif Kedi | 900 elmas | [Oyun kareleri](ExplorerCat.jpg) |
| Kristal Kedi | 1.500 elmas | [Oyun kareleri](CrystalCat.jpg) |
| Gece Yıldızı | 1.800 elmas | [Oyun kareleri](NightCat.jpg) |
| Orman Perisi | 1.200 elmas | [Oyun kareleri](FairyCat.jpg) |
| Kraliyet Kedisi | 1.800 elmas | [Oyun kareleri](RoyalCat.jpg) |

Her sayfa gerçek Unity yakalamalarından oluşur. Sıra Ana Menü, Dünya Haritası, uyku, uyanış, gerinme, ayakta, yürüyüş A, yürüyüş B, oyun içi bekleme ve bölüm sonu. [Satın almadan önce yürüyüş açısı](SidePreview.jpg).

## Uygulama

- Altı kostüm ayrı dokuz karelik RGBA atlas kullanır. Pembe taç ve elbise, keşif şapkası ve fener, kristal taç ve pelerin, hilalli şapka, çiçekli kanat ve kraliyet tacı ile kürklü pelerin çizilmiş aksesuarlar. Mevcut maskotun mavi gözleri, tekir desenleri, beyaz patileri ve yüz yapısı görsel referans olarak korundu.
- Ana menü ve oyun içi mevcut nefes alma rig’ini sürdürür. Kutlama sırasında gerçek kutlama sprite’ı görünür. Bölüm haritasının mevcut 99 → 100 yürüyüşünde uyku, uyanış, gerinme, ayağa kalkış ve iki yürüyüş karesi aynı kostümden alınır. Dünya haritasında aktif adanın yanında oturma pozu gösterilir.
- Kostüm kartına veya satın alma düğmesine basınca iki açı oyunda kullanılan sprite’larla gösterilir. Ana mağaza sayfalarının tasarımı ve sıralaması korunur.
- Elmas düşümü ve sahiplik mevcut işlem kaydında birlikte yazılır. Altın kullanılmaz. Tekrarlanan istek ve sahip olunan ürünü tekrar alma engellenir. Satın alma, kuşanma, kostümler arası geçiş ve ücretsiz Büyücü Kedi’ye dönüş farklı işlemlerdir. Eski sahiplik kayıtları aynı kimliklerle açılır.

## Eksik açılar ve animasyon varlıkları

- Tam arka ve özel sol profil çizimleri yok. Harita ters yöne dönerken sağ yürüme karelerini aynalar. Önizleme iki gerçek açı sunar.
- Özel kostümlerde kapalı gözlü uyku, yarı açık gözlü uyanış ve gözleri kısık kutlama kareleri var. Mevcut Büyücü Kedi’ye ait ayrı göz kırpma maskesi yeni kostümlerin yüzüne uygulanmaz. Her kostüm için hizalanmış ön göz kırpma maskesi ayrıca gerekir.
- Taç, pelerin, fener ve kanatlar her pozda kedinin üzerine çizildi. Ayrı kemikli pelerin salınımı ve kanat çırpma için omuz ankrajlı katmanlar, ara yürüyüş kareleri ve bağımsız animasyon gerekir.
- Yeni yüz kareleri referansın yeniden çizimleridir. Yüzün orijinal dokusu piksel düzeyinde tekrar kullanılmadı. Zaferde maskot arkasındaki yerel fon yamasıyla ana çerçeve arasında hafif bir doku geçişi kalıyor.

## Doğrulama ve dosyalar

- [345 kostüm Play Mode kontrolü](PLAY_MODE.json) · Altı kostümde iki açı, satın alma, tekrar ücret engeli, kuşanma, yeniden açılan kayıt ve sahne entegrasyonu. Varsayılan ve altı kostüm için Kristal Şelaleler’in gerçek uyku → uyanma → yürüme akışı ve zafer kutlaması çalıştırıldı. Zafer testi tahta çözümünü gerçek döküş kurallarıyla uygulayıp mevcut sonuç akışını tetikledi.
- [722 mağaza regresyon kontrolü](SHOP_PLAY_MODE.json) · Dört mağaza sayfası, üç portre oranı, bakiye ve satın alma durumları.
- Tekrar çalıştırma. Unity’de `Mewgic → QA → Run Costume Integration`. Son koşuda alan yeniden yükleme açık kullanıldı ve Editör tercihi sonrasında geri yüklendi. Elle inceleme. `Home → Mağaza → Kostümler`.
- Kod. `CostumeArt.cs`, `WizardCatPresentation.cs`, `MapWorld.cs`, `MapWorldJourneyMotion.cs`, `GameplayEndScreen.cs`, `MagicShopPanel.cs`, `MagicShopProbe.cs`.
- Varlıklar. `Assets/Resources/CatCostumes/` altında altı RGBA atlas, kare tanımları ve yalnızca giydirilmiş zafer maskotunun arkasında kullanılan küçük fon parçası. Paketleme aracı `tools/CostumeArt/pack_poses.py`.
- Fiziksel cihaz testi, mobil build ve bulut dağıtımı yapılmadı.
