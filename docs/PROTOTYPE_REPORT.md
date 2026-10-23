# Mewgic ilk oynanabilir prototip

Tarih: 23 Ekim 2026 (Europe/Kiev).

## Teslim
- Mevcut C:\Dev\Mewgic projesi kullanıldı; ikinci proje veya servis entegrasyonu yok.
- Beş kaynak görsel References/Visual içine kopyalandı; SHA256 eşitliği doğrulandı.
  Kaynaklar korunur. Altıncı ekran görüntüsü dahil edilmedi.
- Bağımsız model: Assets/Scripts/Core/Puzzle.cs.
- Ayrı bölüm verisi: Assets/Resources/Levels/Level01.json (alttan üste katmanlar).
- Gerçek etkileşimli UI: Assets/Scripts/Presentation/PrototypeApp.cs.
- Boot, Home ve Gameplay sahnelerine giriş bileşeni eklendi; kamera ve ışıklar korundu.
  LevelMap ve SampleScene değiştirilmedi.
- Home → Oyna → Gameplay; kaynak/hedef seçimi, aktarım animasyonu, hamle sayacı,
  geri alma, yeniden başlatma, kazanma paneli, tekrar oynama, ana menüye dönüş.
- InputSystemUIInputModule fare ve dokunmatik girişini destekler.
- CanvasScaler ve dinamik Screen.safeArea kullanılır.
- Paket eklenmedi; Unity sürümü değiştirilmedi.

## Doğrulama
12 model kontrolü başarılı: aynı şişe, geçersiz indeks, farklı renk, dolu hedef,
kısmi kapasite, kesintisiz üst renk grubu, geri alma, yeniden başlatma, boş geçmiş,
tam dolmadan kazanamama, tamamlanmış şişe, kapasite reddi ve bölüm çözümü.
Bazı kontroller aynı doğrulama ifadesinde birlikte sınanır.
BFS 18 durumu ziyaret ederek çözümü buldu: 1→3, 2→1, 2→3.
MODEL_TEST_RESULTS.md ayrıntıyı içerir.

Play Mode'da buton callback'leri kullanılarak Boot → Home → Gameplay → Home akışı,
animasyon sırasında ek seçim reddi, Undo, Restart, kazanma ve Replay doğrulandı.
Bu test fiziksel fare/dokunma olaylarını veya telefon işletim sistemini simüle etmez.
PLAY_SMOKE_RESULTS.md sonuç kaydıdır.

900×1600 ve 900×1950 çözünürlüklerinde Canvas, kamera RenderTexture'ına çizdirildi;
prototype-home.png, prototype-gameplay.png, prototype-gameplay-tall.png ve
prototype-win.png görüntüleri açılıp incelendi. Metin ve kontroller görünür.
Bu çizimler cihaz ekran görüntüsü değildir; çentikli cihaz testi yapılmadı.

Play Mode günlüğünde UnityEditor.Search.SearchDatabase kaynaklı başlangıç
indeksleme istisnası görüldü; oyun kodundan gelmedi ve akış testini engellemedi.
İlk test koşusunda sahne Start tamamlanmadan kontrol yapıldı; test beklemesi
düzeltildi ve yeniden çalıştırıldı. Sonraki derleme hatası yardımcı test dosyası
düzenlenirken oluştu, düzeltildi; son Play Mode çalışması tamamlandı.

## Görsel sınırlar
Sade geometrik prototip kullanılır; kapak ve UI referanslarının nihai sanat
kalitesine ulaşılmış değildir. Ayrı kedi, logo, arka plan, cam şişe ve efekt
varlıkları ART_DIRECTION.md içinde planlanmıştır. Tam ekran referans resmi
çalışan arayüz yerine kullanılmadı.

## Android
Android IL2CPP/ARM64 geliştirme derlemesi başarılı; Unity çıkış kodu 0. APK boyutu 60.333.481 bayt (yaklaşık 57,5 MiB). aapt ile com.mewgic.game, minimum API 26, hedef API 36 ve arm64-v8a doğrulandı. apksigner v2 imza doğrulaması başarılı; bu geliştirme imzasıdır. Günlük: prototype-build.log.
Hedef: Builds/Android/Mewgic-Prototype.apk.
Gerçek Android cihazda kurulum, dokunma, performans ve güvenli alan testi yapılmadı.

## Çalıştırma
Unity 6000.3.27f1 ile C:\Dev\Mewgic açılır.
Assets/Scenes/Boot.unity açılıp Play düğmesine basılır.
Gerekirse Unity kullanım koşulları penceresini kullanıcı tamamlamalıdır.

## Sonraki adım
Android cihaz üzerinde ilk deneme, ardından ayrı görsel varlıklar ve ilk 10 bölüm.
Mağaza, reklam, satın alma, can, etkinlik, takım, envanter, sezon bileti ve kayıt
sistemi bu prototipte bulunmaz. GitHub/DevControl bağlantısı yapılmadı.

