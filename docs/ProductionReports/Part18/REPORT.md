# PART 18 · Oyun hissi, ses ve yerelleştirme

4 Kasım 2026 · Unity 6000.3.27f1 · Başlangıç `98bf6f1`

## Oyun hissi ve arayüz

Ana menüdeki zarif arka plan hareketi, Play düğmesindeki küçük ölçek değişimi ve maskeli şelale akışları mevcut sanatla korunuyor. Geçerli döküş hedef şişede kısa nane renkli vurgu ve tiz bir ses, geçersiz hamle pembe uyarı, farklı bir ses ve neden metni veriyor. Standart döküş mevcut eğilme, sıvı hacim geçişi ve dönüş akışını kullanıyor. Kazanış sesi ve coin sesi başarıyla kaydedilen sonuçtan sonra tetikleniyor. Önceki bölüm sonu yıldız dizisi, coin sayacı ve atlama düğmesi kullanılıyor.

Hareketi azalt seçeneği menü efektlerini ve şelale zamanını sakinleştiriyor, harita odağını ve yolculuğunu kısaltıyor, şişe uçuşu yerine kısa tahta güncellemesi yapıyor ve ödül sayacını doğrudan son değere alıyor. Pati öğreticisi temel hareketi göstermeye devam ediyor. Mevcut kedi çizimi ve pozları kullanılıyor. Yeni kedi resmi eklenmedi.

Ayarlar kartı müzik, efekt, titreşim, hareket, dil ve pati tekrarı için ortak düğme düzenini kullanıyor. Eski birleşik ses düğmesi kaldırıldı. Mevcut doğrudan Günlük Görevler kısayolu ve Etkinlikler merkezi ayrı erişim yolları olarak tutuldu. Referansları topluca doğrulanmamış sanat dosyaları silinmedi.

## Tercihler ve ses

Müzik, ses efektleri, titreşim ve hareket tercihi ayrı ayrı kaydediliyor. Müzik ve efektlerin ilk değeri eski ses tercihinden geliyor. Önceki harita hareket tercihi ilk yüklemede aktarılıyor. Haritadaki hareket kontrolü Ayarlar ile aynı kaydı kullanıyor. Mobil titreşim varsayılanı kapalı.

Ana ekran ve bulmaca müzikleri ile sekiz ses özgün sentez. [Ses hakları ve varlık envanteri](AUDIO.md)

## Yerelleştirme

Mevcut Unity Localization tabloları ve TMP bağlayıcısı kullanıldı. İlk kurulumda cihaz diline veya desteklenen bölgesel eşine bakılıyor. Desteklenmeyen dil İngilizceye düşüyor. Oyuncunun seçtiği dil öncelikli ve kalıcı. Var olan sekiz dil tutuldu.

187 anahtarın sekiz Unity tablosuyla kaynak eşitliği ve iki TMP fontunun glif kapsamı doğrulandı. Ayarlar için eklenen metinler müzik, efekt, titreşim, hareket ve açık/kapalı ayrımına göre gözden geçirildi. Çeviriler yayın onayı almamış taslak olarak işaretli. Ana dil editörü onayı ve tüm dinamik UI durumlarının ayrı dil denetimi açık.

## Unity Game View galerisi

| Dil | Oran | Home | Map | Gameplay | Rewards | Settings |
| --- | --- | --- | --- | --- | --- | --- |
| Türkçe | 9:16 | [Ana menü](tr/Home.jpg) | [Harita](tr/Map.jpg) | [Oynanış](tr/Gameplay.jpg) | [Ödüller](tr/Rewards.jpg) | [Ayarlar](tr/Settings.jpg) |
| İngilizce | 9:19.5 | [Ana menü](en/Home.jpg) | [Harita](en/Map.jpg) | [Oynanış](en/Gameplay.jpg) | [Ödüller](en/Rewards.jpg) | [Ayarlar](en/Settings.jpg) |
| Almanca | 9:20 | [Ana menü](de/Home.jpg) | [Harita](de/Map.jpg) | [Oynanış](de/Gameplay.jpg) | [Ödüller](de/Rewards.jpg) | [Ayarlar](de/Settings.jpg) |
| Fransızca | 9:16 | [Ana menü](fr/Home.jpg) | [Harita](fr/Map.jpg) | [Oynanış](fr/Gameplay.jpg) | [Ödüller](fr/Rewards.jpg) | [Ayarlar](fr/Settings.jpg) |
| İspanyolca | 9:19.5 | [Ana menü](es/Home.jpg) | [Harita](es/Map.jpg) | [Oynanış](es/Gameplay.jpg) | [Ödüller](es/Rewards.jpg) | [Ayarlar](es/Settings.jpg) |
| Portekizce (Brezilya) | 9:20 | [Ana menü](pt-BR/Home.jpg) | [Harita](pt-BR/Map.jpg) | [Oynanış](pt-BR/Gameplay.jpg) | [Ödüller](pt-BR/Rewards.jpg) | [Ayarlar](pt-BR/Settings.jpg) |
| İtalyanca | 9:16 | [Ana menü](it/Home.jpg) | [Harita](it/Map.jpg) | [Oynanış](it/Gameplay.jpg) | [Ödüller](it/Rewards.jpg) | [Ayarlar](it/Settings.jpg) |
| Lehçe | 9:19.5 | [Ana menü](pl/Home.jpg) | [Harita](pl/Map.jpg) | [Oynanış](pl/Gameplay.jpg) | [Ödüller](pl/Rewards.jpg) | [Ayarlar](pl/Settings.jpg) |

[İngilizce ada detayı](en/Island.jpg) · [İngilizce kazanış](en/Victory.jpg) · [Unity test verisi](UNITY_TESTS.json)

Unity Play Mode’da 132 kontrol geçti. Denetlenen görünür TMP etiketlerinde taşma adayı veya eksik anahtar çıkmadı. Konsol hatası veya uyarısı kaydedilmedi. Müzik ve şelale kanallarının oynatımı, ayrı sessize alma, tercihlerin diskten yüklenmesi, dil seçimi ve 44 px asgari ayar düğmesi yüksekliği doğrulandı. Gerçek geçersiz hamle, üç normal döküş, kaydedilen galibiyet ve kısa hareket modu sınandı. Ana oyuncu kaydı, yedek ve bozuk kayıt alanları değişmedi. Fiziksel telefon hoparlöründe ses dengesi, haptik davranış, performans ve cihaz işletim sistemi askıya alma davranışı ölçülmedi. Türkçe ve İngilizce dahil tüm dillerin beş temel ekranı Editor Game View’da görüntülendi.

Git teslim dalı recovery/part09-completion.
