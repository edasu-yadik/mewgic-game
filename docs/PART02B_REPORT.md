# PART 02B — Unity denetimi ve büyücü kedi animasyonları

Tarih: 24 Ekim 2026 (Europe/Kiev). Proje: C:/Dev/Mewgic, Unity 6000.3.27f1.

## Unity sağlık kontrolü

Başlangıçta PART 02A karakter dosyaları henüz commit edilmemişti. Bu dosyalar ayrı bir koruma commit'ine alındı; mevcut geçmiş korunarak çalışıldı. Unity CLI 1.0.0-beta.14 güncel bulundu. MCP durum aracı bağlantıyı yeniledi; canlı Editor okumaları ve işlemleri Codex kaydını kullanan MCP SDK istemcisi üzerinden yapıldı.

Son Git kontrolünde Android derlemesinin diskte bıraktığı geçici Input System preloadedAssets girdisi görüldü. Editor API'sinde özgün liste zaten boştu; yalnız derlemenin bıraktığı tek satırlık disk farkı özgün haline döndürüldü. Son karşılaştırmada ProjectSettings, paketler, oyun kaynakları ve dört ana sahne başlangıç kaydıyla aynı.

Boot, Home, Gameplay ve LevelMap tek tek incelendi: eksik script, bozuk prefab bağlantısı ve kayıp serialized asset referansı bulunmadı. Başlangıçta prefab yoktu. Ortografik kameralar ve URP yapılandırması mevcut. Android hedefi, Portrait, IL2CPP, ARM64, minimum API 26 ve otomatik hedef API doğrulandı. Başlangıç APK'ları korunur.

İlk denetimde OpenPreviewScene, açık Boot sahnesindeki Global Light 2D ile birlikte URP'nin ışık kayıtlarına girdi ve dört geçici 'More than one global light' mesajı üretti. Mesajların stack trace'i denetim çağrısını gösterdi. Denetim yöntemi sahneleri Single modda sırayla açıp önceki düzeni geri yükleyecek şekilde düzeltildi. Sahnelere ışık eklenmedi/silinmedi. Bu yöntemle tekrar denetimde 0 hata, 0 uyarı elde edildi. İlk teşhis kayıtları Temp/Part02B_Diagnostics altında tutulur; Git'e eklenmez.

### Yapılan düzeltmeler

- Mevcut oyun kodunda mekanik hata bulunmadı; Puzzle.cs ve PrototypeApp.cs değiştirilmedi.
- MewgicPlaySmoke, Gameplay Start tamamlanmadan şişe arayabildiği için zamanlama hatası veriyordu. UI oluşmasını ve animasyon kilidinin açılmasını bekleme eklendi. RunInteractive ile test sonunda Editor açık bırakılıyor; eski batch çalıştırma davranışı korunuyor.
- Geçersiz hedefe dökme sonrası hamle sayısının değişmediği ve uyarı verildiği Play Mode senaryosuna eklendi.
- Yeni kıvılcım bileşeninde eksik CanvasRenderer görsel kontrolde saptandı. RequireComponent ve açık bileşen oluşturma ile düzeltildi; renderer varlığı animasyon testine eklendi. Bu, eklenen FX altyapısının geliştirme sırasında giderilen hatasıdır.

## Oyun testleri

- MewgicPrototypeSetup.TestModel: 12 assertion geçti. Kapasite, geçersiz aktarım, üst renk grubu, kısmi aktarım, Undo/Restart ve kazanma kontrolü sınandı. Başlangıç bölümünün çözümü 1→3, 2→1, 2→3; BFS 18 durum ziyaret etti.
- Play Mode: Boot → Home → Play → Gameplay → Home başarılı. Şişe seçimi, geçersiz hamle reddi, aktarım, hamle sayacı, animasyon kilidi, Undo, Restart, kazanma ve Replay doğrulandı. Sonuç kodu 0; PART02B_GAMEPLAY_TEST.json.
- Mevcut oyun hâlâ tek bölüm içeriyor. Kazandıktan sonra Replay var; sonraki bölüme geçiş henüz uygulanmış değil. Bu özellik başarılı test edilmiş gibi raporlanmaz ve bu aşamada yeni bölüm sistemi eklenmedi.
- LevelMap sahnesi sağlam fakat işlevsel bölüm haritası henüz yok; ana akışa bağlanmadı.

