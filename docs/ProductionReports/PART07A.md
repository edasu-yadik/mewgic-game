# PART07A — Günlük görevler ve hediyeler

25 Ekim 2026 • Unity6000.3.27f1 • C:/Dev/Mewgic

## Tamamlanan sistem

Dört günlük görev gerçek oyun olaylarına bağlandı: kazanılan bölümlerde4 tamamlanmış şişe(+1 Undo),2 kazanma(+1 Hint),6 kazanılmış yıldız(+1 Extra Bottle),1 başarılı güçlendirici kullanımı(+1 Shuffle). Şişe sayımı yalnız kazanılmış koşunun dolu/tek renkli şişeleridir; ara dökme/Undo/Restart ile farm edilmez. Tekrar oynayıp yeniden kazanmak ayrı gerçek koşu olarak sayılır. Her koşu GUID makbuzu ile aynı gün bir kez sayılır; değerler hedefte durur. Kazanma ve günlük sayaç aynı kayıt Commit'inde; gerçek hak harcaması ve güçlendirici görevi de aynı Commit'inde tutulur. Alınmamış ödül erken verilemez; alındı işareti ile+1 hak aynı belgeye gider.

Günlük hediye+1 İpucu. İlk gün hemen, sonra yeni yerel gün VE en az20 saat UTC aralığı gerekir. Atlanan günler birikmiş hediye oluşturmaz. En büyük görülen UTC zamanı ve yerel gün kaydedilir. Saat5 dakikadan fazla veya yerel tarih geriye giderse günlük işlemler bekler; normal bulmaca oynanabilir. Yerel gece yarısında görevler yenilenir; önceki günün alınmamış görev ödülleri sona erer. Bilgisayarın saati değiştirilmedi; testlerde saat enjeksiyonu kullanıldı.

Yerel PlayerPrefs sunucu güvenliği sağlamaz. Kayıt düzenlenebilir ve ileri saat değişimleri güvenilir biçimde ayırt edilemez. İleri tarih sonrası normal saate dönmek günlük işlemleri kayıtlı tarihe yetişene kadar kilitleyebilir. Sunucu, satın alma, reklam veya para ekonomisi eklenmedi.

## Kayıt koruması

Mewgic.Progress.v1 anahtarı ve önceki alanlar korunur. daily nesnesinin bağımsız version=1 alanı vardır; eski kayıtta yoksa sıfır günlük durumla açılır. Bölüm yıldızları, haklar ve harita sandıkları korunur. Geçersiz sürüm/veri mevcut kayıt hata korumasıyla engellenir; özgün anahtarın üstüne yazılmaz. Kaydetme hatasında hediye/görev ödülü ve alındı işaretleri uygulanmaz.

## UI ve sanat

Kaynaklarda önceki Daily Gift/Missions yan butonları bulunmadığından yalnız bu iki gerçek Home giriş düğmesi eklendi. Logo, kedi, PLAY ve navigasyonla çakışmadıkları dört çözünürlükte sınandı. İki düğme DailyActivitiesScreen'in görev/hediye sekmelerini açar; GİT mevcut doğrulanmış başlangıç bölümüne gider. Ekran Home Canvas'ında Safe Area altında gerçek UI katmanıdır; yeni sahne veya tek bir ekran resmi kullanılmadı.

Üç ayrı ImageGen PNG üretildi: Assets/Art/UI/Daily/Daily_Chest_v001.png, Daily_Parchment_v001.png, Daily_Button_v001.png. Ham dosyalar korunur; alpha sınırına göre Sprite rect ve panel border'ı Art.asset içinde hazırlanır. Kaynak alpha0–yüksek opaklık import kontrolünden geçti; Read/Write/mipmap kapalı, Android ASTC4x4/en fazla2048. Asset bağlantıları Assets/Resources/Daily/Art.asset içindedir. Mevcut WizardCat.prefab, yıldız ve güçlendirici ikonları, Home gökyüzü/kale yeniden kullanıldı; onaylı kaynakları değiştirilmedi. Blender kullanılmadı. İstemler PART07A_PROMPTS.md'dedir.

Parşömen görev paneli, ayrı görev satırları, yeşil ilerleme çubukları, dinamik sayaçlar, ödül ikonları, mavi GİT/ÖDÜLÜ AL düğmeleri ve mor-altın başlık vardır. Büyük açık sandık ve gerçek kedi prefabı üst kompozisyonu oluşturur. Sandığın dekoratif kristal/yıldızları gerçek para veya elmas bakiyesi değildir; gerçek ödüller ayrı satırdaki hak ikonlarıdır.

## Gerçek testler

