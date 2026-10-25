# iOS 6.3.26 uyumluluk ve Cloud Build düzeltmesi

Ana proje C:/Dev/Mewgic, master,6000.3.27f1 korunmuştur. Çalışma kopyası C:/Dev/Mewgic-iOS-Compat; branch ios/testflight-6000.3.26. Baz commit4e3e32feaeab4521ceb2f7c06d8e99aced632a69.

## Kesin neden ve çözüm
Unity Cloud6000.3.27f1'i desteklemediği için ilk isteği Unity çalışmadan reddetti. Güncel liste6.3.26f1'e kadar. Bu branch yalnız desteklenen6.3.26f1 patch'ini kullanır; ana branch sürümü değiştirilmez. Proje kökünde Assets/Packages/ProjectSettings var, subfolder boş.
Resmi Editor6.3.26f1/de8167e7440c Unity CLI1.0.0-beta.14 ile yan yana kuruldu. CLI güncellik kontrolü aynı sürümü döndürdü.

## Yerel gerçek testler
- İlk import ve C# derlemesi tamamlandı; model batch çıkış kodu0.
- Bulmaca, kayıt, harita ilerleme, güçlendirici, günlük görev ve ödül modeli:6 mevcut test grubu geçti.
- Boot,Home,Gameplay,LevelMap eksik script ve kamera kontrolleri geçti.
- Gerçek Play Mode smoke iki kez tamamlandı: Boot→Home→Gameplay→Home, geçersiz hamle, başarılı aktarım, animasyon kilidi, Undo,Restart,kazanma,Replay.
-900x1600 ve900x1950 gerçek render görüntüleri iOSCompatibility klasöründe. Bu görevde900x2000 yeniden test edilmedi; fiziksel iPhone testi yapılmadı.
- Editor Search.SearchDatabase indekslemesi başlangıçta ArgumentOutOfRangeException üretiyor. İki smoke başarılı olsa da Console tamamen temiz denemez. Oyun kodunda hata olarak gösterilmez. İlk importta2D Animation USS unsupported selector mesajları da var; rapor özetlerinde saklandı. C# compile hatası yok.
- Editor çözümleyicisi packages-lock Burst1.8.31→1.8.30,Collections2.7.1→2.6.8 uyarladı. Manifest aynı, oyun/sahne/art/PlayerSettings aynı.
- Test araçlarındaki ana klasöre sabit çıktı yolları yalnız yerel test sırasında kopyaya yönlendirildi; ardından geri alındı. Runtime kodu değiştirilmedi.

## Cloud hedefi
Mevcut imzalama korunuyor: Mewgic TestFlight App Store2026,appstore,com.edasuyadik.mewgic,Apple Distribution EDASU YADIK(VKW7GYV7B9),son tarih25 Ekim2027.
Mac Standard,macOS Tahoe,Xcode26.5.0,Apple Silicon mevcut. Kota0/100Mac dakika; upgrade yok. Ücretli build yetkisi verilmedi.
PreExport ve Gymfile uyumlu kimlikleri kullanır. Test edilmiş c6496d5d1906362b6b8410497fdb9837414b977e commit private branch üzerine push edildi, uzak SHA eşleşti. Bulut hedefi bu branch ve kesin6.3.26f1 olarak kaydedildi. Build #1 isteği kabul edildi; eski sürüm doğrulama reddi kalktı. Son durum Sent to Builder; henüz log yok. IPA/TestFlight sonucu bekleniyor, başarılı sayılmadı.

## Kaynaklar
https://unity.com/releases/editor/whats-new/6000.3.26f1
https://docs.unity.com/en-us/build-automation/basic-build-configuration/overview

## Teslim durumu
Sürüm engeli çözüldü; iOS derleme sonucu henüz doğrulanmadı. Gerçek log ekranı No logs are available for this build diyor. Kalan iş: Mac builder başlatması, Unity/iOS/Xcode/signing loglarının kontrolü, başarılıysa IPA artefakt doğrulaması ve ayrıca App Store Connect yükleme yetkisi/işlemi. Fiziksel iPhone testi yok. Ana master4e3e32f ve290 dosya hash kontrolü korunuyor.
