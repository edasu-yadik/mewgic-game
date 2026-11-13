# Unity IAP 5.5.0 · Registry çözümleme düzeltmesi

Önceki IAP çalışmasında Unity Package Manager indirmeye HTTP 401 aldı. Yerel Unity oturumunda testleri sürdürebilmek için teşhis arşivi manifestte file:../Library/IapPackageInspection/iap.tgz olarak tanımlandı. Kilit dosyası bu kaynağı local-tarball olarak kaydetti. Unity, Library altındaki arşivi yerel paket kaynağı olarak geçersiz sayıyordu.

Yerel Packages/manifest.json ve Packages/packages-lock.json, depoda zaten bulunan 5.5.0 Registry yapılandırmasına döndürüldü. Başka paket sürümü değişmedi. İnceleme arşivi proje dışına taşındı. SHA-1 değeri 00a2945f1a91ab88ed74741e26cd64ef5f29399d ve Unity Registry metaverisiyle eşleşiyor. Arşiv depoya eklenmedi.

## Unity doğrulaması

- Unity 6000.3.27f2 resmî Registry’den 5.5.0 paketini indirdi. UPM günlüklerinde yeni çözümleme HTTP 200 ile tamamlandı. Editor PackageInfo sürümü 5.5.0, kaynağı Registry ve hata listesini boş döndürdü.
- Package Manager ekranında Unity Registry kaynağı ve Unity Technologies imzası görüldü. IAP konum uyarısı yoktu. Console derleme hatası yoktu. Açılış günlüğünde IAP dışındaki mevcut PremiumButton.Start CS0114 derleyici uyarısı kaydedildi.
- Mevcut StorePayments.cs, IAP 5 StoreController API’siyle değişiklik gerektirmeden derlendi. Katalog, önizleme, yerelleştirilmiş test fiyatı, iptal ve hata, gecikmeli onay, tekrar teslimat koruması ve kayıt kontrolleri mevcut Magic Shop Play Mode senaryosuyla çalıştırıldı.
- [İzole Play Mode raporu](PLAY_MODE.json) toplam 822 başarılı kontrol, sıfır çalışma zamanı hatası ve oyuncu kayıt anahtarlarının değişmediğini gösteriyor. [Game View’da yerel test fiyatı](IapTestLocalizedPrice.png).
- Apple ve Google cihaz sandbox testleri bu turda çalıştırılmadı. Gerçek ödeme veya mobil derleme başlatılmadı.

Yerelde düzeltilen iki paket dosyası GitHub’daki sürümle birebir eşleştiği için bu teslimde yalnızca doğrulama raporu ve kanıtlar commit edilir.
