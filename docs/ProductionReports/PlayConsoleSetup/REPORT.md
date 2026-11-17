# Google Play zorunlu kurulum · MeWGic 0.8.0 / 11

17 Kasım 2026. Mevcut MeWGic uygulamasına Play Console üzerinden erişildi. Beş görev kaydedildi ve panel 5/11 tamamlandı gösterdi. Beyanlar yayın özetinde incelemeye hazır. Google incelemesine gönderim veya yayın yapılmadı.

| Görev | Hazırlanan cevap / sonuç | Gönderim öncesi gereken |
|---|---|---|
| Gizlilik | Türkçe ve İngilizce HTML hazır, Store Assets/GooglePlay/Privacy | Yayıncı, doğrulanmış destek iletişimi ve destek verisi saklama süreci. HTTPS yayını ve girişsiz erişim doğrulaması. |
| Uygulama erişimi · Console’a kaydedildi | Temel oyun için hesap gerekmez. İlk dört bulmacada pati rehberi var. Çevrimiçi hesap ve sunucu eşitlemesi bu pakette kullanılamaz. | Formda tüm işlevlere kısıtsız erişim cevabı verirken kapalı çevrimiçi alanların durumunu açıklayın. Çalışmayan test hesabı eklemeyin. |
| Reklam · Console’a kaydedildi | Bu paket reklam göstermiyor. Resmî manifestte INTERNET, AD_ID ve reklam servis izinleri yok, MobileAdsInitProvider yok, üretim reklam ayarı kapalı. | Diğer aktif kanal veya sürümlerde reklam varsa uygulama geneli beyan yeniden değerlendirilmeli. |
| İçerik derecelendirmesi | Oyun / bulmaca. İncelenen içerik renkli sıvı sıralama, fantastik kedi, yıldız ve yerel ödüller. Gerçek para, açık sohbet ve çevrimiçi yarış bu pakette etkin değil. | Güncel IARC anketinin her sorusu okunmalı. Grafikler tüm sürüm boyunca incelenmeden tüm şiddet/korku alt soruları otomatik yanıtlanmamalı. Son dereceyi IARC verir. İletişim adresi bekleniyor. |
| Hedef kitle | Sevimli grafikler yaş grubu kararını tek başına belirlemez. | Ürün sahibinin hedef yaş grupları kararı bekleniyor. Çocuk yaş grubu seçilirse Families koşulları değerlendirilmeli. |
| Veri güvenliği | Bu paket için cihaz dışına oyuncu verisi toplama/paylaşma bulunmadığı yönünde statik kanıt mevcut. Yerel ilerleme Data Safety anlamında cihaz dışı toplama değildir. Hesap oluşturma etkin değil. Reklam, analitik, ödeme ve sunucu eşitlemesi kapalı. | Form uygulamanın bütün aktif dağıtımlarını kapsar. Diğer kanal/sürümler doğrulanamadı. Fiziksel ağ yakalaması yapılmadı. Destek talepleri ve barındırma sağlayıcısının bağımsız verileri ayrıca açıklanmalı. |
| Şifreleme | Oyun sunucusuna veri iletimi bu pakette etkin değil. | Kullanılmayan aktarıma koşulsuz şifreleme garantisi verilmemeli. Güncel formun koşullu sorularına göre uygulanamaz yanıtı değerlendirilmeli. |
| Hesap/veri silme | Bu pakette sunucu hesabı yok. Android Ayarlar > Uygulamalar > MeWGic > Depolama > Verileri temizle yerel ilerlemeyi siler. Yedekler Android/sağlayıcı ayarlarından yönetilir. | Bu işlem denetim sırasında gerçek oyuncu verisinde uygulanmadı. Destek iletişimi ve taleplerin işlenme süreci bekleniyor. |
| Resmî kurum · Console’a kaydedildi | İncelenen uygulama kamu hizmeti veya resmî kurum işlevi sunmuyor. | Formda devlet tarafından veya devlet adına geliştirilmiş mi sorusuna Hayır kaydedildi. |
| Finans · Console’a kaydedildi | İncelenen oyun bankacılık, kredi, yatırım, transfer veya gerçek paraya dönüşen ödül sunmuyor. | Güncel formdaki “Uygulamamda finans ile ilgili özellik sağlanmıyor” seçildi, belge adımı geçildi ve kaydedildi. |
| Sağlık · Console’a kaydedildi | İncelenen oyun sağlık verisi, tıbbi karar veya sağlık takibi sunmuyor. | Güncel formdaki “Uygulamamda sağlık özellikleri yok” seçildi, bölgesel koşullar adımı geçildi ve kaydedildi. |
| Kategori ve iletişim | Game / Puzzle önerisi. com.mewgic.game / MeWGic. | Yayıncı tarafından herkese açık kullanımı onaylanan destek adresi. |
| Mağaza | Listing-ClosedTest.json içinde en-US ve tr-TR metinleri, başlık ve açıklama sınırları kontrol edildi. | Eski 12 ada, eski sonuç ekranı ve ada seçimi görselleri mevcut paketi temsil etmiyor. Kapalı test kaynaklı iki güncel Unity Game View kopyası Phone-ClosedTest klasöründe hazır. |

