# Mewgic — GitHub ve MCP kurulumu / PART 01.5

23 Ekim 2026 (Europe/Kiev). GitHub, Unity MCP ve Blender MCP canlı bağlantı testleri başarılı.
Ham protokol sonuçları: [MCP_TEST_RESULTS.json](MCP_TEST_RESULTS.json).

## GitHub

- Gerçek Unity ve Git kökü: `C:/Dev/Mewgic`. İkinci Unity projesi veya iç içe depo oluşturulmadı.
- Hesap `edasu-yadik`; mevcut GitHub CLI keyring oturumu doğrulandı. Token kaydedilmedi.
- Repository önceden bulunmadı; **PRIVATE** oluşturuldu: https://github.com/edasu-yadik/mewgic-game
- Origin: `https://github.com/edasu-yadik/mewgic-game.git`.
- `master` dalı ve `9ebcae0 → 49d2cc5` geçmişi korundu; force push yok. İlk push sonrası yerel ve uzak HEAD aynıydı.
- GitHub Desktop'ın resmî yerel CLI'siyle klasör açıldı. Desktop günlüğündeki `adding repository at C:\Dev\Mewgic to store` kaydı tanındığını doğruladı.
- Library, Temp, Builds, Logs, UserSettings, .utmp; .env, imzalama anahtarları ve pipeline bağlantı dosyaları .gitignore ile dışarıda tutulur.
- Tracked dosya adlarında gizli anahtar/imza dosyaları, bilinen token desenleri ve 10 MB üzeri tek dosya kontrolü temizdi. Bu kontrol bütün olası sır biçimlerini bulma garantisi değildir.
- Bu teslimin paket ve dokümanları da aynı private remote'a gönderilir. DevControl'a müdahale edilmedi.

## Unity seçimi ve sürümler

- Editor: **6000.3.27f1 / Unity 6.3 LTS**; değiştirilmedi.
- Resmî çözüm: **Unity CLI + Unity Pipeline**, Codex'e STDIO MCP.
- CLI: `C:/Users/mehme/AppData/Local/Unity/bin/unity.exe`, **1.0.0-beta.14**. Kullanıcı dizinindeki beta.13 resmî self-update ile yükseltildi; Hub içindeki beta.13 değiştirilmedi.
- Paket: **com.unity.pipeline 0.9.1-exp.1**, `https://packages.unity.com` kayıt kaynağı. Manifest/lock dosyaları sürümü sabitler; UPM gerekli bağımlılıkları çözdü.
- Güncel resmî belgede Assistant paketinin eski MCP sunucusu deprecated; yerine `unity mcp` ve Pipeline öneriliyor. Unity 6.0+ ve localhost desteği var; Unity AI aboneliği gerekmiyor.
- CLI beta ve paket experimental sürümdür; kararlı etiketli ürün olarak raporlanmaz. Mevcut Editor üzerinde derlendi ve gerçek çağrılarla doğrulandı.

**Coplay alternatifi:** `CoplayDev/unity-mcp` v10.0.0 incelendi; commit `d49ae2953580f3481beb1e084a1da2682f0b5610`. Belgeleri Unity 2021.3 LTS–6.x ve Python 3.10+ desteği belirtiyor. MIT lisanslı topluluk projesi; Unity Technologies ürünü değil. Resmî çözüm çalıştığı için Coplay Unity paketini kurmadık; aynı projede iki kontrol köprüsü açılmadı.

## Unity Codex kaydı

Kullanıcı yapılandırması: `C:/Users/mehme/.codex/config.toml`.

```toml
[mcp_servers.mewgic-unity]
command = 'C:\Users\mehme\AppData\Local\Unity\bin\unity.exe'
args = ['mcp', '--project-path', 'C:\Dev\Mewgic', '--tools', 'eval,list_open,get_hierarchy']

[mcp_servers.mewgic-unity.env]
UNITY_NO_CLI_INVOKED_TELEMETRY = '1'
```

CLI'ın geniş araç listesi farklı gruplarda aynı create/get/status adlarını döndürüyor. Ad çakışmasını önlemek için resmî `--tools` filtresiyle yalnız benzersiz üç giriş açıldı. Ayrı proxy veya özel sunucu yazılmadı. `eval` C# çalıştırabilir; salt okunur bir güvenlik sınırı değildir. Bu aşamada yalnız okuma kodu kullanıldı. Gerekli geliştirme işlemleri ileride kontrollü eval ile yapılabilir. Console sayacı çakışan get aracı yerine salt okunur reflection ile okundu. CLI bu adlandırmayı düzelttiğinde filtre genişletilebilir.

