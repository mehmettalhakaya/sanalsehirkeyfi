# 🏙️ Sanal Şehir Keşfi (Virtual City Exploration)

🎓 **Fırat Üniversitesi – Yazılım Mühendisliği**
📚 *Yazılım Mühendisliği Temelleri Dersi Projesi*

![Unity](https://img.shields.io/badge/Unity-6.3%20LTS-black?logo=unity)
![URP](https://img.shields.io/badge/Render-URP%2017.3-blue)
![C#](https://img.shields.io/badge/Language-C%23-purple?logo=csharp)
![Node.js](https://img.shields.io/badge/Backend-Node.js%20%2B%20Express-green?logo=nodedotjs)
![MySQL](https://img.shields.io/badge/DB-MySQL-orange?logo=mysql)
![Status](https://img.shields.io/badge/Durum-Tamamland%C4%B1-success)
![License](https://img.shields.io/badge/Lisans-Akademik-lightgrey)

![Ana Ekran](images/MainScreen.png)

---

## 📖 Proje Hakkında

Bu proje, kullanıcıların **Paris Louvre Müzesi'nin hem dış avlusunu hem de iç galerilerini** birinci şahıs perspektifinden sanal gerçeklik ortamında keşfedebileceği bir simülasyon uygulamasıdır.

Kullanıcılar; gerçekçi 3B sanal şehir/müze ortamında dolaşarak ünlü tabloları inceleyebilir, etkileşimli bilgi panelleri ile eserler hakkında bilgi alabilir, klasik klavye-mouse veya VR başlığı ile akıcı bir keşif deneyimi yaşayabilir.

Proje aynı zamanda kullanıcıya özel deneyim sunabilmek için kendi sunucumuzda barındırılan **PHP/Node.js + MySQL** tabanlı bir **kullanıcı kayıt ve giriş sistemi** içerir.

---

## 🎯 Projenin Amacı

- 🌍 Sanal ortamda şehir/müze keşfi deneyimi sunmak
- 🏛️ Tarihi ve kültürel mekânları (Louvre) dijital ortamda tanıtmak
- 🕶️ Sanal gerçeklik teknolojileri ile etkileşim sağlamak
- ♿ Fiziksel erişim engelini kaldırarak müzeyi herkes için ulaşılabilir kılmak
- 🎓 Eğitim ve turizm alanlarına dijital katkı sağlamak

---

## ⚙️ Kullanılan Teknolojiler

### 🎮 İstemci (Client)
- 🎮 **Unity 6.3 LTS** — oyun motoru
- 💻 **C#** — script dili
- 🧱 **Blender** — 3D modelleme
- 🥽 **Oculus SDK / XR Interaction Toolkit** — VR desteği
- 🎨 **URP 17.3** — Universal Render Pipeline
- 🔤 **TextMeshPro** — modern font sistemi

### 🌐 Sunucu (Backend)
- 🟢 **Node.js + Express** — REST API
- 🗄️ **MySQL** — veritabanı
- 🔐 **bcrypt** — şifre hashleme
- 🎫 **JWT** — oturum tokenı
- 🛡️ **helmet + express-rate-limit** — güvenlik
- 🖥️ **Ubuntu VPS + Nginx** — hosting

### 🛠️ Geliştirme Araçları
- 🐙 **Git & GitHub** — versiyon kontrol
- 📋 **Jira** — sprint/görev takibi
- ⌨️ **Visual Studio 2022** — C# IDE
- 🤖 **Claude / ChatGPT / GitHub Copilot** — yapay zekâ destekli geliştirme

---

## 🧩 Proje Özellikleri

- 🏛️ **Louvre Müzesi 3B modeli** (dış avlu + iç galeri)
- 🚶 **Birinci şahıs hareket sistemi** (WASD + mouse / VR kontrolcü)
- 🏃 **Koşma, zıplama, uçuş modu** desteği
- 🎯 **Sağ tık teleport** ile hızlı konum değiştirme
- 🖼️ **Etkileşimli tablo bilgi panelleri** (~25 tablo)
- 🚪 **Kapı / portal sistemi** ile sahne geçişleri
- 🌳 **Procedural peyzaj** — 53 kayın + 15 meşe + çim + lambalar
- 🐦 **Uçan kuş sürüleri** — waypoint döngülü
- 🌅 **Procedural gökyüzü + güneş** + soft shadows
- 🔐 **Kullanıcı kayıt / giriş sistemi** (kendi sunucu)
- ⚙️ **Ayarlar menüsü** — ses, grafik, mouse hassasiyeti
- ❓ **Navigasyon HUD** — H tuşu ile yardım overlay
- 🕶️ **VR uyumlu** — Meta Quest, HTC Vive
- 🌍 **Türkçe arayüz + Türkçe karakter desteği**

---

## 📸 Ekran Görüntüleri

### 🏠 Ana Menü
Uygulamanın açılış ekranı — Louvre arka planı ile ve "Keşfe Başla" / "Ayrıl" butonlarıyla kullanıcıyı karşılar.

![Ana Menü](images/Begin.png)

---

### 🔐 Giriş Paneli
Kullanıcı kayıt ve giriş paneli — kendi sunucumuzdaki PHP/Node.js + MySQL API'ye bağlanır.

![Giriş Paneli](images/LoginPanel.png)

---

### 🏛️ Louvre Avlusu — Farklı Açılardan
Procedural peyzaj, ağaçlar, lambalar, demir korkuluk ve cam piramit ile avlu.

<table>
  <tr>
    <td><img src="images/Angle1.png" alt="Avlu Açı 1" width="100%"/></td>
    <td><img src="images/Angle2.png" alt="Avlu Açı 2" width="100%"/></td>
  </tr>
  <tr>
    <td colspan="2"><img src="images/Angle3.png" alt="Avlu Açı 3" width="100%"/></td>
  </tr>
</table>

---

### 🖼️ Müze İçi
İç galeri sahnesi — tablolar, aydınlatma ve gezilebilir koridorlar.

![Müze İçi](images/InsideMuseum.png)

---

### 🎨 Tablo Etkileşimi
Tablolara yaklaşıp tıklandığında açılan bilgi paneli — eser adı, ressam, dönem ve açıklama bilgileri.

<table>
  <tr>
    <td><img src="images/Table1.png" alt="Tablo Etkileşim 1" width="100%"/></td>
    <td><img src="images/Table2.png" alt="Tablo Etkileşim 2" width="100%"/></td>
  </tr>
</table>

---

## 🏗️ Sistem Mimarisi

```
┌─────────────────────────────────────────────────────┐
│              UNITY 6.3 LTS (URP)                    │
│  ┌─────────────────────────────────────────────┐    │
│  │ MainMenu  →  Outside_Museum  →  Interior    │    │
│  │  (Login)        (Avlu)           (Galeri)   │    │
│  └─────────────────────────────────────────────┘    │
│             ▼ UnityWebRequest (JSON)                │
└─────────────┼───────────────────────────────────────┘
              │
   ┌──────────▼──────────┐
   │  Node.js + Express  │
   │  REST API           │
   │  (mtkaya.me)        │
   │  ├─ /api/register   │
   │  └─ /api/login      │
   └──────────┬──────────┘
              │
   ┌──────────▼──────────┐
   │  MySQL Veritabanı   │
   │  (users, eserler,   │
   │   ziyaretler...)    │
   └─────────────────────┘
```

---

## 👥 Proje Ekibi

| Rol | Üye | Numara |
|-----|-----|--------|
| 👨‍💼 **Proje Yöneticisi / Scrum Master** | **Mehmet Talha Kaya** | 250542009 |
| 👩‍💻 **Veri Entegrasyonu / Ses–Ambiyans / Veritabanı** | **Melike Gücin** | 240541081 |
| 👩‍💻 **VR Hareket / Etkileşim / UI-UX** | **Cemre Yurtsever** | 250541127 |
| 👨‍💻 **VR Fizik / Oculus / 3D Modelleme** | **Mustafa Murat Hilaloğlu** | 240541025 |
| 👨‍💻 **UI-UX Tasarımcı / VR Deneyim** | **Fırat Seçkin** | 250541042 |

---

## 📂 Proje Dokümanları

| Doküman | İçerik |
|---------|--------|
| 📄 [Proje Akışı](projeakisi.md) | Hafta hafta ilerleme ve görev dağılımı |
| 🎯 [Unity Optimizasyon Stratejileri Raporu](doc/new/Unity_Optimizasyon_Stratejileri_Raporu.md) | LOD, occlusion culling, batching, instancing analizi |
| 🧪 [Kullanıcı Test Senaryoları ve Değerlendirme Metrikleri](doc/new/Kullanici_Test_Senaryolari_ve_Degerlendirme_Metrikleri.md) | Test senaryoları + Likert ölçeği |
| 🏛️ [Sanal Şehir Modeli Detaylandırma ve Optimizasyon Planı](doc/new/Sanal_Sehir_Modeli_Detaylandırma_Ve_Optimizasyon_Planı.md) | Blender model + poligon optimizasyon planı |
| 💾 [Sanal Şehir Veri Entegrasyonu ve Yönetim Planı](doc/new/Sanal_Şehir_Veri_Entegrasyonu_ve_Yönetim%20Planı.md) | REST API + MySQL veri akışı |
| 🥽 [Oculus Entegrasyonu ve Performans Optimizasyonu Planlaması](doc/new/Oculus_Entegrasyonu_ve_Performans_Optimizasyonu_Planlaması) | XR Plugin + 72/90 FPS hedefleri |
| 🎮 [VR Etkileşimleri ve UX Analiz Raporu](doc/new/VR_UX_Analiz_Raporu_FiratSeckin.md) | Teleport, snap turn, motion sickness analizi |
| 📄 [Proje Dokümantasyonu](doc/documentation.docx) | Proje dokümantasyonu |

---

# 📅 Proje Süreci

> 🗓️ **Başlangıç:** 20 Nisan 2026 • **Teslim:** 02 Haziran 2026 • **Süre:** 6 Hafta

## 📌 Hafta 1 (20 – 26 Nisan 2026) — Proje Tanıma ve Başlangıç
- Teknoloji araştırması ve değerlendirme (Unity, C#, Blender, Oculus SDK)
- Proje yönetimi ve iş birliği araçlarının kurulumu (Jira, GitHub)
- Geliştirme ortamının kurulumu (Unity 6.3 LTS + Visual Studio + Blender)
- Gereksinim toplama ve belgeleme
- Proje analizi ve kapsam belirleme
- Risk analizi ve önlem planı

---

## 📌 Hafta 2 (27 Nisan – 3 Mayıs 2026) — Analiz ve Planlama
- Unity optimizasyon stratejileri araştırması (LOD, occlusion culling, instancing)
- Kullanıcı test senaryoları ve değerlendirme metrikleri tanımlama
- Blender'da Louvre modeli detaylandırma ve optimizasyon planı
- Sanal şehir/müze veri entegrasyonu ve yönetim planı
- VR etkileşimleri ve kullanıcı deneyimi (UX) analizi
- Veritabanı şema taslağının çıkarılması

---

## 📌 Hafta 3 (4 – 10 Mayıs 2026) — Tasarım
- Oculus entegrasyonu ve performans optimizasyonu planlaması (72/90 FPS)
- Şehir veri entegrasyonu ve yönetim sistemi tasarımı (REST API + MySQL)
- VR etkileşim mekaniklerinin tasarımı (teleport, snap turn, raycast)
- Ses peyzajı ve ambiyans tasarımı (3D spatial audio)
- Etkileşimli menü tasarımı ve Figma prototipleme
- Renk paleti ve tipografi standartlarının belirlenmesi

---

## 📌 Hafta 4 (11 – 17 Mayıs 2026) — Temel Geliştirme
- Louvre tarihi binası 3D modellerinin Blender'da oluşturulması
- Unity'de etkileşimli bilgi paneli sisteminin geliştirilmesi
- Oculus SDK ile VR hareket sisteminin entegrasyonu
- Tarihi mekânlar için bilgi paneli sisteminin Unity'de uygulanması
- Tablolar için raycast tıklama akışının kurulması
- Ana menü ve sahne geçişlerinin tamamlanması

---

## 📌 Hafta 5 (18 – 24 Mayıs 2026) — İleri Geliştirme ve Test
- Ses efektlerinin entegrasyonu ve optimizasyonu (3D spatial blend)
- Login/Register sistemi ve UnityWebRequest API bağlantısı
- Node.js + Express + MySQL backend yazılımı (bcrypt + JWT + helmet)
- Çarpışma algılama ve fizik motoru optimizasyonu
- Görünmez sınır duvarları + FallProtection sistemi
- VR ortamında navigasyon ve hareket mekanizmalarının iyileştirilmesi
- Etkileşimli nesnelerin (kapılar, lambalar, info kioskları) geliştirilmesi

---

## 📌 Hafta 6 (25 Mayıs – 1 Haziran 2026) — Entegrasyon, Toparlama ve Sunum
- VR simülasyonunun optimizasyonu ve cilalanması (texture rebind + lightmap baked)
- Final sunum hazırlığı (slayt + demo video + sonuç raporu)
- Proje dokümantasyonunun tamamlanması (README, mimari, ER diyagramı, kullanım kılavuzu)
- Kullanıcı testleri ve geri bildirim toplama (5 katılımcı, Likert ölçeği)
- Proje entegrasyonu ve hata ayıklama (URP shader, sahne geçiş, login flow)
- Final build alımı (Windows x64, ~600 MB)

---

# 🚀 Proje Durumu

✅ **TAMAMLANDI**

> 🎉 Tüm hedeflenen özellikler başarıyla geliştirilmiş, kullanıcı testleri tamamlanmış ve uygulamanın **final sürümü** alınmıştır.

### 📊 Proje İstatistikleri
| Metrik | Değer |
|--------|-------|
| 📅 Toplam Süre | 6 Hafta |
| 📜 C# Script | 23 runtime + 9 editor |
| 🎬 Sahne | 3 (MainMenu, Outside_Museum, LouvreInteriorOptimized) |
| 🌳 Sahnedeki ağaç | 68 (53 Beech + 15 Oak) |
| 🖼️ Etkileşimli tablo | ~25 |
| 🐦 Uçan kuş sürüsü | 6–8 |
| ⭐ Kullanıcı memnuniyeti | 4.6 / 5 (5 katılımcı ortalaması) |
| 🎯 FPS performansı | Stable 60+ (1080p) |
| 📦 Build boyutu | ~600 MB |
| ⏱️ API yanıt süresi | < 250 ms (ortalama) |

---

## 🎮 Kontroller

| Tuş / Mouse | İşlev |
|-------------|-------|
| **WASD** | Yürüme |
| **Mouse** | Bakış yönü |
| **Sol tık** | Tabloya tıklayınca bilgi paneli |
| **Sağ tık (basılı)** | Teleport hedefi |
| **E** | Etkileşim (kapı vb.) |
| **H** | Yardım menüsü |
| **ESC** | Ayarlar / ana menüye dönüş |

---

## 🚀 Kurulum

### Geliştirici Olarak
```bash
git clone https://github.com/mehmettalhakaya/sanalsehirkeyfi.git
cd sanalsehirkeyfi
```
1. **Unity Hub** → Open → bu klasörü seç
2. **Unity 6.3 LTS** sürümünü kullan
3. İlk açılışta asset import 5–15 dakika sürer
4. `Edit → Project Settings → Player → Allow downloads over HTTP: Always Allowed`
5. **Play** → MainMenu sahnesi açılır

### Son Kullanıcı Olarak
1. [Releases](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases) sayfasından son sürümü indir
2. ZIP'i aç → `sanalsehirkeyfi build` çalıştır
3. Kaydol veya giriş yap → "Keşfe Başla"

---

## 🤖 Yapay Zekâ Destekli Araç Kullanımı

Geliştirme sürecinde aşağıdaki yapay zekâ destekli araçlar şeffaflık ilkesi gereği belirtilmiştir:

| Araç | Kullanım Amacı | Aşama |
|------|----------------|-------|
| **Claude** | Kod geliştirme, hata ayıklama, Unity script düzenlemeleri, rapor desteği | Geliştirme, test, dokümantasyon |
| **ChatGPT** | Fikir üretme, metin düzenleme, sunum/rapor içerikleri | Analiz, dokümantasyon, sunum |
| **GitHub Copilot** | C# ve JavaScript kod önerileri | Geliştirme |
| **AI 3D Modelleme Araçları** | 3D model düzenleme, Blender/Unity aktarım desteği | Tasarım, 3B modelleme |

---

## 📜 Lisans ve Krediler

Bu proje **Fırat Üniversitesi Yazılım Mühendisliği Temelleri (YMT1104)** dersi kapsamında akademik amaçla geliştirilmiştir.

### 🎨 Üçüncü Taraf Asset'ler
- **Louvre Museum modeli** — Sketchfab (Creative Commons)
- **Beech & Oak ağaç modelleri** — Sketchfab
- **Trafalgar Square Lamp** — Sketchfab
- **SkySeries Freebie** — Unity Asset Store
- **Gogo Casual Pack** — Unity Asset Store
- **Easy-Population** — Unity Asset Store

---

## 🔗 Bağlantılar

- 🐙 **GitHub:** [github.com/mehmettalhakaya/sanalsehirkeyfi](https://github.com/mehmettalhakaya/sanalsehirkeyfi)
- 📄 **Proje Akışı:** [projeakisi.md](projeakisi.md)
- 🌐 **API Sunucusu:** [mtkaya.me](https://mtkaya.me)

---

> ⭐ Bu proje **Fırat Üniversitesi Yazılım Mühendisliği Temelleri (YMT1104)** dersi kapsamında geliştirilmiş, **2025–2026 Bahar Yarıyılı** sonunda başarıyla tamamlanmıştır.
>
> 💙 Beğendiyseniz GitHub'da ⭐ vermeyi unutmayın!
