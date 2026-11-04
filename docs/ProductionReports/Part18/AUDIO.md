# PART 18 · Ses hakları ve envanter

On klip MeWGic için `tools/generate_part18_audio.py` ile özgün olarak sentezlendi. Müzik dizileri, ton sentezi, zarf ve gürültü üretimi kodda tanımlı. Üçüncü taraf örnek, müzik kaydı, ses kütüphanesi veya dış hizmet kullanılmadı. Projenin hak dosyasında dış lisans veya atıf kalemi gerektiren bir kaynak bulunmuyor.

| Varlık | Süre | Kullanım |
| --- | ---: | --- |
| HomeMusic.wav | 16,00 sn | Ana ekran ve dünya haritası müzik döngüsü |
| PuzzleMusic.wav | 18,00 sn | Bulmaca müzik döngüsü |
| Waterfall.wav | 6,00 sn | Düşük seviyeli şelale ambiyansı |
| Pour.wav | 0,36 sn | Sıvı akışının başlangıcı |
| Valid.wav | 0,28 sn | Kazanışla bitmeyen geçerli döküşün sonu |
| Invalid.wav | 0,34 sn | Geçersiz kaynak veya hedef |
| Victory.wav | 1,28 sn | Başarıyla kaydedilmiş bölüm tamamlanması |
| Coin.wav | 0,55 sn | Kaydedilmiş coin ödülü |
| Chest.wav | 0,90 sn | Günlük veya haftalık sandık alımı |
| Map.wav | 0,72 sn | Dünya haritasına giriş veya çıkış |

Kaynaklar mono, 24 kHz, 16 bit PCM. Dosya tepe ölçümlerinin aralığı −12,7 ila −2,3 dBFS. Unity’de müzik, ambiyans ve anlık efekt farklı AudioSource kanallarında çalıyor. Şelale, ses efektleri tercihine bağlı. Titreşim ayrıca kapatılabiliyor ve yalnız desteklenen mobil çalışma zamanında geçersiz hamle ile kazanış için tetikleniyor. Fiziksel telefon hoparlöründe seviye, işitsel denge ve titreşim şiddeti ölçülmedi.
