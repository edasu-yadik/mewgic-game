# PART 11F — Hareketli pati ve kısa kedi düşünce balonu

31 Ekim 2026 — TAMAMLANDI. Proje `C:\Dev\Mewgic`; dal `recovery/part09-completion`; başlangıç `d4a988c234af7aab5f46986c0f8a9136e5cce285`. AGENTS.md ve son PART11E teslimi incelendi; başlangıç tracked çalışma ağacı temizdi. Unity MCP bağlantısı doğrulandı. Editor **6000.3.27f1** ve lisans/ProjectSettings korundu; testler aynı Editor'a Pipeline/CLI ile yürütüldü.

## İstenen yeni deneyim

Mevcut `PawGuide` yeniden düzenlendi; ikinci bir öğretici yöneticisi oluşturulmadı.

1. İlk Bölüm 1'de, hazır Gameplay ve geçerli UI/kedi/şişeler beklenir; ardından **0,6 saniye** normal sahne gösterilir.
2. Krem-mor, altın kenarlı pati yumuşak görünerek sahneye girer. Kaynak şişeye SmoothStep easing ve hafif eğimle gider; yaklaşık 0,16 saniye hover yapar.
3. Pati basma-kalkma, %16'ya kadar küçük ölçek tepkisi ve genişleyen altın dokunma halkasıyla **gerçek kaynak Button olayını** çağırır. Şişenin mevcut seçilme görseli kullanılır.
4. Doğrulanmış hedefe ilerler ve aynı tıklama hareketiyle **gerçek hedef Button olayını** çağırır. Mevcut `PrototypeApp.Select → Puzzle.Pour → AnimatePour` çalışır. Öğretici kendi sahte sıvı verisini veya ikinci Pour çağrısını üretmez.
5. Aktarımın gerçekten bittiği callback beklenir; mevcut WizardCat Wand Magic ile kısa ve hafif onay verilir. Büyük öğretici kutlaması açılmaz.
6. Pati kedinin gerçek RectTransform merkezine gider ve dokunur. Sonra pati solarak kaybolur.
7. Kedinin yanında/altındaki boşlukta, ince mor-altın kenarlı **oval düşünce balonu** şu kısa metni gösterir:

> Renkleri aynı şişede topla.
> Boş ya da üst rengi aynı şişeye aktar.
> Takılırsan bana dokun!

8. Balon dokunmadan itibaren yaklaşık **3 saniye** görünür, ardından 0,2 saniyelik yumuşak çıkışla kapanır. Normal şişe/güç girişleri serbest kalır. Normal Gameplay'de kalıcı öğretici veya teknik alt yazı bırakılmaz.

## Kaldırılan kutulu yaklaşım

`PawHow`, `PawHint`, `PawBoosters`, `PawNext`, `PawClose`, eski parşömen kutusu, güç daveti ve menü/araç raycast kalkanları **artık oluşturulmuyor**. “Nasıl Oynanır / Hamle Öner / Güçlendiricileri Tanı / Kapat” seçenekleri yok. Tam ekran görsel, ağır modal veya tam ekran raycast engeli eklenmedi. Otomatik örnek sırasında yalnız mevcut giriş kontrol kapısı kullanılır; Pause, Restart ve geri ile güvenli iptal mümkündür.

Sonradan kediye dokunmak aynı kısa düşünce balonunu açar; butonlu menü açmaz. Manuel balon modal değildir, normal aktarımı ve güçleri engellemez; kendi kendine kapanır. Mevcut Hint düğmesi/kuralları aynıdır; teknik kaynak/hedef satırları geri gelmez.

Eski API/enum adlarının bir kısmı geçmiş QA araçlarının derlenebilmesi için korundu; bu, eski menünün çalışma anında etkin olduğu anlamına gelmez. PART11B–11E'nin önceki butonlu arayüz beklentileri bu kullanıcı tasarım değişikliğiyle geçersizleşti; güncel kabul kanıtı bu PART11F testleridir. Eski rapor ve onaylı görseller silinmedi.

## Hareket, güvenlik ve kayıt

Öneri, gerçek model için mevcut solver'ın doğruladığı çözümün ilk hamlesidir. Doğrulanmış, terminal olmayan örnek bulunamazsa rastgele hamle gösterilmez; yalnız genel kısa balon açılır. Gerçek şişeler/karakter hareket ederken hedef konumu takip edilir; üç farklı oranda tıklama merkezleri yeniden yerleşime uyar.

Yalnız öğreticinin kendi kaynak/hedef çağrısı dar kapsamlı `driving` kontrolünden geçebilir. Gerçek oyuncu dokunuşu veya güç callback'i bu arada model/hak değiştiremez. Aktarım timeout'u, eksik hedef, eksik kedi, Pause, Restart ve geri yollarında rehber kapanıp giriş kısıtlamasını bırakır. Oyuncunun normal şişe, dökme, Undo, Restart, Hint, ödül ve kayıt kodu yeniden yazılmadı.

