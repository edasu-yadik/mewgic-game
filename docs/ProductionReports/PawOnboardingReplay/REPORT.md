# İlk dört bölümde tekrar kullanılabilen pati öğreticisi

7 Kasım 2026 · MeWGic · `release/0.8.0-rc.1`

Pati yardımı 1–4. bölümlere her girişte etkinleşir. Öğrenilmiş ders işaretleri ve bölümün tamamlanmış olması bu kararı etkilemez. Her geçerli döküşün ardından bekleme yeniden başlar. Oyuncu yaklaşık üç saniye etkileşimde bulunmazsa mevcut şişe düzeninden çözüme götüren bir hamle hesaplanır. Oyuncu veya İpucu güçlendiricisi kaynak seçmişse pati o şişe için uygun bir hedef arar. Seçili kaynaktan çözüme götüren bir hedef doğrulanamazsa pati yeni seçimi bekler.

Süre, pati görseli ve kaynak ile hedef arasındaki hareketler mevcut ayarları kullanır. Normal öğretici yalnızca sahne içi durum tutar. Kayıttaki yıldızlar, tamamlanmalar, ödüller ve eski öğretici işaretleri yerinde kalır. Sürekli yardım 1–4. bölümlerle sınırlıdır.

| Dosya | Değişiklik |
| --- | --- |
| `Assets/Scripts/UI/PawOnboarding.cs` | Bölüm girişinde etkinleştirme, hamle sonrası bekleme ve güncel bulmacadan hamle seçimi |
| `Assets/Scripts/UI/PawGuide.cs` | Döküş bitişi ve yeniden başlatma bağlantıları |
| `Assets/Scripts/Presentation/PrototypeApp.cs` | Öğreticinin oyundaki gerçek şişe seçimini okuması |
| `Assets/Scripts/Tests/GameplayPolishProbe.cs` | İlk giriş, döküş sonrası yardım, yeniden giriş ve kayıt açılışı senaryoları |
| `Assets/Editor/GameplayPolishValidation.cs` | İzole pati testi için Unity menüsü |

## Unity'de deneme

1. Play Mode kapalıyken **Mewgic → Gameplay Polish → Run Paw Replay QA** menüsünü çalıştırın. Test izole kayıtla çalışır ve sonuçları bu klasöre yazar.
2. Elle denemek için haritadan 1–4. bölümlerden birini açın. Üç saniye bekleyin, gösterilen aktarımı yapın ve tekrar bekleyin. Pati yeni şişe düzenine uygun hamleyi göstermeli.
3. Bölümden çıkıp aynı bölüme dönün. Bunu yıldızları olan tamamlanmış bir bölümde ve oyun yeniden açıldıktan sonra da tekrarlayın. 5. bölümde beklediğinizde bu yardım başlamamalı.

## Doğrulama

Unity 6000.3.27f2 üzerinde **58 Play Mode kontrolü** geçti. İlk dört bölümde ilk giriş ve döküş sonrası yardım, aynı oturumda yeniden giriş, eski öğretici işaretleri taşıyan tamamlanmış 1–4. bölümler, İpucu güçlendiricisinin seçtiği kaynak ve 5. bölümün kapsamı doğrulandı. Yeniden açılış, ayrı bir PlayerPrefs profili diske yazılıp `GameplayProgress.Open` ile yeniden okunarak ve `Gameplay` sahnesi yeniden yüklenerek sınandı. Fiziksel cihazda uygulama sürecini sonlandırma testi yapılmadı.

Derleme ve Console sıfır hata, sıfır uyarı verdi. İzole test, oyuncunun ana kaydı ve yedek kayıt anahtarlarını byte düzeyinde karşılaştırdı. `UNITY_TESTS.json`, `CONSOLE.json` ve iki Unity Game View karesi bu klasörde yer alıyor.
