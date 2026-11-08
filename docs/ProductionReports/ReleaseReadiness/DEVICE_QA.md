# MeWGic · Fiziksel cihaz kabul senaryoları

Bu senaryolar sonraki aday paket için hazırlandı. Her iki platformda da taze kurulum ve eski sürüm üstüne güncelleme ayrı koşul olarak kaydedilir. Sonuç kaydında cihaz, işletim sistemi, paket numarası, ağ koşulu, beklenen ve gözlenen sonuç yer alır.

| Sıra | Akış | Kabul ölçütü |
| --- | --- | --- |
| 1 | Android ARM64 ve iPhone’da soğuk açılış, sıcak dönüş ve dikey ekran | Boot bir kez açılır. Home’da etkileşim başlar. Dar, çentikli ve uzun ekranlarda başlıklar, satın alma, sonuç, etkinlik ve harita kontrolleri güvenli alandadır. |
| 2 | Taze oyuncu, ilk dört bölüm ve 9 → 10 → 11 / 99 → 100 → 101 | Pati yardımı geçerli döküşe yönlendirir. Sonraki Bölüm bir kez ilerler. Tekrar Oyna aynı bulmacayı, Haritaya Dön doğru adayı açar. |
| 3 | Kazanış → mağaza → satın alma → ürünü kullanma → uygulamayı kapatma | Altın eksiye düşmez. Çift dokunma tek işlem yazar. Altın, envanter, kuşanılan ürün, açık bölüm ve yıldızlar yeniden açılışta korunur. |
| 4 | Günlük görev ve yerel saat | İki kazanış görev ödülünü bir kez verir. 24 saatlik sayaç yeniden açılış, arka plan ve gün sınırında doğru devam eder. |
| 5 | AdMob resmi test reklamı, erken kapatma, ağ kesintisi ve tamamlama | UMP ve gizlilik seçenekleri çalışır. Tamamlanmayan reklam bonus vermez. Başarılı reklam, kazanış toplamını 100 altına tamamlar. Ağ hatasında Sonraki Bölüm çalışır. |
| 6 | Üretimden ayrı HTTPS hazırlık ortamında canlı imzalı SSV | Ödül kaydı doğru reklam birimi ve tek makbuzla doğrulanır. Uygulama arka plana giderse, kapanırsa veya doğrulama gecikirse teslim davranışı ayrıca kaydedilir. |
| 7 | İki gerçek hesap, klan kurma ve istek/onay | Yetkisiz üye işlemleri reddedilir. İstek, kabul ve üyelik başka cihazda yeniden girişle görünür. |
| 8 | Lig ve etkinlik, iki hesap ve aralıklı bağlantı | Geçerli kazanış LP ve etkinlik puanını ayrı artırır. Tekrarlı talep ve yeniden bağlanma ödülü çoğaltmaz. Gerçek HTTP hatasında çevrimdışı bölüm ve mağaza akışı sürer. |
| 9 | Kristal Şelaleler 99–100 ve harita kaydırma | Şelaleler ve maske kenarları akıcı. Kedi uyanır, yürür, doğru hedefte durur. Dokunma bölgeleri, scroll ve yıldızlı düğümler doğru. |
| 10 | Cihaz performansı | En az beş soğuk açılış, Gameplay’e geçiş ve bir harita yürüyüşü ölçülür. Süreler, düşen kareler, pik bellek ve tekrarlanabilir gecikmeler cihazla birlikte kaydedilir. |

İlk cihaz turu için geliştirme reklam kimlikleri ve hazırlık HTTPS ortamı kullanılır. Üretim reklam birimleriyle yayın öncesi doğrulama, AdMob ve imzalama erişimi hazır olduğunda ayrı koşulur.
