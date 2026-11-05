# MeWGic · Özgün iOS ikonuna dönüş

5 Kasım 2026 · Başlangıç `d4150ab` · Dal `release/0.8.0-rc.1`

## Bulunan referanslar ve seçim

- En eski kapak `References/Visual/Cover.png`, `9ebcae0` (23 Ekim). Esas onaylı kopya `References/Visual/Approved/00_Master_Cover.png`, `49d2cc5` (23 Ekim). Dosya biçimi/hash farklı olsa da RGB pikselleri birebir aynı. Bu dikey kapaklar özgün halleriyle korundu; kare ikona kırpılmadı.
- En eski doğrulanmış dağıtım ikonu `ios/testflight-6000.3.26` dalındaki `f434f8d` (27 Ekim): `Assets/Art/AppIcon/Mewgic_AppIcon_1024_v001.png`. Asa tutan, yakın plan büyücü kedi; parlak mor fon. PNG ve GUID orijinal Git nesnelerinden geri getirildi. Yeni çizim, kırpma veya renk değişikliği yapılmadı.
- SHA-256: `bcb3f10d1dfbb6d58088b320da1f1780915aa6e154b7aab6e0f95879ae3e0410`. RGB, 1024×1024; kaynak dosya tarihsel commit ile byte düzeyinde aynı.
- `1b935ff` raporu bu ikonun imzalı 0.8.0 (2) IPA içinde doğrulandığını kaydediyor. Build 1 kaydında iOS ikon yuvaları/1024 marketing ikonu eksik ve Transporter 91111 hatası var. Bu nedenle seçilen dosya **ilk doğrulanmış iOS ikonu, Build 2** olarak adlandırılır; Build 1'de aynı ikonun kullanıldığı veya geçmiş kullanıcı onayının ayrıca doğrulandığı iddia edilmez. Kullanıcının tarifine en güçlü teknik eşleşme budur.
- Build 11 adayı: `Assets/Art/Store/Mewgic_Icon1024.png`, `9840c9e` (4 Kasım). Daha küçük kedi, koyu fon ve alt tarafta fade kullanıyor. Karşılaştırma için `Before-Build11.png` saklandı.
- 57 ilgili mevcut dosyanın ad, boyut ve görsel bilgisi `INVENTORY.json` içinde. Git taraması mevcut dalın yanında eski iOS dalını da kapsadı.

## Kök neden

Eski iOS ikon dosyası ve ayarları ayrı uyumluluk dalındaydı; release dalına taşınmamıştı. `tools/prepare_part20_assets.py` içindeki `icon()` Android ve Apple için yeni kompozisyonu birlikte üretti. `MewgicIosCloudBuild.PreExport()` → `MewgicStoreBuild.ApplyIosIcons()` aynı `Assets/Art/Store/Mewgic_Icon1024.png` dosyasını iOS yuvalarına atıyordu. Böylece yalnız Player Settings'i elle düzeltmek sonraki bulut export'unda tekrar ezilecekti. Mevcut release PlayerSettings dosyasında kalıcı iPhone ikon grubu da bulunmuyordu.

Önceki IPA doğrulayıcısı boyut, opaklık, kimlik ve katalog varlığını denetliyordu; görselin tarihsel içerikle aynı olduğunu denetlemiyordu. Build 11 kaynak zinciri açıkça yeni kompozisyona gidiyor. Bu görevde Build 11 IPA'sı veya canlı TestFlight ekranı yeniden açılmadı.

## Değişiklikler

1. `Assets/Art/AppIcon.meta`, tarihsel PNG ve PNG meta dosyası geri getirildi. GUID `a34b07ce9e2794e4a8486d2b0d92b647` korundu. Import ayarları 1024, sRGB, alpha/mipmap kapalı ve iPhone RGB24/sıkıştırmasız olarak doğrulandı.
2. `ProjectSettings/ProjectSettings.asset`: yalnız iPhone ikon grubu eklendi. Application, Spotlight, Settings, Notification ve 1024 marketing dahil 19 yuva eski GUID'ye bağlı. Android grubuna, sürüm ve build numaralarına dokunulmadı.
3. `Assets/Editor/MewgicStoreBuild.cs`: iOS ikon ataması yeni `MewgicIosIcon.Configure()` girişine yönlendirildi. Android yolu aynı.
4. `Assets/Editor/MewgicIosIcon.cs` ve meta: yalnız iOS için kaynak SHA kontrolü, tekrar uygulanabilir ikon ataması, tüm iOS build'leri için preprocess ve Xcode katalog postprocess kontrolü. Gerçek 1024 Any Appearance RGB PNG'nin pikselleri tarihsel kaynakla karşılaştırılıyor. Yanlış içerik build'i durduruyor. Ürün adı, kimlik, sürüm, imza ve sahneler değiştirilmiyor.
5. `Store Assets/AppStore/AppIcon-1024.png`: tarihsel dosyayla byte düzeyinde aynı. `Store Assets/IMAGE_VALIDATION.json` içindeki ilgili dosya boyutu güncellendi.
6. `tools/prepare_part20_assets.py`: Apple ikonu doğrulanmış tarihsel dosyadan kopyalanıyor; Android üretimi aynı. Betik yeniden çalışsa da Apple ikonu yeni Android kompozisyonuyla ezilmiyor.
7. `tools/IconRestore/IconRestoreValidation.cs`, bu rapor, test/karşılaştırma kanıtları ve `docs/PRODUCTION_PROGRESS.md` eklendi/güncellendi.

