# MeWGic · Unity görsel önizleme

9 Kasım 2026 · Unity 6000.3.27f2 · `release/0.8.0-rc.1`

Görsel önizleme eklenmeden önce yerel ve GitHub dalı `3828dc1` commit'inde eşleşiyordu. Var olan yerel TMP ve bağımlılık çözümleyici değişiklikleri korundu. Üretim sahneleri, sprite ve prefab varlıkları bu değişiklikte düzenlenmedi.

## Kullanım

Unity'de **Mewgic → Visual QA → Open Full Game Preview** seç. Önizleme `Home` sahnesinden Play Mode başlatır. **MeWGic Visual QA** penceresi ekran kısayollarını gösterir. Normal oyun açılışı için `Assets/Scenes/Boot.unity` kullanılır.

1. Ana Menü
2. Oyun
3. Bölüm Sonu
4. Harita
5. Mağaza
6. Klan
7. Lig
8. Etkinlikler
9. Günlük Görevler

**Test bölümü** 1–101 aralığında seçim yapar. **1 yıldız**, **2 yıldız**, **3 yıldız** düğmeleri mevcut sonuç animasyonunu yeniden başlatır. Sonuçtaki **Sonraki Bölüm** normal bölüm geçişini çalıştırır. **Kristal Şelaleler · 91–100** sabit harita önizlemesini, **Kedi yürüyüşü · 99 → 100** harita ilerleme animasyonunu açar. **9:16**, **9:19.5** ve **9:20** oran düğmeleri Game View boyutunu değiştirir. Normal kontroller de önizlemenin içinde kullanılabilir.

Önizleme oyun ilerlemesini bellekte tutar. Menü yerelde derlenmiş ClanServer'ı yalnız `127.0.0.1` üzerinde, ayrı geçici SQLite dosyasıyla başlatır. Örnek klan, lig ve etkinlik verileri gerçek yerel API üzerinden üretilir. Reklam örnek sağlayıcı kullanır. Önizleme durdurulunca test profili bırakılır ve yerel yardımcı kapanır. API DLL'i yoksa repo kökünde `dotnet build tools/ClanServer -c Release` çalıştırılır.

## Doğrulama

- 23 Play Mode kontrolü geçti. Dokuz ekran, üç yıldız örneği, örnek reklam ödülü, gerçek raycast ile 2 → 3 geçişi, 1–4 pati öğreticisi, 99 → 100 kedi yürüyüşü ve üç oranda mağaza metni doğrulandı. Oyuncunun kayıt anahtarları aynı kaldı. [Play Mode çıktısı](PLAY_MODE.json)
- 268 sahne, script, prefab ve varlık bağlantısı kontrolü geçti. Eksik bağlantı saptanmadı. [Bağlantı taraması](BINDINGS.json)
- Unity derlemesi geçti. Son görsel Play Mode probu sıfır hata kaydetti. Ayrı Editör bağlantı taramasında önizleme sahneleri açılırken daha önce kaydedilen URP global ışık iletisi görülebilir. Fiziksel cihaz doğrulaması ve görsel revizyon kararı açık.
- [Ana Menü](Home.png) · [1 yıldız](Stars-1.png) · [2 yıldız](Stars-2.png) · [3 yıldız](Stars-3.png) · [Yerel Etkinlikler](Page-7.png) · [Kristal Şelaleler](Page-9.png)

Yeni kod `FullGamePreviewWindow.cs` ve `FullGamePreview.cs` içindeki Editör araçlarıyla sınırlıdır.
