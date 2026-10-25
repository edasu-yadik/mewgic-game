# Mewgic — Proje Planı

## Mevcut aşama: ilk oynanabilir prototip
Tek proje: C:\Dev\Mewgic, Unity 6.3 LTS (6000.3.27f1), Universal 2D URP / C#.
Android Portrait, IL2CPP ARM64.

## Uygulananlar
- Beş görsel References/Visual altında korundu; ART_DIRECTION.md hazır.
- Unity'den bağımsız Puzzle modeli: dört katman kapasite, üst renk grubu aktarımı,
  geçersiz hamle denetimi, hamle sayacı, geri alma, yeniden başlatma ve kazanma.
- Resources/Levels/Level01.json: ayrı bölüm verisi; katmanlar alttan üste.
- Boot → Home → Gameplay; Play, Undo, Restart, Home ve kazanma/tekrar oynama.
- Fare ve dokunmatik için Input System UI; animasyon boyunca hamle kilidi.
- Canvas ölçeklemesi ve değişen Screen.safeArea uyarlaması.
- LevelMap mevcut haliyle korunur; bu aşamada akışa dahil edilmez.

## Doğrulama
Model testleri ve çözüm: MODEL_TEST_RESULTS.md.
Play Mode ve Android sonuçları: PROTOTYPE_REPORT.md.
Temel kurulum geçmişi SETUP_REPORT.md içinde korunur.

## Sonraki aşama
1. Gerçek cihazda dokunma, farklı ekran oranları/güvenli alan ve performans testi.
2. Ayrı kedi, cam şişe, sıvı maskesi, arka plan ve buton varlıklarıyla görsel iyileştirme.
3. İlk 10 bölüm, zorluk eğrisi, bölüm seçimi ve ilerleme kaydı.
4. LevelMap akışı ve ses/efektler.
5. Yayın kimliği, ikon ve imzalama; sonraki dönemde iOS.

Mağaza, reklam, satın alma, can sistemi, etkinlik, takım, envanter, sezon bileti,
çevrimiçi servis, GitHub ve DevControl kapsam dışındadır; bu özellikler uygulanmadı.

## Çalışma kuralları
Mevcut dosyalar korunur; ikinci proje oluşturulmaz.
Paketler somut ihtiyaç olmadan eklenmez. Yapılmayan test yapılmış gibi raporlanmaz.

## Görsel mimari hazırlığı — PART 01
Görsel analiz, tasarım standartları, üretim envanteri ve aşamalı UI geçiş planı hazırlandı.
Belgeler: VISUAL_DESIGN_SYSTEM.md, REFERENCE_ANALYSIS.md, ASSET_PRODUCTION_PLAN.md,
UI_IMPLEMENTATION_ROADMAP.md. Esas beş referans References/Visual/Approved içindedir.
Bu çalışma yeni görsel kalite uygulaması değildir; prototip kodu ve sahneleri korunur.
Yerel Git başlangıç kaydı vardır; uzak depo ve DevControl bağlantısı yoktur.

## PART 02B tamamlanan işler
Unity sağlık denetimi, 12 model kontrolü ve oyun Play Mode akışı geçti.
WizardCat prefab, Idle/Blink/Wand Magic ve ayrı WizardCatAnimationTest sahnesi hazır;
41 animasyon/UI kontrolü başarılı. Ayrı Android karakter test APK'sı alındı.
Sonraki iş PART 02C: karakteri Home'a entegre etmek. Yeni bölüm geçişi, tam vücut rig'i
ve gerçek cihaz performans testi hâlâ yapılacak işlerdir. Rapor: PART02B_REPORT.md.

## PART 02C tamamlanan işler
Mevcut WizardCat prefabı yalnızca Home'a entegre edildi; düzenli altı görsel katman,
güvenli alan yerleşimi ve Home'a özel periyodik sihir davranışı hazır.
49 Play Mode kontrolü başarılı; üç gerçek Game View çözünürlüğü ve ek safe area
simülasyonu doğrulandı. Console 0 hata / 0 uyarı. Gerçek Home PNG'leri ve kapsam
sınırları PART02C_REPORT.md içinde. Sonraki aşama: ayrı kapsamda Home arka planı
ve UI görsel kalitesi; fiziksel mobil cihaz testi bekliyor.

## PART 03A tamamlanan işler
Home için altı ayrı çevre PNG'si, bağımsız katman entegrasyonu, hafif şelale/pencere
shader hareketi, kayan sis ve 16 ortam ışığı hazır. Mevcut karakter ve UI korunur.
49 menü/karakter kontrolü ve üç gerçek dikey çözünürlükte 34'er çevre kontrolü geçti.
Console 0 hata / 0 uyarı. Rapor ve görsel farklar PART03A_REPORT.md içinde;
yan yana karşılaştırma PART03A_COMPARISON.html. Mobil cihaz profillemesi ve nihai
UI okunabilirliği sonraki aşamalar olarak kalır; yeni UI tasarımı yapılmadı.

