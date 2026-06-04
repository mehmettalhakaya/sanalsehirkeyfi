# 🏛️ Sanal Şehir Keşfi (Virtual City Exploration)

**Fırat Üniversitesi – Yazılım Mühendisliği**  
*Yazılım Mühendisliği Temelleri Dersi Projesi*

![Unity](https://img.shields.io/badge/Unity-6.3%20LTS-black?logo=unity)
![URP](https://img.shields.io/badge/Render-URP%2017.3-blue)
![C#](https://img.shields.io/badge/Language-C%23-purple?logo=csharp)
![Node.js](https://img.shields.io/badge/Backend-Node.js%20%2B%20Express-green?logo=nodedotjs)
![MySQL](https://img.shields.io/badge/DB-MySQL-orange?logo=mysql)
![Status](https://img.shields.io/badge/Durum-Tamamlandı-success)
![License](https://img.shields.io/badge/Lisans-Akademik-lightgrey)
[![Latest Release](https://img.shields.io/github/v/release/mehmettalhakaya/sanalsehirkeyfi?label=Sürüm&color=blue)](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/mehmettalhakaya/sanalsehirkeyfi/total?label=İndirme&color=brightgreen)](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey?logo=windows)

![Ana Ekran](images/MainScreen.png)

---

## 📌 Proje Hakkında

**Sanal Şehir Keşfi**, kullanıcıların Paris Louvre Müzesi'nin dış avlusunu ve iç galerisini birinci şahıs bakış açısıyla keşfedebildiği sanal gerçeklik destekli bir simülasyon projesidir.

Kullanıcılar; 3B müze ortamında gezinebilir, ünlü tabloları inceleyebilir, etkileşimli bilgi panelleriyle eserler hakkında bilgi alabilir ve klasik klavye-mouse ya da VR başlığı ile deneyimi yaşayabilir.

Proje ayrıca kullanıcıya özel deneyim sunabilmek için kendi sunucumuzda barındırılan **Node.js + Express + MySQL** tabanlı bir **kayıt ve giriş sistemi** içerir.

---

## ⬇️ İndir (Hazır Build'ler)

Projeyi Unity kurmadan denemek için işletim sistemine uygun final build'i aşağıdaki bağlantıdan indirebilirsin.

| Platform | İndirme Bağlantısı | Açıklama |
|---|---|---|
| **Windows** | [Latest Release](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases/latest) | `.exe` çalıştırılabilir paket |
| **macOS** | [Latest Release](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases/latest) | `.app` paketi |
| **Linux** | [Latest Release](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases/latest) | x86_64 çalıştırılabilir paket |

> Tüm platform build'leri tek release sayfasındadır. Sistemine uygun ZIP dosyasını indir, arşivden çıkar ve çalıştırılabilir dosyayı başlat. Login/Register sistemi için internet bağlantısı gereklidir.

**Sürüm Notları:** [v1.0.0 - Final Build](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases/tag/v1.0.0)

---

## 🎯 Projenin Amacı

- Sanal ortamda şehir/müze keşfi deneyimi sunmak
- Louvre gibi tarihi ve kültürel mekânları dijital ortamda tanıtmak
- VR teknolojileri ile etkileşimli ve sürükleyici bir deneyim oluşturmak
- Fiziksel erişim engelini azaltarak müzeyi daha ulaşılabilir hâle getirmek
- Eğitim ve turizm alanlarına dijital katkı sağlamak

---

## ⚙️ Kullanılan Teknolojiler

### İstemci (Client)

- **Unity 6.3 LTS** — oyun motoru
- **C#** — script dili
- **Blender** — 3B modelleme
- **Oculus SDK / XR Interaction Toolkit** — VR desteği
- **URP 17.3** — Universal Render Pipeline
- **TextMeshPro** — modern font sistemi

### Sunucu (Backend)

- **Node.js + Express** — REST API
- **MySQL** — veritabanı
- **bcrypt** — şifre hashleme
- **JWT** — oturum tokenı
- **helmet + express-rate-limit** — güvenlik
- **Ubuntu VPS + Nginx** — hosting

### Geliştirme Araçları

- **Git & GitHub** — versiyon kontrol
- **Jira** — sprint/görev takibi
- **Visual Studio 2022** — C# IDE
- **Claude / ChatGPT / GitHub Copilot** — yapay zekâ destekli geliştirme

---

## ✨ Proje Özellikleri

- **Louvre Müzesi 3B modeli**: dış avlu + iç galeri
- **Birinci şahıs hareket sistemi**: WASD + mouse / VR kontrolcü
- **Koşma ve yürüme** desteği
- **Sağ tık teleport** ile hızlı konum değiştirme
- **Etkileşimli tablo bilgi panelleri**: yaklaşık 25 tablo
- **Kapı / portal sistemi** ile sahne geçişleri
- **Procedural peyzaj**: 53 kayın + 15 meşe + çim + lambalar
- **Uçan kuş sürüleri**: waypoint döngülü hareket
- **Procedural gökyüzü + güneş + gece modu + soft shadows**
- **Kullanıcı kayıt / giriş sistemi**
- **Ayarlar menüsü**: ses, grafik, mouse hassasiyeti
- **Navigasyon HUD**: H tuşu ile yardım ekranı
- **VR uyumluluğu**: Meta Quest, HTC Vive
- **Türkçe arayüz ve Türkçe karakter desteği**

---

## 🖼️ Ekran Görüntüleri

### Ana Menü

Uygulamanın açılış ekranı, Louvre arka planı ve **Keşfe Başla / Ayrıl** butonlarıyla kullanıcıyı karşılar.

![Ana Menü](images/Begin.png)

---

### Giriş Paneli

Kullanıcı kayıt ve giriş paneli, kendi sunucumuzdaki **Node.js + Express + MySQL** API'ye bağlanır.

![Giriş Paneli](images/LoginPanel.png)

---

### Louvre Avlusu — Farklı Açılardan

Procedural peyzaj, ağaçlar, lambalar, demir korkuluk ve cam piramit ile oluşturulan avlu sahnesi.

![Avlu Açı 1](images/Angle1.png)
![Avlu Açı 2](images/Angle2.png)
![Avlu Açı 3](images/Angle3.png)

---

### Müze İçi

İç galeri sahnesi; tablolar, aydınlatma ve gezilebilir koridorlardan oluşur.

![Müze İçi](images/InsideMuseum.png)

---

### Tablo Etkileşimi

Tablolara yaklaşıp tıklandığında eser adı, ressam, dönem ve açıklama bilgilerini gösteren bilgi paneli açılır.

![Tablo Etkileşim 1](images/Table1.png)
![Tablo Etkileşim 2](images/Table2.png)

---

## 🧩 Sistem Mimarisi

```text
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

Bu proje, ekip üyelerinin farklı alanlardaki katkılarıyla geliştirilmiştir.

<table>
  <tr>
    <td align="center" width="160" valign="top">
      <a href="https://github.com/mehmettalhakaya">
        <img src="https://github.com/mehmettalhakaya.png?size=120" width="90" height="90" alt="Mehmet Talha Kaya" />
      </a>
    </td>
    <td align="center" width="160" valign="top">
      <a href="https://github.com/melikegcncodes">
        <img src="https://github.com/melikegcncodes.png?size=120" width="90" height="90" alt="Melike Gücin" />
      </a>
    </td>
    <td align="center" width="160" valign="top">
      <a href="https://github.com/cemreeyrtsvr">
        <img src="https://github.com/cemreeyrtsvr.png?size=120" width="90" height="90" alt="Cemre Yurtsever" />
      </a>
    </td>
    <td align="center" width="160" valign="top">
      <a href="https://github.com/murathilaloglu">
        <img src="https://github.com/murathilaloglu.png?size=120" width="90" height="90" alt="Mustafa Murat Hilaloğlu" />
      </a>
    </td>
    <td align="center" width="160" valign="top">
      <a href="https://github.com/firatseckin">
        <img src="https://github.com/firatseckin.png?size=120" width="90" height="90" alt="Fırat Seçkin" />
      </a>
    </td>
  </tr>
  <tr>
    <td align="center" valign="top">
      <a href="https://github.com/mehmettalhakaya"><strong>Mehmet Talha Kaya</strong></a>
    </td>
    <td align="center" valign="top">
      <a href="https://github.com/melikegcncodes"><strong>Melike Gücin</strong></a>
    </td>
    <td align="center" valign="top">
      <a href="https://github.com/cemreeyrtsvr"><strong>Cemre Yurtsever</strong></a>
    </td>
    <td align="center" valign="top">
      <a href="https://github.com/murathilaloglu"><strong>Mustafa Murat Hilaloğlu</strong></a>
    </td>
    <td align="center" valign="top">
      <a href="https://github.com/firatseckin"><strong>Fırat Seçkin</strong></a>
    </td>
  </tr>
</table>

---

## 📄 Proje Dokümanları

| Doküman | İçerik |
|---|---|
| [Proje Akışı](projeakisi.md) | Hafta hafta ilerleme ve görev dağılımı |
| [Unity Optimizasyon Stratejileri Raporu](doc/new/Unity_Optimizasyon_Stratejileri_Raporu.md) | LOD, occlusion culling, batching, instancing analizi |
| [Kullanıcı Test Senaryoları ve Değerlendirme Metrikleri](doc/new/Kullanici_Test_Senaryolari_ve_Degerlendirme_Metrikleri.md) | Test senaryoları + Likert ölçeği |
| [Sanal Şehir Modeli Detaylandırma ve Optimizasyon Planı](doc/new/Sanal_Sehir_Modeli_Detaylandırma_Ve_Optimizasyon_Planı.md) | Blender model + poligon optimizasyon planı |
| [Sanal Şehir Veri Entegrasyonu ve Yönetim Planı](doc/new/Sanal_Şehir_Veri_Entegrasyonu_ve_Yönetim%20Planı.md) | REST API + MySQL veri akışı |
| [Oculus Entegrasyonu ve Performans Optimizasyonu Planlaması](doc/new/Oculus_Entegrasyonu_ve_Performans_Optimizasyonu_Planlaması) | XR Plugin + 72/90 FPS hedefleri |
| [VR Etkileşimleri ve UX Analiz Raporu](doc/new/VR_UX_Analiz_Raporu_FiratSeckin.md) | Teleport, snap turn, motion sickness analizi |

---

## 🗓️ Proje Süreci

> **Başlangıç:** 20 Nisan 2026 • **Teslim:** 02 Haziran 2026 • **Süre:** 6 hafta

### Hafta 1 (20 – 26 Nisan 2026) — Proje Tanıma ve Başlangıç

- Teknoloji araştırması ve değerlendirme
- Proje yönetimi ve iş birliği araçlarının kurulumu
- Geliştirme ortamının kurulumu
- Gereksinim toplama ve belgeleme
- Proje analizi ve kapsam belirleme
- Risk analizi ve önlem planı

---

### Hafta 2 (27 Nisan – 3 Mayıs 2026) — Analiz ve Planlama

- Unity optimizasyon stratejileri araştırması
- Kullanıcı test senaryoları ve değerlendirme metrikleri tanımlama
- Blender'da Louvre modeli detaylandırma ve optimizasyon planı
- Sanal şehir/müze veri entegrasyonu ve yönetim planı
- VR etkileşimleri ve kullanıcı deneyimi analizi
- Veritabanı şema taslağının çıkarılması

---

### Hafta 3 (4 – 10 Mayıs 2026) — Tasarım

- Oculus entegrasyonu ve performans optimizasyonu planlaması
- Şehir veri entegrasyonu ve yönetim sistemi tasarımı
- VR etkileşim mekaniklerinin tasarımı
- Ses peyzajı ve ambiyans tasarımı
- Etkileşimli menü tasarımı ve prototipleme
- Renk paleti ve tipografi standartlarının belirlenmesi

---

### Hafta 4 (11 – 17 Mayıs 2026) — Temel Geliştirme

- Louvre tarihi binası 3B modellerinin Blender'da oluşturulması
- Unity'de etkileşimli bilgi paneli sisteminin geliştirilmesi
- Oculus SDK ile VR hareket sisteminin entegrasyonu
- Tarihi mekânlar için bilgi paneli sisteminin uygulanması
- Tablolar için raycast tıklama akışının kurulması
- Ana menü ve sahne geçişlerinin tamamlanması

---

### Hafta 5 (18 – 24 Mayıs 2026) — İleri Geliştirme ve Test

- Ses efektlerinin entegrasyonu ve optimizasyonu
- Login/Register sistemi ve UnityWebRequest API bağlantısı
- Node.js + Express + MySQL backend yazılımı
- Çarpışma algılama ve fizik motoru optimizasyonu
- Görünmez sınır duvarları + FallProtection sistemi
- VR ortamında navigasyon ve hareket mekanizmalarının iyileştirilmesi
- Etkileşimli nesnelerin geliştirilmesi

---

### Hafta 6 (25 Mayıs – 1 Haziran 2026) — Entegrasyon, Toparlama ve Sunum

- VR simülasyonunun optimizasyonu ve cilalanması
- Final sunum hazırlığı
- Proje dokümantasyonunun tamamlanması
- Kullanıcı testleri ve geri bildirim toplama
- Proje entegrasyonu ve hata ayıklama
- Final build alımı

---

## ✅ Proje Durumu

**TAMAMLANDI**

> Tüm hedeflenen özellikler geliştirilmiş, kullanıcı testleri tamamlanmış ve uygulamanın final sürümü Windows, macOS ve Linux platformları için yayınlanmıştır.

### Proje İstatistikleri

| Metrik | Değer |
|---|---|
| Toplam Süre | 6 hafta |
| C# Script | 23 runtime + 9 editor |
| Sahne | 3 (MainMenu, Outside_Museum, LouvreInteriorOptimized) |
| Sahnedeki ağaç | 68 (53 Beech + 15 Oak) |
| Etkileşimli tablo | ~25 |
| Uçan kuş sürüsü | 6–8 |
| Desteklenen platform | Windows · macOS · Linux |
| Kullanıcı memnuniyeti | 4.6 / 5 (5 katılımcı ortalaması) |
| FPS performansı | Stable 60+ (1080p) |
| Build boyutu | ~600 MB |
| API yanıt süresi | < 250 ms (ortalama) |

---

## 🎮 Kontroller

| Tuş / Mouse | İşlev |
|---|---|
| **WASD** | Yürüme |
| **Shift** | Koşma |
| **Mouse** | Bakış yönü |
| **Sol tık** | Tabloya tıklayınca bilgi paneli |
| **Sağ tık (basılı)** | Teleport hedefi |
| **E** | Etkileşim (kapı vb.) |
| **H** | Yardım menüsü |
| **ESC** | Ayarlar / ana menüye dönüş |

---

## 🚀 Kurulum

### Son Kullanıcı Olarak

1. [Releases](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases/latest) sayfasından işletim sistemine uygun ZIP dosyasını indir.
2. Arşivi çıkar.
3. Çalıştırılabilir dosyayı başlat:
   - **Windows:** `sanalsehirkeyfi.exe`
   - **macOS:** `sanalsehirkeyfi.app`
   - **Linux:** `chmod +x sanalsehirkeyfi.x86_64 && ./sanalsehirkeyfi.x86_64`
4. Kaydol veya giriş yap.
5. **KEŞFE BAŞLA** butonuna tıkla.

### Geliştirici Olarak

```bash
git clone https://github.com/mehmettalhakaya/sanalsehirkeyfi.git
cd sanalsehirkeyfi
```

1. **Unity Hub** üzerinden `ProjectFiles/Desktop` klasörünü aç.
2. **Unity 6.3 LTS** sürümünü kullan.
3. İlk açılışta asset import işleminin tamamlanmasını bekle.
4. `Edit → Project Settings → Player → Allow downloads over HTTP: Always Allowed` ayarını kontrol et.
5. **Play** tuşuna basarak `MainMenu` sahnesini çalıştır.

---

## 🤖 Yapay Zekâ Destekli Araç Kullanımı

Geliştirme sürecinde aşağıdaki yapay zekâ destekli araçlar şeffaflık ilkesi gereği belirtilmiştir.

| Araç | Kullanım Amacı | Aşama |
|---|---|---|
| **Claude** | Kod geliştirme, hata ayıklama, Unity script düzenlemeleri, rapor desteği | Geliştirme, test, dokümantasyon |
| **ChatGPT** | Fikir üretme, metin düzenleme, sunum/rapor içerikleri | Analiz, dokümantasyon, sunum |
| **GitHub Copilot** | C# ve JavaScript kod önerileri | Geliştirme |
| **AI 3D Modelleme Araçları** | 3B model düzenleme, Blender/Unity aktarım desteği | Tasarım, 3B modelleme |

---

## 📜 Lisans ve Krediler

Bu proje **Fırat Üniversitesi Yazılım Mühendisliği Temelleri (YMT1104)** dersi kapsamında akademik amaçla geliştirilmiştir.

### Üçüncü Taraf Asset'ler

- **Beech & Oak ağaç modelleri** — Sketchfab
- **Trafalgar Square Lamp** — Sketchfab
- **SkySeries Freebie** — Unity Asset Store
- **Gogo Casual Pack** — Unity Asset Store
- **Easy-Population** — Unity Asset Store

---

## 🔗 Bağlantılar

- **GitHub:** [github.com/mehmettalhakaya/sanalsehirkeyfi](https://github.com/mehmettalhakaya/sanalsehirkeyfi)
- **Releases:** [v1.0.0 Final Build](https://github.com/mehmettalhakaya/sanalsehirkeyfi/releases/latest)
- **Proje Akışı:** [projeakisi.md](projeakisi.md)
- **API Sunucusu:** [mtkaya.me](https://mtkaya.me)

---

> ⭐ Bu proje **Fırat Üniversitesi Yazılım Mühendisliği Temelleri (YMT1104)** dersi kapsamında geliştirilmiş, **2025–2026 Bahar Yarıyılı** sonunda tamamlanmıştır.