### Unity gerçek MCP testleri

Python MCP SDK istemcisi Codex kaydındaki komutu başlattı; initialize ve tools/list sonrası gerçek tools/call çağrıları yaptı:

- `eval`: proje **Mewgic**, Assets yolu `C:/Dev/Mewgic/Assets`.
- Build Settings: Boot, Home, Gameplay, LevelMap ve devre dışı SampleScene okundu.
- `list_open`: Boot aktif/yüklü; **isDirty=false**.
- `get_hierarchy`: Main Camera, Global Light 2D, Mewgic Prototype ve bileşenleri okundu.
- `eval`: Console sayacı kontrol anında **0 hata, 0 uyarı**. Geçmiş logların tamamının hatasız olduğu iddia edilmez.
- Canlı Pipeline API `http://127.0.0.1:7800`; Windows dinleyicisi loopback olarak doğrulandı. Port ileride değişebilir; CLI otomatik keşif kullanır.
- Testte sahne açılmadı/kaydedilmedi, GameObject oluşturulmadı ve Play Mode'a girilmedi. Oyun kaynakları, sahneler, bölüm verisi ve görseller Git karşılaştırmasıyla değişmemiş bulundu.

## Blender seçimi ve kaynak incelemesi

- Mevcut **Blender 5.2.3 LTS** kullanıldı; yeni Blender sürümü kurulmadı.
- Uygulama: `C:/Program Files/Blender Foundation/Blender 5.2/blender.exe`.
- Seçim: **Blender Lab resmî blender_mcp v1.0.3**; https://projects.blender.org/lab/blender_mcp
- Sabit tag commit: `2cea8d566dde07fbac28a61d698909d69724e853`.
- Resmî Lab sayfasındaki addon ZIP indirildi; bütün dosyalar tag kaynağıyla satır sonları normalleştirilerek aynı içerikte doğrulandı.
- Addon minimum Blender 5.1 gerektiriyor; mevcut 5.2.3 uygun. Modül: `bl_ext.user_default.mcp`.
- Kurulu extension: `C:/Users/mehme/AppData/Roaming/Blender Foundation/Blender/5.2/extensions/user_default/mcp`.
- MCP Python paketi resmî tag'ın mcp dizininden ayrı venv'e kuruldu. Aynı isimli rastgele PyPI blender-mcp paketi kullanılmadı.
- Venv: `C:/Users/mehme/.codex/tools/mewgic-mcp/venv`. Blender'ın Python 3.13 yorumlayıcısından oluşturuldu; **blender-mcp 1.0.3**, **MCP SDK 1.30.0**. Bağımlılıklar aynı tools klasöründeki requirements.lock.txt içinde kayıtlı.

**teamipc alternatifi:** README incelendi. uv/uvx, addon ve Blender Python çalıştırma yanında harici asset/üretim servisleri ve varsayılan telemetri onayı belgeleniyor. Bu görev için o ek servisler gerekmiyor; alternatif kurulmadı. Resmî çözümde addon/socket, weak_sandbox ve server giriş kodları incelendi. WeakSandbox gerçek izolasyon değildir; Python kodu Blender kullanıcısının yetkileriyle çalışır. Resmî kaynak seçimi bu yetki riskini ortadan kaldırmaz.

## Blender Codex kaydı ve güvenlik

```toml
[mcp_servers.mewgic-blender]
command = 'C:\Users\mehme\.codex\tools\mewgic-mcp\venv\Scripts\python.exe'
args = ['-m', 'blmcp', '--transport', 'stdio']

[mcp_servers.mewgic-blender.env]
BLENDER_MCP_HOST = '127.0.0.1'
BLENDER_MCP_PORT = '9876'
BLENDER_PATH = 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe'
```

- Codex ↔ MCP STDIO; MCP ↔ Blender **127.0.0.1:9876**. Dinleyici Windows üzerinden doğrulandı.
- HTTP kontrol sunucusu, 0.0.0.0 kontrol dinleyicisi, firewall/router kuralı açılmadı.
- Addon etkin; genel **Auto Start kapalı**. Sıradan Blender projelerinde köprü kendiliğinden başlamaz.
- Başlatıcı ayrı Blender süreci ve yeni geçici dosya açar; mevcut çalışmaları yüklemez.
- `--online-mode` addon'un yerel socket izin kontrolü için süreç bazında gerekir. Sunucuyu internete açmaz; Blender'ı OS sandbox'ına da almaz.
- API anahtarı veya harici model servisi yapılandırılmadı. Blender/Unity logları ve Codex config yedeği Git'e eklenmez.
- Kişisel dosyalarda tarama/silme ve DevControl değişikliği yapılmadı. Test dışı dosyaya işlem yapılmadan hedef açıkça doğrulanmalıdır.

