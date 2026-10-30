# PART10C — Gameplay üç yıldızlı hedef kutusu

30 Ekim2026 • Unity6000.3.27f1 • recovery/part09-completion.

## Kaynak ve değişiklik

Gameplay sahnesindeki HUD,GameplayHud.Mount tarafından runtime üretilir.
Hiyerarşi: Safe area/GameplayHUD/PerformancePanel; çocukları
PerformanceStar1–3 ve PerformanceValue. Ayrı UI prefabı yoktu. Gerçek bir
Filled ilerleme barı bulunmuyordu; üç ikon ve hedef metni vardı.
Panel,üç ikon ve metnin oluşturulması/güncellenmesi kaldırıldı.
Paylaşılan yıldız/panel sprite asset'leri silinmedi.

LevelPanel,GoalPanel,MovesPanel,Pause ve şişelerin anchor/ölçüleri aynı.
Yeni tasarım veya büyütme yapılmadı. LayoutGroup/spacer olmadığından boş
nesne veya sabit yükseklik rezervi kalmadı. Serbest kalan kutu alanı Safe Area
normalize X0.19–0.81,Y0.753925–0.8164;900×1600 tam Safe Area'da yaklaşık
558×100px. Bu alan sonraki part için arka plan olarak boş bırakıldı.

## Ödül koruması ve gerçek testler

GameplayHudState yıldız hesabı,GameplayEndScreen,ProgressLedger kayıt/ödül
ve MapWorld yıldız sunumu değiştirilmedi. HUD.State veri alanı korunur.
GameplayHudTests yalnız kaldırılan görsel nesnelere bakmayacak şekilde
güncellendi; ödül birim testleri silinmedi.

StarPanelProbe:22 kontrol geçti,runtime hata0/uyarı0,gerçek oyuncu PlayerPrefs
belgesi değişmedi. Panel/ikon/metin yokluğu,diğer HUD elemanları,Pause/Resume,
gerçek bölüm çözümü/zafer,üç yıldız ödülü,JSON yüklemede yıldız korunması,
Home→Map akışı ve harita model/rozetinde3 yıldız doğrulandı.
GameplayHudTests.UnitTests (1/2/3 yıldız eşikleri),Daily ve RewardHub model
grupları ayrıca çalıştırılır; MODELS.json sonucun kaydıdır.

Before/After900×1600/1950/2000 (sonuncu Safe Area iç boşluğuyla) gerçek Unity
kamera render'ları ve Before/After-GameView doğrudan ScreenCapture görüntüleri
docs/Part10C altında. Görsel karşılaştırmada diğer elemanların konumları korundu.
Victory.png ve MapStars.png bağımsız yıldız sunumlarını gösterir.

MCP status Pipeline instance bulamadı; sahne aynı Unity Editor'de
executeMethod ile açılıp test edildi. C# derlemesi başarılıdır. Önceden
bilinen Editor kapanış JobTempAlloc mesajları runtime Console sayısı değildir.
Fiziksel iPhone testi yapılmadı.

Üretim değişikliği yalnız GameplayHud.cs; hash karşılaştırması
PRESERVATION.json. Yeni test/rapor/kanıt dosyaları bu part kapsamındadır.
Master merge ve yeni TestFlight/Cloud Build/IPA işlemi yoktur.
Commit/push mevcut geliştirme dalına yapılır ve uzak SHA teslimde doğrulanır.