## Karakter dosyaları ve prefab

Kaynak MageCat klasörü korundu. İstenen Assets/Art/Characters/WizardCat klasörüne üç PNG byte-identical olarak kopyalandı: MageCat_FullBody_v001.png, MageCat_NoWand_v001.png, MageCat_Wand_v001.png. Çözünürlükler 1122×1402 ve alpha korundu. Gövde için onaylı değneksiz varyant, hareketli aksesuar için mevcut değnek kullanılır; yeni kedi tasarlanmadı.

Yeni yardımcı görsel WizardCat_ClosedEyePatches_v001.png, mevcut yüz referansıyla ImageGen üzerinden üretildi. Tek dokudaki iki kapak Unity Sprite rect'leriyle BlinkLeft.asset ve BlinkRight.asset olarak tanımlandı ve mevcut gözlerin konumuna hizalandı. Kaynak PNG piksel boyaması programla değiştirilmedi. Üretim promptu References/ProductionSources/WizardCat/Blink_v001/PROMPT.md içindedir.

Import: Sprite/Single, 100 PPU, Full Rect, alt-orta pivot, sRGB, alphaIsTransparency, mipmap kapalı, Clamp/Bilinear, NPOT ölçeklemesi yok, max 2048, başlangıçta sıkıştırmasız. Kaynakların çözünürlüğü düşürülmedi.

- Prefab: Assets/Prefabs/Characters/WizardCat.prefab.
- Controller: Assets/Animations/WizardCat/WizardCat.controller.
- Test sahnesi: Assets/Scenes/WizardCatAnimationTest.unity.
- Runtime: Assets/Scripts/Characters altındaki WizardCatPresentation, WizardSparkGraphic ve WizardCatTestPanel.
- Yeniden üretim/otomasyon: Assets/Editor/WizardCatAssetBuilder.cs ve WizardCatAnimationTests.cs.

Prefab UGUI/RectTransform tabanlıdır; bir Canvas altında kullanılır. Alt-orta kökten ölçeklenir. Değnek tutuş pivota göre döner; arka/ön çizim ve HandOcclusionMask patinin önünden geçen sapı sınırlar. Baş, kulak, kuyruk veya pati için bağımsız sprite üretilmiş gibi gösterilmez. Tam iskeletli veya anatomik gövde rig'i yoktur.

## Üç animasyonun durumu

1. **Idle:** otomatik 3,6 saniyelik yumuşak döngü. Görsel grupta dikey ölçek en fazla %0,6, yatay ölçek %0,2 ve 2,2 mantıksal birim yer değiştirme. Değnek ve kapaklar aynı nefes grubunda olduğu için hizaları birlikte hareket eder. Bu birleşik sprite nefesidir; bağımsız göğüs animasyonu değildir.
2. **Blink:** 0,28 saniyelik kapak overlay klibi; yaklaşık 65 ms kapanma, kısa kapalı kalma ve geri açılma. Mavi gözler boyutlandırılmaz; onaylı açık gözler kapak altında kalır ve geri görünür. Butonla tetiklenebilir; prefab ayrıca 4,5–7 saniyelik aralıklarla otomatik kırpma içerir.
3. **Wand Magic:** 1,6 saniyelik hareket; tutuş çevresinde 7 dereceye kadar dönüş, yıldız ucunda parıltı, altın kıvılcımlar ve başlangıç açısına dönüş. Tekrar tetikleme işlem sürerken engellenir. Ön/arka değnek parçaları aynı eğriyi kullanır.

Animator'daki WandRest ve BlinkRest klipleri bu iki eylemin başlangıç/değer sıfırlama durumlarıdır; ek karakter eylemleri değildir. Base Idle, Wand Magic ve Blink ayrı Animator katmanlarında çalışır.

FX sabit 18 UI kıvılcım nesnesi kullanır; her tetiklemede Instantiate/Destroy yapılmaz. Yeni rastgele doku, gerçek zamanlı refraction veya ağır ışık shader'ı yoktur. Bu tasarım mobil için sınırlı bir başlangıç bütçesidir; gerçek cihaz FPS/GPU ölçümü henüz yapılmadı.

