# Mewgic çalışma kuralları

- Bütün açıklamalar ve raporlar Türkçe olmalıdır.
- Her görev öncesi mevcut dosyalar, AGENTS.md, son üretim raporu ve sistem incelenir.
- Çalışan mekanikler, onaylı sanat sürümleri, sahneler ve kayıt sistemi korunur; başarılı asset'ler silinmez.
- Unity ve gerektiğinde Blender MCP mümkün olduğunda kullanılır. Unity işlemleri aynı anda başlatılmaz.
- Onaylı referanslara sadık kalınır. İstenmeyen özellik eklenmez, tek görevde farklı partların uygulaması karıştırılmaz.
- Akış: incele → oluştur/düzenle → test et → gerçek Unity Game View ile referansı karşılaştır → aynı partta düzelt → yeniden test et → doğrulanan değişiklikleri commit/push et → kısa rapor yaz.
- Test başarılı olsa bile referanstan belirgin uzak görsel tamamlanmış sayılmaz.
- Düşük kaliteli dikdörtgenler veya ilgisiz stok varlıklar nihai sanat olarak sunulmaz. Sprite alpha/kenar/kalite kontrolü yapılır.
- UI öğeleri ayrı ve etkileşimli olmalıdır; her butonun gerçek görevi veya açıkça belirtilen geçici davranışı olmalıdır.
- Mobil Safe Area, 9:16, 9:19.5 ve 9:20 oranları korunur. Sprite atlas/efekt yaklaşımı mobil performansa uygun seçilir.
- Her aşamada değişikliğe uygun otomatik testler, mümkünse mevcut testler, C# derlemesi, Console, Play Mode ve temel akış doğrulanır; gerçek ekran görüntüsü kaydedilir.
- Kritik Console hataları giderilir. Belirsiz araç çıktısı başarı kabul edilmez. Test edilmemiş sonuçlar başarılı gösterilmez. Cihaz testi yapılmadıysa açıkça belirtilir.
- Aynı partta hata kaynağı bulunur, onarılır ve test tekrarlanır; tekrarlayan hatada yalnız ilgili değişiklik en son sağlam sürüme döndürülür. Kullanıcının diğer çalışmaları korunur.
- Değişiklikler küçük ve geri alınabilir tutulur. Riskli, yıkıcı veya geri alınamaz işlemler otomatik yapılmaz.
- DevControl projesine müdahale edilmez.
- GitHub'da yalnız doğrulanan ilgili değişiklikler commit edilir; push sonucu uzak commit ile doğrulanır.
- Her part için docs/ProductionReports/ altında tamamlanan işler, dosyalar, görseller, gerçek testler, Unity görüntüleri, kalan farklar, commit/push durumu ve sonraki aşamaya uygunluk raporlanır. docs/PRODUCTION_PROGRESS.md güncellenir.
- Bir part tamamlanmadan sonraki partın dosyaları değiştirilmez. Sonraki part kullanıcı talimatıyla başlar; verilen görev kendi içinde gereksiz onay beklemeden tamamlanır.