- DailyActivitiesTests:219 kontrol başarılı. Model/saat koşulları, gerçek UI raycast'leriyle Home→görev/hediye→Gameplay→Hint→iki gerçek kazanma→Home→dört görev ödülü ve Home→Map dönüşü doğrulandı.
- Geçersiz güçlendirici/erken görev ödülü/sahte kazanma reddi; aynı koşu makbuzu ve tekrar hediye/ödül alma engeli; hedefte sayaç sınırı; legacy kayıt uyumu; yazma hatası ve hediye sayaç taşması sınandı.
- Yerel gece yarısı,20 saat sınırı, saat geri alma, saat dilimiyle geçmiş güne dönme, eski saate yetişerek düzelme ve atlanan günlerde tek hediye sınandı. Gerçek sistem saati değiştirilmedi.
- Benzersiz PlayerPrefs test anahtarında gerçek disk yazımı ve ledger'ı yeniden açma yapıldı. İlerleme/alındı/makbuz alanları korundu. UI'yı kapatıp yeniden açınca hediye tekrar alınamadı. İşletim sistemi uygulama sürecini tamamen kapatıp yeniden başlatma testi yapılmadı.
-900x1600,900x1950,900x2000,Safe Area inset ve360x640 görev yerleşimleri; hediye için Safe Area ve küçük ekran kontrolleri geçti. Aktif günlük düğmelerin dokunma alanları en az44px.
- DailyHomeEntryTests:64 kontrol; dört çözünürlükte logo/kedi/PLAY/alt navigasyon/alt başlıkla çakışmama,44px hedef, iki girişin gerçek raycast'i ve doğru sekmesi doğrulandı.
- Mevcut MapProgressionTests:200 kontrol başarılı; harita/kilit/yıldız/tek sandık ödülü ve geri dönüş korunur. GameplayBoosterTests:723 kontrol başarılı; mevcut güçlendirici/bölüm sonu ve iki gerçek bölüm korunur.
- İlk UI testinde Hint'in kaynak şişeyi otomatik seçtiği test senaryosunda hesaba katılmadığından kazanma adımı başarısız oldu; test seçim adımı düzeltildi. İlk hata PART07A_FIRST_TEST_FAILURE.json içinde korunur. Saatin disk fabrikasına enjeksiyon aktarımı da düzeltilip açık bir testle doğrulandı. Son219 kontrol geçti; hata gizlenmedi.
- İlk mavi düğme9-slice görünümü Game View'da bozuldu; tam Sprite çizimine geçilerek düzeltildi. Gökyüzü uzun oranlarda esnetilmeden uniform cover kullanır.

## Görsel kanıtlar ve kalan farklar

PART07A_TASKS_GAMEVIEW.png, READY_GAMEVIEW.png, CLAIMED_GAMEVIEW.png, GIFT_GAMEVIEW.png ve HOME_GAMEVIEW.png gerçek Unity ScreenCapture çıktılarıdır; görev sayaçları için izole test profili kullanıldı. Gerçek oyuncu kayıt anahtarının varlık/içeriği değişmedi. PART07A_LAYOUT_* ve HOME_LAYOUT_* Unity kamera RenderTexture çıktılarıdır. Referans karşılaştırması PART07A_COMPARISON_Dark.png'dedir.

Referansın sandık/kedi/parşömen/yeşil çubuk/mavi buton dili korundu. Ancak mevcut onaylı kedi pozu, ayrı üretilen sandık, çerçeve kalınlıkları ve görev satırı oranları birebir değildir. Referanstaki haftalık etkinlik, sezon bileti ve koleksiyon ödülleri kapsam dışında bırakıldı. Bu ekran onların tamamlandığı izlenimini vermez. Fiziksel Android cihazda FPS/bellek/dokunma testi ve APK üretimi yapılmadı.

## Dosyalar ve teslim

Yeni: Core/DailyActivities.cs; Daily/DailyArt.cs, DailyActivitiesPanel.cs; Editor/DailyArtSetup.cs, DailyActivitiesTests.cs, DailyHomeEntryTests.cs; üç PNG ve Resources/Daily/Art.asset. Güncellenen yalnız3 mevcut script: BoosterActions.cs (ledger alanı/işlemleri), GameplayProgress.cs (saat enjeksiyonu), PrototypeApp.cs (gerçek olaylar/Home UI). Sahne dosyaları, seviye JSON'ları ve onaylı tüm eski sanat kaynakları korunur;283 dosyalık hash kanıtı PART07A_PRESERVATION.json içindedir. Kurallar:docs/DAILY_ACTIVITIES.md.

Mevcut12 model kontrolü ve Boot→Home→Gameplay→Home smoke(exitCode0) başarılı. Son Console:0 hata,0 uyarı. Test profili kapalı; Home edit modunda bırakıldı. Git teslimi doğrulama sonrası eklenecektir. Sonraki part başlatılmadı.
