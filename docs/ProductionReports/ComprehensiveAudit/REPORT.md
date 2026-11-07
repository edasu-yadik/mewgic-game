# MeWGic · Unity denetimi

Başlangıç dalı `release/0.8.0-rc.1`, başlangıç commit'i `09ba118`. Yerel SHA uzak dal ile eşleşiyordu. Proje ve çalışan Unity Editor sürümü 6000.3.27f2.

| Özellik | Önceki durum | Son durum | Unity'de doğrulandı mı? |
|---|---|---|---|
| Kristal Şelaleler görünümü | Referansa göre hazırlanmış 91–100 çevre çizimi | Tam sahne kadrajı, sağ üst saray, pembe ağaç, fenerler, kristaller ve mor-altın 100 birlikte kontrol edildi. Ekrana bağlı kenar sisi eklendi. | Play Mode ve üç portre oranı |
| Hareketli şelaleler | Kristal su shader'ı | Sağ üst, sol yamaç, kemer altı ve sağ alt şelalelerde gerçek Game View kare farkı ölçüldü. | Play Mode ve piksel örneklemesi |
| Yol ve perspektif | Boyanmış, dört kemerli kıvrımlı yol | Yol, taşlar ve maskot birlikte kayıyor. Teraslar kadraja hizalanıyor, kenar sisi ve arayüz viewport'a bağlı. | Play Mode |
| Kedi dinlenme animasyonu | Oturma ve hafif nefes | Asasız kıvrılma, zemin temaslı nefes, kısa göz ve kulak hareketi. | Play Mode |
| Kedi uyanma animasyonu | Yok | Gözünü açma, gerinme ve mutlu ayağa kalkma. | Play Mode |
| Kedi yürüyüşü | Yol eğrisinde iki adım pozu | İki adım pozu izlerle aynı ilerleme ritminde. 100'e uğrayıp alt sol kayaya yürür. | 99 → 100 Play Mode |
| Altın pati izleri | Adım boyunca kademeli çizim | Yürüyüşle eşzamanlı görünüm ve varışta sönme. | Game View ve mesh kontrolü |
| Bölüm açılma animasyonu | Altın vurgu ve bekleyen kayıt | 100'e ulaşınca altın açılış. Yarım kalan geçiş yeniden oynar, gösterilmiş geçiş tekrar etmez. | Play Mode ve geçici PlayerPrefs günlüğü |
| İlk dört bölüm öğreticisi | Yeniden oynatılabilen, yaklaşık üç saniye bekleyen pati | 1–4, seçili kaynak, yeniden bekleme, tamamlanmış kayıt ve 5. bölüm sınırı doğrulandı. | Play Mode |
| Şişe tamamlanma efekti | Altın halka ve çevresinde yıldızlar | Yıldızlar yükseliyor. Gerçek döküş, bağımsız tetiklenme ve tek seferlik kutlama kontrol edildi. | Play Mode ve Game View |
| Oyun içi maskot boyutu | Yerleşim sınırı olan büyütme | Önceki formüle göre üç oranda da %13,0 artış, şişelerden ayrı ve güvenli alanda. | Üç portre oranında Play Mode |
| Günlük görevler | Tıklanabilir parşömen, ilerleme ve ödüller | İkon üzerinden gerçek raycast, panel ve çift tıklamada tek ödül kontrolü. | Home Play Mode |
| 24 saatlik sayaç | UTC döngü ve iki sayaç | Ana sayfa ve panel canlı. 24 saat sınırı, kayıt yeniden açılışı, çevrimdışı süre ve geri alınan saat denetlendi. | Play Mode ve kayıt testleri |
| Dünya geçişleri | Kayıtlı açılış ve kamera geçişi | 100 → 101 ve toplam 12 dünya sınırında doğru düğüm, kadraj ve kayıt. | Play Mode |

## Dosyalar

- `Assets/Art/Environment/Map/CrystalJourney/CrystalCat_RestWake.png`, içe aktarma ayarı ve `Assets/Resources/Map/JourneyThemes/crystal.asset` Kristal maskotunun dört yeni pozu.
- `MapJourneyTheme.cs`, `MapWorldCrystalJourney.cs`, `MapWorldJourneyMotion.cs` Kristal'e özel sis ve maskot akışı.
- `BottleCompletionGraphic.cs` altın parçacıkların yükselme hareketi. `GameplayPresentation.cs` üç oranda ölçülen %13 maskot ölçeği.
- `MapFlowProbe.cs`, `GameplayPolishProbe.cs`, `ComprehensiveAuditValidation.cs`, `MapFlowValidation.cs`, `GameplayPolishValidation.cs` tekrar çalıştırılabilir sahne, kayıt ve Play Mode denetimleri.