## Mağaza taslağı ve mevcut engeller

İngilizce kısa ve uzun açıklamalar mevcut varsayılan en-US mağaza girişine girildi ve taslak olarak kaydedildi. Orijinal Icon-512.png ve iki güncel Unity Game View kaydı (01-Gameplay.png, 02-ContinuousMap.png) yüklendi. Play Console bunları 512×512 ve 900×1600 olarak doğruladı. Türkçe metin yerel JSON dosyasında hazır, Console'a eklenmedi.

Eski Feature-1024x500.jpg dosyası geçerli ölçülere sahip ama kaldırılan ada seçimi ekranını içeriyor. Yükleme sonrası bu görsel taslaktan çıkarıldı. Galeride kalan kaynak, yayınlanmış mağaza görseli değildir. Yerel orijinal dosya korundu. Güncel 1024×500 özellik grafiği gerekiyor. Eski altı telefon görseli de yüklenmedi. Hazırlanan iki güncel görsel aynı AAB için yapılan önceki Unity Play Mode doğrulamasındandır, cihaz ekran görüntüsü değildir.

GitHub Pages'in ücretsiz yayını kontrol edildi fakat GitHub araç erişimi oyun deposunu göremedi, yerel CLI oturumu yok. Destek iletişimi ve yayıncı teyidi gelmediği için politika taslak kaldı. İnceleme sırasında hesap adından e-posta kopyalanıp destek adresi varsayılmadı. Yaş grupları seçilmedi.

## Denetim dayanakları

AAB_PRIVACY_AUDIT.json gerçek AAB SHA-256 ve bundletool ile yeniden çıkartılan izinleri içerir. AndroidManifest.xml doğrudan mevcut imzalı paketten çıkarıldı. Üretici kaynak snapshot'ı Builds/Android/GooglePlay/work-20261115-223512 altında incelendi. Oyunun izin ve yapılandırmaları statik olarak kontrol edildi. Cihazda tüm kod yollarının çalıştırıldığı veya ağ trafiği yakalandığı iddia edilmez.

Google Mobile Ads, UMP, Unity IAP ve Play Billing bağımlılıkları pakette var. SDK'nın pakette olması aktif veri aktarımıyla eş tutulmadı. Unity Analytics ve bulut tanılama ayarları kapalı. Yerel dosya ve PlayerPrefs kayıtları gözlendi. Klan istemcisinin Release uç noktaları boş.

Oyunda reklam izin seçenekleri düğmesi koşullu, etkin reklam sağlayıcısına bağlı. Genel yayımlanmış gizlilik politikası URL'sine giden bağlantı incelenen kaynaklarda doğrulanamadı. Google hem mağaza hem uygulama içinde politika bağlantısı istiyor. URL yayımlandıktan sonra mevcut AAB'nin bu koşulu karşılaması ayrıca çözülmeli.

## Yayın erişimi

GitHub CLI için aktif oturum yok. Bağlı GitHub aracı kullanıcı profilini okuyabildi, ancak edasu-yadik/mewgic-game deposuna erişimde 404 döndürdü ve erişilebilir depo listesi boş geldi. Bu yüzden özel oyun deposu veya görünürlüğü üzerinde işlem yapılmadı. GitHub Free Pages için ayrı public gizlilik deposu kullanılabilir. Özel oyun deposunun görünürlüğünü değiştirmek gerekmez. Şu anda doğrulanmış canlı politika adresi yok.

## Kaynaklar

- [Google uygulama içeriği ve gizlilik bağlantısı](https://support.google.com/googleplay/android-developer/answer/9859455?hl=en)
- [Google kullanıcı verisi politikası](https://support.google.com/googleplay/android-developer/answer/10144311?hl=en)
- [Google Data Safety](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en)
- [Google hedef kitle](https://support.google.com/googleplay/android-developer/answer/9867159?hl=en)
- [GitHub Pages erişim koşulları](https://docs.github.com/en/pages/getting-started-with-github-pages/creating-a-github-pages-site)
