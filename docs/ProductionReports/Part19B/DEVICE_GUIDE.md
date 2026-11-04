# PART 19B · Fiziksel cihaz kabul testi

Her cihazda ayrı kopya doldurun. Cihaz modeli ve RAM, OS sürümü, çözünürlük, yenileme hızı, uygulama sürümü, build numarası, Git SHA, dağıtım kanalı, pil/şarj ve test tarihini yazın. Mevcut oyuncu kaydı olan cihazda uygulamayı kaldırmayın ve veriyi temizlemeyin. Güncelleme testi aynı imza ve dağıtım kanalını kullanır.

Android'de USB hata ayıklamayı yetkilendirin. `adb devices -l` ile bağlantıyı doğrulayın, ardından ayrılmış test cihazına `adb install -r <apk-yolu>` ile kurun. İmza uyuşmazlığı veya sürüm düşürme reddinde durun. Android 15+ 16 KB cihazı varsa `adb shell getconf PAGE_SIZE` çıktısını kaydedin. iOS'ta yetkili TestFlight iç test grubundan build'i kurun ve görünen build numarasını kaydedin.

| ID | Uygulama ve beklenen davranış | Sonuç | Hata / tekrar adımları | Ölçüm | Düzeltme |
| --- | --- | --- | --- | --- | --- |
| D01 | Temiz test cihazına kur. İlk açılış ve cihaz dilini denetle. | | | Kurulum süresi | |
| D02 | Üç soğuk açılış yap. Dokunuştan Play kullanılabilir olana kadar ölç. | | | Tekil süreler ve medyan | |
| D03 | Ana menü, Ayarlar ve Ödüller'i aç. Çentik, taşma, dokunma ve geri dönüşü kontrol et. | | | Görüntü ve çözünürlük | |
| D04 | Sandığı aç. +100 coin ve +1 İpucu kaydolmalı. Animasyon sırasında uygulamayı kapatıp aç. Aynı hak tekrar verilmemeli. | | | Bakiye ve stok | |
| D05 | 1–10. bölümleri oyna. Pati rehberi, geçerli/geçersiz döküş, hızlı temas, güçler, yıldız ve coin’i denetle. | | | Bölüm ve hamleler | |
| D06 | Dünya ve ada haritasını iki yönde kaydır. Sürükleme bölüm seçmemeli. Bulunduğum Bölüm doğru taşı göstermeli. | | | Kare süreleri / video | |
| D07 | Ayrı QA kaydında 50 → 51 veya 100 → 101 geçişini oyna. Yeni ada açılmalı ve ilerleme kaydedilmeli. | | | Ada ve bölüm | |
| D08 | İlk kazanıştan dönünce kedi yolu takip etmeli. Aynı bölümü tekrar kazanmak yolculuğu tetiklememeli. Hareket azaltmayı da dene. | | | Video ve süre | |
| D09 | Müzik, efekt ve titreşimi ayrı değiştir. Yeniden açılışta tercihler korunmalı. | | | Hoparlör, kulaklık ve titreşim | |
| D10 | Önceki aynı imzalı build üzerine kur. Yıldız, coin, güçler ve tercihler korunmalı. | | | Önce/sonra değerler | |
| D11 | Döküş sırasında arka plana geç, ekranı kilitle ve geri dön. Çift hamle veya ödül oluşmamalı. | | | Uyku ve dönüş süresi | |
| D12 | Uçak modunda soğuk aç, bölüm oyna ve uygun sandığı aç. Ağı yeniden açınca kaydı kontrol et. | | | Bağlantı durumu / log | |
| D13 | En az 30 dakika oyna. İlk ve son beş dakikada aynı menü–harita–oynanış rotasını ölç. | | | FPS, kare p95, bellek, ısı | |
| D14 | Test cihazında işlemi zorla kapatıp aç. Kalıcı bölüm ve ödüller korunmalı. Yarım bulmaca başlangıç dizilimine dönebilir. | | | Açılış süresi / kayıt | |

Android için Perfetto veya Android Studio System Trace ile kare sürelerini, `adb shell dumpsys meminfo com.mewgic.game` ile aynı sahnede PSS değerini ve `adb shell dumpsys thermalservice` ile termal durumu kaydedin. iOS için Instruments App Launch, Animation Hitches veya Metal System Trace ve Allocations/VM Tracker kullanın. Doğrudan sıcaklık okunamıyorsa N/A yazın. Pil sıcaklığı ile SoC sıcaklığını ayrı tutun.

60 FPS için kare bütçesi 16,67 ms. Development APK ile release AAB/TestFlight sonuçlarını ayrı kaydedin. Kurulamama, crash/ANR, kayıt kaybı, çift ödül ve durdurulmuş ilerleme yayın engelidir. Düzeltmeyi aynı cihazda ve yeni build numarasıyla tekrar sınayın. Cihaz loglarını ve kişisel verileri Git'e koymayın.

Cihaz kapsamı için mevcut en düşük destekli sürüme yakın bir cihaz ve güncel çentikli bir cihaz seçin. Android tarafında API 36 ve mümkünse 16 KB cihazı, iOS tarafında iOS 15 tabanına yakın cihaz ve güncel iPhone seçin. Android am start -W ile sistem başlatma süresi ayrıca kaydedilebilir. Ana menüde Play düğmesi kullanılabilir olana kadar ölçülen süreyi bu değerle ayrı tutun.
