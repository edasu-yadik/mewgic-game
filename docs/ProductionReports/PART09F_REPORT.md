# PART09F — BÜYÜ çubukları ve MASALSI TMP tipografi

30 Ekim2026 • Unity6000.3.27f1 • recovery/part09-completion.
Geri dönüş: recovery/part09f-safety-629e7d1. Master değiştirilmedi.

## Tasarım ve fontlar

Onaylanan altı seçenekli görselin yalnız3 BÜYÜ ve5 MASALSI stilleri esas alındı.
Başlıklar Cormorant Garamond650, açıklama/buton/rakamlar Noto Sans550.
Kaynak Google Fonts resmî deposu; iki SIL OFL1.1 lisansı fontlarla birlikte
Assets/Art/Typography altında bulunur. Lisanslar uygulamaya gömme/ticari dağıtıma
izin verir; fontlar tek başına satılmaz. Statik ağırlık örnekleri FontTools ile
oluşturuldu; lisans/copyright metinleri korundu. Yeni fontlar güvenilmeyen bir
font sitesinden alınmadı. Türkçe12 karakter ve0–9 iki TMP SDF asset'inde doğrulandı.

Kaynaklar:
- https://github.com/google/fonts/blob/main/ofl/cormorantgaramond/OFL.txt
- https://github.com/google/fonts/blob/main/ofl/notosans/OFL.txt

MewgicTypographySettings dört düzenlenebilir rol içerir: headingStyle,
descriptionStyle,numericStyle,buttonStyle. Başlık serif; görev açıklaması
sade sans, rakam/butonlar net sans kullanır. TMP auto-size alt sınırları
role göre korunur. Parşömende koyu metin,mor panellerde açık metin kullanıldı.
Eski Text üreticileri gerçek TextMeshProUGUI bileşenlerine dönüştürüldü;
değişken veri alanları TMP_Text oldu. Font değişimi veri modeline taşınmadı.
TMP Essential Resources Unity'nin kurulu ugui paketinden alındı.

## Dinamik bileşen

Assets/Prefabs/UI/MagicalProgressBar.prefab ve Resources/UI/ProgressStyle.asset.
MagicalProgressBar SetValue(current,max,min,ready,isLocked,animate) API'si
minimum/maksimum,gerçek yüzde,sayısal etiket,animasyon,kilit/yüklenmemiş durum
ve tamamlanma tepkisi sağlar. Normalize double hesapla taşmayı engeller;
NaN/sonsuz/ters aralıkta güvenli0 üretir. Örnek3/5=%60.

Altın bevel çerçeve,koyu mor hazne,menekşe dolum ve kristal uçlar kodla
üretilen MaskableGraphic mesh'idir; sabit doluluk sprite'ı değildir.
Dolum alanındaki3 küçük parıltı tek mesh'te hareket eder; tamamlanmada kısa
altın tepki olur. Sayısal etiket altındaki koyu mor alan kontrastı korur.
Sürekli nesne/partikül/materyal üretimi ve özel pahalı shader yoktur.
Ekran kapalıyken bileşen Update'i çalışmaz. İki1024² SDF atlası ortaktır.

Bağlanan11 bar:4 Daily,1 Weekly,1 Season,1 Collection ve4 harita bölgesi.
Koleksiyon ve haritanın önceki sayısal ilerleme alanları aynı veriyi kullanan
bara dönüştü. Harita bilgi kartı110 yüksekliğe çıkarılarak başlık/bar/yıldız
satırı ayrıldı. Daily09D anchor'ları korunur; yan Events09E geri gelmedi.
Mavi buton sprite'ları,para/elmas/enerji ikonları ve Boot barı değişmedi.
Bu öğelerdeki metinler genel TMP tipografi geçişine dahildir.

## Gerçek test sonucu

docs/Part09F/After/UI_AUDIT.json: başarılı,7070 assertion,79 ekran/durum render'ı,
runtime0 Error/Exception/Assert ve0 Warning. Bu7070 ayrı oyun oturumu değildir;
metinler ve Safe Area üzerinde tekrarlanan kontrolleri içerir.
Son derlemede C# hata0 ve uyarı0. CONSOLE_SUMMARY.json kapanış JobTempAlloc
mesajlarını ayrıca sayar; runtime Console sonucu ile karıştırılmaz.

