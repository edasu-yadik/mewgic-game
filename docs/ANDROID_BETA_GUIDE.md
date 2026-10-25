# Mewgic Android beta adayı — 0.8.0 / kod 8

Bu paket cihaz kabul testi tamamlanana kadar kararlı beta olarak kabul edilmez.
Oyun ARM64, Android 8.0/API26 ve üstünü hedefler. Paket kimliği mevcut
`com.mewgic.game` olarak korunur; önceki kurulumla güncelleme/kayıt sürekliliği içindir.
Yayın kimliğinin sahipliği doğrulanmadı. Test imzası yayın imzası değildir.

## Kurulum ve gerçek telefon kabul testi

1. APK'yı telefona kopyalayıp Dosyalar uygulamasında aç; yalnız bu dosya kaynağına
   kurulum izni ver. Alternatif: USB hata ayıklaması ve bilgisayar yetkisi sonrası
   `adb install -r "C:\Dev\Mewgic\Builds\Android\Mewgic-0.8.0-beta.apk"`.
2. Önceki uygulamada imza uyuşmazlığı çıkarsa kaldırma/veri silme işlemi yapma.
   Önceki imza ve kayıt yedeği incelenmeli. Paket silinirse yerel ilerleme kaybolabilir.
3. Mewgic'i aç. Home, logo, kedi, şişeler, PLAY ve alt navigasyonu kontrol et.
4. Bölüm1'i çöz, haritaya dön, Bölüm2'nin açıldığını ve yıldızların kaldığını doğrula.
   Geri Al/İpucu/Ek Şişe/Karıştır haklarını kontrol et. Hızlı tekrar dokunuşlarda
   çift hamle/ödül/geçiş oluşmamalı.
5. Günlük hediye/görev ve etkinlik ödülünü al; aynı ödülü tekrar alma girişimini dene.
6. Ayarlarda sesi kapat. Uygulamayı arka plana alıp döndür; Gameplay duraklamalı.
   Sistem uygulama ayarından Zorla Durdur yapıp tekrar aç; seçili bölüm, yıldız,
   haklar, alınmış ödüller ve ses tercihi korunmalı. Verileri temizleme.
7. Not: yarım bırakılan bulmacanın sıvı dizilimi kaydedilmez. Seçili bölüm yeniden
   başlangıç dizilimiyle açılır; harcanmış haklar ve kalıcı ödüller geri verilmez.
8. En az10–15 dakika oyna. Cihaz modeli/Android sürümü/çözünürlük/çentik,
   dokunma/kaydırma, açılış süresi, bellek, GC, GPU, ısı ve pil gözlemlerini kaydet.
   Unity Profiler/Android araçlarıyla ölç; Editor sayılarını cihaz FPS'i sayma.
9. USB ile günlük: `adb logcat -d` çıktısında uygulamaya ait crash/ANR/Unity
   exception'larını ayır. Kişisel cihaz günlüklerini Git'e ekleme.

## Bilinen içerik sınırları

- Yalnız iki doğrulanmış bölüm; sonraki bölgeler görüntülenebilir ancak oynanamaz.
- Missions ve Events Home içinde gerçek panellerdir; ayrı sahne değildir.
- Settings gerçek tek ses tercihi ve kapatma kontrolüdür; müzik/ses içerikleri yoktur.
- Para/elmas kalıcı sunum alanıdır; başlangıç0/0, ödül/mağaza/ödeme ekonomisi yoktur.
  Enerji kaydı vardır; Gameplay tüketimi/yenilenmesi yoktur.
- Team/Inventory açıklama panelidir; Shop/Starter Pack sayfaları yoktur.
- Premium satın alma, reklam, sunucu hesabı, bulut kayıt ve çevrimiçi etkinlik yoktur.
- Yerel saat/kayıt kontrolleri sunucu güvenliği sağlamaz; elle veri düzenleme ve
  ileri tarih değişimi tamamen engellenmez.

## Yayın öncesi

Google Play: kalıcı paket sahipliği, sürüm politikası, release AAB, güvenli imzalama
ve Play App Signing, güncel hedefAPI gereksinimleri,16KB sayfa uyumu ve cihaz testi,
Play Console dahili test/ön lansman raporu, ikon/açılış ekranı/mağaza görselleri,
gizlilik/veri güvenliği/içerik derecelendirmesi gerekir. Gizli anahtarlar Git dışında.
16KB kontrolü için [Android'in resmi belgesi](https://developer.android.com/guide/practices/page-sizes)
esastır; APK hizalama denetimi gerçek16KB cihaz testinin yerine geçmez.

iOS: iOS Build Support, yerel derleme için macOS/Xcode veya uygun bulut derlemesi,
Apple kimliği/dağıtım üyeliği,imza/provisioning, iPhone Safe Area/performance ve
TestFlight/App Store incelemesi gerekir. Bu Windows çalışmasında iOS paketi/testi
üretilmedi. [Unity6000.3 iOS kurulum belgesi](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/iphone/getting-started/ios-environment-setup).
