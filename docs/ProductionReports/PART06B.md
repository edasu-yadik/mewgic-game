# PART 06B — Seviye ilerleme sistemi

25 Ekim 2026 • C:/Dev/Mewgic • Unity 6000.3.27f1

## Tamamlananlar

Haritanın bağımsız düğümlerine gerçek numara, tamamlanma rengi, kaydedilmiş üç yıldız, mevcut bölümün parlama/altın işareti ve kapalı konum kilitleri eklendi. Bölge etiketleri gerçek tamamlanan/toplam bölüm ve yıldız toplamını gösterir. Köy2/2 tamamlanınca tamamlama göstergesi ve sandık hakkı açılır. Diğer bölgeler gezilebilir ancak doğrulanmış bölüm olmadığı için oynanamaz; açıkça kilitli/hazırlanıyor gösterilir.

Harita→Gameplay→Harita, tamamlanan seviyeyi tekrar oynama, Pause'dan haritaya dönme ve Home alt Map sekmesi bağlandı. Gameplay'deki geri/sonuç/Pause etiketleri başlangıç kaynağına göre HARİTA veya ANA MENÜ gösterir. Home üzerinden başlayan önceki akış korunur. Oyun şişesi modeli ve dökme algoritması değiştirilmedi. Hiçbir sahne veya onaylı görsel kaynak değiştirilmedi.

## Gerçek içerik ve kapsam

Projede DesignData/Levels dosyaları bulunamadı (SourceArt içi dahil proje yolları kontrol edildi). Mevcut Resources/Levels/Level01.json ve Level02.json çözücü ile yeniden doğrulandı. Çözümleri sırasıyla3 ve4 hamledir; gerçek Puzzle üzerinde uygulandığında kazanırlar. Bu iki JSON değiştirilmedi. Köy yalnız bu iki gerçek bölümü kapsar; üçüncü konum0/henüz hazır değil alanıdır. Sonraki3–11 kimlikleri gelecek harita konum tanımlarıdır, hazırlanmış içerik değildir ve kataloğa alınmaz. Yalnız dosya eklemek veya kayda bir sonuç yazmak seviye açmaz.

Bölge geçiş mantığı önceki bölgelerin tamamlanmasını ve yeni bölgede doğrulanmış içerik bulunmasını ister. Gelecek bölgelerin tamamlanma/açılma zinciri sentetik veri sağlayıcıyla model testinde sınandı; gerçek oyuna test seviyeleri eklenmedi. Bu sürümde Crystal Falls/Moon Garden/Sky Castle'a gerçek bölüm geçişi yapılamaz. Bu sınır eksik içeriği otomatik açmama talimatının sonucudur.

## Kayıt ve ödül

Mevcut ProgressLedger ve Mewgic.Progress.v1 anahtarı korunur. claimedMilestones ek alanı eski v1 belgelerinde yoksa boş listeyle okunur; yıldızlar ve hak bakiyeleri korunur. En iyi yıldız sonucu azalmaz. Ayrı MilestoneReward modeli yalnız yerel güçlendirici hakkı sunar; bu sürümde bölge sandığı+1 İpucu verir. Köydeki iki bölüm bitmeden alınamaz. Ödül artışı ve alındı işareti aynı Commit belgesindedir; yazma hatasında ikisi de uygulanmaz, tekrar tıklamada ödül çoğalmaz. Gerçek para, mağaza veya çevrimiçi servis eklenmedi.

## Gerçek testler

