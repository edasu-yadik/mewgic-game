# PART 05D — Güçlendiriciler ve bölüm sonu

25 Ekim 2026 • C:/Dev/Mewgic • Unity 6000.3.27f1

## Tamamlananlar

Dört ayrı mor-altın düğme, ayrı yeniden kullanılabilir PNG ikonları ve canlı hak sayaçları eklendi: Geri Al, İpucu, Ek Şişe, Karıştır. Kullanılamayan düğmeler/ikonlar pasif görünür. Hak sıfırsa veya oyun paused/aktarımda/kazanılmış durumdaysa callback korumaları da kullanımı engeller. Satın alma veya reklam işlemi yoktur.

- Undo yalnız son geçerli dökmeyi geri alır. Geçersiz hamle ve boş geçmiş hak harcamaz.
- Hint, mevcut durumun BFS çözüm yolundaki ilk hamlesini önerir; kaynak seçilir ve hedef mavi vurgulanır. Aynı gösterilen ipucunu tekrar istemek ek hak harcamaz. Çözüm bulunamazsa veya 10000 durumluk arama sınırına ulaşılırsa öneri üretilmez ve hak harcanmaz. Limit ve çözümsüzlük ayrı sonuçlardır.
- Extra Bottle mevcut denemeye kapasitesi4 olan bir boş şişe ekler; deneme başına en fazla1. İkonun içindeki mor sıvı yalnız araç ikonudur; gerçek ek şişe boş/dinamik Gameplay renderer kullanır.
- Shuffle tanımı: sıvı katmanları değiştirilmeden bütün şişeler bir konum sağa çevrimsel kaydırılır. Önceki ve sonraki durumun çözümü doğrulanır; doğrulama başarısızsa hak harcanmaz. Rastgele sıvı karıştırma yapılmaz. Davranış başlangıç açıklamasında ve işlem sonucunda belirtilir.

Puzzle.cs dökme kuralları değiştirilmedi. Ek şişe ve yer değiştirme için20 satırlık topoloji/geçmiş desteği eklendi. Önceki dökme geçmişi yeni şişe dizilimine taşındığından Undo son gerçek dökmeye karşılık gelir; Undo ek şişeyi silmez. Restart orijinal bölüm dizilimini ve şişe sayısını geri getirir.

## Haklar, ödül ve kayıt

HomeResourceModel yalnız önizleme verisi olduğundan para/elmas değerlerine müdahale edilmedi. Bağımsız yerel Gameplay ilerleme kaydı oluşturuldu: PlayerPrefs anahtarı Mewgic.Progress.v1; JSON belge sürümü1.

Yeni profilin bir defalık ücretsiz başlangıç hakları: Undo3, Hint3, Extra1, Shuffle1. Kullanımlar kaydedilir; Restart, Replay, sahne veya uygulama yeniden açılışı hakları yenilemez. Ek şişe avantajı yalnız o deneme içindir; Restart harcanan hakkı geri vermez. Mağaza/yenileme/gerçek para ekonomisi eklenmedi.

Her bölümün ilk tamamlamasında gerçek bir ipucu hakkı verilir. Ödül işareti, hak bakiyesi ve en iyi yıldız sonucu aynı JSON belgesinde kaydedilir. Tekrarlanan kazanma, UI yenileme veya kayıt yeniden yükleme ikinci ödül üretmez. Daha iyi yıldız sonucu ödül tekrar verilmeden güncellenir. Kayıt hatasında hak harcanmaz/model değiştirilmez; okunamayan özgün kayıt sıfırlanmaz ve korumalı davranış kullanılır. Bu yerel kayıt çözümüdür; çevrimiçi hesap doğrulaması/senkronizasyon değildir.

Bölüm sonuna sırayla büyüyen yıldızlar, en fazla24 altın parçacık, kazanma/hamle/yıldız mesajı, ödül durumu, Tekrar Oyna, Sonraki Bölüm ve Ana Menü eklendi. Pause kazanma sunumunu da güvenli biçimde durdurur.

## Doğrulanmış sonraki bölüm

