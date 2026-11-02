# PART14A — Core loop, ödül sistemi ve iki katmanlı harita

3 Kasım 2026 · Unity6000.3.27f1 · recovery/part09-completion · başlangıç aa4deb9233b109f8412d805635ff86df5ac9f532

## Sonuç

Home üzerindeki sabit koleksiyon kutusu pasifleştirildi, sabit günlük hediye girişi kaldırıldı; görevler ve mevcut etkinlik/koleksiyon altyapısı korundu. Uygun girişte günlük sandık akışı açılıyor. Gerçek bölüm kazanma artık coin veriyor, sonuç ekranı ödül dökümünü ve animasyonlu coin sayacını gösteriyor. Dünya adaları korunarak ada içi10 tekil bölüm kartlı, beş sayfalı detay görünümü uygulandı. Oyuncu kayıtları sıfırlanmadı; onaylı ada PNG/prefabları ve bulmaca kuralları değiştirilmedi.

AGENTS.md,13J final raporu ve son13K raporu incelendi. Aynı mevcut Unity sürümü/Pipeline7800 kullanıldı; paket/sürüm değişikliği, yeni build veya master merge yok. Üretim sahneleri diskte değiştirilmedi. Değişiklikler mevcut Home,Gameplay veLevelMap sahnelerinin runtime UI üreticilerindedir.

## Günlük ödül

- İlk uygun Home girişinde tam ekran karartma ve günlük sandık kartı açılır. Sabit günlük hediye Home düğmesi yoktur; görev düğmesi durur.
- İlk hediye hazırdır. Son başarılı hediyenin UTC zamanından **24 saat** sonra yenisi açılır. Eski20 saat + takvim günü şartı kaldırıldı. Günlük görevlerin gece yarısı sıfırlanması ayrı ve korunmuştur.
- Kapalı sandık resmiyle başlanır. Oyuncu Sandığı Aç'a basar;1 saniyelik hafif sallanma/büyüme, ardından mevcut açık sandık görseline geçiş olur. **Animasyon tamamlanınca100 coin +1 ipucu** aynı kayıt işlemiyle verilir. Coin, ipucu ve son hediye zamanı birlikte commit edilir.
- Ana sayfa coin sayacı kayıt revizyonunu okuyup animasyonla artar. Onayla düğmesi oyuncuyu ana akışa döndürür. Görevler içinden erişilen hediye de aynı açılış bileşenini kullanır.
- Aynı hediye tekrar ödenmez. Kayıt hatasında bakiye/zaman tüketilmez; yeniden dene ve çıkış mümkündür. Var olan yerel saat geri alma koruması korunmuştur; sunucu saatli hile önleme eklenmedi.
- Oturumda aynı alınmamış hediyeye tekrar tekrar popup açılmaz. Son hediye kimliği/zamanı değişip24 saat dolduğunda yeni Home girişi veya Home'da uygulamaya dönüş yeniden kontrol eder. Uygulama Gameplay'den devam ederse ödül Home'a dönüldüğünde sunulur.

## Bölüm sonu coin ve completion

|Öğe|İlk kazanış|Yeniden kazanış|
|---|---|---|
|Tamamlama|20 coin|5 coin|
|Yıldız bonusu|Yıldız başına10 coin|Yıldız başına2 coin|
|Toplam|30–50 coin|7–11 coin|
|Mevcut ilk kazanış ödülü|+1 ipucu korunur|Tekrar verilmez|

Coin her **yeni kazanılmış oyun turunda** verilir; aynı turun callback/save tekrarı ikinci ödeme yapmaz. Her bölüm kaydına opsiyonel `lastCoinReceipt` alanı eklendi; eski kayıtta null olarak açılır. En son kazanma makbuzu ve ödül aynı ilerleme belgesinde saklanır; çift dokunma ve aynı makbuzla yeniden çağrı idempotenttir. Eski tamamlanmış bölümlere açılışta geriye dönük coin basılmaz. Daha iyi yıldızlar korunur. Gerçek yeniden oynama yeni tur kimliğiyle küçük coin ödülü kazanabilir; bu bilinçli ekonomi tercihidir.

İlk deneme/no-hint bonusu bu aşamada eklenmedi; bunlar isteğe bağlıydı. Mevcut kayıt geçmişinden böyle bir başarı uydurulmadı. Değerler önceki0 coin kazanımını canlı bir temel döngüye dönüştüren başlangıç ayarıdır; uzun süreli ekonomi/oyuncu telemetrisi dengesi yapılmadı.

Sonuç paneli: kısa yumuşak ölçek girişi, sıralı yıldızlar ve altın→beyaz parlak giriş; mevcut24 küçük yıldız parçacığı. Efekt ve sayaç güncellemesi2.4 saniyeden sonra durur. Coin0→kazanılan değere1.2 saniyede çıkar; tamamlama/yıldız bonusu ve varsa ipucu dökümü görünür. Sonraki Bölüm, Tekrar Oyna ve Harita/Ana Menü düğmeleri korunur. Kayıt başarısızsa ödül verilmiş gösterilmez. Yeni ağır parçacık sistemi veya gerçek zamanlı fizik yok.