Otomatik gösteri, Bölüm 1 ve temel öğretici tamamlanmamışsa açılır. Mevcut **`paw.basic.v1`** geçmişi kullanılır; eski tamamlanmış oyunculara zorla yeni gösteri açılmaz. Yeni akışın kaydı, gerçek aktarım ve kedi balonu tamamlandıktan sonra yazılır. Yarım kalan gösteri tamamlanmış sayılmaz; sonraki Bölüm 1 girişinde baştan güvenle çalışır. Örnek gerçek bölüm denemesinde **bir gerçek hamledir**; hamle sayacı ve Undo geçmişine normal biçimde girer. Eğitim kendi başına güç hakkı tüketmez.

Kayıt şeması değiştirilmedi. Önceki güç eğitimi geçmişleri silinmedi. Gelecekteki kısa mekanik/güç mesajları için mevcut `PawTutorialDefinition` ve tamamlama anahtarı sözleşmesi ile `BoosterHelpRequested` bağlantısı korundu; bu teslimde yeni güç menüsü oluşturulmadı.

## Gerçek testler

- `TESTS.json`: **2517 kontrol geçti**, runtime **0 hata / 0 uyarı**.
- `RELOAD_TESTS.json`: Play Mode kapatılıp ayrı oturumda gerçek izole PlayerPrefs kaydı yeniden açıldı; **41 kontrol geçti**, runtime **0 hata / 0 uyarı**. Tamamlanmış eğitim tekrar başlamadı, bütün test kayıt/ödül/hak JSON'u aynı kaldı ve kediden kısa yardım açıldı.
- Toplam **2558 assertion**; ayrı oyuncu oturumu sayısı değildir. Tekrarlı metin/çizim/yerleşim/girdi kontrolleri dahildir.
- C# derlemesi **0 hata / 0 uyarı** (`COMPILE.json`). Son canlı Console ve Editor groundTruth **0 hata / 0 uyarı** (`CONSOLE.json`). Mevcut güç modeli testleri ayrıca geçti (`MODELS.json`).
- Otomatik kaynak seçimi ve hedef aktarımı gözlendi; model hamle sayısı yalnız bir arttı. Sıvı verisi gerçek aktarım sonrasında değişti; pati ancak bundan sonra kediye gitti.
- Tıklama basıncı, gerçek UI çizim geometrisi ve kaynak/hedef merkezleri kontrol edildi. İlk görsel turda yakalanan eksik CanvasRenderer onarıldı; görünür geometri ve gerçek ekran görüntüleriyle tekrar doğrulandı. Bu kusurlu ilk tur nihai görsel başarı kanıtı sayılmadı.
- Gösteri boyunca stok **3 Geri Al / 3 İpucu / 1 Ek Şişe / 1 Karıştır** olarak aynı kaldı. Gerçek oyuncu şişe/güç callback'leri gösteriye karışamadı.
- Balonun yaklaşık üç saniyede kapanması, pati/metnin tamamen temizlenmesi ve ardından oyuncunun gerçek sıvı aktarımı yapması geçti.
- Normal Undo, Restart, metinsiz Hint, Pause/Resume, gösteri sırasında Pause, yarım gösteride Restart, eksik hedefte güvenli kapanış ve güvenilir çözüm bulunamayan modelde hamlesiz yardım geçti.
- Tamamlanmış oyuncu yeniden girişi, Bölüm 1 kazanma/yıldız kaydı, Bölüm 2 ve Home → PLAY geçişleri geçti.
- Gelecek kısa mesaj tanımı için yanlış koşul/anahtar ve doğru tamamlama kaydı sınandı; yeni oyun mekaniği eklenmedi.
- 900×1600, 900×1950 ve 900×2000 renderlarıyla **9:16, 9:19.5, 9:20** kontrol edildi. Son oranda Safe Area kenar payı simüle edildi. Metin taşması, balonun şişeleri örtmesi ve pati hedef sapması bulunmadı.
- Ana gerçek oyuncu kaydı, yedek ve bozuk-kayıt kopyası iki oturumda aynı kaldı. Yazmalar yalnız `Mewgic.Tests.Part11F.*` anahtarlarında yapıldı; bunlar test sonunda temizlendi.

## Görseller ve hareket kanıtı