Unity MCP ile LevelMap açılıp kökleri okundu: yalnız Main Camera ve Global Light 2D; oynanabilir harita yoktur. Sahne değiştirilmedi.

Level02.json bağımsız bölüm verisi eklendi. Katalog iki mevcut/doğrulanmış seviyeyi kabul eder ve kimlik, renk miktarı, çözüm ve yıldız eşiğini doğrular:

- Bölüm1: 4 şişe,2 renk; en kısa çözüm3 hamle: 1>3 2>1 2>3.
- Bölüm2: 5 şişe,3 renk; en kısa çözüm4 hamle: 1>4 3>1 2>3 2>4.

Sonraki Bölüm tamamlanmış bölümden doğrulanmış sonraki veriye gider. İkinci bölümde var olmayan üçüncü bölüme geçiş kapalıdır; Son Bölüm Tamamlandı gösterilir. Gameplay → Bölüm2 → Home ve Home → seçili bölüm akışı test edildi. Seçili bölüm oturum içinde tutulur; tam harita/kalıcı bölüm seçimi arayüzü bu parta eklenmedi.

## Dosyalar ve koruma

Yeni core: PuzzleSolver.cs, BoosterActions.cs (hak/ödül defteri ve güvenli işlemler).
Yeni sunum: GameplayProgress.cs, GameplayLevelCatalog.cs, GameplayTools.cs, GameplayEndScreen.cs.
Yeni test: Assets/Editor/GameplayBoosterTests.cs.
Yeni veri: Assets/Resources/Levels/Level02.json.
Yeni sanat: Assets/Resources/Gameplay/Tools/Tool_Undo_v001.png, Tool_Hint_v001.png, Tool_Extra_v001.png, Tool_Shuffle_v001.png, Tool_Tile_v001.png ve meta dosyaları.

PrototypeApp Gameplay bağlantıları ve GameplayBottleView hedef vurgusu güncellendi. Home kod akışı, Level01 verisi, bütün sahneler, onaylı kedi/şişe/çevre/HUD sanatı korunur. 200 önceki sanat/prefab/animasyon/sahne dosyası SHA256 ile aynı. Geri dönüş: C:/Dev/Mewgic-Part05D-Backup. DevControl'e dokunulmadı.

## Gerçek testler

- PART05D_TEST_RESULTS.json: 723 core/gerçek UI/yerleşim kontrolü başarılı. Bunlar723 ayrı oyun oturumu değildir.
- Dört ek sınır kontrolü: bakiye varken ek şişe üst sınırı ve fazla çekmeme; değişiklik üretmeyen Shuffle'da hak harcamama; Undo kayıt hatasında hamleyi/ücreti koruma; geçersiz yıldız ödülünü reddetme başarılı.
- Geçerli/geçersiz hamle, doğru çözüm ipucu, eski ipucu vurgusunun temizlenmesi, tükenen haklar, aynı ipucunun tekrarında ücret alınmaması, aktarımda bütün araçların kilidi, Undo/Extra/Shuffle geçmişi, Restart/Replay'de hakların yenilenmemesi doğrulandı.
- İlk ödül, yeniden kazanma/Refresh'te çift ödül engeli, daha iyi yıldız kaydı ve gerçek PlayerPrefs yazma/yeniden açmada ödül işareti test edildi. Yalnız teste özel UUID anahtarı oluşturulup temizlendi; oyuncu kaydı değiştirilmedi. Diğer etkileşim testleri ayrı bellek profili kullandı; görseller test kazanmasını gösterir, oyuncunun gerçek bakiyesine ödül eklendiği iddia edilmez.
- Gameplay → Next(Level2) → Home → Gameplay, iki bölümün gerçek kazanması, son bölümde Next'in kapalı olması başarılı.
- Mevcut336 HUD ve3183 şişe kontrolü,12 model kontrolü ve Boot → Home → Gameplay → Home smoke testi başarılı. Smoke exitCode0.
- 900×1600,900×1950,900×2000 ve Safe Area simülasyonu: araçlar/sonuç paneli sınırda kalır; dokunma alanları ≥44px, şişe alanıyla araç örtüşmesi yok. Ek şişe, sonuç ve Bölüm2 için de görüntüler üretildi.
- Son C# derlemesi/Unity Console:0 hata,0 uyarı. Fiziksel Android cihazı/yeni APK ve cihaz GPU/GC profili test edilmedi.

