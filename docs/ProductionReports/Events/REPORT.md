# MeWGic · Büyülü Etkinlikler

8 Kasım 2026 · Unity 6000.3.27f2 · `release/0.8.0-rc.1`

## Etkinlik takvimi

On iki tür, gerçek hesaplar ve mevcut bulmaca doğrulayıcısıyla çalışan ASP.NET Core ve SQLite hizmetine bağlandı. Üretim adresi henüz yapılandırılmadı. Aşağıdaki takvim, sunucu saati kontrollü gerçek yerel hizmette doğrulandı. İstemcide puan veya ödül üreten çevrimdışı bir taklit kullanılmıyor. Eski çevrimdışı günlük görevler, haftalık ödüller, sezon yolu ve koleksiyon **Klasik Ödüller** içinde çalışmaya devam ediyor.

| Etkinlik | Etkinleşme | İlerleme ve ödül |
| --- | --- | --- |
| Günlük Büyü Görevleri | Her UTC günü | İlk kazanış başına 40 ve yıldız başına 20 puan. Standart kademe. |
| Haftalık Yıldız Avı | Pazartesi başlayıp 7 gün | Yıldız başına 35 puan. Standart kademe. |
| Altın Pati Yarışı | 14 günlük rotasyonun ilk günü | Zorlukla artan 100–500 puan. 2.000 puana ulaşan ilk üç sıra kapanışta Efsanevi Sandık açar. |
| Kristal Koleksiyonu | Rotasyonun ikinci günü | Zorlukla artan 80–400 kristal puanı. 500 puanda Kristal Sandık. |
| Kedi Hazinesi | Rotasyonun üçüncü günü | Yeni kazanış başına 100 puan. İlk üç kademede Normal, Büyülü ve Kristal Sandık. |
| Büyülü Maraton | Rotasyonun dördüncü günü | Doğrulanmış en uzun ardışık bölüm numarası serisi × 100. Yerel başarısız denemeler seriyi değiştirmez. |
| Kusursuz Büyü | Rotasyonun beşinci günü | Yalnızca üç yıldızlı yeni kazanış başına 125 puan. Standart kademe. |
| Hafta Sonu Şenliği | Cumartesi ve pazar UTC | Yeni kazanış başına 80 ve yıldız başına 25 puan. Sandık kademeleri. |
| Klan Yıldız Yarışı | Rotasyonun 8. ve 9. günleri | Yıldız başına 45 ortak puan. Ödül için oyuncunun katkısı gerekir. |
| Klan Kristal Kupası | İki lig sezonundan ikincisi, 28 gün | Zorlukla artan klan kristal puanı. İlk üç sıradaki klanın katkı yapan üyeleri 2.000 puandan sonra Efsanevi Sandık açar. |
| Büyülü Ada Festivali | Rotasyonun 10. ve 11. günleri | Tek numaralı adalardaki yeni kazanış başına 100 puan. 500 puanda Kristal Sandık. |
| Efsanevi Pati Sezonu | İki lig sezonundan ilki, 28 gün | Kazanış başına 60 ve yıldız başına 20 puan. 500'de Büyülü Sandık, 1.000'de Kristal Büyücü kostümü. |

Standart kademeler 100 puanda 50 altın, 250 puanda iki Renk İpucu, 500 puanda Normal Sandık, 1.000 puanda Gül Efekti ve 2.000 puanda Efsanevi Sandık verir. Etkinlik puanları altın ve lig puanından ayrı kaydedilir. Eşit puanlar aynı yarış sırasını paylaşır. Klan kupası lig takvimini paylaşır, kendi sıralamasını tutar. Bir oyuncunun klan katkısı, etkinlikte ilk katkı yaptığı klana bağlanır.

## Sandıklar

| Sandık | Sunucudaki başlangıç havuzu |
| --- | --- |
| Normal | %70 50 altın · %25 iki Renk İpucu · %5 100 altın |
| Büyülü | %55 100 altın · %35 üç Geri Al · %10 Gül Efekti |
| Kristal | %50 150 altın · %35 iki Ekstra Şişe · %15 Kristal Efekti |
| Efsanevi | %55 350 altın · %30 beş Karıştırma · %15 Lig Büyücüsü kostümü |

Önizleme etkinliğin kaydedilmiş havuzundan gerçek yüzdeleri gösterir. Talep geçerliyse sunucu ödülü ağırlıklı olarak bir kez seçip cüzdana bağlanan tek makbuzla saklar. Unity, altın, güçlendirici, kozmetik ve makbuzu aynı kayıt işleminde yazar. Bağlantı kesilirse veya oyun kaydı yazılamazsa aynı cüzdan özgün teslimi yeniden isteyebilir. Sandık yaklaşık 0,9 saniyede sallanır, 0,36 saniyede açık çizime geçer ve altın yıldız ışığıyla ödülü gösterir. Azaltılmış harekette açık sandık doğrudan gösterilir. Sandıklar oyun içi hedeflerle kazanılır.

## Yapılandırma ve kayıtlar

