# PART 04C — Üst kaynak çubuğu

25 Ekim 2026. Yalnız Home üst kaynak sunumu eklendi.

## Sanat varlıkları

Assets/Art/UI/TopBar/:TopBar_Panel.png,TopBar_Coin.png,TopBar_Gem.png,TopBar_Heart.png,TopBar_Plus.png,TopBar_Settings.png. Altı yüksek çözünürlüklü gerçek RGBA PNG,yerleşik image_gen ile Home referansına göre ayrı ayrı üretildi. PNG rakam içermez; yalnız artı ve dişli gibi ikonlar görsele gömülüdür. Rakamlar ve Full Unity Text'tir.

Koyu mor,altın çerçeveli panel;parlak altın para,mor elmas,pembe-kırmızı kalp;yeşil artı ve yuvarlak ayarlar hazır. Referansa göre altın kenarlar daha parlak/kalın,para C simgesi ve elmas facetleri biraz farklıdır. Yazı için mevcut LegacyRuntime fontu kalın/beyaz ve gölgeli kullanılır;referans yazı tipinin birebir kopyası değildir.

PNG saydam marjları Sprite rect ile import sırasında hariç bırakıldı,kaynak dosyalar değiştirilmedi. Sprite Multiple içinde tek adlandırılmış sprite,pivot(.5,.5),PPU100,FullRect,sRGB,alphaIsTransparency,Bilinear/Clamp,mipmap/ReadWrite kapalı. Panel1024,ikonlar512 maksimum import;Android ASTC4x4. Mobil boyutlar PART04C_MOBILE_IMPORT.json'dadır. Kaynak alpha denetimi0–255 ve temiz saydam dış sınırları doğrular;panel kaynak2172×724 olsa da mobil texture1024×341'dir.

## Model ve işlevler

Assets/Scripts içinde mevcut ekonomi/bakiye/enerji veya oyuncu kayıt sağlayıcısı bulunmadı. HomeResourceModel ayrı,observable önizleme modelidir. Her Home açılışında12450/853/5 test değeriyle oluşur;gerçek oyuncu bakiyesi değildir,kalıcı yazılmaz. IsPreview=true. SetValues ile para/elmas/enerji değişince Changed olayı UI'ı anında günceller. Negatif değerler0,enerji0–5 aralığına sıkıştırılır. Tam enerjide Full,eksikteX/5 ve kalpteX gösterilir. Büyük değerler taşmayı önlemek için M/B/T/Q ile kısaltılır. Bu kısaltmalar yalnız sunum içindir. Gameplay tüketimi/yenilenme/zamanlayıcı/satın alma eklenmedi.

HomeTopBar Home sahnesinde altı sprite referansını saklar. Runtime UI/HomeTopBar altında CoinsDisplay,GemsDisplay,EnergyDisplay,SettingsButton kurulur. Her kaynak Panel/Icon/Value ve ayrı yeşil Button içerir. Üç artı yalnız bilgilendirme paneli açar,veri değiştirmez. Ayarlar aynı geçici modal altyapıyla açılır,KAPAT ile kapanır. Modal arkadaki girişleri engeller. Yeni ekonomi/kayıt entegrasyonu için model değiştirilebilir;mevcut sistemlere dokunulmadı.

## Doğrulama

PART04C_TEST_RESULTS.json:236 kontrol başarılı. Event ile para/elmas güncellenmesi,kısmi/tam enerji,negatif/maksimum sınırlar,long.MaxValue biçimlendirmesi;üç artı/ayarlar için gerçek EventSystem raycast tıklaması,panel aç/kapat ve bakiye değişmemesi doğrulandı. PLAY→Gameplay→Home,basılma/çift tıklama kilidi,kedi Idle/Blink/Wand Magic ve şişe hareketleri de geçti.

900×1600,900×1950,900×2000 ve çentik/safe-area simülasyonunda üst çubuk sınırlar içinde ve logoyla örtüşmüyor. Uzun sayı görüntüsü home-topbar-1600-long.png içinde. Bu kontroller fiziksel telefon dokunma ergonomisi testi değildir;Android cihazı/APK yeniden denenmedi.

Test düzenlenirken oluşan geçici test derleme hataları giderildi;erken Console kayıtları PART04C_TEST_REPAIR_DIAGNOSTICS.json içinde saklandı. Yeni derleme ve son test sonrası Console0 hata/0 uyarı. Home dirty=false. Diğer160 korunan dosya SHA256 ile değişmeden doğrulandı;logo,PLAY,kedi,şişeler,arka plan,oynanış ve önceki kayıt dosyaları korunur.

PART04C_GAMEVIEW.png gerçek900×1600 Game View ScreenCapture;PART04C_SETTINGS.png gerçek ayarlar modalı. Diğer oranlar çalışan Home Canvas'ının kamera yakalamalarıdır. PART04C_COMPARISON_Dark/Light.png referans üst barıyla gerçek Home'u yan yana gösterir. Eski sahneler silinmedi,DevControl'e müdahale edilmedi. Geri dönüş kopyaları C:/Dev/Mewgic-Part04C-Backup.
