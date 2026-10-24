# PART04F — Home bütünsel görsel kalite denetimi

25 Ekim 2026. **Denetim tamamlandı; Home'a referansa tam uyum/final sanat onayı verilmedi.** Teknik kontroller başarılıdır. En büyük açık:PART04D sol/sağ menüleri hiç uygulanmamış. Yeni büyük özellik kapsam dışı olduğundan bu görevde üretilmedi.

## İncelenen kaynaklar

AGENTS.md;docs/PART04A_REPORT.md;docs/PART04B_REPORT.md;docs/ProductionReports/PART04C.md;docs/ProductionReports/PART04E.md. PART04D raporu/dosyaları yok;PRODUCTION_PROGRESS bunu doğrular. Görsel kaynaklar REFERENCE_REGISTRY'deki Approved/00_Master_Cover.png ve01_Home.png;ikisi de açılarak incelendi.

## Aynı görevde düzeltilenler

- HomePotionDisplay.cs:ana onaylı şişeleri değiştirmeden ek glow opaklığı%6–11'den%2.5–4.5'e,sparkles%8–24'ten%2–7.5'e indirildi. İç yıldızlar/spiral ana sprite'a gömülü kaldığından onları silme veya yeniden üretme yapılmadı.
- HomeEnvironment.cs:ön sis%19.5–24.5 yerine%12.5–15.5,ortam ışık parçacıkları%16–48 yerine%10–30. Ön platform/merdiven/alt UI üzerindeki ışık örtüsü azaldı. Mimari/arka plan PNG'leri değişmedi.
- HomeTopBar.cs:üç artıya8,ayarlara4 UI birimi raycastPadding eklendi. Görünüm ve düzen aynı. Butonun görünür dikdörtgeninin dışındaki yeni kenar noktalarının gerçek raycast ile hedefe ulaşması test edildi.900×1600'de artı görsel kutusu yaklaşık55.7×59px;etkili hedef genişliği yaklaşık71.7px oldu. Bu değer fiziksel dp/telefon ergonomisi ölçümü değildir.
- PrototypeApp.Home:üst açıklama ve alt metne hafif siyah gölge,alt metne daha açık renk. İçerik/konum/ölçek değişmedi;Gameplay Label/metin koduna dokunulmadı.
- HomeVisualAuditTests.cs:yeni regresyon paketi. Hiçbir karakter/logo/şişe sanatı yeniden üretilmedi. Home.unity dahil178 korunan sanat/prefab/animasyon/sahne dosyasının hash'i aynı.

## On başlıkta referans karşılaştırması

| Bölüm | Yakınlık ve kalan önemli fark |
|---|---|
| Logo | Master'ın altın harf/mor plaka/pati/kristal kimliği var;harf/süs geometrisi birebir değil. Home referansında bu üst logo yok;önceki onaylı entegrasyon korunur. |
| Arka plan | Uçan ada,kale,şelale,sıcak pencere,fener ve gece paleti yakın. Kale/köprü yerleri,mimari ve yoğunluğu farklı;yeniden üretilmedi. |
| Üç şişe | Ayrı silüetler,renk sırası,cam,sol madalyon/sağ kristal ve baskın merkez korunuyor. Kaynak sprite'larda parıltılar hâlâ referanstan yoğun;ek efektler azaltıldı. Sol şişe biraz daha dolgun. |
| Platform | Şişeler mevcut taş yüzeye oturuyor;platform altın sembollü,referanstaki mavi rune yok. Yeni platform yapılmadı. |
| Büyücü kedi | Beyaz-gri tüy,mavi göz,mor şapka/pelerin,mavi kolye ve değnek korunur. Onaylı sprite oturan pozda;Home referansındaki ayakta poz ve şapka kıvrımları farklı. Prefab ve sprite değiştirilmedi. |
| PLAY | Yeşil oval/altın çerçeve/PLAY/üçgen yakın. Mevcut buton referanstan küçük ve aşağıda;karaktere taşmayan güvenli yerleşim korunur. Ana PNG hafif saydamdır. |
| Üst kaynak | Ayrı katmanlar ve dinamik metin yakın. Altın daha doygun,ikon geometrisi/font farklı. Veriler önizleme;gerçek ekonomi veya enerji tüketimi değil. |
| Sol/sağ menüler | **Eksik:**Daily Gift,Events,Missions,Collection,Shop,Starter Pack mevcut değil. İşlev testi yapılamadı;başarılı diye gösterilmedi. Bu en büyük final eksikliğidir. |
| Alt navigasyon | Beş ikon/etiket ve Home altın vurgu yakın. Ortak panel yerine ayrı taş kartlar,çatlaklar ve font farklı. Map boş LevelMap nedeniyle bilgi paneli;diğer gelecek sayfalar da açıkça geçici. |
| Boşluk/hizalama | Mevcut öğeler Safe Area içinde;butonlar,kedi/şişe/PLAY çakışmıyor. Yan menüler olmadığından üst-yan alanlar referanstan boş. Uzun ekranlarda logo ile dünya/şişe arasında daha fazla çevre alanı var. Prototip alt/üst açıklamalar referansta yok;okunabilirlikleri düzeltildi fakat kaldırılmadı. |

