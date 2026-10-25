# Mewgic

Unity6000.3.27f1 / Universal2D URP / Android Portrait / IL2CPP ARM64.
Tek gerçek proje: `C:\Dev\Mewgic`. Private repo: edasu-yadik/mewgic-game.

Boot → Home → Gameplay ve LevelMap çalışır. İki doğrulanmış bölüm, dinamik cam
şişeler, Undo/Hint/Extra/Shuffle, Pause, yıldız ve ilerleme kaydı, günlük görev/hediye,
yerel haftalık etkinlik, ücretsiz sezon ve iki mevcut koleksiyon kartı vardır.
Home onaylı karakter/logo/şişe sanatını kullanır. Görsel referanslar
`References/Visual/Approved` altında; yollar `docs/REFERENCE_REGISTRY.md` içindedir.

Missions/Events/Settings ayrı sahneler değildir. Team/Inventory bilgi panelidir;
mağaza, satın alma, reklam, gerçek enerji ekonomisi ve çevrimiçi servis yoktur.
Home para/elmas artık önizleme12450/853 yerine kalıcı yerel alanı gösterir; yeni
profil0/0 başlar. Yeni bir ekonomi/ödül sistemi eklenmemiştir.

Çalışma kuralları AGENTS.md. Gerçek tamamlanma/sınırlar
`docs/PRODUCTION_PROGRESS.md` ve `docs/ProductionReports/` altındadır.
Android kurulum/kabul ve yayın öncesi açıklar: `docs/ANDROID_BETA_GUIDE.md`.
PART08 cihaz kabul testi bitmeden kararlı beta teslimi olarak değerlendirilmez.

Kayıt: `Mewgic.Progress.v1`, bağımsız sürümlü alt kayıtlar; aynı commit'in doğrulanmış
`.backup` kopyası ve bozuk özgün veri için `.corrupt`. Uyumsuz gelecek sürüm sessizce
eski yedeğe düşürülmez. Kayıt iki kopyada da bozuksa korumalı salt okunur profil
kullanılır; özgün veri silinmez. Yarım kalan bulmaca katmanları kayıt kapsamı dışındadır.

Editor doğrulama araçları Assets/Editor altında. Testler sırayla, izole profille
çalıştırılır. `BetaValidation.SaveTests`, `BetaPlayTests.Run`, `RewardHubTests.Run`,
`DailyActivitiesTests.Run`, `MapProgressionTests.Run`, `GameplayBoosterTests.Run`,
`GameplayHudTests.Run`, `GameplayBottleTests.Run`, `MapWorldTests.Run` ve
`MewgicPlaySmoke.RunInteractive` temel girişlerdir. Gerçek oyuncu verisiyle test
ödülü üretmeyin. APK ve geçici build çıktıları Git'e dahil edilmez.