## Unity animasyon testi ve görüntüler

WizardCatAnimationTest sahnesinde Idle otomatik başlar. Göz Kırp ve Değnek Büyüsü gerçek Button bileşenleridir. Açık/Koyu Zemin düğmesi alpha ve kenar kontrolü içindir. Yeni sahne üretim Build Settings listesine eklenmedi; Boot/Home/Gameplay sırası korunur.

Animasyon testleri butonların GraphicRaycaster hit'ini ve PointerClick yolunu kullanır. Nefes değişimi/genlik sınırı, kapağın kapanıp açılması, büyünün tetiklenmesi, tekrar giriş kilidi, kıvılcım bütçesi, CanvasRenderer varlığı, değneğin başlangıç rotasyonuna dönmesi ve havuzun büyümemesi doğrulandı. Son kayıt PART02B_ANIMATION_TESTS.json içindedir.

Son koşuda **41 animasyon/UI kontrolü geçti**; otomatik kırpma zamanlayıcısının kapakları kapatıp yeniden açtığı da sınandı. Windows üzerinden Unity penceresi ayrıca incelendi. Game görünümü Free Aspect'ten **16:9 Portrait (9:16 dikey)** seçeneğine alındı; böylece Editor'ün yatay önizlemesinde şapkanın kırpılması giderildi. Play düğmesi ve Değnek Büyüsü düğmesi gerçek fare tıklamasıyla da çalıştırıldı. Unity test sahnesi canlı Play Mode'da bırakıldı. Son canlı Console sayacı **0 hata, 0 uyarı**; PART02B_LIVE_PREVIEW.json.

Play Mode'daki gerçek Unity Canvas ve kamera RenderTexture çıktıları:

- wizardcat-idle.png
- wizardcat-blink.png
- wizardcat-magic.png
- wizardcat-light-background.png

Bunlar çizim/mockup değildir; çalışan sahnenin 900×1600 Unity render'larıdır. Fiziksel telefon veya işletim sistemi ekran görüntüsü değildir. Açık/koyu zeminde görünür arka plan plakası veya belirgin siyah/beyaz dikdörtgen kenarı görülmedi. Tüy uçlarında kaynak görselin sıcak renkli rim ışığı korunur. Değnek ve kapak hizası bu boyutta incelendi; bütün telefon ekranlarında kalite/FPS onayı verilmez.

## Android ve kalan sınırlar

Yalnız karakter test sahnesini içeren ayrı Android geliştirme APK'sı hedefi Builds/Android/Mewgic-WizardCat-Test.apk. Üretim sahne listesi değiştirilmeden BuildPlayerOptions.scenes ile derlenir. Sonuç PART02B_ANDROID_BUILD.json içine yazılır. Önceki oyun APK'sı korunur. Son derleme durumu bu raporun teslim güncellemesinde belirtilir.

**Android sonucu: BAŞARILI, 0 hata, 0 uyarı.** APK 60.336.859 bayt; metadata ve geliştirme imzası kontrolü geçti. ARM64, minimum SDK 26 ve hedef SDK 36 doğrulandı. Bağlı yetkilendirilmiş Android cihaz sayısı 0; telefona kurulmadı. Test APK'sı mevcut `com.mewgic.game` uygulama kimliğini kullanır; telefona kurulursa önceki prototip uygulamasının yerini alabilir. İki APK'yı ayrı uygulama olarak aynı anda kurma testi yapılmadı.

Tam baş/gövde/pati/kuyruk rig'i hâlâ yok. Göz kapağı katmanları kısa blink için hazırlanmıştır; tam yüz animasyonu seti değildir. Karakter Home veya Gameplay'e henüz yerleştirilmedi. Sonraki bölüm sistemi ve LevelMap akışı da bu aşamanın dışında kalır. DevControl'a müdahale edilmedi.

## Sonraki aşama

PART 02C'de bu prefab'ın ana menüye yerleştirilmesine geçilebilir. Mevcut görsel kimlik ve mekanikler korunarak Canvas ölçeği, Safe Area ve gerçek Android cihaz testi yapılmalıdır. Daha gelişmiş yüz/vücut hareketi için onaylı katmanlı karakter kaynaklarına ihtiyaç devam eder.
