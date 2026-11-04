# RC.1 · Performans kanıtı

PART 19A Editor profili Ryzen 7 6800H, RTX 3060 Laptop ve Direct3D 12 üzerinde 900×1600 Game View ile alındı. Bu değerler RC fiziksel cihaz ölçümü olarak kullanılmaz.

| Sahne | Kare p95 ms | Ana iş parçacığı p95 ms | GPU p95 ms |
| --- | ---: | ---: | ---: |
| Ana menü | 4,742 | 4,598 | 0,270 |
| Dünya haritası | 4,906 | 4,846 | 0,267 |
| Kristal Şelaleler | 4,698 | 4,664 | 0,325 |
| Oynanış boşta | 6,920 | 6,863 | 0,292 |
| Döküş başlangıcı | 5,717 | 5,378 | 0,287 |

Döküşte GC medyanı 18.804 bayt/kare. Sekiz aynı Gameplay dönüşünde 86 etkin Transform ve yaklaşık 1,973 GB toplam Editor belleği gözlendi. Editor ve test altyapısı ölçüme dahil. Döküş optimizasyonu mobil çalışma zamanı için 60 FPS hedefini koruyor. RC üzerinde fiziksel FPS, kare p95, PSS/Resident Memory, pil ve termal durum ölçülmedi. 30 dakikalık cihaz senaryosu BLOCKED.

[Ham Editor ölçümü](../Part19A/AFTER.json) · [Cihaz ölçüm adımları](../Part19B/DEVICE_GUIDE.md)
