# MEWGIC — UI uygulama yol haritası

PART 01 / 23 Ekim 2026. Bu aşama plan ve klasör hazırlığıdır.
Puzzle.cs, PrototypeApp.cs, Level01.json, mevcut sahneler ve paket ayarları bu
tasarım çalışması kapsamında değiştirilmez. Referans kalitesi henüz uygulanmadı.

## Tamamlanan PART 01 işleri
- Beş gerçek PNG incelendi, rolleri ve geçerli kimlik sırası kaydedildi.
- References/Visual/Approved içine kaynak kopyaları ve manifest eklendi.
- Mevcut kod/sahne/URP/UI/test envanteri ve fark analizi hazırlandı.
- Renk, tipografi, 9:16, Safe Area, katman, prefab ve atlas sözleşmesi tanımlandı.
- Asset üretim listesi, tür/boyut/animasyon/öncelik bilgisiyle hazırlandı.
- İlgili tasarım klasörleri oluşturuldu; boş klasörler .gitkeep ile saklanır.
- Yerel Git başlangıç kaydı: 9ebcae0. Uzak depo veya GitHub bağlantısı kurulmadı.
- Son doğrulama kaydı: PART01_VALIDATION.md.

## PART 02 — İlk görsel üretim paketi
Bağımlılık: VISUAL_DESIGN_SYSTEM.md ve ASSET_PRODUCTION_PLAN.md.
İşler: Master'a sadık kedi; metinsiz arka plan katmanları; cam arka/ön/maske/rim;
Primary, Secondary, panel ve temel ikonlar. Önce bir şişe ve bir kedi denemesi.
Çıktı: alpha/pivot/maske hizası doğrulanmış, sürüm ve kaynakları kayıtlı dosyalar.
Kabul: kimlik ve ışık referansla yakın; yazı ve kontroller raster içine gömülü değil.
Bu kapı geçmeden tüm ekran assetleri topluca üretilmez.

## PART 03 — Tekrar kullanılabilir UI altyapısı
İşler:
1. UI theme tanımını renk/font/sprite alanlarıyla oluştur.
2. SafeAreaRoot davranışını mevcut uygulamadan tek ortak bileşene çıkar.
3. Primary/Secondary/Icon buton ve panel prefab'larını oluştur.
4. Font kaynağı/lisansı ve Türkçe karakterleri doğrula; gerekiyorsa TMP geçişini yap.
5. Statik çevre, dinamik oyun, HUD ve Modal katmanlarını oluştur.
Kabul: 900×1600, 900×1950 ve 1080×2400'de uzun Türkçe etiketler sığar;
buton görselinden bağımsız hit alanı ve basılı/devre dışı durumları çalışır.
Paket eklemeden önce projede mevcut araçlar kullanılır.

## PART 04 — Home görünümünü değiştir
İşler: HomePresenter ile PrototypeApp.Home sorumluluğunu görünümden ayır;
arka plan, ayrı kedi, dekoratif şişeler ve yeşil Play'i onaylı assetlerle bağla.
Kabul: Boot → Home → Gameplay aynı çalışır; katmanlar bağımsızdır.
Referanstaki ekonomi/yan menü/alt navigasyonun sadece işlevi tanımlı parçaları eklenir.
Sahte bakiye veya çalışmayan mağaza butonu teslim edilmez.
Eski görünüm Git kaydıyla geri alınabilir kalır.

