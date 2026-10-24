# PART 04B — Profesyonel PLAY butonu

25 Ekim 2026. Yalnız Home PLAY sunumu değişti.

## Çıktılar

Assets/Art/UI/PlayButton/ altında PlayButton_Normal.png(1967×800),PlayButton_Pressed.png(1966×800),PlayButton_Glow.png(1966×800). Gerçek alpha,Sprite Single/FullRect,pivot(.5,.5),PPU100,max2048,sRGB,Bilinear/Clamp,mipmap/ReadWrite kapalı,Android ASTC4x4. Üç sprite'ın tahmini toplam ASTC belleği4.50MiB. Kaynak PNG'ler korunur.

Referanstaki yatay oval form,parlak yeşil yüzey,altın kabartma,beyaz PLAY/üçgen ve süslemeler korundu. İstendiği gibi üst kristal mavi-mor yapıldı. Referansa göre altın daha doygun,harf ve yan süs kıvrımları biraz farklıdır; kaynak temelli görsel üretimi birebir kesim değildir. Ana yeşil yüzeyde hafif saydamlık vardır. Pressed sürüm aynı tasarımın daha koyu/az yansımalı çeşididir; küçük üretimsel piksel farkları ve1px canvas farkı vardır. Yazı/ikon sprite'a gömülüdür.

Glow ayrı üretimsel katmandır; tam piksel eşleşmeli maske değildir. Kenar alpha en yüksek21/255,ana sprite1/255,pressed0; glow runtime %3–6 opaklıkla kullanıldığından dış sınır çok hafiftir. Açık/koyu Unity birleşimlerinde kontrol edildi. Glow büyük opaklıkta kullanılmamalıdır.

## Entegrasyon

HomePlayButtonPresenter Home sahnesinde üç sprite referansını saklar. PrototypeApp.Home içinde mevcut Play Button'a bağlanır; eski onClick olayı saklanır ve aynı Gameplay bağlantısı0.14 saniyelik bırakma animasyonu sonrası çağrılır. Gameplay kodu değiştirilmedi.

PremiumPlayButton eski Outline ve Label'ı yalnız runtime devre dışı bırakır. Görsel ayrı PlayArtwork/FrameGlow çocuklarıdır. Button referansı ve EventSystem girişi korunur. Oval hit filtresi saydam köşe tıklamalarını reddeder. Safe-area konumu x(.15,.85),y(.14,.28); üst sınır ve karakter boşluğu korunur. Görsel preserveAspect ile esnemez.

Idle:çok düşük glow salınımı. PointerDown:scale0.966'ya yumuşak yaklaşım/Pressed sprite. PointerUp/Exit:normal boyuta dönüş. Click:queued kilidi hemen açılır,button.interactable=false,AcceptedClicks bir kez artar.0.14 saniye sonra mevcut olay çağrılır. Aynı frame tekrar onClick çağrısı bile ikinci geçiş başlatmaz. PointerExit yalnız görseli normale döndürür; dışarıda bırakma standart Button davranışıyla geçiş yapmaz.

## Gerçek testler

PART04B_TEST_RESULTS.json:164 kontrol başarılı.900×1600,900×1950,900×2000 ve safe-area simülasyonu; kedi/şişelerle örtüşme yok. Logo,Idle/Blink/Wand Magic ve şişe hareketleri çalışıyor. Gerçek EventSystem PointerDown/PointerUp/PointerClick olayları ile basılı sprite/küçülme,tekrar tıklama kilidi,bırakma gecikmesi,PLAY→Gameplay→Home doğrulandı. C# Editor derlemesi başarılı. Son Console0 hata/0 uyarı,Home dirty=false. Fiziksel Android cihazı ve yeni APK denenmedi.

PART04B_GAMEVIEW.png gerçek900×1600 Game View ScreenCapture çıktısıdır. home-play-*.png çalışan Home Canvas'ından oran/pressed/magic/blink kamera yakalamalarıdır. PART04B_COMPARISON_Dark/Light.png:referanstaki PLAY / yeni normal sprite / gerçek Home butonu. Ana menüde referansa göre buton biraz daha küçük ve aşağıdadır; karakter/şişeleri oynatmadan mevcut kompozisyon korundu. Alt kaynak/navigasyon sistemleri eklenmedi.

Korunan asset/kod hash kanıtı PART04B_PRESERVATION.json içindedir. Home.unity ile PrototypeApp.Home dışındaki eski dosyalar değiştirilmedi. Logo,kedi prefabı/animasyonları,şişeler,arka plan,Gameplay ve kayıt sistemi korunur. Geri dönüş kopyaları C:/Dev/Mewgic-Part04B-Backup içinde. DevControl'e dokunulmadı.