## Gerçek Game View kanıtları

- [Tam Kristal kadrajı ve kıvrılmış kedi](Map/03-Crystal-100.png), [dinlenmede göz ve kulak hareketi](Map/11-Idle-Blink.png), [uyanma](Map/09-Wake.png), [ayağa kalkma](Map/10-Standing.png), [yürüyüş ve izler](Map/07-Walk-99-100.png), [92 altın açılışı](Map/02-Unlock.png), [100 → 101](Map/05-Island-101.png).
- [99 → 100 animasyonu](Map/Crystal-99-100.gif), [ikinci şelale karesi](Map/08-Water-Later.png), [şelale piksel ölçümü](Map/WATER_CHECK.json).
- [Pati kaynak](Gameplay/01-Paw-Source.png), [pati hedef](Gameplay/02-Paw-Target.png), [gerçek döküş kutlaması](Gameplay/07-Real-Pour-Completion.png).
- [Ana sayfada görev ikonu ve sayaç](Gameplay/06-Home-Quest-Icon.png), [görev paneli](Gameplay/05-Quests-1600.png), [alınmış ödül](Gameplay/08-Daily-Claimed.png).
- Kristal [9:16](Map/04-Portrait-1600.png), [9:19.5](Map/04-Portrait-1950.png), [9:20](Map/04-Portrait-2000.png). Oynanış [9:16](Gameplay/03-Gameplay-1600.png), [9:19.5](Gameplay/03-Gameplay-1950.png), [9:20](Gameplay/03-Gameplay-2000.png).

## Tekrar çalıştırma

1. `C:\Dev\Mewgic` projesini Unity 6000.3.27f2 ile aç. Play Mode kapalı olsun.
2. **Mewgic → Comprehensive Audit → Validate Bindings and Save Rules** ile sahne, prefab, görsel bağlantıları ve kayıt kurallarını doğrula.
3. `Assets/Scenes/LevelMap.unity` sahnesinde **Run Crystal Map QA** komutunu çalıştır. 99 → 100'de uyanmayı, yürüyüşü, altın açılışı ve sol kayadaki dinlenmeyi izle.
4. İlk Play Mode testi bitince **Run Gameplay and Daily QA** komutunu çalıştır. Test, `Gameplay.unity` ve `Home.unity` sahnelerini geçici oyuncu profiliyle açar.

## Çalıştırılan denetimler

- [Harita Play Mode](Map/UNITY_TESTS.json) 107, [oynanış ve günlük görev Play Mode](Gameplay/UNITY_TESTS.json) 114 kontrol. Her iki tur da oyuncu kaydını ve kurtarma anahtarlarını byte düzeyinde korudu.
- [Harita kayıt modeli](Map/MODEL_TESTS.json) 80, [günlük kayıt modeli](Gameplay/MODEL_TESTS.json) 16 ve [günlük regresyon](Gameplay/DAILY_REGRESSION.json) 53 kontrol.
- [Sahne, prefab ve görsel bağlantı denetimi](BINDINGS.json) 268 kontrol. Son [Console](CONSOLE.json) ve derlemede sıfır hata ve sıfır uyarı.
- 99 → 100 GIF'i, 360 × 640 boyutunda 25 gerçek Game View karesinden oluşturuldu.

## Doğrulama kapsamı

Sahneler, oyun arayüzü ve çalışma zamanı kareleri Editor'de doğrulandı. Su hareketi görsel renk örneklerinden ve shader saatinden denetlendi. Kale, ağaç ve kayalıklar teras çizimiyle birlikte hareket eder. Kenar sisi ve kontroller ekran konumunu korur. Başlık ve alt kontroller oyunun çalışan Unity arayüzüdür. Portre oranları Unity Game View profilleridir. Fiziksel cihazda dokunma, pil/GPU profili ve dağıtım build'i denenmedi. Kapatıp açma kontrolleri ayrı PlayerPrefs anahtarları ve sahne yeniden yüklemesi kullanır. İşletim sistemi sürecini sonlandırıp başlatma senaryosu bu turda uygulanmadı. Kristal dışındaki dünyalar aynı sanat varlıklarını kullanıyor.

## Git teslimi

Uygulama commit'i `2d782fa02155995c65d22fab75b2fd71b9cf3325` uzak `origin/release/0.8.0-rc.1` dalında SHA eşleşmesiyle doğrulandı.
