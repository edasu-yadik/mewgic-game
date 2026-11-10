# Şişe tamamlanma efekti

Eski efekt büyüyen düşey bir oval ve seyrek yıldızlar kullanıyordu. Yeni efekt şişe tabanında kısa bir mühür halkasıyla başlar. Yaklaşık üç tur atan altın spiral yukarı doğru açılır. Yoğun kıvılcımlar aynı yolu izler. Ön taraftaki iz daha parlak, arka tarafı daha soluktur. Ağızdan kıvrımlı bir ışık izi ve küçük bir parıltı yükselir. Efekt **1,05 saniye** içinde söner. Az hareket tercihinde 0,35 saniyelik halka ve iki kıvılcım gösterilir.

Döküş bittiğinde mevcut `GameplayBottleView.Present` tetikleyicisi şişenin dört katmanının aynı renk olduğunu denetler. Efekt şişe başına ayrı sayaç ve tek bir UI mesh'i kullanır. Dolu durumun tekrar sunulması efekti başlatmaz. Geri alıp gerçekten yeniden tamamlamak yeni bir tetikleyicidir. Altın ana renk, kuşanılan efektin rengi bazı kıvılcımlarda küçük bir vurgu olarak kullanılır. Şişenin kendi camı ve sıvı rengi mevcut oyun görünümünden gelir.

## Unity doğrulaması

Unity 6000.3.27f2 C# derlemesi ve izole **31 Play Mode kontrolü** geçti. Gerçek Gameplay sahnesine, birbirini takip eden iki tamamlamayı göstermek için bir test tahtası yerleştirildi. Gerçek düğme raycast ve pointer olaylarıyla sarı, ardından pembe şişe tamamlandı. Geçici dolum, yarım şişe, karışık dolu şişe ve sahne başında zaten dolu şişe tetiklemedi. Aynı karede iki tamamlanma, duraklatma, az hareket, bir kez oynatma ve üç portre oranı ayrıca denetlendi. Oyuncu kaydı byte düzeyinde korundu. Fiziksel cihaz testi ve mobil build yapılmadı.

- [Gerçek ilk döküş](01-Sari-Sise.jpg) ve [gerçek ikinci döküş](02-Pembe-Sise.jpg)
- [Aynı karede bağımsız efektler](03-Ayni-Kare.jpg) ve [efekt bittikten sonra](04-Efekt-Sonrasi.jpg)
- [9:20 portre](05-9x20.jpg) ve [Play Mode kaydı](PLAY_MODE.json)

## Değişen dosyalar

- `Assets/Scripts/Presentation/BottleCompletionGraphic.cs`
- `Assets/Scripts/Tests/GameplayPolishProbe.cs`
- `docs/PRODUCTION_PROGRESS.md` ve bu raporun Game View kanıtları
