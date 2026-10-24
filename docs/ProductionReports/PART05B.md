# PART 05B — Etkileşimli cam şişeler

Tarih: 25 Ekim 2026. Proje: C:/Dev/Mewgic, Unity 6000.3.27f1.

## Uygulama

Gameplay'in eski dikdörtgen şişeleri, ayrı şeffaf cam kabuk ve modelden çizilen dinamik sıvı katmanlarıyla değiştirildi. Yeni kabuk Assets/Resources/Gameplay/Gameplay_GlassShell_v001.png; 941×1672 RGBA, kaynak alpha korunur. Saydam yatay boşluklar yalnız Sprite rect ile dışarıda bırakıldı. Android ASTC 4×4, mipmap kapalı; yaklaşık sıkıştırılmış doku belleği 1,51 MiB (cihaz ölçümü değildir). Üretim promptu PART05B_PROMPTS.md içinde.

GameplayBottleView.cs cam, sıvı, seçim ve tamamlanmış şişe sunumunu ayırır. Sıvı renkleri ve katman sayısı mevcut Puzzle modelinden gelir; kabukta renkli sıvı resmi yoktur. Boş şişe ve tam dolu tek renkli şişe desteklenir. Hafif dalga, renk geçişleri, baloncuklar ve cam kenar yansımaları bulunur.

PrototypeApp.cs yalnız Gameplay Refresh/Select/AnimatePour bölümünde değişti. Şişeler her seçimde yeniden yaratılmaz. Seçim yumuşakça yükselir; geçerli aktarımda kaynak hedefe taşınıp eğilir, doğru renkli akış gösterilir, kaynak hacmi azalırken hedef artar ve kaynak yumuşakça yerine döner. Model Pour çağrısı bir kez uygulanır; renderer modeli değiştirmez. Animasyon sırasında şişe girişi, Undo, Restart ve Home kilitlidir. Sonunda görüntü modelden tam olarak yenilenir. Eğilmiş sıvı yüzeyleri yatay kesme düzlemleri kullanır; bu stilize bir sunumdur, fiziksel akışkan simülasyonu değildir.

GameplayPresentation slot oranları cam silüetine ve okunabilirliğe göre ayarlandı. Sahne dosyaları değiştirilmedi. Home kod akışı, onaylı kedi/potion sanatları, Puzzle.cs ve Level01.json korunur; temel dosya hash kanıtı PART05B_PRESERVATION.json, Git diff yalnız ilgili sunum kodunu gösterir. Geri dönüş kopyası C:/Dev/Mewgic-Part05B-Backup.

## Gerçek doğrulama

- Mevcut 12 model kontrolü başarılı.
- GameplayBottleTests: 3183 kontrol başarılı. Çoğunluğu 1–24 slot geometrisi kontrolleridir; 3183 ayrı oyun oturumu değildir.
- Gerçek EventSystem raycast ve pointer tıklamaları: kaynak seçimi/yükselme, geçersiz hedef, geçerli aktarım, animasyonda tekrar tıklama ve tek hamle sayımı, Undo/Restart kilidi, bitişte açılma, Undo, Restart, kazanma, Replay ve Gameplay → Home → PLAY → Gameplay başarılı.
- Her kararlı aşamada render edilen katman sayısı, renk kimlikleri, hacim ve tamamlanmış şişe işareti gerçek modelle birebir karşılaştırıldı.
- 900×1600, 900×1950, 900×2000 ve çentik/Safe Area simülasyonu geçti. Şişeler görünür ve dokunulabilir; kedi ve kontrol alanlarına taşma yok.
- Mevcut Boot → Home → Gameplay → Home smoke testi exitCode 0 ile geçti.
- C# derlemesi ve son Unity Console: 0 hata, 0 uyarı. Refresh/domain reload sırasında kısa MCP bağlantı kesintileri yeniden bağlanarak aşıldı; başarısız bağlantı başarı sayılmadı.
- Fiziksel Android cihazı ve GPU/GC profili test edilmedi; yeni APK üretilmedi.

## Görseller ve kalan farklar

PART05B_GAMEVIEW.png gerçek Unity Game View ScreenCapture, 900×1600 tam ekran görüntüsüdür. gameplay-05b-*.png diğer oranların çalışan Canvas/kamera yakalamalarıdır. PART05B_POUR.gif gerçek Game View'dan kaydedilmiş 8 karelik kısa dökme önizlemesidir. PART05B_COMPARISON_Dark.png hedef ve gerçek ekranı yan yana gösterir.

Cam silüeti, yuvarlak taban, açık boyun ve mavi-beyaz kenarlar referansa yaklaştırıldı. Dinamik sıvılar referansın boyanmış illüstrasyonuna göre daha sade; yoğun yıldız ışıltısı, hacimsel kırılma ve karmaşık menisküs ayrıntısı aynı düzeyde değildir. Eğilmede baloncuklar yerine sade sıvı kesiti çizilir. Bu teslim teknik entegrasyon/test başarısıdır; referansla birebir premium final sanat eşleşmesi veya sanat onayı olarak sunulmaz. Gameplay kontrol panelleri ve 4 şişelik başlangıç seviyesi bu partta korunmuştur; referanstaki 8 şişe ve diğer UI bu işin kapsamı değildir.

## Git ve devam durumu

Yalnız ilgili kod, yeni cam asset'i, testler ve kanıtlar commit'e alınır. Önceki untracked Blender/deneme dosyaları korunup dışarıda bırakılır. Commit/push sonucu teslim kaydına eklenecek. Başka part başlatılmadı; yeni görsel cilalama ve cihaz performans kontrolü ayrı değerlendirme gerektirir.

## Teslim kaydı
Uygulama commit'i: be780cf96bc560d5b16d0db30d45d2452a96323e (feat: add interactive gameplay glass bottles). Private origin/master push başarılı; git ls-remote aynı SHA'yı doğruladı. Bu doğrulama teknik entegrasyon içindir; referansla birebir final sanat onayı değildir.