Görsel kontrolde eksik CanvasRenderer nedeniyle görünmeyen sayaç rozetleri düzeltildi; pasif ikonlar solduruldu; geçersiz hedef sonrası eski hint vurgusu temizlendi. Bir dosya yazımındaki UTF-8 hatası, görev öncesi yedekten geri yüklenip UTF-8 ile yeniden uygulanarak giderildi. Son testler bu düzeltmelerle başarılıdır.

## Görsel kanıtlar ve kalan farklar

PART05D_GAMEVIEW.png ve PART05D_RESULT_GAMEVIEW.png gerçek900×1600 ScreenCapture çıktılarıdır. PART05D_DELIVERY_Dark.png bu iki gerçek görüntüyü yan yana gösterir. PART05D_CELEBRATION.gif gerçek Game View'dan12 karelik yıldız/parçacık önizlemesidir. gameplay-05d-* kamera/Canvas yakalamaları diğer oranlar, ek şişe ve iki bölümün sonuçlarını belgeler. PART05D_COMPARISON_Dark/Light.png referans karşılaştırmasıdır. Üretim promptları PART05D_PROMPTS.md içindedir.

Alt araçlar ayrı parlak ikonlar ve kabartmalı mor-altın zeminle referansa yaklaştırıldı. Yazı tipi/etiket formu ve kırmızı sayaç rozetleri birebir aynı değildir. Referansın örnek kullanım sayıları kopyalanmadı. Shuffle sıvıları rastgele değiştirmez; işlevi bilerek şişe yerleriyle sınırlıdır. Tam Gameplay ekranının referansla birebir final sanat eşleşmesi iddia edilmez; önceki sıvı/HUD farkları korunur.

## Teslim

Yalnız bu partın doğrulanmış dosyaları ve kanıtları commit'e alınır; eski untracked denemeler korunur. Git commit/push bilgisi teslim kaydına eklenecek. Başka part başlatılmadı.

### Kalıcı test izolasyonu
GameplayHudTests, GameplayBottleTests, GameplayVisualTests ve MewgicPlaySmoke artık kendi Run/Finish akışlarında otomatik geçici profil açıp kapatır ve doğrulanmış ilk bölüme sıfırlanır. Böylece doğrudan çalıştırılan eski testler de oyuncunun yeni kalıcı haklarını/ödüllerini tüketmez. Eski çevre testinin beklemesi mevcut dökme süresine uyarlandı. Regresyonlar sırayla ve ayrı çalıştırılır: GameplayHudTests.Run(), GameplayBottleTests.Run(), GameplayVisualTests.Run(), MewgicPlaySmoke.RunInteractive(). Yeni partın kapsamlı testi: GameplayBoosterTests.Run(). Gerçek oyuncu PlayerPrefs belgesinin değişmediği ayrıca doğrulanır.

Test zincirinin ilk denemesinde üç regresyon geçti; smoke başlatması Editor'ın Play Mode geçişi henüz tamamlanmadan çağrıldığı için durdu. Smoke asenkron Play geçişine çevrildi; kararsız toplu yardımcı teslim kapsamından çıkarılıp testler ayrı ayrı doğrulandı. İlk çıktı PART05D_QA_REPAIR.json içinde saklanır. Bu, yardımcı test yürütücüsü geçiş hatasıydı; oyuncu kayıt belgesi değişmedi.

Son bağımsız regresyonlar tekrar başarılı:336 HUD,3183 şişe,2755 çevre/slot kontrolü ve smoke exitCode0. PART05D_TEST_ISOLATION.json gerçek oyuncu kaydının değişmediğini ve test profilinin kapatıldığını doğrular. Son Console0/0; Editor Gameplay sahnesinde, Play Mode kapalı ve sahne kaydedilmemiş değişiklik içermiyor.
