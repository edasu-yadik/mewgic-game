# PART 05B — Üretim yöntemi

Cam kabuk, yerleşik image_gen aracıyla, Approved/02_Gameplay.png stil referansı kullanılarak üretildi. Gerçek alpha korunarak Resources/Gameplay/Gameplay_GlassShell_v001.png olarak kopyalandı; kaynak dosya silinmedi. Sprite import rect saydam yan boşlukları dışarıda bırakır. Blender ve Python çizimi kullanılmadı.

Prompt:
Use case: stylized-concept. Unity water sort gameplay sprite. Reference image defines exact glass bottle silhouette and premium illustrated 3D material style. Create ONE EMPTY colorless transparent bottle shell, no liquid whatsoever, no cork. Front orthographic view, symmetrical upright slender bottle, width/height 0.42. Rounded cylindrical bottom/body, gently rounded shoulders, narrow short neck and flared open glass lip. Brilliant thin cyan-white polished edge reflections, subtle lavender glass edge refraction. Interior and background genuinely transparent, not black/gray filled. NO background, floor, shadows outside, other objects, text or decorative colored potions. Entire bottle visible filling portrait canvas with only 3 percent margin. Body sides nearly straight from 8 percent to 71 percent height; shoulders 71-83 percent, narrow neck 83-94 percent, rim 94-97 percent. Interior remains open transparent for dynamic liquids drawn beneath in Unity. Fine exquisite high quality illustration matching reference.

Sıvılar üretilmiş PNG'nin parçası değildir. Model katmanları, renk paleti ve aktarım hacmi ile Unity UI mesh olarak çizilir. Dalga/baloncuklar aynı Graphic içinde sınırlı geometri kullanır. Eğilme sırasında yatay kesme düzlemleriyle alan korunur; bu bir fiziksel akışkan simülasyonu değildir.

GIF yalnız gerçek Game View ScreenCapture karelerinin kodlanmasıdır; sahte animasyon veya çizim değildir.
