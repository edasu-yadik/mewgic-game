# PART 02C — WizardCat Home entegrasyonu

24 Ekim 2026. Proje: C:\Dev\Mewgic, Unity 6000.3.27f1.

## Sonuç

Mevcut WizardCat prefabının bağlı instance'ı Home sahnesine eklendi. Yeni kedi,
arka plan, şişe veya UI tasarımı üretilmedi. Referanstaki sol alt-orta kompozisyon
izlendi: karakter şişelerin solunda, değnek yukarı-sağa dönük, OYNA'nın üzerinde.
Geçici şişe panelinin sol kenarıyla sınırlı görsel örtüşme vardır; şişeler ve buton
yer değiştirmedi. Nihai referans arka planı ve navigasyonu henüz uygulanmadı.

## Yapı

Home sahnesindeki Home kökü Background, WorldDecorations, PotionDisplay,
WizardCat, UI ve Effects katmanlarını içerir. HomeCharacterPresenter mevcut
runtime Canvas'a bağlar; mevcut gece zeminini Background'a, Safe Area UI kökünü
UI katmanına taşır. PotionDisplay, WizardCat ve Effects aynı güvenli alanı izler.
WorldDecorations ve Effects sonraki aşama için boştur; mevcut karakterin yerel
efekt havuzu prefab içinde kalır. Karakter Edit Mode'da pasif, Home açılınca aktiftir.

Karakter genişliği güvenli alanın en fazla %34'üdür. Ölçek her eksende aynıdır;
tam sprite dikdörtgeni OYNA üstünden 18 Canvas birimi yukarıda başlar. Mevcut
prefab, PNG'ler, Animator ve animasyon klipleri değiştirilmedi. Idle otomatik,
Blink prefabın 4.5–7 saniyelik sistemiyle çalışır. Home'a özel Wand Magic ilk
3.25 saniyeden sonra, ardından 8–13 saniye aralıklarla tetiklenir; meşgulken
üst üste başlatılmaz. Mevcut 18 parçacıklı havuz yeniden kullanılır.

## Gerçek doğrulamalar

- Yerel Unity MCP eval çağrılarıyla proje, sahne, prefab ve Console okundu.
- HomeCharacterTests.Run(): gerçek Home Play Mode içinde 49 kontrol başarılı.
- Idle dönüşümü, mevcut kapakların kapanıp açılması, periyodik Wand Magic,
  elle tetikleme, aktif altın parçacıklar ve normale dönüş doğrulandı.
- OYNA gerçek EventSystem raycast hedefi olarak tıklandı; Gameplay açıldı ve
  şişeler bulundu. Ana Menü ile dönüldü; ikinci karakter oluşmadı.
- Boot üzerinden ayrıca Play başlatıldı; Home ve karakter yüklendi.
- Gerçek Game View çözünürlükleri 900×1600, 900×1950, 900×2000 olarak değiştirildi.
  Screen.width/height, karakterin güvenli alanda kalması ve eşit ölçek doğrulandı.
  Sonrasında önceki Game View seçimi geri yüklendi.
- Play Mode'daki gerçek Home Canvas'ı aynı üç çözünürlükte kamera/RenderTexture
  ile PNG olarak yakalandı. Prefab önizlemesi veya kompozit tasarım değildir.
- 900×2000 görüntüde ek çentik/güvenli alan simülasyonu yapıldı; tam karakter
  güvenli alan içinde, OYNA ile çakışma yok. Bu fiziksel Android cihaz testi değildir.
- Son Console: 0 hata, 0 uyarı. C# Editor derlemesi ve Play Mode başarılı.
- Boot, Gameplay, LevelMap, WizardCatAnimationTest sahne dosyaları; mevcut prefab,
  karakter kaynakları ve oyun modeli Git karşılaştırmasında değişmedi.

Ham test sonuçları PART02C_TEST_RESULTS.json; MCP kontrolleri PART02C_*CHECK.json
ve PART02C_GAMEVIEW_*.json dosyalarındadır. Bu aşamada önceki 12 model testinin
yeniden çalıştırıldığı iddia edilmez. Yeni APK üretilmedi; cihaz testi yapılmadı.

## Görseller

- home-character-1600.png: 9:16 gerçek Home.
- home-character-1950.png: 9:19.5 gerçek Home.
- home-character-2000.png: 9:20 gerçek Home.
- home-character-2000-safe.png: ek güvenli alan simülasyonu.
- home-character-1600-blink.png ve home-character-1600-magic.png: animasyon anları.

Sonraki aşama, ayrıca onaylanan kapsamda Home arka planı ve UI görsel üretimidir.
Gerçek mobil cihazda safe area, performans ve dokunma testi hâlâ gereklidir.
