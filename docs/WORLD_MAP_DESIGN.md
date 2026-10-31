# PART 12B — Genişletilebilir dünya haritası

## Kapsam ve veri

`Assets/Resources/Map/World.json`: sürüm 1; ilk 12 ada, ardışık 50 bölüm. İlk dört adanın sprite referansları ve sırası korunur. Yeni sekiz adanın **sanatı bekliyor**. Bu çalışmada yeni ada resmi üretilmedi; mevcut gökyüzünde bölüm yolu, tema başlığı ve “Sislerin ardında” bilgisi gösterilir. Nihai ada yerine düşük kaliteli geometri kullanılmaz.

Her ada: kalıcı key, displayName, theme, firstLevel, levelCount, artIndex/artKey/artReady, tutorialIds ve ödül tanımı taşır. Şimdilik hiçbir yeni mekanik veya otomatik eğitim eklenmedi. tutorialIds, adanın ilk bölümünde ileride Pati sistemine bağlanabilecek tanım alanıdır; çalışma zamanı tetikleme entegrasyonu yeni mekanik görevinin parçasıdır.

## Harita ve gezinme

Dünya dikey kaydırılır. 12 ada bir dünya paketidir. Önceki/Sonraki Ada kontrolleri paket sınırını da aşar; Bölümüme Git ilk tamamlanmamış bölüme odaklanır. Daha önce incelenen ada ayrı bookmark olarak saklanır; bu konum Gameplay aktif bölümünü değiştirmez.

Her adada beş adet 10 bölümlük grup vardır. Gruptaki gerçek bölüm düğümleri tek bir yeniden kullanılan seçicide açılır. Kilit, güncel bölüm, kayıttaki 0–3 yıldız ve yeniden oynama durumları burada gösterilir. Ada üzerindeki grup düğümü bir bölümün üç yıldızını taklit etmez. Adanın barı gerçek tamamlanan bölüm sayısını /50 gösterir.

Beş yeniden bağlanan ada görünümü ×(5 grup+1 sandık), ayrıca 10 bölüm düğümü: toplam **40 MapPoint**, en fazla 30 ada noktası ve açık seçicide 10 ek bölüm noktası. 600 GameObject üretilmez. Ekran dışı ada kökleri kapatılır, aynı instance'lar yeniden bağlanır. Başlık/navigasyon Safe Area içinde sabittir; dünya RectMask2D altında kırpılır. Sprite, ortak malzeme ve düşük sayıda mevcut yıldız efekti kullanılır. Her piksel kaydırmasında yeni bölüm veya solver çalıştırılmaz.

601. bölüm 13. adaya girer. Tanımlı sanat paketi yoksa `Yeni Ufuklar 13` verisi türetilir; oynama engellenmez. Paket başına sınırlı içerik yüksekliği çok uzak koordinatlardaki float hassasiyeti sorununu azaltır. Yeni ada verileri JSON sonuna, 50'lik aralığı koruyarak eklenebilir. Yeni sprite'lar mevcut terrain dizisinin sonuna eklenir; artIndex ve artReady açılır. İlk dört referansın yeri değiştirilmez. Yeni tanımlı adaların ödül kimlikleri `map.island.<ilkBölüm>.v1` olmalıdır; böylece daha önce oynanan gelecek paketlerdeki sandıklar tekrar ödül vermez. Katalog doğrulaması aralık/ödül çakışmasını reddeder.

“Sonsuz” matematiksel sınırsızlık değildir: pozitif 32 bit bölüm kimlikleri ve tamamı 50 bölüm içeren ada aralıkları kullanılır. İlk teslimdeki 600 sınırı oyun sonu değildir.

## PART 12A bağlantısı ve zorluk

Bölüm 1 ve 2 authored tanımları aynıdır. 3+ talep üzerine `PrepareAsync` ile işçi thread'inde hazırlanır; sertifikalı tam pin diske yazılmadan Gameplay seçilmez. Kilitli bölüm tıklaması üretim başlatmaz. Hazırlama başarısızsa kayıt korunur, yeniden deneme sunulur.

Yeni açılan adaların ilk üç **henüz sabitlenmemiş** bölümü Easy / Normal / Normal, üç renktir; kalan bölümler 12A temposunu kullanır. `WorldMapRules.Request` bu planı üretir. Önceden kaydedilen pin her zaman önce okunur; eski bölüm, seed, recipeDifficulty, üretici version ve tam çözüm değişmez. Dünya haritası kayıt sürümü ile üretici sürümü farklı kavramlardır; üretici sürümü her pin içinde kalıcıdır. Bu tempo bir tasarım metriğidir; gerçek oyuncu zorluğu araştırması yerine geçmez.

Harita girişi mevcut Select/SetCurrentLevel yolunu, geri dönüş mevcut ReturnScene yolunu kullanır. Undo, Restart, Hint, Shuffle, Pati, sıvı aktarımı ve ödül modelini yeniden yazmaz.

## Eski kayıt uyumluluğu

Ana kayıt schema version=1 korunur; isteğe bağlı `worldMap` alt belgesi version=1 eklenir. Kaydı ilk açarken mevcut tamamlanmış kimliklerden eski sandık hakları yakalanır: village 1–2, crystal 3–5, moon 6–8, castle 9–11; önceki bölge önkoşulları korunur. Bu yakalama ilk ledger oluşturulduğunda bir kez yapılır. Yeni oyuncuya sonradan ikinci bölümde eski sandık hakkı yaratılmaz.

Eski dört sandık kimliği aynıdır. Önceden alınmış ödül tekrar verilmez; kazanılmış fakat alınmamış eski sandık, yeni 50'lik ada henüz kilitliyken de alınabilir. Bu dört ada için eski sandık ile yeni 50-bölüm sandığı **aynı ödüldür**; ikinci bir ödül üretilmez. Sonraki adalarda her 50 bölüm tamamlanınca bir İpucu sandığı açılır. Ödül miktarı ve idempotency işareti mevcut atomik save yolunda birlikte saklanır.

worldMap: migration işareti, eski haklar, en yüksek açılan ada, son kutlanan ada, son odaklanan bölüm. Açık/kapalı bölüm ve yıldızlar mevcut level records'tan türetilir. Oyuncunun gerçek mevcut bölümü mevcut presentation alanında; tam bölümler GeneratedLevels arşivinde kalır. Gelecek map save sürümü eski yedeğe düşürülmez. Save hatasında sayaç/odak/ödül değişmez.

## Doğrulama

`Part12BEditor.Models()` model/uyumluluk kontrolleri; `Run()` gerçek Play Mode, izole PlayerPrefs ve geçici FileLevelArchive ile kabul turudur. Gerçek kullanıcı primary/backup/corrupt belgelerinin byte eşitliği doğrulanır. `Preview()` izole QA profiliyle LevelMap açar; gerçek oyuncu verisini değiştirmez. Örnek oyun ekranları gerçek Unity UI/camera render'larıdır; ayrıca native Editor Game View görüntüleri teslim edilir.

Fiziksel iPhone/Android GPU ve dokunma testi yapılmadı. Yeni iOS/IPA/TestFlight build'i alınmadı. Gelecek sekiz adanın özgün sanat üretimi aşağıdaki ayrı plana tabidir.