- MapProgressionTests:200 başarılı kontrol. Doğrulanmış iki bölümün çözümü, kilit sırası, tamamlananı tekrar oynama, mevcut seviye/yıldız UI eşleşmesi, gerçek raycast/tıklamalarla harita→iki ayrı kazanma→harita, sandık, Home Map sekmesi ve Pause dönüşü sınandı.
- Eski v1 kayıt uyumu, düşük yıldızlı tekrarın en iyi sonucu düşürmemesi, bilinmeyen kaydın seviye doğrulayamaması, yanlış sıralı tamamlamanın kilidi/sandığı atlayamaması, yinelenen harita/ödül kimliği ve sayaç taşması sınandı.
- Benzersiz test PlayerPrefs anahtarına gerçek kayıt ve yeniden okuma yapıldı; sandık ve yıldızların yeniden yüklenmesi/çift ödül engeli doğrulandı. Yapay disk yazma hatasında ödül ve alındı işareti değişmedi.
- Oyuncunun gerçek kayıt anahtarının varlık/içerik değeri test öncesi ve sonrası aynı kaldı. Play akışları izole test profili kullandı.
- MapWorldTests:551 yerleşim/etkileşim kontrolü. Eski salt önizleme varsayımları yeni gerçek bölüm akışına güncellendi.900x1600,900x1950,900x2000,Safe Area inset ve360x640 kontrol edildi; kaydırma ve kapalı düğüm/sandık açıklamaları çalışır.
- Önceki GameplayBoosterTests:723 kontrol başarılı. Undo/Hint/Extra/Shuffle, ödül tekrar engeli, iki gerçek bölüm ve Home'a dönüş korunur.
- Mevcut12 temel model kontrolü ve Boot→Home→Gameplay→Home smoke (exitCode0) başarılı. Son Console:0 hata,0 uyarı.274 mevcut dosyanın hash karşılaştırmasında yalnız beklenen8 script değişti; tüm sahneler, sanat varlıkları ve Level01/Level02 kaynakları aynı.
- Derleme sırasında eski PremiumPlayButton.light alanının Component.light ile ad çakışması uyarısı görüldü; yalnız private alan frameLight olarak yeniden adlandırıldı. Buton görseli ve davranışı değiştirilmedi.

## Görsel kanıt ve farklar

PART06B_INITIAL_GAMEVIEW.png, PART06B_PROGRESS_GAMEVIEW.png ve PART06B_COMPLETED_GAMEVIEW.png gerçek Unity ScreenCapture çıktılarıdır. Sırasıyla yeni profil, ilk bölümden dönüş ve köy/sandık tamamlanma durumlarını gösterir. Bunlar test profili gösterimleridir; oyuncunun gerçek ilerlemesi değildir. PART06B_LAYOUT_* kamera RenderTexture ile oran test çıktılarıdır. PART06B_COMPARISON_Dark.png onaylı03_Map referansını ilerleme görüntüsüyle yan yana gösterir.

Düğüm numaraları, altın yıldızlar, yeşil tamamlanma ve mor kilitler referans diline yakındır. Düğüm biçimi önceden onaylı boş medalyon sprite'ıdır; kenar/ölçek referansın tam kopyası değildir. Kilit küçük vektör UI simgesidir. Sandık önceki mor-altın sprite'ı kullanır; alındığında onay işareti gösterilir, yeni açılma illüstrasyonu üretilmedi. PART06A'nın seyrek çevre düzeni ve büyülü bağlantı yolları korundu. Referanstaki üst ekonomi/yan etkinlikler/kedi bu görev kapsamında haritaya eklenmedi. Nihai birebir görsel eşleşme iddiası yoktur.

## Dosyalar

Yeni: Core/MapProgression.cs, Map/MapProgressionCatalog.cs, Map/MapWorldProgression.cs, Resources/Map/World.json, Editor/MapProgressionTests.cs ve Unity meta dosyaları.

Güncellenen: MapWorld.cs, Core/BoosterActions.cs (yalnız ledger/kayıt uzantısı), GameplayLevelCatalog.cs, PrototypeApp.cs (geri dönüş bağlamı), GameplayHud.cs, GameplayEndScreen.cs (geri etiketi), HomeNavigation.cs (hazır harita girişi), PremiumPlayButton.cs (private alan adı), MapWorldTests.cs. Mimari/katalog ekleme kuralları docs/MAP_PROGRESSION.md içindedir. İlerleme planı ve bu rapor güncellendi.

## Sınırlar ve teslim

Fiziksel Android cihazda test/FPS/bellek ölçümü ve APK üretimi yapılmadı. Yeni bölgeler için gerçek, çözülebilirliği doğrulanmış seviye içeriği gerekir. Bir sonraki part otomatik başlatılmadı. Console son durum ve dosya koruma kanıtı PART06B_FINAL_UNITY.json / PART06B_PRESERVATION.json içinde tutulur. Özellik commit:91de3608dd161063b17fce671ce7a65fa3498e7f. Private origin/master push başarılı; git ls-remote aynı SHA döndürdü. Teslim kaydı ayrı belge commitinde tutulur.
