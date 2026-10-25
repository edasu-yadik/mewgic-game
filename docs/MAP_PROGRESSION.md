# Harita ilerleme verisi

PART06B harita tanımı: Assets/Resources/Map/World.json.

Enchanted Village, doğrulanmış Level01 ve Level02 dosyalarını kullanır. Üçüncü konumun değeri0 olduğundan bölüm değildir. Diğer bölgelerdeki3–11 kimlikleri yalnız gelecek konum tanımlarıdır; gerçek bölüm dosyası veya açık seviye sayılmaz. Projede DesignData/Levels bulunamadı. Yalnız Assets/Resources/Levels/Level01.json ve Level02.json bağlandı; kaynakları değiştirilmedi.

Yeni bir JSON dosyası kopyalamak tek başına seviye açmaz. GameplayLevelCatalog.Load hem açık katalog kimliğini, metadata/yıldız eşiklerini, kapasite/renk miktarlarını hem de mevcut PuzzleSolver çözümünü doğrular. MapProgressionCatalog yalnız başarılı doğrulama sonuçlarını kullanır; başarısız sonuçlar Editor domain/oyun oturumu boyunca kapalı tutulur. Yeni üretim seviyeleri için katalog ve harita tanımı birlikte gözden geçirilmeli ve gerçek çözüm testleri çalıştırılmalı.

## Erişim

Harita sırasındaki tüm önceki pozitif kimlikler doğrulanmış ve tamamlanmış olmalıdır. Bölge ayrıca önceki bölgelerin bitmesini ve kendisinde doğrulanmış içerik bulunmasını ister. Tamamlanan seviyeler yeniden oynanır. Bilinmeyen, eksik veya bozuk dosya asla oyuncu kaydı sayesinde doğrulanmış hale gelmez.

Kilitli bölgenin sekmesi çevreyi gezdirir; bu bir bölüm kilidi açma işlemi değildir. Kilitli düğüm açıklama açar. Haritadan seçilen bölüm, alt geri düğmesi, Pause geri düğmesi veya sonuç geri düğmesiyle LevelMap'e döner. Home'dan PLAY ile başlayan önceki akış Home'a döner. Home alt Map sekmesi veri ve sahne doğrulandıktan sonra haritayı açar.

## Kayıt ve sandık

Mevcut Mewgic.Progress.v1 anahtarı ve ProgressLedger korunur. bestStars en yüksek kazanma sonucudur; düşük yıldızlı tekrar sonucu düşürmez. claimedMilestones listesi aynı belgedeki ek alandır. Eski v1 belgelerinde alan yoksa boş listeyle açılır; bakiye ve mevcut yıldızlar korunur. Bozuk kayıt özgün anahtarın üzerine yazılmaz.

MilestoneReward, sabit ödül kimliği, BoosterKind ve miktardan oluşur. Bu sürümde bölge sandığı+1 İpucu hakkı verir; para veya satın alma eklenmez. Bölge tamamlama ve önceki bölge koşulları sağlanmadan alınamaz. Ödül hakkı artışı ve alındı kimliği tek ProgressLedger Commit işlemindedir. Yazma hatasında ikisi de değişmez; tekrar alma ve sayı taşması engellenir. Yayınlanmış ödül kimliği değiştirilmemelidir; değiştirmek yeni ödül anlamına gelir.

Bu sürümde yalnız köy sandığı gerçek içerikle alınabilir. Diğer üç sandık gelecek içerik doğrulanmadan kapalıdır. Gelecek bölge sıralaması birim testinde sentetik doğrulama sağlayıcısıyla sınanır; oyuna sahte seviye verisi eklenmez.

## Tekrar doğrulama

Editor Play kapalıyken MapProgressionTests.Run() çağrısı model, ayrı PlayerPrefs anahtarıyla gerçek disk yeniden okuması ve izole profil üzerinde harita→iki gerçek bölüm→harita→sandık→Home→harita→Pause dönüşünü test eder. Oyuncu kayıt anahtarının değişmediğini kontrol eder. MapWorldTests.Run() üç oran, Safe Area, küçük ekran, kaydırma ve kapalı konum etkileşimlerini sınar. Önceki yalnız önizleme varsayımları yeni gerçek seviye akışına güncellendi.
