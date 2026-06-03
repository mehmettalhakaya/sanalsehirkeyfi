# 📊 Proje Akışı

Bu dosya, **"Sanal Gerçeklik Simülatörleri – Sanal Şehir Keşfi"** takımının haftalık proje ilerlemesini ve üyelerin görev dağılımlarını içermektedir.

> 🗓️ **Proje Başlangıç Tarihi:** 20 Nisan 2026
> 📦 **Teslim Tarihi:** 02 Haziran 2026
> 🏛️ **Konu:** Louvre Müzesi – Sanal Gerçeklik Keşfi

---

# 📅 1. Hafta (20 – 26 Nisan 2026)

> 🎯 **Aşama:** Proje Tanıma ve Başlangıç

## 🔎 Mehmet Talha Kaya — Teknoloji Araştırması ve Değerlendirme
Projede kullanılabilecek VR teknolojileri, yazılım araçları ve hareket izleme sistemleri araştırıldı. Unity, C#, Blender ve Oculus SDK incelendi.

## 🛠️ Melike Gücin — Proje Yönetimi ve İşbirliği Araçları Kurulumu
Proje yönetimi için Jira kuruldu. Ekip üyelerinin erişimi sağlandı, görev panosu hazırlandı.

## ⚙️ Cemre Yurtsever — Geliştirme Ortamı Kurulumu
Unity 6.3 LTS, Visual Studio, Blender ve Oculus SDK kuruldu. Geliştirme ortamı yapılandırması tamamlandı.

## 📋 Mustafa Murat Hilaloğlu — Gereksinim Toplama ve Belgeleme
Fonksiyonel ve fonksiyonel olmayan gereksinimler toplandı. Donanım, yazılım ve performans gereksinimleri belgelendi.

## 📊 Fırat Seçkin — Proje Analizi ve Kapsam Belirleme
Projenin amacı, hedef kitlesi ve kapsamı belirlendi. Hedef kullanıcı grupları (öğrenciler, turistler, engelli bireyler, eğitimciler) tanımlandı.

### ⭐ Ekstra
- 🌐 GitHub reposu oluşturuldu, branch koruma kuralları ayarlandı, ekip collaborator olarak eklendi.
- ⚠️ Risk analizi: VR donanım eksikliği, model performans sorunu, ekip iletişimi için önlem planı hazırlandı.

---

# 📅 2. Hafta (27 Nisan – 3 Mayıs 2026)

> 🎯 **Aşama:** Analiz ve Planlama

## 🔎 Mehmet Talha Kaya — Unity Optimizasyon Stratejileri Araştırması
LOD, occlusion culling, batching, instancing ve mesh compression teknikleri araştırıldı. Optimizasyon raporu hazırlandı.

## 🧪 Melike Gücin — Kullanıcı Test Senaryoları ve Değerlendirme Metrikleri
Test senaryoları oluşturuldu (giriş, hareket, etkileşim, çıkış). Likert ölçeği ve metrik değerlendirme sistemi tanımlandı.

## 🏛️ Cemre Yurtsever — Blender'da Sanal Şehir Modeli Detaylandırma ve Optimizasyon Planı
Louvre modelinin Blender'da detaylandırma planı yapıldı. Poligon azaltma ve doku atlas stratejisi belirlendi.

## 💾 Mustafa Murat Hilaloğlu — Sanal Şehir Veri Entegrasyonu ve Yönetim Planı
Veritabanı şeması taslağı çıkarıldı. Eser, kategori, kullanıcı, ziyaret tabloları planlandı. JSON tabanlı veri akışı tasarlandı.

## 🎮 Fırat Seçkin — VR Etkileşimleri ve Kullanıcı Deneyimi (UX) Analizi
VR etkileşim kalıpları (teleport, grab, raycast pointer) araştırıldı. UX prensipleri ve motion sickness önleme yöntemleri listelendi.

### ⭐ Ekstra
- 🏗️ Yazılım mimarisi taslağı çıkarıldı (Unity Client → REST API → MySQL).
- 🎨 UI/UX wireframe yapıları (ana giriş, müze tanıtım, sanal gezi, bilgi paneli) Figma'da taslak olarak çizildi.

---

# 📅 3. Hafta (4 – 10 Mayıs 2026)

> 🎯 **Aşama:** Tasarım

## 🥽 Mehmet Talha Kaya — Oculus Entegrasyonu ve Performans Optimizasyonu Planlaması
XR Plugin Management ile Oculus desteği planlandı. 72/90 FPS performans hedefleri ve optimizasyon ayarları belirlendi.

## 🗄️ Melike Gücin — Şehir Veri Entegrasyonu ve Yönetim Sistemi Tasarımı
REST API + MySQL mimarisi tasarlandı. Node.js + Express + bcrypt + JWT yapısı planlandı.

## 🎮 Cemre Yurtsever — VR Etkileşim Mekaniklerinin Tasarımı
Yürüme, teleport, snap turn ve raycast tıklama mekanikleri tasarlandı. VR kontrolcü tuş eşleştirmesi yapıldı.