`MOTION.json` ve **64 ayrı zamanlı Unity karesinden** oluşan `MovingPaw_Preview.gif`, yaklaşık **9,14 saniyelik** tam akışı gösterir: bekleme → kaynak yolculuğu/tıklama → hedef yolculuğu/tıklama → gerçek aktarım → kedi yolculuğu/tıklama → düşünce balonu → temiz ekran. Statik kare tek başına hareket kanıtı olarak kullanılmadı. GIF süreleri örnekleme zamanlarına dayanır; kamera yakalamasıyla oluşan Editor yükü cihaz FPS ölçümü değildir.

Üç oran için şu gerçek Unity kamera/Canvas görüntüleri kaydedildi:

- `Paw_SourceClick.png`: gerçek kaynağın seçilmesi ve pati tıklaması.
- `Paw_TargetClick.png`: hedef tıklaması ve gerçek aktarım başlangıcı.
- `Paw_ToCat.png`: aktarım sonrası kediye gidiş.
- `Cat_Thought.png`: küçük oval düşünce balonu.
- `Cat_RepeatHelp.png`: oyuncunun kediye tekrar dokunmasıyla açılan balon.
- `Gameplay_Clean.png`: öğretici sonrasında temiz Gameplay.

`-1950` ve `-2000` sürümleri aynı klasördedir. `Part11F_Overview.jpg` bu gerçek karelerden özet görünümüdür. `UnityEditor_MovingPaw.jpg` ve `UnityEditor_Thought.jpg`, doğrudan Unity Editor Game View penceresi yakalamasıdır. Kedi balonu gerçek fare dokunuşuyla, güvenli tekrar oynatma Editor menü çağrısıyla ayrıca doğrulandı (`PREVIEW_MENU.json`). `UnityEditor_Clean.jpg` son temiz Gameplay görünümüdür. Ham hareket kareleri yerelde tutulur; Git'e eklenmez.

## Güvenli tekrar oynatma

Unity menüsü: **Mewgic → QA → PART11F**.

- **Yeni Oyuncu Önizleme:** Play Mode kapalıyken çalışır; gerçek oyuncu kaydını değiştirmeyen yeni bellek profiliyle Gameplay açar ve ilk öğreticiyi otomatik oynatır.
- **Patiyi Yeniden Oynat:** yalnız Play Mode'daki izole test profilinde ve oynanabilir durumda etkindir. Gerekirse önce Yeniden düğmesiyle mevcut deneme başa alınabilir. Gerçek profil için tekrar API'sinin reddettiği test edildi.

Bu araçlar Editor içindir; oyuncu sürümüne test sıfırlama düğmesi eklenmedi. Editor güncel Gameplay'de izole önizlemeyle açık bırakıldı.

## Değişen dosyalar ve sınırlar

- `Assets/Scripts/UI/PawGuide.cs`: eski menü yerine otomatik hareketli örnek ve kısa manuel yardım.
- `Assets/Scripts/UI/PawCursorGraphic.cs` (+meta): krem/mor-altın pati, hafif glow/tıklama halkası ve oval düşünce balonunun UI geometrisi. Tekrar kullanılabilir hafif çizimler; yeni karakter veya raster arka plan üretilmedi.
- `Assets/Editor/Part11FEditor.cs` ve `Assets/Scripts/Tests/Part11FProbe.cs` (+meta): güvenli önizleme, regresyon ve gerçek kare yakalama araçları.
- Bu klasördeki rapor/QA/görseller ve `docs/PRODUCTION_PROGRESS.md`.

Oyun modeli, PrototypeApp, save/ödül uygulaması, WizardCat prefabı/animasyonları, fontlar, şişe sprite'ları, arka planlar, üst/alt HUD, sahneler, ProjectSettings ve DevControl korunuyor. Başlangıç tracked SHA256 listesiyle karşılaştırma `PRESERVATION.json` içindedir. Başka untracked çalışmalar commit'e alınmadı.

Tek pati grafiği ve tek balon tekrar kullanılır; sürekli nesne/partikül üretimi, ağır tam ekran shader veya yeni animasyon framework'ü yoktur. Fiziksel iPhone performans/cihaz kabul testi bu aşamada yapılmadı. Test edilen senaryolarda kalan kritik hata veya taşma saptanmadı; gerçek cihaz hissi sonraki kullanıcı testine açıktır.

**Yeni build alınmadı. Yeni TestFlight, IPA veya Cloud Build oluşturulmadı.** Master'a merge yapılmadı.

## Git teslimi

Uygulama/QA/görsel commit’i `48d253a7a26a9b7426fb20ec4077a9ae2db29136`, private origin `recovery/part09-completion` dalına başarıyla push edildi. `git ls-remote` uzak SHA eşleşmesini doğruladı. Master `f243d7a355fedf9cfbc65f8060f1d3bc14cae169` olarak aynı kaldı. Bu teslim kaydı ayrıca dokümantasyon commit’iyle gönderilir.
