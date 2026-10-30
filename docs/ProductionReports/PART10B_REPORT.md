# PART10B — Pati Yardımı / Paw Guide

30 Ekim2026 • Unity6000.3.27f1 • recovery/part09-completion.

## Çalışma

Gameplay'deki mevcut WizardCat'in üzerine görünmez,dokunulabilir bir alan
yerleştirilir. Kedi prefabı/Animator/Idle/göz kırpma değiştirilmez. Aynı
TriggerMagic API'si menü açılışı ve başarılı öğretici aktarımında kullanılır.
Gönderilen kedi illüstrasyonu mevcut karakteri değiştirmek için kullanılmadı.

Yeni,henüz birinci bölümü tamamlamamış oyuncuya Merhaba→DEVAM→kaynak→hedef
akışı gösterilir. Kaynak/hedef mevcut PuzzleSolver'ın3000 durum sınırı içinde
doğruladığı çözümün ilk hamlesidir. Çözüm yoksa/sınır aşılırsa kesin hamle
sunulmaz; genel açıklama açılır. İlk başarılı önerilen aktarımın animasyonu
tamamlanınca tamamlanma kaydedilir,kedi değnek hareketi ve kısa kutlama olur;
balon1,4 saniye sonra kapanır. Geçersiz hedef tamamlanmış sayılmaz.
Oyun girişleri kilitlenmez; öneriden farklı geçerli hamlede çözüm yeniden
hesaplanır. Genel yardım ilk geçerli aktarımda kapanır. Restart/başarılı
güçlendirici kullanımı eski işaretlemeyi kapatır; seçim kaldırıldığında
kaynak aşaması geri gelir. Sıvı animasyonu ve Pause sırasında balon gizlenir.

Kedi menüsü Nasıl oynanır / Hamle öner / Kapat içerir. Nasıl oynanır sıvıya
ve hamle sayısına dokunmaz. Hamle öner doğrudan mevcut UseBooster(Hint)
yoluna bağlıdır; mevcut hak/çözüm doğrulama/tekrar-hint kuralları korunur.
Tek balon yeniden kullanılır. Escape/Android geri eşlemesi HandleBack ile
kapatır; ekran Kapat butonu aynı Close yolunu kullanır.

## Arayüz ve genişletme

PART10A TutorialOverlay içinde parşömen/altın çerçeve ve TMP Masalsı teması
kullanılır. Balon kedinin altındaki boş bantta; şişe alanı ve güçlendirici
kontrolleri arasındadır. Açılış alpha ve%95→100 ölçekle yumuşatılır.
Altın hafif çerçeve ve küçük hareketli pati native mesh'tir,raycast almaz.
Her kare parçacık/nesne üretilmez; köşe dizisi önceden ayrılır.

PawTutorialDefinition ScriptableObject: tutorialId,conditionKey,message,
targetName,completionKey. Gelecek mekanik sahibi koşulu değerlendirerek
TryStartFeature(definition,conditionMet) çağırır; doğrulanmış olayda
CompleteFeature(completionKey) kullanır. ID öğrenildiyse otomatik gösterim
reddedilir. Gerçek kilitli/hareketli şişe veya yeni mekanik eklenmedi.
Bu bağlar yeni mekaniğin koşul mantığını kendiliğinden uygulamaz; açık host
API'sidir. Temel öğretici ID'si paw.basic.v1'dir; feature ID'leri içerik
tarafından global veya bölüm kapsamlı benzersiz seçilebilir.

## Kayıt koruması

ProgressData'ya isteğe bağlı completedTutorials listesi eklendi; Copy tüm
işlemlerde listeyi taşır. Şema sürümü1 korunur: ek alan eski kayıtta zorunlu
değildir ve null/eksik alan boş listeye taşınır. Geçersiz/tekrarlı geçmiş
ID'leri doğrulanır. ProgressLedger.CompleteTutorial mevcut atomik Commit
yoluyla kaydeder; yazma başarısızlığında ID tamamlanmış sayılmaz.
İlerleme,ödül,Hint stokları ve oyuncunun gerçek kayıtları sıfırlanmaz.
Eski oyuncu birinci bölümü bitirmişse yeni temel giriş zorla gösterilmez.

## Gerçek testler

PawGuideProbe sonucu docs/Part10B/UI_AUDIT.json başarılıdır. Yeni oyuncu,
kaynak/hedef aşamaları,yanlış hedef,doğru aktarım,kapanış,kedi raycast'i,
hızlı çift dokunma,menü,genel yardım,Hint tüketimi,Restart,JSON kapat/aç
simülasyonu,rehberin yeniden oluşturulması,öğrenilmiş ID'nin açılmaması,
geri kapatma,güvenilir çözüm bulunamaması,yeni feature koşulu/ID/tamamlama,
eski kayıt ve başarısız kayıt atomikliği doğrulandı.
DailyActivitiesTests.Models ve RewardHubTests.Models ayrıca geçti.

Play Mode runtime hata0/uyarı0; C# derleme başarılı. Editor kapanışındaki
önceden bilinen JobTempAlloc mesajları runtime sayısına dahil değildir.
Oyuncunun gerçek PlayerPrefs belgesi test öncesi/sonrası birebir aynı.
Welcome/Source/Target/Menu için900×1600/1950/2000 ve Safe Area render'ları,
metin taşma kontrolü mevcut. Welcome-GameView.png doğrudan ScreenCapture'dır.
Normal.png öğretici kapalı ekrandır. Aktarım/öğretici testleri gerçek UI
buton olaylarıyla yürütüldü; fiziksel iPhone dokunma/performans testi yapılmadı.
Geri kontrolü HandleBack API yolunda test edildi; fiziksel Android geri
tuşu ayrıca denenmedi. Tam uygulama işlemi kapatılıp açılmadı; JSON yükleme
ve yeni guide oluşturma ile bu davranış simüle edildi.

MCP status çağrısı Pipeline instance bulamadı; testler gerçek Unity Editor
executeMethod ile sırayla çalıştı. Kullanıcı onayı olmadan CLI güncellenmedi.

## Teslim ve sınırlar

Mevcut iki üretim dosyası BoosterActions.cs (yalnız kayıt alanı/kopyalama/
doğrulama) ve PrototypeApp.cs (rehber bağlantıları) değişti. Yeni dosyalar
TutorialHistory,PawGuide,PawTutorialDefinition ve Editor test araçlarıdır.
PRESERVATION.json diğer dosyalar ve önceki133 untracked dosyanın korumasını
kaydeder. Şişe sprite'ları,fontlar,barlar,karakter prefabı ve sahneler aynı.
DevControl'e dokunulmadı. Master'a merge ve yeni TestFlight/IPA build yok.
Kalan kabul: fiziksel cihazda balonun okunurluğu,dokunma ve performans.
Commit/push mevcut geliştirme dalına yapılır; uzak SHA teslimde doğrulanır.