Başlangıçtaki TMP fallback değişikliği ve diğer kullanıcı dosyaları teslim kapsamı dışında. `Assets/Art/Store` dosyaları Android tarafından kullanıldığı için silinmedi. Sahne/runtime/oynanış/kayıt dosyası değişmedi.

## Açılış, kapak ve diğer marketing varlıkları

`Boot.unity` içindeki açılış `Assets/Art/Boot/Mewgic_Startup_Background_v001.png`, mevcut logo ve tam gövde kediyi kullanıyor; yanlış Store ikonuna referansı yok. Açılış görseli onaylı kapağın dünya/iksir dilini koruyor. Unity splash kapalı. Bu bölümde ikon değişiminden kaynaklanan yanlış bağlantı bulunmadığından Boot veya launch düzeni değiştirilmedi.

App Store screenshot kartları gerçek eski Game View kaynaklarını kullanıyor; uygulama ikonu yerine geçmiyor. Google Play feature grafiği, telefon kartları ve Android ikonları bu iOS sorunu nedeniyle yeniden tasarlanmadı. Platform eşlemesi: iOS cihaz/App Store → `Assets/Art/AppIcon/Mewgic_AppIcon_1024_v001.png`; Apple teslim kopyası → `Store Assets/AppStore/AppIcon-1024.png`; Android → mevcut `Assets/Art/Store/*`.

## Gerçek doğrulamalar ve sınırlar

- Ana projenin sürümü `6000.3.27f2` olarak korundu. Bu sürümün yerel iOS modülü bulunmadığından ana oyun yeni baştan derlenmedi.
- Kurulu `6000.3.26f1` + iOS Build Support ile ayrı geçici, yalnız ikon içeren Unity projesi kullanıldı. Yeni editor helper C# derlemesi geçti; gerçek Unity API'sinde 19 yuva atandı. Atamanın tekrar çalışması ve ürün/sürüm/build alanlarının korunması doğrulandı.
- Doğru katalog kabul edildi. Eksik filename, yalnız dark appearance ve tek pikseli farklı 1024 RGB ikon reddedildi.
- Yalıtılmış projede **gerçek iOS Xcode export başarılı, build uyarısı 0**. Üretilen 1024 RGB marketing ikonu tarihsel kaynakla piksel düzeyinde aynı. `UNITY_TESTS.json`, `XCODE_CONTENTS.json`, `After-Xcode-Marketing.png`.
- Python ikon üretimi ayrı çıktı klasöründe yeniden çalıştırıldı. Apple çıktısı tarihsel SHA ile aynı. Üç Android kaynak ikonu ve Google Play 512 çıktısı mevcut dosyalarla byte düzeyinde aynı. Yanlış Apple kaynak dosyası reddedildi. `SOURCE_TESTS.json`.
- `PRESERVATION.json`: başlangıç dosya hash'lerine göre kapsam dışı değişiklik yok; mevcut kullanıcı değişiklikleri korunuyor.
- Bu test imzalı IPA, .27f2 üzerinde tam oyun build'i, TestFlight yüklemesi veya fiziksel iPhone kabulü değildir. Canlı mağaza durumu doğrulanmadı.

## Yeni build'den önce ve sonra

1. Bulut hedefi `release/0.8.0-rc.1` dalının bu düzeltmeyi içeren commit'ini kullanmalı. Eski Build 11 IPA'sını tekrar yüklemek ikonu düzeltmez.
2. Yetkili App Store Connect hesabında son kullanılan numarayı kontrol edin. Yeni ve kullanılmamış build numarasını **`BuildConfig/iOS-TestFlight.json` içindeki `candidateBuildNumber` alanına** yazın; örneğin uygunsa 12. Mevcut hook bu alanı ortam değişkeninden öncelikli okur. Bu görev sürüm/build numarasını değiştirmedi.
3. Unity `6000.3.27f2`, doğru iOS bundle kimliği ve imzalama hedefini koruyun. Pre-export `MewgicIosCloudBuild.PreExport`, post-build `BuildConfig/verify-ios-icon.sh` bağlı olmalı.
4. Unity'de `Mewgic > Branding > Apply and Verify Original iOS Icon` menüsüyle veya build kancasıyla atama doğrulanabilir. iOS Player Settings'te 19 yuva ve 1024 marketing kaynağı eski görsel olmalı.
5. Build logunda `MEWGIC_ORIGINAL_IOS_ICON_PASS` ve `MEWGIC_ORIGINAL_IOS_CATALOG_PASS` ile yukarıdaki SHA görülmeli. İmzalı IPA'nın mevcut `MEWGIC_IPA_ICON_PASS` kontrolü de geçmeli. İlk iki işaret tarihsel içerik/export kanıtı, üçüncüsü imzalı paket/katalog kanıtıdır.
6. Yeni arşivi TestFlight'a yükleyip Apple işlemesini bekleyin. Yeni build'in ikonunu hem App Store Connect'te hem TestFlight üzerinden güncellenen cihazda kontrol edin; gerekiyorsa App Store sürüm taslağına yeni build'i seçin. Oyuncu kaydını silmeyin.

Apple kaynakları: [Build yükleme ve kimliklendirme](https://developer.apple.com/help/app-store-connect/manage-builds/upload-builds), [Asset catalog ile ikon yapılandırma](https://developer.apple.com/documentation/xcode/configuring-your-app-icon), [Sürüm için build seçme](https://developer.apple.com/help/app-store-connect/manage-builds/choose-a-build-to-submit/).

Git teslimi: bu rapor ve doğrulanmış ikon değişiklikleri ayrı commit ile mevcut release dalına gönderilir. Teslim commit'i son kullanıcı yanıtında ve Git geçmişinde bulunur.