## PART 05 — Gameplay şişe ve platform görünümü
İşler:
1. BottleView görünümünü prefab'a çıkar; model verisi sadece okunur.
2. Board görünümünü bir kez oluştur; her hamlede Destroy/Create yerine güncelle.
3. Mevcut seçim, geçersiz hedef, hamle sayısı ve Undo/Restart davranışını koru.
4. Cam katmanlarını ve sıvı maskesini bağla; katman kimliklerini değiştirme.
5. Boyun çıkış noktası → hedef ağzı animasyonu ve kaynak/target vurgusunu ekle.
6. Aktarım sırasında kilidi tüm kontrollere tutarlı uygula.
Kabul: mevcut 12 model kontrolü ve Play Mode senaryosu geçer; dört şişelik bölüm
aynı üç hamleyle çözülebilir; sıvı yanlış şişede/yanlış miktarda gösterilmez.
Daha çok şişeli düzen için ayrı test verisi kullanılır; gerçek bölüm değişikliği
ayrıca planlanmadan Level01 üzerine yazılmaz.

## PART 06 — FX ve mobil kalite
İşler: sınırlı kabarcık, değnek/şişe parıltısı, dökme akışı, kısa kazanma animasyonu,
atlaslar ve cihaz kalite profilleri. Shader UI clipping ve alpha kenarlarını doğrula.
Kabul: en az bir hedef Android cihazında kurulum, gerçek dokunma, çentik,
çözünürlük, GPU bellek ve kare süresi ölçümü. 60/30 FPS hedefleri ölçümle değerlendirilir.
Input callback testi fiziksel cihaz kontrolünün yerine geçmez.

## Sonraki ürün aşamaları
İlk 10 bölüm, ilerleme kaydı, LevelMap; ardından ayrıca tanımlanırsa görev/etkinlik,
ekonomi ve sosyal içerik. 03_Map ve 04_Events bu aşamada yalnız görsel rehberdir.
Yıldız eşikleri, booster etkileri, can ve Combo ayrı oyun tasarımı kararı ister.
GitHub/DevControl bağlantısı bu yerel Git koruma kaydından ayrı bir iştir.

## Yeniden çalıştırma
Unity kapalıyken model kontrolü:
Unity.exe -batchmode -nographics -projectPath C:/Dev/Mewgic -buildTarget Android
-executeMethod MewgicPrototypeSetup.TestModel -quit -logFile docs/part01-model-tests.log

Play Mode kontrolü (Run kendi sonunda Editor'ü kapatır; -quit eklenmez):
Unity.exe -batchmode -projectPath C:/Dev/Mewgic -buildTarget Android
-executeMethod MewgicPlaySmoke.Run -logFile docs/part01-play-tests.log

Komutlar burada okunabilirlik için satırlara ayrılmıştır; terminalde tek komut olarak verilir.
Aynı proje başka Editor örneğinde açıksa ikinci örnek çalıştırılmaz.
Kullanıcıda kaydedilmemiş sahne varsa zorla kapatılmaz; kaydetme penceresi kullanıcıya bırakılır.
Android APK yalnız kod/asset değişen sonraki aşamada yeniden alınır; PART 01 için önceki
Mewgic-Prototype.apk korunur.

## Dosya düzeni
| Dizin | Amaç |
| --- | --- |
| Assets/Art/Backgrounds | Uzak/orta/ön çevre PNG'leri |
| Assets/Art/Characters/MageCat | Kedi ve karakter katmanları |
| Assets/Art/Bottles | Cam, maske, rim, sıvı kaynakları |
| Assets/Art/UI/Common ve Icons | UI yüzeyleri, süsler ve ikonlar |
| Assets/Art/FX ve Atlases | FX sprite'ları ve gelecek atlas varlıkları |
| Assets/UI/Prefabs/Common, Home, Gameplay | Tekrar kullanılabilir UI ve ekran prefab'ları |
| Assets/UI/Themes | Gelecek tema/font/sprite referansları |
| Assets/Prefabs/Gameplay/Bottles ve FX | Şişe görünümü ve efekt prefab'ları |
| Assets/Materials/UI/Bottles ve Shaders/UI | Gelecek sıvı/cam UI materyal/shader'ları |
| Assets/Animations/UI, Characters, FX | Gelecek animasyon klip/controller'ları |

Bunlar klasör hazırlığıdır; henüz prefab, animasyon klibi, atlas veya shader oluşturulmadı.
