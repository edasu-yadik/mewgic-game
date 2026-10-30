# PART 11D — Güçlendiriciler için Pati Rehberi

31 Ekim 2026 — TAMAMLANDI. Dal: `recovery/part09-completion`.
Unity: 6000.3.27f1; mevcut sürüm korundu. Unity MCP bağlantısı doğrulandı; testler bağlı Editor Pipeline/Unity CLI üzerinden çalıştırıldı.

## Deneyim

Mevcut `PawGuide` genişletildi; ikinci bir öğretici yöneticisi eklenmedi. Bölüm 1'in karşılama → gerçek kaynak/hedef hamlesi → kutlama sırası korunuyor. Kutlamadan sonra yalnızca “Özel güçleri de öğrenmek ister misin?” daveti açılıyor. “Evet, Öğrenelim” dört kısa adıma geçer; “Şimdi Değil” oyuna döner.

Sıra: **Geri Al → İpucu → Ek Şişe → Karıştır**. Her adım aynı parşömen balonunu, Masalsı TMP fontlarını ve mevcut yumuşak giriş/çıkışı kullanır. Gerçek ilgili düğmenin etrafında tek altın ışık halkası ve sakin hareket eden altın pati görünür. İleri/son adımda Bitir ve her adımda Atla bulunur. Kedi → Güçlendiricileri Tanı ile istenildiğinde ilk adımdan yeniden açılabilir. Eksik düğme durumunda güvenle kapanır.

## Gerçek güç davranışı ve koruma

| Güç | Açıklamanın doğruladığı mevcut işlev |
|---|---|
| Geri Al | Son geçerli sıvı aktarımını geri alır; normal kullanım mevcut Geri Al hakkını tüketir. |
| İpucu | Solver tarafından doğrulanan hamlenin kaynak/hedefini görsel gösterir; normal Hint kuralları değişmedi. |
| Ek Şişe | O denemeye fazladan boş şişe ekler; mevcut tek kullanım kuralı değişmedi. |
| Karıştır | **Sıvıları karıştırmaz; şişelerin yerlerini değiştirir.** Öncesinde/sonrasında çözülebilirlik denetlenir. Kullanıcı metni bu gerçek davranışa uyarlandı. |

Rehber güçleri çalıştırmaz, ücretsiz hak üretmez. Mevcut mantıksal giriş kilidi ve araçların üzerindeki saydam raycast koruması yanlış güç/şişe dokunuşlarını engeller. Vurgu raycast almaz. Pause rehberi kapatır; kapanış geçişinden sonra koruma katmanları kalkar.

Mevcut save şeması ve `completedTutorials` listesi kullanılır: `paw.boosters.offer.v1` davet tercihini, `paw.boosters.v1` dört adımın bitmesini tutar. Yarıda Atla tamamlandı sayılmaz; kediden yeniden başlatılabilir. Temel eğitim kaydı `paw.basic.v1` korundu. Zaten temel eğitimi bitirmiş eski oyunculara girişte yeni zorunlu ekran açılmaz; kediden ulaşırlar. Asıl oyuncu kaydı, yedeği ve bozuk belge kurtarma anahtarı test öncesi/sonrası aynı kaldı.

## Gerçek test sonuçları

