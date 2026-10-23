# Mewgic

Mewgic, Android ve daha sonra iOS için planlanan, fantastik temalı bir mobil sıvı sıralama bulmaca oyunudur. Görsel yön: büyük mavi gözlü gri-beyaz büyücü kedi, mor şapka, iksir şişeleri, uçan adalar ve fantastik kaleler.

## Teknolojiler ve kapsam

- Unity 6.3 LTS (6000.3.27f1), C#.
- Universal 2D şablonu, URP ve 2D Renderer.
- İlk hedef Android; ekran yönü Portrait.
- Android: IL2CPP, ARM64, minimum API 26, hedef API otomatik (kurulu en yüksek sürüm).
- Uygulama kimliği: `com.mewgic.game` (yayın öncesi sahipliği doğrulanacak geçici kimlik).

İlk oynanabilir prototip hazırdır: Home üzerinden Oyna ile tek bölüme girilir. Kaynak ve hedef şişeye dokunarak renkleri sıralayın. Geri Al, Yeniden ve Ana Menü kontrolleri çalışır. Detaylar docs/PROTOTYPE_REPORT.md içindedir.

## Projeyi açma

Unity Hub üzerinden `C:\Dev\Mewgic` klasörünü ekleyin ve **6000.3.27f1** ile açın. Başlangıç sahnesi `Assets/Scenes/Boot.unity` dosyasıdır. Android Build Support, SDK, NDK ve OpenJDK bu Editor sürümüyle eşleşmelidir.

## Klasörler

`Assets/Art`, `Audio`, `Materials`, `Prefabs`, `Scenes`, `Scripts`, `ScriptableObjects`, `UI` proje içeriği için hazırdır. Şablonun mevcut varlıkları korunur. `Assets/Editor/MewgicFoundationSetup.cs` yalnızca temel ayar/doğrulama aracıdır; oyuncu derlemesine dahil edilmez.

## Sahneler

Derlemeye etkin olarak sırayla Boot, Home, Gameplay ve LevelMap eklenir. Her sahnede ortografik kamera ve Global Light 2D bulunur. Boot ilk sahnedir ve Home sahnesini açar. Mevcut akış Boot → Home → Gameplay şeklindedir. LevelMap sonraki aşama için korunur.

## Doğrulama

Kurulum ve test sonuçları `docs/SETUP_REPORT.md` dosyasındadır. APK oluşursa `Builds/Android/Mewgic-Setup-Test.apk` yalnızca temel kurulum testidir; dağıtım sürümü değildir. Fiziksel cihaz davranışı ayrıca sınanmalıdır.

Sonraki işler: [Proje planı](docs/PROJECT_PLAN.md).

GitHub private bağlantısı PART 01.5 aşamasında tamamlandı. DevControl ve oyun içi bulut servis entegrasyonu yapılmamıştır. Mevcut dosyalar silinmez; gereksiz paket eklenmez.

## Prototip kaynakları

- Assets/Scripts/Core/Puzzle.cs: Unity bağımsız model.
- Assets/Scripts/Presentation/PrototypeApp.cs: etkileşimli UI ve sahne akışı.
- Assets/Resources/Levels/Level01.json: bölüm verisi.
- References/Visual: beş görsel referans.
- Assets/Editor/MewgicPrototypeSetup.cs: model testleri, sahne kurulumu ve APK derleme.
- Assets/Editor/MewgicPlaySmoke.cs: otomatik Play Mode akış kontrolü.



## Görsel dönüşüm / PART 01
Çalışan mekanikler korunarak görsel mimari ve üretim planı hazırlandı.
- [Tasarım sistemi](docs/VISUAL_DESIGN_SYSTEM.md)
- [Referans ve fark analizi](docs/REFERENCE_ANALYSIS.md)
- [Asset üretim planı](docs/ASSET_PRODUCTION_PLAN.md)
- [UI uygulama yol haritası](docs/UI_IMPLEMENTATION_ROADMAP.md)
- [PART 01 doğrulama](docs/PART01_VALIDATION.md)

Esas referanslar References/Visual/Approved içindedir. Henüz yeni ekran veya sanat asset'i uygulanmadı.
Yerel Git koruma kaydı oluşturuldu; sonraki PART 01.5 aşamasında GitHub private bağlantısı tamamlandı. DevControl bağlantısı yapılmadı.


## Altyapı — PART 01.5
Private GitHub: https://github.com/edasu-yadik/mewgic-game
Resmî Unity CLI/Pipeline ve Blender Lab MCP bağlantıları gerçek protokol çağrılarıyla doğrulandı.
Kurulum, yeniden başlatma adımları ve sınırlar: [MCP_SETUP.md](docs/MCP_SETUP.md).
DevControl'a müdahale edilmedi; oyun kaynakları ve mevcut görseller korunur.