## 🎵 Mustafa Murat Hilaloğlu — Ses Peyzajı ve Ambiyans Tasarımı
Avlu ve iç galeri için ambient ses tasarımı yapıldı (kuş sesi, fısıltı, ayak sesleri). 3D spatial audio ayarları belirlendi.

## 🎨 Fırat Seçkin — Etkileşimli Menü Tasarımı ve Prototipleme
Figma ile ana menü, ayarlar, login/register panelleri prototipleri hazırlandı. Renk paleti ve tipografi standartları belirlendi.

### ⭐ Ekstra
- 🌅 **Procedural gökyüzü + güneş sistemi** tasarlandı (panoramik gradient + güneş diski + bulutlar + Directional Light).
- 🪨 **PBR taş döşeme planı** (avlu zemini için procedural texture + normal map).
- 🌳 **Procedural peyzaj sistemi** taslağı: çim, ağaç, korkuluk, lamba, gravel path, taş bordür.

---

# 📅 4. Hafta (11 – 17 Mayıs 2026)

> 🎯 **Aşama:** Geliştirme (Temel)

## 🏗️ Mehmet Talha Kaya — Tarihi Bina 3D Modellerinin Blender'da Oluşturulması
Louvre Müzesi dış cephesi, avlu ve piramit Blender'da modellendi. FBX olarak Unity'ye aktarıldı.

## ℹ️ Melike Gücin — Unity'de Etkileşimli Bilgi Paneli Sistemi Geliştirilmesi
InfoPanelController scripti yazıldı. Tabloya tıklayınca açılan 600px geniş bilgi paneli geliştirildi.

## 🥽 Cemre Yurtsever — Oculus SDK ile VR Hareket Sistemi Entegrasyonu
XR Origin oluşturuldu. Continuous move, snap turn ve teleport entegre edildi.

## 🖼️ Mustafa Murat Hilaloğlu — Tarihi Mekânlar İçin Etkileşimli Bilgi Paneli Sistemi Geliştirilmesi
InteractableObject scripti yazıldı. Raycast tıklama → bilgi panel akışı kuruldu, test edildi.

## 🎯 Fırat Seçkin — Tarihi Mekânlar İçin Etkileşimli Bilgi Paneli Sisteminin Unity'de Geliştirilmesi
TextMeshPro ile responsive panel UI uygulandı. Image container ve scroll view eklendi.

### ⭐ Ekstra
- 🌳 **LouvreLandscapeBuilder** editor scripti: 53 kayın + 15 meşe ağacı procedural yerleştirme, materyal otomasyon, scale/rotation varyasyonu.
- 🌅 **LouvreSkyAndSun** scripti: 1024×512 panoramik gökyüzü texture procedural üretimi + Directional Light + Ambient + Fog.
- 🪨 **PavementUpgrader** editor scripti: avlu zeminini otomatik olarak PBR taş döşemeye çevirir.
- 🚪 **PyramidPortal & ReturnPortal**: piramide yaklaşma prompt'u + iç sahneye geçiş + dönüş sistemi.
- 🎬 **Sahne ayrımı**: MainMenu, Outside_Museum, LouvreInteriorOptimized olarak 3 farklı sahne tasarımı.

---

# 📅 5. Hafta (18 – 24 Mayıs 2026)

> 🎯 **Aşama:** Geliştirme (İleri) ve Test

## 🎵 Mehmet Talha Kaya — Ses Efektlerinin Entegrasyonu ve Optimizasyonu
Sahne ambient sesleri Unity'ye entegre edildi. 3D Spatial Blend ve Audio Mixer ayarları yapıldı.

## 🔐 Melike Gücin — Kullanıcı Arayüzü (UI) Tasarımı ve Uygulaması
LoginManager + LoginUIBootstrap geliştirildi. Login/Register panelleri TextMeshPro ile tasarlandı, API'ye bağlandı.

## ⚙️ Cemre Yurtsever — Çarpışma Algılama ve Fizik Motoru Optimizasyonu
CharacterController fizik ayarları yapıldı. Görünmez sınır duvarları ve FallProtection scripti eklendi.

## 🧭 Mustafa Murat Hilaloğlu — VR Ortamında Navigasyon ve Hareket Mekanizmalarının İyileştirilmesi
Snap turn açısı optimize edildi. Comfort mode (vignette) eklendi, motion sickness azaltıldı.

## 🎯 Fırat Seçkin — VR Ortamında Etkileşimli Nesnelerin Geliştirilmesi
LouvreDoor, EToInteract, Billboard sistemleri geliştirildi. Kapı geçişi ve etkileşim prompt'ları eklendi.

