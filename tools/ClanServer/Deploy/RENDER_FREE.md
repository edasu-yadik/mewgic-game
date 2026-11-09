# MeWGic · Render Free ve Supabase Free

Bu plan tek bir Free Web Service ve ayrı bir Supabase Free projesi kullanır. İlk deneme için Render'ın sağladığı HTTPS alan adı yeterlidir. Her iki hesabın planını ve limitlerini onay ekranında kontrol et. YAML bir ücretli disk, veritabanı veya ek işçi tanımlamaz.

## Supabase hesabında

1. Free organizasyonda bir proje aç. Render bölgesine yakın bir Supabase bölgesi seç. Proje ve veritabanı parolası kontrolünde kalsın.
2. **Connect → Session pooler** bölümündeki IPv4 uyumlu ana bilgisayar, port **5432**, veritabanı ve kullanıcı adını kullan. Ücretli IPv4 eklentisine gerek yok. Bağlantıyı `SSL Mode=VerifyFull` ile aç. Sertifika için Supabase'in önerdiği CA'yı kullan. [Bağlantı ve TLS](https://supabase.com/docs/guides/database/connecting-to-postgres)
3. API için yalnız kendi şemasına erişebilen ayrı bir PostgreSQL giriş rolü kullan. Yönetici oturumunda rolü oluşturup güvenli bir istemcide parola ata. Örnek roller ve şema adları:

   ```sql
   CREATE ROLE mewgic_api LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;
   CREATE SCHEMA mewgic AUTHORIZATION mewgic_api;
   REVOKE ALL ON SCHEMA mewgic FROM PUBLIC;
   ```

   Pooler kullanıcısı özel rol için `mewgic_api.<proje-ref>` biçimindedir. Supabase Data API'ye `mewgic` şemasını ekleme. API'nin PostgreSQL bağlantısı dışında istemcilere şema erişimi verme. [Özel rol ve pooler](https://supabase.com/docs/guides/database/connecting-to-postgres)
4. Eski kayıt aktarılacaksa önce aşağıdaki adımlarla boş hedefe aktar. Render kurulumu ve ilk API istekleri daha sonra yapılır.

## SQLite aktarımı

Önce eski API'ye yeni yazıları durdur ve geçerli v4 veritabanından ayrı SQLite kopyası al. Daha eski şemada önce veritabanının bir kopyasını mevcut SQLite API ile aç, geçişi tamamla ve API'yi kapat:

```powershell
dotnet ClanServer.dll backup "C:\secure\game.db" "C:\secure\migration.db"
dotnet ClanServer.dll import-sqlite "C:\secure\migration.db"
```

İkinci komuttan önce `Postgres__ConnectionString` ve gerekiyorsa `Postgres__RootCertificate` değişkenlerini yerel, gizli bir ortamda ayarla. Kaynak dosyayı depoya veya Render'a yükleme. İçe aktarıcı bütünlük ve foreign key kontrolü yapar, hedef şemadaki 19 tabloyu denetler, dolu hedefi reddeder ve satır sayılarıyla birlikte tek işlemde aktarır. Hesap kimliği ve parola karması, oturum karması, klan üyeliği, kazanışlar, sezonlar, etkinlik kopyaları ve ödül makbuzları taşınır. Kaynak kopya salt okunur açılır.

Hazırlıkta iki hesabı, örnek eski klanı, ilerlemeyi ve önceki ödülü karşılaştır. Ardından tek bir API'ye yazı aç. Aktarım sonrası PostgreSQL'de kabul edilen yeni yazılar eski SQLite'a geri akmaz. Üretim geri dönüşü sırasında iki tarafı aynı anda yazıya açma. PostgreSQL için sağlayıcıdan bağımsız `pg_dump --format=custom --no-owner --schema=mewgic` kopyasını şifreli, erişimi sınırlı bir konumda tut ve ayrı veritabanına geri yükleyerek test et.

## Render ve GitHub hesaplarında

1. Render hesabını GitHub'a bağla ve Render GitHub uygulamasına yalnız MeWGic deposu için erişim ver. **New → Blueprint** üzerinden `release/0.8.0-rc.1` dalındaki `render.yaml` dosyasını seç. Alternatif olarak tek Free Docker Web Service oluşturup Dockerfile yolu `tools/ClanServer/Dockerfile`, bağlam `.`, sağlık yolu `/health`, otomatik dağıtım **Off** ayarlarını gir. [Render Blueprint](https://render.com/docs/blueprint-spec)
2. Önizlemede yalnız bir **Free Web Service** olduğunu doğrula. `Postgres__ConnectionString` değerini Render **Environment** üzerinden gizli değişken olarak ver. Npgsql biçimi:

   ```text
   Host=<session-pooler-host>;Port=5432;Database=postgres;Username=<role>.<project-ref>;Password="<secret>";SSL Mode=VerifyFull
   ```

   CA gerekiyorsa Render Secret File olarak yükle ve `Postgres__RootCertificate` değerini ilgili dosya yoluna ayarla. GitHub Actions, Unity ve GitHub dosyalarında veritabanı parolasına veya Supabase servis anahtarına gerek yok. [Render gizli değişkenleri](https://render.com/docs/configure-environment-variables)
3. Yerel PostgreSQL testleri geçen commit'i Render'da elle dağıt. Otomatik deploy kapalı kalır. Release dalında inceleme ve branch protection kullan. Render hesabına deploy hook veya API token eklemek gerekmez. [Render elle dağıtım](https://render.com/docs/deploys)
4. HTTPS alan adında önce `/health`, sonra `/ready` ve `/v1/event-calendar` kontrol edilir. İlk rota işlem sağlığını, ikinci rota veritabanı erişimini doğrular. Devamında eski ve yeni hesaplar, klan üyeliği, lig puanı ve ödül makbuzu kontrol edilir. Render'ın proxy başlıklarından gelen istemci IP'si varsayılan olarak güvenilir kabul edilmez. Çok kullanıcılı hazırlıkta paylaşımlı proxy/IP limitlerinin etkisini ölç.
5. Gerçek HTTPS URL'yi Unity `Assets/Resources/Clans/Connection.json` içindeki `developmentBaseUrl` alanına koyup Editör kabulü yap. Cihaz ve veri geçişi kabulü tamamlandığında ayrı `productionBaseUrl` alanı kullanılır. Değerler yalnız HTTPS API kök adresidir. Unity Editor, geliştirme alanı boşsa yerel loopback sunucusuna döner.

## Free sınırları ve kabul

Render Free yaklaşık 15 dakika gelen istek olmadığında uyur ve ilk istekle yaklaşık bir dakikada açılır. Geçici disk restart ve redeploy sırasında kaybolur. API bu yüzden Render profilinde PostgreSQL ister. Free servis saatleri ve diğer kota tüketimini panodan izle. Isıtma pingi veya ücretli yükseltme ayarı açma. [Render Free sınırları](https://render.com/docs/free)

Supabase, bir haftalık düşük etkinlik gösteren Free projeyi duraklatabilir. Bu durumda Dashboard üzerinden **Resume project** kullanılır. Hazırlık testinde Unity'nin 503 veya açılış sayfasında mevcut oturumu ve bekleyen kanıtı koruduğunu, sunucu geri geldiğinde kaydın bir kez gönderildiğini doğrula. [Supabase proje duraklatma](https://supabase.com/docs/guides/platform/free-project-pausing)