- `tools/ClanServer/Events.json` etkinlik tanımlarını, kuralları, başlangıç ofsetlerini, tekrar aralıklarını, süreleri, ödül kademelerini ve sandık ağırlıklarını içerir. Başlangıç ayarı dört aktif takvim hattıdır. `Events:MaxActive` sınırı sunucudan ayarlanabilir. `Events:ConfigPath` ile dağıtımda ayrı JSON kullanılabilir. Yapılandırma sunucu başlangıcında doğrulanır.
- Başlayan etkinliğin kuralları ve ödülleri SQLite'a kopyalanır. Yapılandırma değişikliği geçmiş ödülleri değiştirmez. Tamamlananlar sekmesi son 12 genel etkinliği ve hesaba ait katkı geçmişini sayfalar. Kazanılmış ödül kapanıştan sonra da alınabilir.
- Sıralamalar gerçek hesap ve klanlardan gelir. Sunucu istemciye güvenmeden hamle dizisini bulmacada tekrar yürütür, zorluk ve yıldızları hesaplar. Aynı hesabın aynı bölüm kazanışı yeni puan oluşturmaz. Sunucuya ulaşmayan oynanış, etkinlik puanı olarak geriye dönük yazılmaz. Oturum açıkken bekleyen kanıt gönderimi mevcut klan istemcisinin oturum içi kuyruğunu kullanır.
- Klasik kayıtlar, koleksiyon ve sahip olunanlar için eski veri biçimi korunur. Yeni ödül sunucuda teslim makbuzuyla, cihazda mevcut oyun ekonomisi içinde saklanır. Yeni çevrimiçi arayüz Türkçe ve İngilizce. Diğer altı dilde İngilizce metin gösterilir.
- Kartlar mevcut yıldız, sandık, kristal, araç ve maskot varlıklarıyla hazırlanmış önizlemelerdir. Yeni nihai kapak illüstrasyonları onaya açıktır.

## Unity ve sunucu doğrulaması

- `dotnet build tools/ClanServer -c Release` ve Unity derlemesi sıfır hata ve sıfır uyarıyla tamamlandı.
- `python tools/ClanServer/test_events.py` ile 37 gerçek HTTP/SQLite kontrolü geçti. Takvim, 12 tanım, puan, sıralama, arşiv, kapanış, eski ödül yapılandırması, eşzamanlı talep, sunucu yeniden başlatma ve cüzdan sahipliği doğrulandı. Lig ve klan HTTP regresyon paketleri de geçti. [HTTP çıktısı](API_TESTS.txt)
- Unity Test Framework bu projede kayıtlı PlayMode testi listelemedi. Projenin çalışan Editör probunda gerçek GameObject raycast'leri, Gameplay sahnesindeki döküşler, UnityWebRequest ve SQLite ile **88 Play Mode kontrolü** geçti. Bölüm 5 günlük puanı 400'den 500'e çıkardı. Sandık açılışı, makbuzla tek teslim, kayıt yazma hatası, tekrar açılış, bağlantı kesintisi, eski koleksiyon ve üç portre oranı doğrulandı. Oyuncu kaydı QA öncesi ve sonrasında aynıydı. [Play Mode çıktısı](PLAY_MODE.json)
- `Home` sahnesinde **Etkinlikler** açılır. Geliştirme sunucusuyla tekrar üretmek için Unity açıkken derlemeden sonra `python tools/ClanServer/events_unity_qa.py` çalıştırılır. Prob geçici hesap ve ayrı kayıtla `Home → Etkinlikler → Giriş Yap → Oyna → bölüm 5 → Etkinlikler → Ödül` akışını yürütür.

| Gerçek Game View | Oran |
| --- | --- |
| [Aktif etkinlikler](01-Active-9x16.png) | 9:16 |
| [Yaklaşan etkinlikler](02-Upcoming-9x19_5.png) | 9:19.5 |
| [Puan ve ödül kademeleri](03-Rewards-9x20.png) | 9:20 |
| [Sandık olasılıkları](04-Chest-Odds-9x20.png) | 9:20 |
| [Açılmış sandık ve gerçek ödül](05-Chest-Reveal-9x20.png) | 9:20 |

## Değişiklikler ve açık işler

- Unity ekranı ve ekonomi bağlantısı `MagicalEventsPanel.cs`, `EventClient.cs`, `EventWallet.cs`, `LeagueWallet.cs`, `HomeNavigation.cs` ve `PrototypeApp.cs` içinde. Yerelleştirme `Mewgic.json`, paylaşılan tablo ve sekiz dil tablosunda. Yeni prefab veya sahne değişikliği yok.
- Backend `EventModels.cs`, `EventStore.cs`, `Events.json`, `ClanStore.cs`, `LeagueModels.cs`, `Program.cs` ve `ClanServer.csproj` içinde. `EventsProbe.cs`, `events_unity_qa.py`, `test_events.py` ve eski merkeze uğrayan navigasyon probları güncellendi.
- Üretim için mevcut hizmetin HTTPS dağıtımı ve `Assets/Resources/Clans/Connection.json` adresi gerekiyor. Üretim hesapları, cihaz testi, yeni iOS/Android paketi ve nihai kapak illüstrasyonları doğrulama kapsamında açık.