## İki katmanlı harita

1. **World/Island:**12 onaylı ada, arka plan, kaydırma, kilit, ilerleme plakası ve sandık korunur. Her adada beş dar aralık düğümü yerine tek geniş BÖLÜMLER kapsülü vardır. Uzun aralık metni artık dünyadaki yuvarlak düğümlere basılmaz.
2. **Island detail:** seçilen adanın50 bölümü10'ar tekil karttan oluşan beş sayfadır. Önceki/sonraki sayfa ve Haritaya Dön düğmeleri vardır. Dünya giriş düğmesi o adadaki güncel bölümün sayfasını açar; eski adada son, henüz açılmamış adada ilk sayfa seçilir.

Kartlar iki sütun/beş satır,210×100 UI birimi; küçük ekranlarda dokunma yüksekliği en az44 ekran pikseline genişletilir;150×42 numara alanı ve autosize32→18 kullanır.100 ve601 gibi numaralar resmin içinde ayrı TMP metnidir. Kilitli: mor/soluk kart ve kilit; tamamlanmış: yeşil kart ve kazanılan yıldızlar; güncel: altın glow/ok ve güçlü vurgu. Kilitli bölüme basmak oyun veya üretici başlatmaz. Bölüm seçimi gerçek katalog/ilerleme verisinden gelir.

Aynı10 kart sayfalar arasında yeniden kullanılır.5 ada görünüm havuzu ve toplam40 MapPoint nesnesi korunmuştur; kullanılmayan eski dört grup slotu gizlidir. Ekrana600 bölüm nesnesi üretilmez. Sanat yükleme/release/nesil koruması değişmedi; görünür yeni sanat sınırı5. World yolları yalnız aktif ada kapsüllerini bağlar.601+ bölge/üretici devamı korunur; yeni ada resmi eklenmedi.

## Kurallar ve scoring

- Şişe kapasitesi4 katmandır. Kaynak dolu, hedefte yer ve hedef boş veya üst rengi aynıysa dökülebilir.
- Üstteki aynı renk serisi, hedefteki boşluk kadar aktarılır. Yalnız başarılı döküş hamle sayısını artırır.
- Kazanma: her şişe boş veya tam dolu/tek renktir. Bulmaca/Undo/Hint/Extra/Shuffle mantığı değiştirilmedi.
- Üç yıldız: hamle sayısı bölümün `threeStarMoves` eşiğinde veya altında; iki yıldız: `twoStarMoves` eşiğinde veya altında; diğer kazanışlar bir yıldız. Kazanmadan yıldız/coin verilmez.
- Üretilen bölümün üç yıldız eşiği doğrulanmış optimal hamle sayısı; iki yıldız eşiği optimal + max(2,optimal/2). İlk iki onaylı bölüm kendi HUD eşiklerini korur.
- İpucu kullanımı ayrıca yıldız azaltmaz; yıldız hesabı mevcut hamle eşiklerine bağlıdır. Yeni no-hint bonusu yoktur.
- `CompletionReward` yalnız coin formülünü toplar; `GameplayHudState` yıldızları değerlendirir, `ProgressLedger.Claim` ödül/ilerlemeyi atomik kaydeder, `GameplayEndScreen` sadece sunar.

## Dosyalar

Yeni: `Assets/Scripts/Core/CompletionReward.cs`, `Assets/Scripts/Daily/DailyArrivalPopup.cs`, Editor-only `Assets/Scripts/Tests/Part14AProbe.cs`; `tools/Part14A` denetimleri.

Değişen mevcut scriptler: Core/BoosterActions.cs (makbuz/kopya/coin atomikliği), Core/DailyActivities.cs (24 saat ve100 coin), Daily/DailyActivitiesPanel.cs (Home düzeni/ortak sandık), Rewards/RewardHubPanel.cs (Home koleksiyon girişi pasif), Presentation/PrototypeApp.cs (popup ve coin sunum bağlantısı), GameplayEndScreen.cs, HomeResourceModel.cs, HomeTopBar.cs, Map/MapWorld.cs ve MapWorldProgression.cs.

Localization/Source/Mewgic.json ve sekiz StringTable + SharedTableData: beş yeni anahtar, günlük bekleme/kayıt metinleri ve dünya kaydırma yönergesi güncellendi. Ada başlıkları/sayılar görsellere yazılmadı. Sahne/prefab/ada sanatı/Puzzle.cs/üretici dosyası değişikliği yoktur.

## Testler ve gerçek görseller