- C# derleme: **0 hata, 0 uyarı** (`COMPILE.json`).
- PART 11D Play Mode: **436 kontrol geçti** (`TESTS.json`). Bu sayı ayrı oyuncu oturumu değil; buton, veri, yerleşim ve tekrar kontrollerinin toplamıdır.
- Kedi menüsü/11B–11C regresyonu: **192 kontrol geçti** (`Regression11C/TESTS.json`). Menü, hızlı tekrar, kural balonları, oyun ortasında yardım, Pause/Resume, gerçek Hint hakkı, kaynak/hedef ve temel öğretici akışı doğrulandı. Eski testin “11D yok” beklentileri yeni isteğe bağlı davet/Bitir davranışına güncellendi; önceki teslim görselleri üzerine yazılmadı.
- Mevcut `GameplayBoosterTests.ModelTests()` geçti; izole gerçek PlayerPrefs anahtarına rehber geçmişi yazma/yeniden açma ve stok koruma testi geçti (`MODELS.json`). Ana oyuncu kaydına test yazılmadı.
- Başlangıç 3 Geri Al / 3 İpucu / 1 Ek Şişe / 1 Karıştır: dört eğitim adımından sonra **3 / 3 / 1 / 1**. Her adımda dört güç düğmesine ve şişelere doğrudan çağrı da uygulandı; tahta ve hamle sayısı aynı kaldı. Sıfır hakla eğitim de 0 / 0 / 0 / 0 olarak bitti.
- Kabul, daveti reddetme, ortada atlama, tekrar açma, tamamlanmış geçmişi yeniden yükleme, Pause ve normal sıvı aktarımı/Geri Al/Hint geçti.
- 900×1600 (9:16), 900×1950 (9:19.5), 900×2000 (9:20) gerçek Unity kamera/Canvas renderları alındı. 2000 testinde Safe Area kenar payı simüle edildi. Dört gerçek hedefin vurgu merkezi, tek vurgu, pati/balon ayrımı, metin taşması ve şişeyi örtmeme denetlendi.
- Yerel Unity penceresinde fareyle kedi → rehber → İleri akışı denendi. Rehber açıkken gerçek İpucu düğmesine fareyle basıldı: rehber aynı adımda ve stok **3** kaldı (`POINTER_CHECK.json`, `UnityEditor-PawGuide.jpg`).
- Son Console hem canlı buffer hem Editor groundTruth: **0 hata / 0 uyarı** (`CONSOLE.json`). Domain reload öncesindeki Editor yerleşim/OverlayCanvasesData mesajları temizlenip çalışma testleri ayrı yürütüldü; son test oturumlarında oluşmadı.

## Görseller

Gerçek Unity çalışma anı çıktılarıdır; arayüz mockup'ı değildir.

- `BasicStart.png`: başlangıçta yalnız temel eğitim.
- `Offer-1600.png`, `Offer-1950.png`, `Offer-2000.png`: isteğe bağlı davet.
- `Undo-*`, `Hint-*`, `ExtraBottle-*`, `Shuffle-*`: dört adımın üç orandaki görüntüleri.
- `BoosterSteps.png`: bu gerçek karelerden kesilmiş dört adımlı karşılaştırma.
- `PawBoosterPreview.gif`: 12 farklı Unity karesinden hareket önizlemesi; pati/halkadaki hareket görülebilir. Ham kareler yerelde tutulur, Git'e eklenmez.
- `UnityEditor-PawGuide.jpg`: Editor Game View ve Play Mode araç çubuğuyla doğrudan pencere yakalaması.

## Dosyalar ve sınırlar

Üretim değişikliği yalnız `Assets/Scripts/UI/PawGuide.cs`. Yeni `Assets/Editor/Part11DEditor.cs` ve `Assets/Scripts/Tests/Part11DProbe.cs` tekrar çalıştırılabilir Editor QA araçlarıdır; probe oyuncu derlemesinden hariçtir. `Part11CProbe.cs` regresyon beklentisi/ayrı çıktı yolu güncellendi. Meta dosyaları, bu rapor ve üretim ilerleme kaydı eklendi.

1376 tracked dosyalık başlangıç SHA256 listesiyle kontrol edildi. Sahne, prefab, karakter, font, şişe sanatı, oyun modeli, ödül, save uygulaması, ProjectSettings ve DevControl değiştirilmedi. Önceden bulunan başka untracked sanat/rapor dosyaları commit'e katılmadı. Ayrıntı `PRESERVATION.json`.

Fiziksel iPhone testi ve mobil GPU profili bu aşamada yapılmadı. Oran/Safe Area sonuçları Unity simülasyonudur. Yeni görsel asset/partikül sistemi eklenmedi; mevcut tek vurgu grafiği tekrar kullanıldı. Yeni TestFlight, IPA veya Cloud Build başlatılmadı; master'a merge yapılmadı. Editor izole önizleme kaydıyla rehber açık bırakıldı.

## Git teslimi

Doğrulanan değişiklikler mevcut geliştirme dalına ayrı commit ile gönderilecektir; kesin uzak SHA doğrulaması teslim kaydına eklenir.
