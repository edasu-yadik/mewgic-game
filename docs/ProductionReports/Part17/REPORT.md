# PART 17 · Oyun ekonomisi ve ödüller

4 Kasım 2026 · Unity 6000.3.27f1 · Başlangıç `3227be5`

## Ekonomi tablosu

| Kalem | PART 16B | PART 17 |
| --- | ---: | ---: |
| İlk tamamlamada bölüm tabanı | 20 | 20 |
| 1 / 2 / 3 yıldız | 10 / 20 / 30 | 5 / 10 / 15 |
| İlk denemede tamamlamak | — | 10 |
| İlk tamamlamayı ipucu kullanmadan yapmak | — | 5 |
| Aynı veya daha düşük yıldızla tekrar | Yeni galibiyet kimliğinde 7 / 9 / 11 | 0 |
| Daha iyi yıldızla tekrar | Yeni galibiyet kimliğinde 7 / 9 / 11 | Her yeni yıldız için 5 |
| İlk tamamlamada mevcut yardımcı güç ödülü | 1 ipucu | 1 ipucu |
| Günlük sandık | 100 coin + 1 ipucu | 100 coin + 1 ipucu |

İlk denemede ve ipucusuz alınan 1, 2 ve 3 yıldızın toplamı sırasıyla 40, 45 ve 50 coin. Önceki sistemde bu toplam 30, 40 ve 50 coin idi. Dolayısıyla tam bonuslu 3 yıldızlık tavan korunuyor. Testte aynı bölümde 1 → 2 → 3 yıldız, iki defa 5 coin verdi. Tekrar galibiyet ve daha düşük yıldız bakiye artırmadı.

İlk giriş kaydedilir. Bölümü terk etmek, uygulamayı kapatmak veya tahtayı yeniden başlatmak sonraki galibiyetin ilk deneme bonusunu kaldırır. İpucusuz bonusu yalnız ilk tamamlamada, kazanılan denemede İpucu çalıştırılmadığında verilir. Eski kayıttaki en iyi yıldız bir sonraki ödül hesabının tabanıdır. Eski bakiyeye veya stoklara geriye dönük düzeltme uygulanmaz.

## Coin harcama

Ana ekrandaki coin artısı mevcut yardımcı güçlerin küçük satın alma panelini açıyor.

| Güç | Coin |
| --- | ---: |
| Geri Al | 35 |
| İpucu | 40 |
| Karıştır | 50 |
| Ek Şişe | 60 |

Bir sandığın 100 coin’i bir İpucu ve bir Ek Şişe karşılıyor. Otomatik test bu alımları ve ardından sıfır bakiyeyle reddedilen ek alımı doğruladı. Fiyatlar başlangıç değerleri. Kullanım sıklığı veya uzun vadeli coin birikimi için oyuncu verisi toplanmadı. Kozmetik satışı bu kapsamda eklenmedi.

## Sandık ve kayıt

Ana ekrana uygun durumda gelince kapalı sandık açılır pencerede canlanıyor. Oyuncu kapatırsa Günlük etkinlik ekranındaki sandık sekmesinden açabilir. İlk ödül hemen kullanılabilir. Sonraki hak başarılı alımın UTC zamanından 24 saat sonra doğar. Açma düğmesi coin, ipucu ve alım zamanını aynı kayda işler. Başarılı yazımdan sonra animasyon oynar. Tarihi eski bir sandık penceresi yeni hakkı tüketemez.

Saat politikası son gözlenen zamanı saklıyor ve belirgin geri alma durumunda günlük ödülü durduruyor. Çalışan oturumda duvar saati ile monoton sayaç arasındaki beş dakikayı aşan fark ileri ve geri sıçramaları da durduruyor. Uygulama tamamen kapalıyken ileri alınmış yerel saati güvenilir gerçek zamandan ayırmak mümkün değil. Uzun askıya alma sırasında monoton sayacın davranışı fiziksel cihazda ölçülmedi.

Bölüm ödülü ve yıldız aynı kayıt işleminin parçası. Sonuç ekranı başarılı yazımdan sonra coin sayacını ve dökümü gösteriyor. Kayıt hatasında coin yerine hata ve tekrar kaydetme düğmesi gösteriliyor. Yıldızlar sırayla beliriyor. Oyuncu animasyonu atlayabiliyor veya Sonraki Bölüm ve Harita kontrollerini kullanabiliyor.

## Doğrulama

Unity Play Mode’da 66 kontrol geçti. İlk bölüm gerçek döküşlerle iki kez tamamlandı. İlkinde 3 yıldız ve 50 coin, tekrarında 0 coin yazıldı. En iyi yıldız farkı, eski JSON, bozuk yazım simülasyonu, tekrar alım, 24 saat eşiği, ileri ve geri saat değişikliği ile negatif bakiye kontrolleri geçti. Günlük sandık animasyonu sürerken kayıt yeniden açılıp ödül ve hak durumu okundu. Uygulama işlemi zorla sonlandırılmadı. Sonraki Bölüm ve Harita düğmeleri üç portre oranında çakışma ve raycast açısından sınandı. Harita düğmesi gerçek sahne geçişini yaptı. İzole test profili kullanıldı ve gerçek oyuncu ana kaydı, yedeği ve bozuk kayıt alanı byte düzeyinde aynı kaldı. Konsolda test uyarısı veya hatası kaydedilmedi.

Kaynak metin denetiminde 180 anahtar ve sekiz dil geçti. Gerçek Game View görüntüleri Türkçe. Cihaz testi yapılmadı.

[Ham Unity sonuçları](UNITY_TESTS.json) · [Kapalı sandık](Gunluk_Kapali.png) · [Açık sandık](Gunluk_Acik.png) · [Yardımcı güçler](Yardimci_Gucler.png) · [Bölüm sonu 9:16](Bolum_Odulu_1600.png) · [9:19.5](Bolum_Odulu_1950.png) · [9:20](Bolum_Odulu_2000.png)

Ödül ve stok işlemleri `CompletionReward.cs`, `BoosterActions.cs` ve `DailyActivities.cs` içinde. Sonuç ekranı, ana ekran coin paneli ve günlük pencere sırasıyla `GameplayEndScreen.cs`, `HomeTopBar.cs` ve `DailyArrivalPopup.cs` içinde. `PrototypeApp.cs` giriş ve galibiyet akışını bağlıyor. Unity testi `Part17EconomyProbe.cs` içinde. Test ve teslim `recovery/part09-completion` dalında.
