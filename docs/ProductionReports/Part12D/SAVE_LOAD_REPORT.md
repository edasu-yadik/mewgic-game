# Save/Load — PART 12D

- Ana kullanıcı primary/backup/corrupt PlayerPrefs değerleri her iki Play Mode turunda önce/sonra aynı: **True**.
- İzole GUID PlayerPrefs ve geçici FileLevelArchive kullanıldı; üretim arşivine yazılmadı. Başlangıçta gerçek GeneratedLevels klasöründe 0 dosya vardı; sonrasında da 0. PRESERVATION.json.
- Harita konumu, tamamlanan bölümler/yıldızlar, güç miktarları, Pati geçmişi ve manualLocale dahil tam JSON diskten yeniden açıldığında eşleşti.
- Ayrı gerçek Boot→Home→PLAY oturumunda **Almanca manuel tercih ve Bölüm601 pin imzası** aynı: RELOAD_TESTS.json, 302 assertion geçti. Gereksiz ilk eğitim tekrar açılmadı.
- Model testinde gelecek üretici sürümü taşıyan tam tanım yeniden üretmeden yüklenir; bozuk pin yedekten kurtarılır; iki bozuk dosya rastgele yeni bölümle değiştirilmez.
- Başlatma seçici testleri tr-TR/en-US/de-DE/pt-PT ve desteklenmeyen ja-JP→en zincirini geçti. İlk otomatik seçim manuel tercih alanına yazılmadı.
- QA örnekleri için farklı kontrollü ilerleme profilleri kuruldu. Bu kurulum gerçek kullanıcının ilerlemesi değildir.

Yeni kayıt şeması veya SaveManager eklenmedi. Preview de ayrı test profili kullanır; Editor Play Mode kapatılınca gerçek profil bağlantısı geri yüklenir.
