# MeWGic · Büyücü Arenası

8 Kasım 2026 · Unity 6000.3.27f2 · `release/0.8.0-rc.1`

**Klan → Lig** üzerinden Büyücü Arenası açılır. Oyuncu ve klan sıralamaları aynı klan hesabı ve SQLite sunucusundan gelir. Kendi sıra, yakın sıralar, ilk 20, sezon kazanış hedefi ve geçmiş sezon sonuçları ayrı gösterilir. Kayıtlı katılımcı yoksa tablo boş görünür.

## Puan ve sezon kuralları

Sunucu doğrulanmış döküşleri mevcut bulmaca kurallarıyla yeniden oynatır. Yıldız sayısını ve ölçülen zorluğu kendisi belirler. Sunucuya ulaşan geçerli yeni kazanışın lig puanı şöyledir:

```text
10 + 4 × zorluk endeksi + 2 × yıldız
+ 12  (UTC haftasının hafta sonundaki ilk geçerli kazanışı)
+ 10  (sezonun üçüncü benzersiz kazanışı)
```

Zorluk endeksi öğreticide 0, kolayda 1, normalde 2, zorda 3 ve uzmanda 4. Klan katkısı kazanış onaylandığı andaki üyeliğe yazılır. Oyuncu klan değiştirdiğinde geçmiş katkı taşınmaz. Aynı hesap ve bölüm kimliği tekrar puan üretmez. Önceden kaydedilmiş klan kazanışları geriye dönük LP oluşturmaz. Ek Şişe, Karıştır ve sunucuda yeniden üretilemeyen eski arşiv düzenleri önceki klan doğrulama kurallarını izler.

Her sezon **28 UTC günü** sürer. Varsayılan başlangıç 2 Kasım 2026 saat 00:00 UTC. Uygulama sunucu saniyesini aldıktan sonra yerel monotonik geçen süreyle geri sayar. Sunucu arka plan görevi ve lig istekleri kapanışı çalıştırabilir. Kapanış sıralamayı, ödül haklarını ve sonraki ligleri tek SQLite işleminde yazar. Sonraki sezon otomatik açılır. Kayıtlı sezonların son 24'ü arayüzden seçilebilir.

Varsayılan kural her ligde en az üç puanlı katılımcı olduğunda en iyi %20 ve en düşük %20'ye uygulanır. Dilim sayısı yukarı yuvarlanır ve toplam katılımcının yarısını aşmaz. Eşit puanlı grup dilim sınırında bölünmez. Terfi için tablodaki puan eşiği de gerekir. Bronzun altında ve Efsanevi'nin üstünde kademe yok. Katkı yapmayan hesabın kademesi korunur.

| Lig | Altın | Terfi için asgari LP | Ek mağaza kozmetiği |
| --- | ---: | ---: | --- |
| Bronz Pati | 100 | 40 | — |
| Gümüş Pati | 160 | 65 | — |
| Altın Pati | 240 | 100 | Gül Parıltısı |
| Kristal Pati | 340 | 145 | Kristal Parıltısı |
| Elmas Pati | 480 | 200 | Kristal Büyücü |
| Efsanevi Büyücü | 650 | — | Arena Büyücüsü |

Sunucu `League` bölümünden puan katsayılarını, etkinlik ve hedef bonuslarını, terfi yüzdesini, katılımcı alt sınırını ve altı kademenin eşiğini ve ödülünü okuyabilir. `League:EpochUtc` üretime çıkarken belirlenir. Var olan bir veritabanında dönem kimliklerinin korunması için başlangıç tarihi sabit tutulur.

## Ödül güvenliği ve görünüm

Lig puanı mağaza bakiyesinden ayrı tablolardadır. Sezon sonu kişisel ödül hakkı hesaba otomatik yazılır. Altın ve mağaza kozmetiği **Ödülü Aktar** ile yerel oyun kaydına eklenir. Sunucu makbuzu ilk başarılı teslimde bir oyun kaydı ve bir hesapla eşleştirir. Başka kayıt ya da hesap aynı teslim bağını kullanamaz. Aynı kayda yeniden teslim, aynı makbuzu döndürür. Unity altını, kozmetiği ve makbuzu birlikte yazar. Yazma hatasından sonra güvenle tekrar denenebilir. Bir kayıttan başka bir kayda ödül taşınmaz.

Sezon rozeti arşiv ödül kartında görünür. Kazanılan profil çerçevesi sonraki sıralama kartlarında lig rengini kullanır. Klanın kazandığı amblem sonraki klan salonunda ve sıralamada görünür. Arena Büyücüsü, mevcut kedi çiziminin yeni bir renk varyantıdır ve ödülden sonra Sahip Olduklarım alanında kuşanılabilir.

Altı geometrik pati rozeti ayrı renk ve şekle sahip **görsel önizlemelerdir**. Nihai çizim için onay bekliyor. Arena metinleri Türkçe ve İngilizce hazır. Diğer altı oyun dili arena bölümünde İngilizce kullanıyor.

## Test ve işletim

```powershell
dotnet build tools/ClanServer -c Release
python tools/ClanServer/test_league.py
python tools/ClanServer/league_unity_qa.py
```

23 gerçek HTTP lig kontrolü, 29 klan API regresyonu, 83 Unity Play Mode arena kontrolü ve 218 mağaza regresyonu geçti. Sunucu testleri 28 günlük sınırı, eşzamanlı puan ve ödül teslimini, terfi ve düşmeyi, özel eşikleri, gerçek klan sırasını ve servis yeniden açılışını doğruladı. Unity gerçek ağ servisine bağlandı. 2. bölümün gerçek döküşleri 20 kişisel LP ve 20 klan LP verdi. Kapalı sezonun 100 altını oyun kaydına bir kez yazıldı. Tekrar teslim ve yerel kayıt hatası denendi. Yeniden açılan yerel kayıt, sunucu puanı ve makbuz korundu. 9:16, 9:19.5 ve 9:20 Game View kontrol edildi. Console temiz ve asıl oyuncu kaydı korundu. .NET Release derleme ve publish başarılı.

QA, geçici SQLite ve gerçek loopback HTTP sunucusu kullanır. Hızlandırılmış sezon saati yalnız Development loopback sunucusunda dosyayla ayarlanabilir. Üretim saati sunucunun UTC saatidir. Mevcut klan servisi yeni sürümle güncellendiğinde lig tabloları eklenir. Üretim için mevcut HTTPS adresi ve kalıcı veritabanı kurulumu gerekir. Üretim yayını ve fiziksel cihaz testi bu doğrulamanın dışında.

[HTTP testleri](HTTP_TESTS.txt) · [Unity Play Mode](PLAY_MODE.json) · [Klan API regresyonu](CLANS_REGRESSION.txt) · [Mağaza regresyonu](SHOP_REGRESSION.json)

[Arena ve altı rozet](01-Arena-9x16.png) · [Geçmiş sezon](02-History-9x16.png) · [Ödül aktarıldı](03-Claimed-9x16.png) · [Oyuncu sıralaması](04-Players-9x19_5.png) · [Klan sıralaması](05-Clans-9x20.png)
