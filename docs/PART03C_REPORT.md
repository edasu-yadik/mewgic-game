# PART 03C — Üç şişenin Home entegrasyonu

24 Ekim 2026. Unity6000.3.27f1; gerçek Mewgic Editor MCP eval bağlantısı kullanıldı.

- Onaylı üç Art_v001 sprite değiştirilmeden HomePotionDisplay ile Unity UI Image olarak bağlandı. PrototypeApp.Home içindeki mor panel/düz şişe üretimi kaldırıldı; Gameplay kodu değiştirilmedi.
- Home.unity üzerinde sekiz sprite referanslı HomePotionDisplay bileşeni kaydedildi. HomePotionDisplay/LeftPotion, CenterPotion, RightPotion çalışma zamanında oluşturulur; her birinde BaseSprite ve Sparkles, yan şişelerde Glow bulunur.
- Taş platform mevcut WorldDecorations/EnvironmentComposition/Layer05_Foreground içindedir. HomePotionDisplay/PotionPlatform yalnız devre dışı hiyerarşi işaretidir; ikinci platform çizmez. Görünen taban bu gerçek katmana bağlanır. Yeni sanat üretilmedi.
- Orta/sol/sağ canvas yükseklikleri560/420/403: oran1/.75/.72. Saydam kenarlar ve pivot(.5,.075) hesaba katıldı; görünür alt sınırlar platform yüzeyine oturur.
- Yavaş1.5 birim nefes hareketi, düşük opaklıklı glow ve aralıklı Sparkles çalışır. Orta spiral onaylı ana sprite'a gömülüdür; ikinci kez çizilmedi. Parlaklık değişimi tüm orta sprite'a çok küçük ölçüde uygulanır, spiral bağımsız dönmez. Sağ kristal mevcut glow/sparkles ile ışıldar, ayrı kristal sprite yoktur.
- WizardCat prefabı, karakter sprite'ları ve animasyon dosyaları korunmuştur. Kedi katmanı şişelerin önündedir; UI en öndedir. PLAY tasarımı değiştirilmedi.

## Doğrulama

PART03C_TEST_RESULTS.json:99 kontrol başarılı. 900×1600,900×1950,900×2000 ve %4 yan/%5 alt safe-area simülasyonu: şişeler/kedi sınırlar içinde, PLAY ile örtüşme yok. Idle konum değişimi, glow parlaklığı, kedinin nefes/Blink/Wand Magic, gerçek EventSystem raycast ile PLAY ve Home tıklamaları başarılı.

12 mevcut model kontrolü tekrar geçti. Mevcut MewgicPlaySmoke.RunInteractive testi Boot→Home→Gameplay→Home, geçersiz hamle, animasyon kilidi, Undo, Restart, kazanma ve replay için exitCode0 ile geçti. C# Editor derlemesi ve Console son kontrolü başarılı. Fiziksel Android cihazı/APK bu kapsamda yeniden denenmedi.

PART03C_GAMEVIEW.png gerçek ScreenCapture.CaptureScreenshot çıktısıdır; Boot üzerinden Home'a geçilerek900×1600 Game View'dan alındı. Diğer oran görselleri çalışan Home Canvas'ının aynı Camera ile RenderTexture yakalamalarıdır. Tam ekran görüntüler docs/home-potions-*.png içinde.

## Referanstan kalan farklar

Üçlü odak, renkler, sol alttaki kedi ve alt PLAY ilişkisi sağlandı. Mevcut başlık, mor dikdörtgen PLAY ve alt metin korunmuştur; referanstaki profesyonel UI/navigasyon bu görevin kapsamı dışındadır. Arka plan mimarisi önceki aşamadaki çizimdir; birebir referans değildir. Platformda altın semboller var, referanstaki mavi rune farklıdır. Sol madalyon ve sağ kristal referanstan daha büyük, iç ışıltılar daha yoğundur. Uzun ekranlarda üst kısımda daha çok dünya alanı görünür; platform ve şişeler birlikte kalır. Sol şişenin alt-sol kısmını kedi doğal olarak örter.

Geri dönüş: C:/Dev/Mewgic-Part03C-Backup içinde önceki Home.unity, PrototypeApp.cs,tracked.patch ve başlangıç git durumu vardır. Önceki aşamalardan kalan ilgisiz yerel Blender kaynakları/denemeleri commit kapsamına alınmamıştır ve diskte korunur.
