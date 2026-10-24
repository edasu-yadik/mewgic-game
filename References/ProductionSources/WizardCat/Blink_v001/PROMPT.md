# Göz kapağı üretimi — PART 02B / 24 Ekim 2026

Araç: yerleşik ImageGen; transparent_background=true.
Referans: Assets/Art/Characters/MageCat/MageCat_NoWand_v001.png.
Yeni kedi tasarımı istenmedi; sadece mevcut yüzün kapanmış göz kapağı parçaları üretildi.

Üretim promptu:
Precise-object-edit for animation. Reference is the APPROVED Mewgic kitten sprite, not permission to redesign the kitten. Produce ONLY a pair of closed-eye eyelid overlay patches for this exact face, on real transparent RGBA. Keep the same 1122 by 1402 canvas and original pixel placement: the image-left open eye center is approximately (520,638), the image-right eye center approximately (706,587), coordinates measured from top-left. The original kitten face slopes upward to image-right. Reconstruct the white/light silver fluffy fur that naturally covers each fully closed eye and the delicate dark curved closed lash lines. Each patch must cover the entire original large blue iris and eye-white when overlaid, about 140 by 135 pixels for left and 125 by 130 pixels for right, with feathered fur edges matching the surrounding original face; no isolated thin lashes that leave blue iris showing through. Use the reference fur color/lighting and correct left/right eyelid contours. Render no open blue eyes. Every other pixel on the canvas must be completely transparent: no head, no nose, no whiskers, no hat, no body, no alternate kitten, no new face, no text. TWO SMALL CLOSED-EYELID FUR PATCHES ONLY, precisely positioned where the reference eyes are, otherwise empty transparent canvas. This must look like the same approved kitten briefly closing both eyes, with naturally relaxed closed lids, not squinting or smiling eye icons. Preserve fine realistic fluffy 3D illustration texture. Do not make geometric black arcs on a solid background.

Çıktı kaynak dokusu: Assets/Art/Characters/WizardCat/WizardCat_ClosedEyePatches_v001.png.
Üretici parçaları istenenden büyük ve farklı konumda verdi. Yeni karakter üretilmedi;
Unity içinde dokuya zarar vermeyen Sprite rect dilimleme ve RectTransform konum/ölçek
ayarlarıyla iki göz için hizalandı. BlinkLeft.asset ve BlinkRight.asset aynı PNG'ye
referans verir; piksel dosyası programla yeniden boyanmadı.

Kaynak piksel rect'leri (Unity sol-alt koordinatı):
- Sol: x=290, y=565, width=325, height=275.
- Sağ: x=620, y=658, width=278, height=244.
UI boyutları: sol 147×124, sağ 133×117; alt-orta pivotlu 1122×1402 karakter düzleminde
merkezleri (-49,763) ve (143,814).

Koyu/açık test zemininde ve Unity Play Mode'da kapanış/gözün geri açılması incelendi.
Bu dosyalar bağımsız göz küresi/baş/kaş rig'i değildir; kısa kapağı kapama overlay'idir.
