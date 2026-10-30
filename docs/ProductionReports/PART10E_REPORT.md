# PART10E — Şişe boyutu ve yerleşim optimizasyonu

30 Ekim2026 • Unity6000.3.27f1 • recovery/part09-completion.

## Alan ve yerleşim

Üst HUD76–100 birim;board Safe Area X0,25–0,95,Y0,30–0,90.
Alt güçlendiriciler0,075–0,195,Pati balonu0,201–0,286;kedi soldaki ayrı
şeritte korunur.900×1600 tam alanda board630×960 birimdir. Safe Area ve
oran değişince boyutları yeniden ölçülür. Board alanı bu partta büyütülmedi.

Eski5–8 şişe→4 sütun kuralı yerine tüm sütun adayları değerlendirilir;
en büyük güvenli şişe yüksekliği seçilir. Son eksik satır ortalanır.
Cam oranı0,42 korunur. Hücre genişliğinin en fazla%82'si kullanılır;
üst28,alt44 birim numara/seçim payı bırakılır. Şişe numarası32 yüksekliğinde,
camın6 birim altında sabit bir bant kullanır; büyüyen cama bağlı taşmaz.
Raf havuzu oran değişiminde gereken satır sayısına göre güncellenir.

630×960 referans board ölçümü (cam rig/dokunma kök kutusu):

| Şişe | Sütun önce→sonra | Boyut önce→sonra | Yükseklik farkı |
|---|---|---|---|
|2|2→2|220,5×525→258,3×615|+%17,1|
|4|2→2|161,3×384→171,4×408|+%6,25|
|5/6|4→3|110,3×262,5→171,4×408|+%55,4|
|8|4→4|110,3×262,5→129,2×307,5|+%17,1|
|12|4→4|107,5×256→104,2×248|−%3,1|

Yoğun12 şişede zorla büyütme yapılmadı:üst boşluğu6,4 yerine28 birime
çıkarıp seçim/pati payını korumak daha güvenlidir. Rakamlar sprite'ın şeffaf
kenarını hariç tutan optik ölçüm değil,Rig RectTransform ölçümüdür.
Orana/Safe Area'ya göre değişir; MEASUREMENTS.json hesapları içerir.

## Animasyon ve dokunma

Puzzle,PuzzleSolver,aktarım/Undo kuralları,cam/sıvı sprite veya liquid mesh
kodları değiştirilmedi. Animasyonda IsAnimating yerleşim kilidi korunur;
dökme pivotu ve Mouth hesabı mevcut rig ölçüsünü kullanır. Pati vurgusu artık
buton kökü yerine hareketli Rig sınırlarını izler. Büyüyen/yükselen camla
hizalanır. Buton kök kutuları ayrık;komşu şişeye taşan ek raycast padding yok.

## Gerçek testler

BottleLayoutProbe:3972 kontrol başarılı,runtime hata0/uyarı0,gerçek oyuncu
PlayerPrefs belgesi aynı. C# derleme başarılı. Kontroller:

- 2,3,4,5,6,8,12,20,32 şişe üç board boyutunda matematiksel sınır/ayrıklık
  ve44 birim minimum kutu kontrolü. Bu,32 şişeli bir bölüm yayınlandığı veya
  tüm yoğunlukların fiziksel cihazda rahat bulunduğu anlamına gelmez.
- Gerçek birinci bölüm/öğretici ve gerçek5 şişeli ikinci bölüm;ayrıca2,6,12
  şişeli geçici görsel fixture'lar900×1600/1950/2000 (Safe Area) render edildi.
  Fixture'lar yeni bölüm/oyun mekaniği değildir;model/kayıt değiştirmez.
- Cam/dokunma kutuları birbirinden ve kedi/HUD/alt araçlardan ayrık;
  gerçek raycast her görselin merkezinde doğru şişeyi buldu.
- Seçilen şişe yükseldi;geçersiz hamle sayılmadı;gerçek aktarımda eğilme,
  sıvı aktarımı ve dönüş görüldü. Hedefin konumu/ölçüsü tüm gözlemde sabit.
- Aktarım sırasında Pause poz/rotasyonu dondurdu;Resume devam ettirdi.
  Undo/Restart ve gerçek zafer→ikinci bölüm akışı başarılı.

İlk test raf sayısının responsive sütun değişiminde güncellenmemesini yakaladı;
Layout içinde raf havuzu eşitlendi. Yeni fixture'ın raycast ölçümü ilk render
sonrasına alındı. Son tur tamamen geçti. Önceki Editor kapanış JobTempAlloc
mesajları runtime hata/uyarı listesine dahil değildir.
MCP status Pipeline instance bulamadı;gerçek Unity Editor executeMethod ile
sırayla çalıştırıldı. Fiziksel iPhone FPS/dokunma kabulü yapılmadı.

## Görseller ve büyütme sınırı

Before görüntüleri değişmemiş PART10D çıktılarıdır. Compare-Four.png ve
Compare-Five.png solda önce,sağda sonra gerçek Unity render karşılaştırması.
After-GameView.png doğrudan ScreenCapture'dır. FiveBottles,Fixture veTutorial
görüntüleri docs/Part10E altında. Sayıların anlamı fixture/canlı bölüm olarak
dosya adında ayrılır.

Daha fazla büyütmek yatay cam aralığını,seçim/pati payını veya numara alanını
azaltır;çok şişede raflar/numaralar çakışmaya başlar. Bu yüzden tüm sayılarda
aynı büyüme zorlanmadı.12 üzerindeki yoğunluklar için cihaz ergonomisi ayrıca
değerlendirilmelidir. Animasyon sırasında ekranın fiziksel olarak yeniden
boyutlandırılması ayrıca test edilmedi;testler kararlı portrait oranlarındadır.

Üretim değişiklikleri GameplayPresentation,PrototypeApp (numara alanı),
PawGuide (Rig vurgusu) ile sınırlı. Kayıt/ödül/oyun mekanikleri,fontlar,
HUD tasarımı ve sanat asset'leri korundu;PRESERVATION.json hash kaydıdır.
Master merge ve yeni iOS/TestFlight/Cloud Build yok. Mevcut geliştirme dalına
commit/push yapılır;uzak SHA teslimde doğrulanır.