- 0/5,1/5,3/5,5/5,8/5,-3/5,0/0,NaN,yüklenmemiş,kilitli,min10–max20
  ve geniş sayısal aralık test edildi. Yüzdeler sonlu ve sınırlı.
- Animasyon0'dan%60'a ulaştı; mesh vertex karşılaştırması parıltının gerçekten
  hareket ettiğini,child sayısı ise yeni nesne üretilmediğini doğruladı.
- Prefab instantiate testinde tek grafik ve tek TMP etiketi kaldı.
- Gerçek bölüm üç kez çözüldü. Daily4/4,Weekly3/3,Season3/6 ve kazanılan
  koleksiyon1/2 verisi ekranda doğrulandı. Günlük/haftalık/sezon ödülleri alındı.
- Ödül alımı barı sıfırlamadı; test profilinin JSON'u yeniden yüklenince
  alınmış ödüller ve yeniden oluşturulan Weekly3/3 korundu.
- Oyuncunun gerçek PlayerPrefs kaydı değişmedi. Daily ve RewardHub model
  test grupları ayrıca başarılı (MODELS.json).
- 900×1600,900×1950,900×2000 ve son oranda Safe Area iç boşluğu; Home,
  Daily/Gift/uzun açıklama,Weekly,Season,Collection,bilgi/Premium,Settings,
  Gameplay,Pause,Victory ve dört harita bölgesi. Tüm aktif TMP metinlerinde
  taşma/kodlama kontrolü ve sabit buton Safe Area kontrolü geçti.
- GİT→Gameplay→Home,alt Events→merkez→Home,Collection,PLAY,Pause/Resume,
  zafer ve harita navigasyonu gerçek Play Mode'da çalıştırıldı.

Unity MCP status çağrısı Pipeline instance bulamadı. Yukarıdaki testler gerçek
Unity Editor'de executeMethod ile çalıştı; MCP'den geçmiş gibi sunulmaz.
Fiziksel iPhone/Metal FPS,pil ve cihaz ekranı kabulü yapılmadı.

## Görseller ve kalan farklar

Before klasörü,başlangıç commit629e7d1 ile değişmemiş olan09E sonrası gerçek
Unity kamera render'larının kopyasıdır (BASELINE.json). After güncel Unity
render'larıdır. Daily-GameView.png doğrudan ScreenCapture ile900×1600
Game View'dan kaydedildi. Compare-Daily/Weekly/Season/Collection/Map.png
dosyaları bu gerçek render'ların yan yana karşılaştırmasıdır.
WeeklyReady,DailyClaimed,SeasonProgress,CollectionProgress dolu gerçek veri
durumlarını gösterir. Progress60,bağımsız bileşenin test sunumudur.

Referansın ana sanat düzeni/04_Events varlıkları yeniden çizilmedi.
Barlar referans3'ün mor-kristal/altın dilini izler; raster boyalı dokular yerine
hafif mesh gradient/bevel kullanır. Sayı küçük ekran okunurluğu için haznenin
içindeki koyu alandadır; örnekteki gibi ayrı alt etiket değildir. Başlıklar
referans5'e yakın serif hissindedir; görselden birebir font kimliği çıkarımı
yapılmadı. Görev açıklamaları bilinçli olarak sade Noto Sans'tır.
Doğrulanan oranlarda kalan kritik hizalama/taşma saptanmadı; cihaz kabulü açık.

## Koruma ve Git

PRESERVATION.json başlangıç SHA'larını ve değişen dosyaları listeler.
Önceki133 untracked dosyanın hash'i aynı. Core ilerleme/ödül/kayıt kodları,
sanat PNG'leri,karakter prefabı,Home/Gameplay/Boot sahneleri ve iOS ayarları
korundu.22 mevcut dosya değişti:10 üretim UI script'i,10 eski testin TMP
erişimleri,2 önceki audit'in TMP ölçümü. Eski UI testlerinin tip erişimleri
güncellendi; her eski UI suite'i ayrıca tekrar çalıştırılmış değildir.
Yeni bar/tema script'leri,prefab,SDF fontlar,lisanslar,TMP bağımlılıkları,
09F Editor/probe araçları ve seçilmiş kanıtlar bu commit kapsamındadır.

Yalnız recovery/part09-completion dalına commit/push yapılır; uzak SHA
teslimde doğrulanır. Master'a merge yapılmaz. Cloud Build,IPA,sertifika ve
App Store Connect işlemi yoktur. Sonraki beta cihaz kabulü ayrı görevdir.
