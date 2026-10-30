# PART10D — Gameplay üst HUD sadeleştirme

30 Ekim2026 • Unity6000.3.27f1 • recovery/part09-completion.

## Yeni düzen

GameplayHUD tek satırdır: sol küçük Bölüm paneli,sağ Hamle paneli,en sağ
Pause. Büyük birleştirilmiş kutu yok. Mevcut altın/mor panel ve Pause
sprite'ları korundu; bölüm başlığı Masalsı serif,hamle ve sayılar sade TMP
fontuyla gösterilir. Küçük mor kristal aksanı yalnız5 vertex içerir.

Üst konteyner yüksekliği Safe Area genişliğine göre76–100 mantıksal birim;
900×1600 tam alanda eski408 yerine94,5 birimdir. Şişe board üst anchor'ı
0,73'ten0,90'a çıktı; alt sınır0,30 aynı. Kullanılabilir board yüksekliği
688→960px (+272px,yaklaşık%40). Mevcut responsive şişe/shelf yerleşimi bu
alanı kullanır; sprite veya oyun kuralı değiştirilmedi. Eski yıldız panelinin
boşluğu oyun alanına katıldı. Kedi,alt araçlar ve Pati balon bandı korundu.

İki gerçek bölümün hedefleri Renkleri ayır ve Üç rengi ayır aynı mekaniktir;
büyük GoalPanel üretilmez. Yeni oyuncunun Pati giriş mesajı ve kedi menüsündeki
Nasıl oynanır açıklaması korunur. Farklı metadata hedefinde kısa SpecialGoal
satırı koşullu gösterilir; board üst sınırı0,84 olur. Bu yol test metadata'sıyla
doğrulandı; gerçek oyuna yeni hedef veya bölüm eklenmedi.

## Dinamik veri ve test

LevelValue aktif GameplayHudState.Level, Moves gerçek state.Moves kullanır.
Örnek28/12 sabitlenmedi. CompactHudProbe gerçek Unity Play Mode'da başarılı:
Pause/Resume,gerçek aktarım→Hamle1,Undo→0,Restart→0,gerçek zafer→NextLevel→
Bölüm2/Hamle0 ve Gameplay→Home çalıştı. Ortak hedefte ek satır kapalı;
farklı hedef kısa satırda görünür. Oyuncunun gerçek PlayerPrefs kaydı aynı.

900×1600/1950/2000 ve son oranda Safe Area iç boşluğu: küçük paneller/Pause
sınırlar içinde,board ile çakışmıyor;iki sayaçta TMP taşması yok.
UI_AUDIT.json tüm assertion'ları içerir; runtime hata0/uyarı0.
GameplayHudTests.UnitTests,DailyActivitiesTests.Models ve RewardHubTests.Models
ayrıca başarılı. C# hata/uyarı yok. Eski UI testlerindeki HAMLE etiket
beklentileri Hamle'ye ve kaldırılan GoalPanel aramaları yeni düzene uyarlandı;
her eski UI suite'i ayrıca çalıştırılmış değildir.

MCP status Pipeline instance bulamadı; gerçek Editor executeMethod ile
çalıştırıldı. Önceden bilinen Editor kapanış JobTempAlloc mesajları runtime
Console sayısı değildir. Fiziksel iPhone kabulü yapılmadı.

## Görsel ve koruma

Before görüntüleri bu part başlangıcıyla aynı PART10C After gerçek Unity
çıktılarının kopyasıdır. After-GameView.png yeni doğrudan ScreenCapture;
After-1600/1950/2000.png oran/Safe Area kamera render'ları,Level2.png gerçek
ikinci bölüm görüntüsüdür. Compare.png solda önce,sağda sonra gösterir.
Önceki yıldız kutusu geri getirilmedi;Pati Yardımı ve ödül sistemi korundu.

Üretim değişiklikleri GameplayHud.cs ve GameplayPresentation.cs ile sınırlı.
Core model/kayıt/ödül,bölüm JSON'ları,font/bar asset'leri ve sahneler aynı;
PRESERVATION.json hash karşılaştırmasını kaydeder. Test araçları Editor
kapsamındadır. Master merge ve yeni TestFlight/Cloud Build/IPA yoktur.
Commit/push mevcut geliştirme dalına yapılır; uzak SHA teslimde doğrulanır.