## PART 03B — Şişe varlıkları hazır
Blender MCP ile üç ayrı kalın cidarlı iksir modeli ve beş bağımsız RGBA PNG
üretildi: Left, Center, Right, Glow, MagicParticles. Kaynak .blend dosyaları
SourceArt/Blender/Potions/Home; sprite'lar Assets/Art/Potions/Home içindedir.
Unity import/alpha/pivot kontrolleri geçti; Home sahnesi değiştirilmedi.
Referans farkları ve efektlerin ön/arka maske ihtiyacı PART03B_REPORT.md içinde.
Sonraki adım: görsel değerlendirme ardından ayrı kapsamda Home'a yerleştirme.

## PART 03B.1 — Yalnız orta şişe v002
Orta şişenin profili, ince camı, yarı saydam katmanları, ışıkları ve ince altın
spirali geliştirildi. v002 Blender/PNG ve ayrı ön/arka efekt katmanları hazır;
v001 ve diğer şişeler SHA-256 ile değişmeden korundu. Unity import/alpha/pivot
kontrolleri geçti; Home'a ekleme yapılmadı. Referansa kalan sanat farkları ve
karşılaştırma PART03B1_REPORT.md / PART03B1_COMPARISON.html içinde kayıtlı.

## PART 03B.2 — Aynı modelde cam/ışık/alpha düzeltmesi
V002'nin 330 özgün mesh'i korunarak v003 üretildi. Üst cam ışığını silen compositor
alpha çarpımı düzeltildi; iç ışık, kenar vurguları, sıvı ve altın aura geliştirildi.
Şeffaf gövde ve ayrı ön/arka efekt PNG'leri hazır. Gerçek Unity RGBA açık/koyu
karşılaştırmaları, import ve dosya koruma kontrolleri geçti; son Console 0/0.
Home'a yerleştirme yapılmadı. Kalan referans farkları PART03B2_REPORT.md içinde.

## PART 03B.3 — Üretim yöntemi 2D'ye geçti
Onaylı Home referansından yerleşik görsel düzenleme ile Potion_Center_Art_v001
üretildi; dış büyüsüz taban ve Glow/Sparkles/MagicSpiral sprite'ları hazır.
Beş varlık aynı canvas/pivot ile Unity'ye aktarıldı ve gerçek RGBA önizlemesinde
kontrol edildi. Önceki Blender kaynakları korunur; Home hâlâ değiştirilmedi.
Üretim onarımı, referans farkları ve katmanların eşleşme sınırları PART03B3_REPORT.md
içinde kayıtlı. Sonraki yerleşim için ana görsel hazır; efekt animasyonları yapılmadı.

## PART 03B.4 — Sol ve sağ 2D şişeler
Sol/sağ Art_v001 ve ayrı Glow/Sparkles PNG'leri üretildi; küçük ışık parçacıkları
Sparkles içinde bulunur. Altı sprite'ın import/alpha kontrolleri geçti.
Gerçek Unity açık/koyu karşılaştırması hazır. Home, orta şişe, karakter ve oyun
kodları korunur. Kontur katmanları üretimsel olduğundan kusursuz piksel ayrıştırma
değildir. Sonraki aşama: ayrı görevde Home yerleşimi. PART03B4_REPORT.md.

## PART 03C — Onaylı şişelerin Home entegrasyonu
HomePotionDisplay ile üç onaylı sprite ve hafif hareket/ışıltılar bağlandı.
Eski Home prototip paneli kaldırıldı; mevcut platform kullanıldı.99 entegrasyon,
12 model ve gerçek Gameplay smoke kontrolleri geçti.900×1600 Game View ve
üç dikey oran/safe-area görselleri kaydedildi. Karakter/oyun mekaniği korunur.
Kalan referans/UI farkları PART03C_REPORT.md içindedir.

## PART 04A — Ana menü logosu
Master kapaktan kontrollü görsel düzenlemeyle1720×914 RGBA logo hazırlandı;
Home'daki düz başlık yerine bağlandı.141 entegrasyon kontrolü geçti; gerçek
Game View ve3 oran/safe-area görüntüleri hazır. Console0/0,önceki114 dosya
korundu. Ayrı glow/sparkles gereksiz görülerek üretilmedi. Referanstan küçük
üretim farkları PART04A_REPORT.md içinde. Diğer UI tasarımı bu aşamada değişmedi.

## PART 04B — Premium PLAY
Normal/Pressed/Glow RGBA sprite'ları Home'a bağlandı. Idle ışık,basılma/kısa
release ve tek geçiş kilidi hazır.164 kontrol,3 oran/safe-area ve gerçek Game
View görüntüleri tamamlandı;Console0/0. Logo,kedi,şişeler ve oynanış korunur.
Referans farkları ve alpha/efekt sınırları PART04B_REPORT.md içinde kayıtlı.

## PART 04C — Üst kaynak çubuğu
Altı ayrı premium RGBA varlık ve HomeTopBar tamamlandı. Dinamik önizleme
modeli,üç artı bilgi paneli ve aç/kapa ayarlar hazır. Gerçek bakiye/kayıt ve
Gameplay enerji tüketimi eklenmedi.236 kontrol,3 oran/safe-area ve gerçek
Game View görüntüleri hazır. Mobil import panel1024/ikon512 ASTC4x4.
Kalan referans farkları ve test kapsamı PART04C_REPORT.md içinde.

