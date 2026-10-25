# Windows IPA yükleme ön kontrolü

Downloads/5774143930948-mewgic-mewgic-ios-testflight-1 (2).ipa yeniden okundu.178864686byte,ZIP CRC geçerli,SHA256 a982747982bd9450f4938b2e271d902283f8bee546b81eb2a1334b85393e6b67. Paket com.edasuyadik.mewgic,version0.8.0,build1.
Apple App Information ekranında Mewgic,AppleID6826795393,BundleID com.edasuyadik.mewgic,SKU mewgic-ios doğrulandı. Sertifika/profil değiştirilmedi.

Mevcut yetkilendirme: App Store Connect API etkin değil,Integrations Request Access gösteriyor. Tarayıcı oturumu resmî JWT yetkisi yerine kullanılmadı; çerez/token çıkarılmadı.
Resmî alternatif: Apple Transporter CLI Windows11 destekliyor,Apple hesabı/app-specific parola ile API anahtarı olmadan çalışabilir. Ancak Windows app upload için -assetDescription AppStoreInfo.plist zorunlu. Apple bu dosyanın xcodebuild export sırasında generateAppStoreInformation=YES ile üretildiğini belgeliyor. Dosya mevcut IPA'da ve Cloud'dan indirilen3101 girdili Build Reports listesinde bulunmadı. Böylece yalnız eldeki dosya/yetkilerle bu yol hazır değil. Dosya içeriği uydurulmadı,ek build başlatılmadı,gereksiz Transporter kurulmadı.
Yeni build açmadan yükleme için Apple Build Uploads REST API uygun bir seçenek. API yetkilendirmesi yok. Kullanıcı önceki Team key isteğini reddetmişti; oluşturulmadı. Yeni talep açıkça gerekli ise açıklayıp onay istemeyi içerdiğinden Developer rolünün tüm ekip uygulamalarına uygulanacağı belirtilerek yeniden açık onay istendi. Henüz anahtar veya erişim talebi oluşturulmadı; sözleşme kabul edilmedi.

Durum: IPA hazır/uygulama kaydı eşleşiyor. Apple'a yükleme yok,Apple kabul/işleme yok,TestFlight'ta build yok. Yetkilendirme onayı bekleniyor. iPhone'da yükleme şu an mümkün değil; kabul+işleme sonrası internal tester/grup ve TestFlight erişimi doğrulanacak.
Ana master/oyun/DevControl değişmedi. IPA ve sırlar Git'e eklenmez.

Resmî kaynaklar:
https://help.apple.com/itc/transporteruserguide/en.lproj/static.html
https://help.apple.com/xcode/mac/current/en.lproj/deva1f2ab5a2.html
https://developer.apple.com/help/app-store-connect/manage-builds/upload-builds/
https://developer.apple.com/documentation/appstoreconnectapi/creating-api-keys-for-app-store-connect-api
