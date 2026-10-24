# PART 04A — Mewgic logosu

25 Ekim 2026. Yalnız Home logosu geliştirildi.

## Üretim

Assets/Art/UI/Logo/Mewgic_Logo_Main.png:1720×914 gerçek RGBA. Yerleşik image_gen ile Master kapak üzerinden kontrollü ayırma/onarım ve kenar payı düzeltmesi yapıldı. Normal font veya kodla çizim değildir. Altın kabartmalı harfler, mor dekoratif plaka, altın işlemeler, pati madalyonu, mavi-mor kristal ve küçük yıldızlar korunmaya çalışıldı. Kedi/şişe/kale/gökyüzü içermez.

Bu üretim birebir piksel kesiti değildir. Referansa göre kristalin boyutu, harf kıvrımları, alt pati şekli, yapraklar ve filigranların konumlarında küçük farklar var; altın tonları daha doygundur. Genel kimlik ve yazım korunur. Kesin orijinal vektör veya katmanlı logo değildir.

Ayrı Mewgic_Logo_Glow.png ve Mewgic_Logo_Sparkles.png üretilmedi. Ana sprite'ın küçük parlamaları yeterli görüldü; gereksiz efekt zorlanmadı. Logo animasyonu eklenmedi.

## Unity

HomeLogoPresenter Home üzerinde kaydedilmiş logo referansını kullanarak runtime UI/MewgicLogo Image oluşturur. PrototypeApp.Home içinde yalnız eski Brand düz metin çağrısı değişti. Logo preserveAspect=true, raycastTarget=false; safe-area anchorları(.1,.775)–(.9,.925). Üstteki%7.5 güvenli alan gelecekte kaynak barı için ayrılır. Mevcut alt başlık, kedi, şişeler, arka plan, PLAY ve Gameplay korunur.

Sprite Single, FullRect, pivot(.5,.5),PPU100,alphaIsTransparency,sRGB,Clamp/Bilinear,max2048,mipmap/ReadWrite kapalı; Android ASTC4x4. Alpha0–255,902352 ara alpha pikseli; dış çerçeve en fazla1/255, görünür dikdörtgen yok. Tahmini ASTC bellek1.50MiB.

## Gerçek kontroller

PART04A_TEST_RESULTS.json:141 kontrol başarılı.900×1600,900×1950,900×2000 ve çentik/güvenli alan simülasyonu: logo sınır içinde, oran korunur, PLAY ve orta şişeyle örtüşmez. Kedi Idle/Blink/Wand Magic, şişelerin hareket/glow kontrolleri, gerçek EventSystem tıklamasıyla PLAY→Gameplay→Home başarılı. Oyun mekaniği yeniden yazılmadı.114 korunan dosya SHA256 ile değişmedi.

PART04A_GAMEVIEW.png gerçek900×1600 Game View ScreenCapture çıktısıdır; Boot→Home üzerinden alındı. Diğer oranlar çalışan Home Canvas'ının kamera render yakalamalarıdır. PART04A_COMPARISON_Dark.png ve Light.png: kaynak kapak logo bölgesi / yeni gerçek alpha sprite / gerçek Game View logo bölgesi. Fiziksel Android cihazı ve yeni APK bu aşamada denenmedi.

Son Editor:Home açık,dirty=false,Console0 hata/0 uyarı. Kanıtlar PART04A_IMPORT.json,ALPHA_AUDIT.json,FINAL_CHECK.json,TEST_RESULTS.json,PRESERVATION.json dosyalarında. Kaynak istem PART04A_PROMPTS.md.

Geri dönüş kopyaları:C:/Dev/Mewgic-Part04A-Backup. Önceki yerel varlıklar korunur ve bu commit'e ilgisiz dosyalar eklenmez. DevControl'e müdahale edilmedi.
