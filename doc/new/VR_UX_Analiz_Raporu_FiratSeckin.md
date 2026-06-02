# VR Etkileşimleri ve Kullanıcı Deneyimi (UX) Analizi

**Proje:** Sanal Şehir Projesi | Teknik Analiz Raporu

| | |
|---|---|
| **Hazırlayan** | Fırat Seçkin |
| **Tarih** | Mayıs 2026 |
| **Öncelik** | Yüksek |
| **Durum** | Tamamlandı |
| **Hafta** | Hafta 2 |

---

## 1. Yönetici Özeti

Bu rapor, Sanal Şehir Projesi'nde kullanıcıların Oculus SDK tabanlı VR ortamıyla nasıl etkileşim kurabilecegini kapsamlı biçimde analiz etmektedir. Hareket izleme, kontrol mekanizmaları, bilgi paneli etkileşimleri ve navigasyon sistemleri incelenerek doğal ve sezgisel bir kullanıcı deneyimi için gerekli tasarım prensipleri ve gereksinimler belirlenmiştir.

---

## 2. VR Etkileşim Mekanizmaları Analizi

Oculus SDK'nın sunduğu hareket izleme ve kontrol mekanizmaları incelenerek aşağıdaki etkileşim yöntemleri belirlenmiştir:

| Mekanizma | Açıklama | Öncelik |
|---|---|---|
| Teleportasyon | Raycast tabanlı nokta seçimi ile anlık konum değişimi | Yüksek |
| Yürüme Hareketi | Joystick ile doğal yürüme simülasyonu | Yüksek |
| Nesne İnceleme | Tabloya veya nesneye tıklanınca sağda bilgi paneli açılır | Yüksek |
| El Hareketleri | Oculus Touch kontrolcüsü ile doğal el pozisyonu | Orta |
| Ses Geri Bildirimi | Etkileşim onayını destekleyen ses efektleri | Orta |

---

## 3. Kullanıcı Akışları ve Senaryo Tasarımları

### Senaryo A: Nesne / Tablo İnceleme
Kullanıcı tabloya veya nesneye tıklar → Ekranın sağında bilgi paneli açılır → Kullanıcı paneli okur → Kapat butonuna tıklar → Panel kapanır, gezintiye devam eder.

### Senaryo B: Teleportasyon ile Hareket
Kullanıcı joystick ile hedef noktayı ister → Zeminde işaretleyici belirir → Tetik basılır → Anlık konum değişimi → Yeni konumdan çevreyi inceleme.

### Senaryo C: Bölüm Geçişi
Kullanıcı piramit portaline yaklaşır → Geçiş animasyonu tetiklenir → İç mekana giriş → Yeni ortamda navigasyon.

---

## 4. Etkileşimli Kullanıcı Arayüzü Tasarım Prensipleri

### 1. Sezgisel Navigasyon
Kullanıcının öğrenme süresi minimumda tutulmalı; hareket ve etkileşim mekanizmaları gerçek dünyayı taklit etmelidir.

### 2. Görsel Geri Bildirim
Her etkileşim anında anlık görsel onay (panel açılması, buton renk değişimi, hedef işaretleyici) sağlanmalıdır.

### 3. Konfor ve Ergonomi
VR'da hareket hastalığı önlemek için smooth locomotion yerine teleport öncelikli tercih edilmeli; bilgi paneli ekranın sağ tarafında sabit konumda açılmalıdır.

### 4. Erişebilirlik
Metin boyutları VR ortamında rahatça okunabilir olmalı (min. 28px), yeterli kontrast oranı sağlanmalıdır.

### 5. Bağlam Duyarlı UI
Bilgi panelleri yalnızca kullanıcı tabloya veya nesneye tıklayınca ekranın sağ tarafında açılmalıdır. Gereksiz bilgi kullanıcının dikkatini dağıtmamalıdır.

---

## 5. Kullanılabilirlik Testleri ve Geri Bildirim Planı

| Test Yöntemi | Kapsam | Katılımcı |
|---|---|---|
| Görev Tabanlı Test | Kullanıcının belirli tablolara ulaşıp bilgi panelini açması | 5-8 kişi |
| Düşünce Seslendirme | Test sırasında kullanıcının düşüncelerini sesli ifade etmesi | 3-5 kişi |
| SUS Anketi | Kullanılabilirlik Ölçeği (0-100 puan sistemi) | 10+ kişi |
| Göz İzleme Analizi | Kullanıcının hangi UI elemanına ne kadar baktığı | 5 kişi |
| VR Konfor Anketi | Bulantı, baş dönmesi, göz yorgunluğu değerlendirmesi | 10+ kişi |

---

## 6. Sonuç ve Öneriler

- Bilgi panelleri tabloya veya nesneye tıklanınca ekranın sağ tarafında açılmalı, kullanıcının bakış açısından bağımsız okunabilir olmalıdır.
- Teleportasyon sistemi varsayılan hareket yöntemi olarak belirlenmeli, smooth locomotion opsiyonel sunulmalıdır.
- Tüm etkileşimli nesnelere görsel vurgulama (highlight) eklenerek kullanıcının etkileşim noktalarını kolayca fark etmesi sağlanmalıdır.
- Panel açılış/kapanış animasyonları 0.3 saniyeyi geçmemeli; ani geçişler VR'da rahatsızlık yaratabilir.

---

*Fırat Seçkin | Sanal Şehir Projesi | Mayıs 2026*