### ⭐ Ekstra
- 🌐 **Node.js + Express REST API** kendi sunucumuzda kuruldu (mtkaya.me). `bcrypt` + `JWT` + `helmet` + `express-rate-limit` ile güvenlik sağlandı.
- 🗄️ **MySQL users tablosu** oluşturuldu (`username`, `email`, `password_hash`, `created_at`, `last_login`).
- 🐦 **BirdFlock sistemi**: waypoint döngüsü ile uçan kuş sürüleri + kanat çırpma animasyonu (sin dalgası, ExecuteAlways ile editor preview).
- 🍂 **FallingLeaves_Particle**: dış sahneye atmosferik düşen yaprak efekti.
- 💡 **Trafalgar Square tarzı OBJ sokak lambaları** import edildi ve sahneye yerleştirildi.
- 🔄 **LouvreTextureRebinder**: sahne her yüklendiğinde lost texture sorunu otomatik fix.
- 🛡️ **FallProtection + InvisibleWall colliders**: 4 yönlü görünmez sınır + tablolar arasından düşme engelleme.
- 🎨 **URP'ye geçiş**: Built-in Render Pipeline → Universal Render Pipeline 17.3 + lightmap baked aydınlatma.
- 🎮 **PlayerController (yeni Input System)**: WASD + Shift sprint + Space jump + F uçma modu + mouse look.
- 📍 **TeleportController**: sağ tık basılı tut → zemin marker → bırakınca ışınlanma.
- ❓ **NavigationHUD**: H tuşu ile açılıp kapanan kontrol yardım overlay'i.
- ⚙️ **SettingsMenuController**: ESC ile açılan ayarlar menüsü (ses, grafik kalitesi, mouse hassasiyeti).

---

# 📅 6. Hafta (25 Mayıs – 1 Haziran 2026)

> 🎯 **Aşama:** Entegrasyon, Toparlama ve Sunum

## 📑 Mehmet Talha Kaya — Final Sunum Hazırlığı
Sunum slaytları, demo video ve final raporu hazırlandı. Jüri sorularına hazırlık yapıldı.

## ✨ Melike Gücin — VR Simülasyonunun Optimizasyonu ve Cilalanması
Görsel hatalar (texture lost, z-fighting) giderildi. LouvreTextureRebinder eklendi, lightmap baked.

## 📚 Cemre Yurtsever — Proje Dokümantasyonunun Tamamlanması
README, mimari tasarım, ER diyagramı, kullanım kılavuzu dokümanları tamamlandı ve GitHub'a yüklendi.

## 🧪 Mustafa Murat Hilaloğlu — VR Simülasyonu Kullanıcı Testleri ve Geri Bildirim Toplama
5 katılımcı ile kullanıcı testleri yapıldı. Likert ölçeği ile geri bildirim toplandı, sonuçlar raporlandı.

## 🔧 Fırat Seçkin — Proje Entegrasyonu ve Hata Ayıklama
Unity + backend + veritabanı entegrasyonu test edildi. Bulunan hatalar düzeltildi, final build alındı.

### ⭐ Ekstra
- 🏗️ **İki sürüm yapılandırması**: Desktop (WASD + Mouse) ve VR (Oculus Quest) sürümleri ayrı projeler olarak hazırlandı.
- 📂 **GitHub monorepo yapısı**: `ProjectFiles/Desktop/` + `ProjectFiles/VR/` ile tek repoda iki sürüm.
- 📦 **Git LFS** entegre edildi (büyük FBX / texture / audio dosyaları için).
- 📊 **Kapsamlı proje dokümantasyonu (.docx, ~25 sayfa)**: teknik mimari, script tablosu, kullanım kılavuzu, sorun giderme.
- 🎬 **2 dakikalık demo videosu**: MainMenu → giriş → avlu → iç galeri → tablo etkileşimi → çıkış.
- 🌐 **Backend deployment**: Ubuntu VPS + Nginx + PM2 ile API canlıya alındı.
- 🔐 **Güvenlik notları**: `.env` dosyaları ASLA repo'ya yüklenmedi, `.env.example` ile şablon paylaşıldı.
- 🎁 **GitHub Releases**: hazır `.exe` (Desktop) ve `.apk` (Quest) build'leri yayınlandı.

---

# 🚀 Proje Durumu

✅ **TAMAMLANDI**

> 🎉 Tüm haftalık görevler başarıyla tamamlanmış, ek modüller (login sistemi, peyzaj, kuş animasyonu, VR navigasyon, post-processing) entegre edilmiş, kullanıcı testleri yapılmış ve final sürümü teslim edilmiştir.

### 📊 Genel İstatistikler
| Metrik | Değer |
|--------|-------|
| 📅 Toplam Süre | 6 Hafta |
| 📜 C# Script | 23 runtime + 9 editor |
| 🎬 Sahne | 3 (MainMenu, Outside_Museum, LouvreInteriorOptimized) |
| 🌳 Ağaç (sahnede) | 68 (53 Beech + 15 Oak) |
| 🐦 Uçan kuş sürüsü | 6–8 |
| 🖼️ Etkileşimli tablo | ~25 |
| 🔐 Backend endpoint | 2 (register, login) |
| 📦 Proje sürümü | 2 (Desktop + VR) |
| ⭐ Kullanıcı memnuniyeti | 4.6 / 5 |
| 🎯 FPS performansı | Stable 60+ (1080p) |

---

> 📌 GitHub: [github.com/mehmettalhakaya/sanalsehirkeyfi](https://github.com/mehmettalhakaya/sanalsehirkeyfi)
