# PART10F — Alt oynanış açıklamasını kaldırma

30 Ekim2026 • Unity6000.3.27f1 • recovery/part09-completion.

Kaynak Gameplay sahnesinde kayıtlı nesne değil,GameplayTools.Mount içindeki
Safe area/BoosterPolicy TMP üretimiydi. “Ücretsiz başlangıç hakları • satın
alma yok” Label çağrısı kaldırıldı. Metin nesnesi veya boş yer tutucu kalmaz.

Restart/Home aynı Y0,027–0,065 anchor'larıyla aynı hizada tutuldu. Konum,
boyut,raycast alanı ve onClick davranışları değişmedi. Anchor tabanlı düzende
silinen metin için LayoutGroup yüksekliği ayrılmıyordu; alttaki kenar payı
Home Indicator güvenliği için korundu. Ekonomi,haklar ve satın alma kodu yok.

FooterCleanupProbe gerçek Unity Play Mode'da30 kontrolü geçti: nesne/metin
yokluğu,üç oranda buton hizası ve korunan yükseklik,Safe Area alt sınırı,
her iki butona gerçek raycast,gerçek aktarım sonrası Restart ve Home'a dönüş.
Runtime hata0/uyarı0;gerçek oyuncu PlayerPrefs belgesi aynı. C# derlemesi geçti.
900×1600/1950/2000 ve sonuncuda Safe Area iç boşluğu render edilip incelendi.
Gameplay-GameView.png doğrudan ScreenCapture çıktısıdır. Fiziksel iPhone
Home Indicator/dokunma testi yapılmadı;Safe Area simülasyonu doğrulandı.

MCP status Pipeline instance bulamadı; testler aynı Unity Editor'de
executeMethod ile çalıştı. Editor kapanışındaki bilinen JobTempAlloc
mesajları runtime hata/uyarı listesi değildir.

Üretimde yalnız GameplayTools.cs değişti;PRESERVATION.json hash kaydıdır.
Master merge ve yeni TestFlight/Cloud Build/IPA yok. Mevcut geliştirme
dalına commit/push yapılır;uzak SHA teslimde doğrulanır.