- MODEL_TESTS.json:18 ödül/24 saat/idempotency/eski kayıt/save-failure kontrolü; mevcut booster model testleri de çağrıldı.
- REGRESSION_MODEL_TESTS.json: mevcut1599 model kontrolü geçti. Eski rapor dosyaları testten sonra başlangıç kopyasından geri kondu.
- UNITY_TESTS.json:1017 kontrol, hata0/uyarı0, ana/yedek/bozuk oyuncu kayıtları aynı. Otomatik popup, animasyon bitmeden ödeme olmaması,100 coin/+1 hint sonrası Home, sabit kutuların yokluğu, tekil sayfalar/gerçek raycast, kilitli101 reddi, gerçek100 çözümü vecoin, yıldız/101 açılması/haritaya dönüş ve ikinci Home popup'ının çıkmaması doğrulandı.
- MAP_ALL_ISLANDS.json:1952 kontrol;12 adanın tüm600 bölümü ve601–610 sayfasının aralığı, numara taşması ve gerçek raycast hedefleri; görünür sanat handle/instance sınırı doğrulandı.
- LOCALIZATION.json:162 anahtar, sekiz dil, eksik0; dört negatif validator vakası geçti. Ada detayında sekiz dilde aktif metin yerleşimi de denetlendi.
- Ada detay/sonuç paneli900×1600,900×1950,900×2000 (detay2000'de Safe Area daraltması) görüntülendi. Numara ve aktif TMP taşması saptanmadı. 360×640 düşük çözünürlükte kartların44 piksel hedefi ve numara taşması da geçti. Home/daily ana test görüntüleri900×1600'dür; fiziksel cihaz Safe Area/dokunma testi değildir.

Model aracının bir ek denemesi Play Mode’da seçili100 üzerinden eski testin varsaydığı1. bölüm durumuyla çelişti; araç Edit Mode şartıyla sınırlandı ve temiz Edit Mode’da18 kontrol yeniden geçti. Doğrudan yakalama turunda önizleme Ready coroutine’i bitmeden Home’a geçilmesi bir QA bekleme hatası verdi; son önizleme yeniden açılıp hazırlanması beklendi. Bunlar başarılı üretim testi diye sunulmadı.

İlk canlı denemenin ATTEMPT1_UNITY_TESTS.json başarısızlığı arşivlendi: Editor Play'e girerken yatay serbest çözünürlüğe dönmüştü; raycast ekran dışında kaldı. QA başlangıcında gerçek Game View900×1600 sabitlenip pencere odaklandı, turlar yeniden geçti. Dünya kapsülünde uzun mevcut yönerge görselde kesiliyordu; ayrı kısa yerelleştirilmiş BÖLÜMLER etiketiyle düzeltildi ve tekrar görüntülendi.

[Galeri](GALLERY.html): [Home](Native_Home.png), [Daily](Native_Daily.png), [World](Native_World.png), [Island detail](Native_Detail.png), [Level complete](Native_Complete.png). Native_* doğrudan Game View ekran yakalamasıdır; diğer oran PNG'leri gerçek Unity Camera.Render çıktılarıdır. Aynı sahneler, ayrı QA profili; temsili çizim/mockup değildir.

CONSOLE.json geçmiş tamponu da korur:2 hata/1998 uyarı; eski13A import kayıtları, Editor ScriptableSingleton, bu parttaki ilk script reimport uyarıları ve QA bekleme hatası örnekleri CONSOLE_HISTORY.json içindedir. Güncel Editor groundTruth hata0/uyarı0; arşivden sonra Console temizlenip güvenli önizleme yeniden açıldı. CONSOLE_FINAL.json son temiz oturumu kaydeder; derleme COMPILATION.json içinde hatasızdır.

## Koruma ve kalan sınırlar

Gerçek oyuncu kaydına test yazılmadı. Test profilleri ve üretici arşivleri benzersiz geçici alanlarda. LevelRecord yeni alanı opsiyoneldir; mevcut sürüm1 kaydı açılır, ilerleme/bakiyeler tutulur. Eski uygulama sürümüne geri dönmek yeni coin makbuzu alanını koruma garantisi taşımaz. Oyun ekonomisi sunucu otoriteli değildir; makbuz koruması normal istemci tur tekrarına yöneliktir.

Fiziksel iPhone/Android GPU/RAM/FPS/termal testi ve uzun süreli ekonomi dengesi yapılmadı; kesin telefon FPS iddiası yok. Yeni texture veya büyük asset eklenmedi. UI efektleri sınırlıdır; sayaç aralığında metin güncellemesi vardır. Ağır refactor, yeni mekanik, paket kurulumu veya yeni build yok.

Editor teslimde ayrı QA profilinde LevelMap ada detayı açık bırakılır; test nesnesiyle üretim sahnesini kaydetmeyin. Önceden değişmiş TMP fallback dosyasının başlangıç byte'ları korunup commit dışında tutulur. İlgisiz untracked dosyalar korunur. Git teslimi aşağıda uzak SHA ile doğrulanmıştır.

## Doğrulanmış Git teslimi

Uygulama/kanıt commit’i **75dce9817e713c1127dc67251cf34073ebae9fc2**, private origin/recovery/part09-completion üzerinde git ls-remote ile doğrulandı. Master f243d7a355fedf9cfbc65f8060f1d3bc14cae169 olarak kaldı; merge yok. [GIT_DELIVERY.json](GIT_DELIVERY.json) uzak doğrulamayı içerir. Bu teslim kaydı ayrı belge commit’iyle aynı branch’e gönderilir. Önceden değişmiş TMP font commit dışında ve byte olarak korunmuştur. Yeni build oluşturulmadı.