### Blender gerçek MCP testleri

Ayrı süreçte `C:/Dev/Mewgic/Temp/MCP/Mewgic_MCP_Test.blend` oluşturuldu. Başlangıç sahnesi boş; adı **Mewgic_MCP_Temporary_Test**.

1. `execute_blender_code`, dosya yolunu doğruladıktan sonra küp oluşturdu ve adını **Mewgic_MCP_Verified_Cube** yaptı.
2. `get_objects_summary` aynı adı, MESH türünü ve sahne koleksiyonunu döndürdü.
3. İkinci kod çağrısı nesnenin varlığını tekrar doğruladı.

İlk denemedeki Windows yol ayırıcı farkı güvenli assert ile durdu; yol normalleştirildikten sonra testler geçti. Test küpü geçici süreçtedir; Assets'e aktarılmadı. Bu test genel modelleme/render araçlarının tamamının sınandığı anlamına gelmez.

## Yeni oturumda başlatma

1. **Unity Hub → Projects → Mewgic**: `C:/Dev/Mewgic` projesini 6000.3.27f1 ile aç. Pipeline otomatik başlar. Sorun varsa **Edit → Project Settings → Pipeline → Editor** bölümünde sunucu durumunu kontrol et.
2. Ayrı Blender test oturumu için PowerShell'de çalıştır:

   ```powershell
   powershell -NoProfile -File C:/Dev/Mewgic/tools/mcp/Start-BlenderMCP.ps1
   ```

   Bu komut ayrı Blender penceresi ve Temp/MCP altında yeni benzersiz dosya açar. 9876 doluysa ikinci süreç başlatmaz; mevcut test penceresini kullan veya kapatıp yeniden çalıştır. Kişisel Blender çalışmasını kapatmak gerekmez.
3. **Codex uygulamasını tamamen kapatıp yeniden aç**, ardından Mewgic sohbetini aç. Yeni kullanıcı MCP kayıtları istemci/oturum başlangıcında yüklenir; devam eden sohbetin başta yüklenmiş araç kataloğu anında değişmez.
4. Codex'ten Unity projesinin adını ve Blender nesnelerini MCP ile okumasını iste. `codex mcp list` içinde **mewgic-unity** ve **mewgic-blender** görünür; kayıt listesi tek başına bağlantı kanıtı değildir.
5. Blender köprüsünü kapatmak için yalnız test penceresini kapat veya **Edit → Preferences → Add-ons → MCP → Stop** kullan. Kişisel çalışmalar için Auto Start açma.

Yollar mevcut makineye aittir. Uygulama konumu değişirse yalnız ilgili iki kayıt güncellenir. GitHub Desktop'ta gerekirse **File → Add Local Repository → C:/Dev/Mewgic** kullan; Clone/Create ile ikinci proje oluşturma.

## Codex doğrulaması ve sınırlar

Kurulum öncesi config yedeği `C:/Users/mehme/.codex/tools/mewgic-mcp/codex-config.before-mcp.toml` içindedir; özel yerel dosyadır. TOML karşılaştırması diğer ayarları ve mevcut MCP kayıtlarını aynı buldu. İki yeni kayıt enabled=true; komutlar kurulu dosyalara işaret ediyor. Her iki tools/list yanıtında yinelenen isim kalmadı.

Gerçek protokol testleri Codex içinden Python MCP SDK istemcisiyle yapıldı. Yeniden başlatılmış Codex uygulamasının yerel araç kataloğu bu oturumda sınanmadı. Canlı köprüler başarılı; yerel araç yüklemesi için Codex yeniden başlatılmalı.

Android build ve oyun mekaniği testleri bu altyapı aşamasında yeniden yapılmadı. Paket entegrasyonu dışında oyun kodu, sahneler, mevcut görseller değiştirilmedi.

## Kaynaklar

- [Unity resmî MCP geçişi ve yerel bağlantı](https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli)
- Kurulu Pipeline README'si ve Unity CLI --help çıktıları.
- [Coplay v10.0.0 alternatifi](https://github.com/CoplayDev/unity-mcp/tree/v10.0.0)
- [Blender Lab resmî kurulum](https://www.blender.org/lab/mcp-server/)
- [Blender Lab kaynak ve sürümler](https://projects.blender.org/lab/blender_mcp)
- [teamipc topluluk alternatifi](https://github.com/teamipc/blender-mcp)
- [Codex MCP yapılandırması](https://learn.chatgpt.com/docs/extend/mcp)