**Sonuç:**malzeme/renk ailesi ve temel odak yakın;tam kompozisyon ve eksik menüler nedeniyle nihai Home sanat işi tamamlanmış sayılmaz. Eksik04D'nin ardından bütünsel denetim tekrarlanmalı.

## Gerçek işlev ve mobil testleri

PART04F_TEST_RESULTS.json:390 kontrol başarılı. Home'daki10 görünür Button'ın tamamı kapsandı:3 artı,ayarlar,PLAY,5 alt sekme. Bilgi paneli KAPAT kontrolü de gerçek raycast tıklamasıyla denendi. Yeni genişletilmiş tıklama kenarları gerçek EventSystem hedefiyle doğrulandı. Gizli/olmayan yan menüler test edilmiş sayılmadı.

PLAY→Gameplay→Home,basılı sprite/release/çift tıklama kilidi;dinamik para/elmas/enerji,modal aç/kapat,bakiyenin değişmemesi;Home vurgusu;Map/Events/Team/Inventory geçici davranışları geçti. Kedi Idle/Blink/Wand Magic,şişe Idle/glow çalışıyor.

900×1600,900×1950,900×2000 ve çentikli safe-area simülasyonu geçti. Taşma/örtüşme kontrolleri ve gerçek kamera görüntüleri incelendi. 12 mevcut model kontrolü tekrar geçti. Mevcut Gameplay smoke exitCode0:Boot→Home→Gameplay→Home,geçersiz aktarım,Undo,Restart,kazanma,Replay ve animasyon kilidi. Son metin düzeltmesinden sonra390 kontrol tekrar çalıştırıldı. C# derlemesi başarılı,Console0 hata/0 uyarı,Home dirty=false.

Fiziksel mobil cihaz,parmakla ergonomi,FPS/GPU/bellek profili ve yeni APK testi yapılmadı;bu sınırlamalar teknik UI testleriyle kapatılmış sayılmaz.

## Görüntüler ve kanıtlar

- docs/PART04F_GAMEVIEW.png:gerçek900×1600 Game View ScreenCapture.
- docs/PART04F_COMPARISON_Dark.png / Light.png:01_Home ve gerçek Home tam ekran yan yana.
- docs/home-audit-1600/1950/2000/2000-safe.png;blink/magic/pressed/long çeşitleri:çalışan Home Canvas'ı kamera yakalamaları.
- PART04F_TEST_RESULTS,SMOKE_CHECK,RUNTIME_AUDIT,FINAL_CHECK,PRESERVATION.json:ham kanıtlar.
- Önceki görünüm docs/PART04E_GAMEVIEW.png içinde korunur. Yeni PNG sanat üretilmedi;yalnız doğrulama ekran görüntüleri eklendi.

## Git ve sonraki aşama

Yalnız bu doğrulanmış küçük iyileştirmeler ve denetim raporu commit edilir. Görsel final onayı verildiği anlamına gelmez. Push doğrulaması teslim kaydına eklenir. Bir sonraki büyük görsel aşama öncesi PART04D eksikliği giderilmeli;bu görevde başka part başlatılmadı. DevControl'e müdahale edilmedi.

## Teslim kaydı
Uygulama/denetim commit'i02b29874767f1d842e3d79bb3502bb129be126c6 private origin/master'a başarıyla gönderildi;git ls-remote aynı SHA'yı doğruladı. Bu kayıt ayrı belge commit'idir. Teknik denetim başarılıdır;Home'un görsel final durumu halen KISMEN TAMAMLANDI,öncelikli açık PART04D'dir.
