# PART 16B · Gameplay ve pati öğreticisi

4 Kasım 2026 · Unity 6000.3.27f1 · Başlangıç `f7d076c`

## Oynanış

- Şişe dokunuşu basıldığı işaretçiyi ve konumu izliyor. Sürükleme, hedef dışında bırakma, aynı karedeki ikinci temas ve çok hızlı aynı şişe tekrarı seçim üretmiyor. Döküş boyunca yeni şişe girişleri kapanıyor. Klavye ve normal UI düğmeleri Unity’nin mevcut seçme yolunu kullanıyor.
- Geçersiz hamlede kaynak seçili kalıyor. Dolu şişe ve uyuşmayan renk için farklı kısa açıklamalar ve hedefte renk uyarısı var. Başarılı her döküşten sonra tekrarlanan durum yazısı kaldırıldı.
- Arka plan olayında uygulanmış döküşün animasyonu toparlanıyor, şişe görselleri modelle eşitleniyor ve duraklatma açılıyor. Hamle kazanışı tamamladıysa, manuel duraklatma açık olsa da mevcut atomik kayıt üzerinden bir kez kaydediliyor. Devam etmek ödülü yinelemiyor.
- Geri Al, İpucu, Ek Şişe ve Karıştır mevcut kuralları ve stok tüketimini kullanıyor. İlk başarılı kullanımda ilgili düğmeye küçük bir pati animasyonu ve kısa açıklama geliyor. Görülme bilgisi mevcut öğretici geçmişinde ayrı kimliklerle tutuluyor.
- Şişe alanı genişletildi ve kedi kendi sol alanına sığdırıldı. Normal oyun HUD’ında yalnız bölüm, hamle ve duraklatma bilgisi var. Eski zorunlu metin kutuları ve 3 yıldız hedef kutusu görünmüyor. Koşullu özel hedef desteği korunuyor.

## Pati ve Ayarlar

İlk bölümde pati doğru kaynağı seçiyor, doğru hedefe bir gerçek döküş yapıyor ve ardından kediye dokunarak küçük balonu açıyor. Balon yaklaşık üç saniye sonra kapanıyor ve X ile erken kapatılabiliyor. Gösteri sırasında oyuncu dokunuşları ve güçlendiriciler döküşe karışmıyor. Tamamlama eski `paw.basic.v1` kimliğini kullanıyor ve bitiren oyuncuda zorunlu olarak tekrarlanmıyor.

Ana menü Ayarlar’daki **Pati Rehberini Tekrar Oynat**, birinci bölümün geçici bir kopyasını açıyor. Ödül ve ilerleme yazımı bu oturumda devre dışı. Balon kapanınca ana menüye dönülüyor.

## Unity Play Mode

259 kontrol geçti. [Makine okunur sonuç](UNITY_TESTS.json). Unity Input System içine bir Touchscreen eklenerek Began ve Ended olayları gönderildi. Şişe, menü ve sonuç düğmeleri Game View raycast’inden geçti. Aynı karede çift temas, animasyonda hızlı tekrar, dolu ve renk uyumsuz hedef, duraklatma ve arka plan, son hamle ödülü, dört güçlendirici, yeniden deneme, haritaya dönüş ve Ayarlar’dan tekrar sınandı.

900×1600, 900×1950 ve 900×2000 Game View oranlarında çentik payı simüle edildi. Şişeler arasındaki çakışma, 44 px minimum dokunma alanı ve gerçek raycast kontrol edildi. Ayrı yerleşim denetimi 2–12 şişe örneklerini kapsadı. Oyuncu ana kaydı, yedek ve bozuk kayıt alanları aynı kaldı. Arka plan denetimi Editor içinde yaşam döngüsü çağrısıyla yapıldı. Fiziksel cihazda işletim sistemi arka plan davranışı, dokunma gecikmesi ve performans ölçülmedi.

## Game View

[Pati kaynak](Pati_Kaynak.png) · [Pati hedef](Pati_Hedef.png) · [Kedi balonu](Pati_Balon.png) · [Geçersiz hamle](Gecersiz_Hamle.png) · [Ek Şişe](Ek_Sise.png) · [9:16](Oynanis_1600.png) · [9:19.5](Oynanis_1950.png) · [9:20](Oynanis_2000.png) · [Bölüm sonu](Bolum_Sonu.png) · [Ayarlar](Ayarlar_Tekrar.png)

`PrototypeApp.cs`, `BottleTapButton.cs`, şişe sunumu ve araçları giriş/animasyon akışını yönetiyor. `PawGuide.cs` ve `HomeTopBar.cs` gösteri, balon ve tekrar düğmesini taşıyor. Sekiz dilde yedi yeni metin eklendi. Test `Part16BGameplayProbe.cs` ile yeniden çalıştırılabilir. Sahne, kayıt şeması, bölüm sıralaması, üretici ve bulmaca kuralları değişmedi.

Git teslimi, bu rapor ve kanıtlarla birlikte recovery/part09-completion dalında yapıldı.