## PART04E — Alt navigasyon
Beş ayrı ikon,aktif Home altın yüzeyi ve güvenli geçici sekme panelleri hazır.
LevelMap yalnız kamera/ışık içerdiğinden Map gerçek harita gibi sunulmadı.
380 entegrasyon,12 model ve mevcut smoke geçti.3 oran/safe-area ve gerçek
Game View görüntüleri hazır. ProductionReports/PART04E.md son rapordur.

## PART04F — Home kalite denetimi
Küçük efekt/kontrast/dokunma iyileştirmeleri doğrulandı.390 entegrasyon,12 model
ve smoke başarılı;gerçek Game View/referans tam ekran karşılaştırması kaydedildi.
Home'a final sanat onayı verilmedi:04D yan menüleri eksik ve raporlanan kompozisyon
farkları sürüyor. Son rapor ProductionReports/PART04F.md.

## PART05A — Gameplay görsel altyapısı
Çevre,taş raflar,kedi ve1–24 şişe slot altyapısı hazır. Mevcut4 şişelik bölüm,
dökme kilidi,Undo/Restart/Win/Replay/geçişler test edildi.2755 karma kontrol,
12 model ve smoke başarılı. Home ve mevcut kaynaklar korunur. Şişe ve kontrol
sanatının yenilenmesi bu partın kapsamı dışıdır. ProductionReports/PART05A.md.

## PART05B — Dinamik cam şişeler
Gameplay cam/sıvı ayrımı ve kilitli aktarım animasyonu hazır;3183 karma kontrol,
12 model ve mevcut smoke başarılı.Home/model/veri korunur.Sıvı görsel cilası
referansın tüm ayrıntılarını henüz karşılamaz;cihaz testi yapılmadı.
ProductionReports/PART05B.md.

## PART05C — Gameplay bilgi panelleri
Level metadata,model hamle/hedef sayacı,performans yıldızları ve Pause/Resume/
Restart/Home hazır.336 HUD+3183 şişe+12 model kontrolü ve smoke geçti.
Home/şişe/çevre sanatı aynı;3 oran/Safe Area ve Console0/0 doğrulandı.
ProductionReports/PART05C.md.

## PART05D — Araçlar ve bölüm sonu
Undo/Hint/Extra/Shuffle canlı hak sayaçları ve yerel kayıtla çalışıyor.
İlk kazanma +1 Hint ödülü yalnız bir kez verilir.3/4 hamlede doğrulanmış iki bölüm
arasında Next ve Home akışı hazır.723+4 yeni,336 HUD,3183 şişe,12 model kontrolü
ve smoke geçti.Console0/0 ve3 oran/Safe Area doğrulandı. ProductionReports/PART05D.md.

## PART06A — Harita çevresi ve UI altyapısı
LevelMap dört bölge, ayrı zemin/konum/sandık/yol/navigasyon ve mobil dikey
kaydırma ile hazır. İlerleme sonraki parta bırakıldı; Home/Gameplay aynı.
556 kontrol+12 model+smoke,3 oran/Safe Area geçti; Console0/0.
Referansa kalan kompozisyon/yol farkları ve cihaz testi sınırı raporda:
ProductionReports/PART06A.md. Birebir final sanat eşleşmesi değildir.

## PART06B — Harita ilerleme bağlantısı
İki mevcut doğrulanmış bölüm, sırayla kilit açma, tekrar oynama, en iyi yıldız,
bölge tamamlama ve tek seferlik+1 İpucu sandığı hazır. Eksik gelecek seviyeler
oyuncuya açılmaz. Harita→Gameplay→Harita/Home Map bağlantısı test edildi.
200+551+723 ve12 model kontrolü;3 oran/Safe Area başarılı. Yeni içerik üretimi
bu parta eklenmedi. ProductionReports/PART06B.md / MAP_PROGRESSION.md.

## PART07A — Günlük görev/hediye
Gerçek kazanma ve hak kullanımından ilerleyen dört görev; kalıcı tek ödül,
20 saat+yeni yerel gün hediyesi ve makul geri saat kontrolü hazır. Mevcut kayıt
korundu; Home girişleri bağlandı.219+64+200+723 ve12 model/smoke geçti.
Üç oran/Safe Area veConsole0/0 doğrulandı;cihaz testi yapılmadı.
ProductionReports/PART07A.md / DAILY_ACTIVITIES.md. Sonraki part başlatılmadı.

## PART07B — Etkinlik ve koleksiyon
Yerel pazartesi haftası(3 kazanma),28 günlük ücretsiz sezon(2/4/6 kazanma),
tek ödül ve iki mevcut görsel kartının kalıcı galerisi hazır. Premium bilgi
katmanıdır. Daily Missions korunur;Home Events/Collection açılır.
603 merkez,219 günlük,723 güçlendirici ve12 model kontrolü geçti.
Üç oran/Safe Area ve gerçek görüntüler kaydedildi.Cihaz testi yapılmadı.
ProductionReports/PART07B.md / REWARD_CENTER.md. Sonraki part başlatılmadı.
