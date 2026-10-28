# PART09C — Home üzerindeki iki açıklama yazısını kaldırma

29 Ekim 2026 • Unity6000.3.27f1 • başlangıç commit6ba7182.

## Uygulama

Home.unity içinde bu yazılara ait kayıtlı Text/TMP içerik bulunmadı.
Gerçek kaynak Assets/Scripts/Presentation/PrototypeApp.cs içindeki Home().
Subtitle: “BİR ŞİŞE DOLUSU SİHİR”; Footnote: “İlk büyü • Renkleri bir araya getir”.
İki Label ve onlara ait Shadow oluşturma satırları kaldırıldı (toplam4 satır).
Footnote tek bir metin olduğundan başındaki “İlk büyü •” de aynı nesneyle kalktı;
ekranda anlamsız bir ön ek bırakılmadı. Yer tutucu veya gizli nesne oluşturulmaz.

Diğer başlıklar/oyun açıklamaları, logo, PLAY, şişeler, kedi ve PART09B
animasyonlarına dokunulmadı. Düzen bağımsız anchor'larla kuruludur; metinlerin
silinmesi diğer elemanları kaydırmaz. Üç orandaki gerçek render incelemesinde
hizalama bozukluğu görülmedi; ek yerleşim değişikliği gerekmedi.

## Doğrulama

Part09CEditor.RunTests, Home sahnesinde geçici Editor probe ile gerçek
Play Mode çalıştırdı; süreç çıkışı0. HOME_MOTION_TESTS.json:41 kontrol geçti,
runtime Error/Exception/Assert listesi boş. C# derlemesi başarılı.

- Subtitle/Footnote nesneleri yok; aktif/pasif Home Text ve TMP_Text
  bileşenlerinde iki ifade bulunmuyor.
- 900×1600 (9:16),900×1950 (9:19.5),900×2000 (9:20 ve Safe Area iç boşluğu)
  gerçek Unity kamera render'ları incelendi.
- PLAY dokunma alanı/63 nokta, basma animasyonu, çift tıklama koruması,
  Gameplay'e geçiş ve Home'a dönüş başarılı.
- Mevcut kedi/şişe/ortam animasyonları ve maskeli mimari kontrolleri geçti.
- Test profili kullanıldı; gerçek oyuncu kaydı değişmedi.

Kanıtlar docs/Part09C altında Home-1600.png,Home-1950.png,Home-2000.png,
Play-Pressed.png,Gameplay.png,Home-Return.png ve HOME_MOTION_TESTS.json.
PRESERVATION.json:946 mevcut tracked dosyadan yalnız PrototypeApp.cs değişti;
945 diğer tracked dosya ve133 önceki untracked dosya byte düzeyinde korundu
(rapor/progress eklenmeden önce ölçüldü). Yeni test araçları yalnız Editor'de
çalışır; probe üretim sahnesine kaydedilmez.

Fiziksel iPhone testi yapılmadı; doğrulama Windows Unity Play Mode'dadır.
Yeni TestFlight/Cloud Build veya IPA üretilmedi. Unity sürümü ve iOS ayarları aynı.
Commit/push sonucu uzak SHA ile doğrulanarak teslim mesajında bildirilir.
