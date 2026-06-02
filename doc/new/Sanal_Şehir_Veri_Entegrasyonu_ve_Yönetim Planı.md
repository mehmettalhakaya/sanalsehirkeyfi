# SANAL ŞEHİR MODELLEMESİ
## Çok Kaynaklı Veri Entegrasyonu, Optimizasyon ve Veri Yönetim Planı

**Hazırlayan:** Mustafa Murat Hilaloğlu

**Özet:**  
Bu doküman; 3D modelleme yazılımları, CBS (Coğrafi Bilgi Sistemleri) harita verileri ve tarihi kayıtlar gibi heterojen kaynaklardan gelen verilerin entegrasyonunu, veri formatlarının karşılaştırılmasını, büyük ölçekli sahneler için optimizasyon stratejilerini ve veri tabanı/CMS tabanlı yönetim prosedürlerini içeren kapsamlı bir Veri Yönetim Planı (VYP) sunmaktadır. 

---

# 1. Çok Kaynaklı Veri Entegrasyon Planı

Büyük ölçekli bir sanal şehir modeli, doğası gereği tek bir kaynaktan beslenemez. Şehrin hem bugünkü fiziksel yapısını, hem mühendislik detaylarını hem de tarihi/kültürel mirasını yansıtmak amacıyla üç temel ana kaynaktan veri entegrasyonu planlanmıştır.

## 1.1 3D Modelleme Yazılımları

**Blender, 3ds Max, Maya**

- Sanatsal ve mimari detay seviyesi yüksek yapılar
- Tarihi binalar
- Özel şehir mobilyaları

Bu kaynaklardan elde edilen modeller genellikle yüksek poligon sayısına ve karmaşık PBR doku yapılarına sahiptir. 

## 1.2 Harita ve CBS Verileri

**CBS / GIS Verileri**

- Topografya
- Yol ağları
- Bina taban alanları (Footprints)
- Bitki örtüsü verileri

Kullanılan veri türleri:

- Shapefile
- GeoJSON
- LiDAR Nokta Bulutları

Bu veriler gerçek dünya koordinat sistemlerine (WGS84, UTM vb.) bağlıdır.

## 1.3 Tarihi Kayıtlar ve Arşiv Belgeleri

Kullanılan kaynaklar:

- Eski haritalar
- Kadastro kayıtları
- Fotoğraflar
- Yazılı tasvirler

Bu veriler model üretiminden önce referans katmanı olarak entegrasyon hattına dahil edilir. 

## 1.4 Entegrasyon Hattı ve Koordinat Senkronizasyonu

Farklı kaynaklardan gelen verilerin aynı koordinat sisteminde çalışabilmesi için **Ortak Merkezli Yerel Koordinat Sistemi (Local Origin System)** kullanılacaktır. 

Yerel koordinat dönüşümü:

```text
X_yerel = X_gerçek − X_merkez
Y_yerel = Y_gerçek − Y_merkez
```

Bu yaklaşım Unity ve Unreal Engine gibi motorlarda oluşabilecek kayan nokta hassasiyeti problemlerini azaltır. 

---

# 2. Veri Formatları Analizi

Projeye dahil edilecek 3D varlıkların saklama ve aktarım formatları aşağıdaki şekilde değerlendirilmiştir. 

| Format | Geliştirici | Avantajlar | Dezavantajlar |
|----------|------------|------------|--------------|
| FBX (.fbx) | Autodesk | Animasyon, rigging, kamera ve ışık aktarımı güçlü | Tescilli yapı, büyük dosya boyutu |
| OBJ (.obj) | Wavefront | Basit ve evrensel format | Animasyon desteği yok, büyük dosya boyutları |
| glTF / GLB | Khronos Group | Açık kaynak, PBR desteği, hızlı yükleme | Üretim aşamasında düzenleme için uygun değil |



## Format Kararı

### Üretim Aşaması

```text
FBX
```

### Nihai Dağıtım Aşaması

```text
glTF / GLB
```

Tercih edilmiştir. Bunun nedeni glTF/GLB formatının yüksek sıkıştırma ve hızlı yüklenme avantajıdır. 

---

# 3. Veri Optimizasyonu Stratejileri

Büyük ölçekli şehir modelleri milyonlarca poligon ve yüksek miktarda doku verisi içerebilir. Bu nedenle performansı korumak amacıyla çeşitli optimizasyon teknikleri uygulanacaktır. 
## 3.1 Mesh Birleştirme (Batching / Combining)

Amaç:

- Draw Call sayısını azaltmak
- CPU darboğazını önlemek

Aynı materyali kullanan statik nesneler tek bir mesh altında birleştirilerek **Static Batching** uygulanacaktır. 

---

## 3.2 LOD (Level of Detail)

Kameraya olan uzaklığa göre farklı detay seviyeleri kullanılacaktır. 

### LOD 0 — Yakın Plan

- Mesafe: 0 – 50 m
- Yaklaşık 50.000 poligon

### LOD 1 — Orta Plan

- Mesafe: 50 – 200 m
- Yaklaşık 10.000 poligon

### LOD 2 — Uzak Plan

- Mesafe: 200 – 500 m
- Yaklaşık 1.000 poligon

### LOD 3 — Ufuk Çizgisi

- Mesafe: 500+ m
- Billboard / Impostor
- Yaklaşık 2 poligon

---

## 3.3 Frustum ve Occlusion Culling

### Frustum Culling

Kameranın görüş alanı dışındaki nesneler çizilmez.

### Occlusion Culling

Başka nesneler tarafından tamamen gizlenen nesneler işlenmez.

Bu yöntemler VRAM kullanımını ve render maliyetini azaltır. 

---

# 4. Veri Tabanı, CMS ve Güncelleme Süreçleri

## 4.1 Veri Katmanı

### PostgreSQL + PostGIS

Özellikler:

- Uzamsal sorgulama
- Coğrafi veri yönetimi
- 2D ve 3D CBS desteği



---

## 4.2 İçerik Yönetim Sistemi (CMS)

CMS üzerinden:

- Yeni 3D modeller yüklenebilir
- Öznitelik verileri güncellenebilir
- Versiyon kontrolü yapılabilir



---

## 4.3 Veri Güncelleme Yaşam Döngüsü

1. Yeni veri/model üretimi
2. Taslak olarak CMS'e yükleme
3. Otomatik optimizasyon
4. PostGIS doğrulaması
5. Yönetici onayı
6. Canlı sisteme aktarım
7. Streaming ile kullanıcılara servis



---

# 5. Veri Güvenliği, Gizlilik ve Yönetim Planı

Sanal şehir modelleri;

- Kritik altyapılar
- Askeri bölgeler
- Kişisel veriler

içerebildiğinden KVKK ve GDPR uyumluluğu gerektirir. 
## Güvenlik Tedbirleri

### Kritik Altyapı Güvenliği

Risk:

- Hassas tesislerin hedef haline gelmesi

Önlem:

- Kamu sürümünde düşük detay seviyesi
- Hassas katmanların kaldırılması
- Yetkili erişim



---

### Kişisel Gizlilik

Risk:

- Yüz ve plaka bilgileri
- Mülkiyet verileri

Önlem:

- Yapay zekâ ile bulanıklaştırma
- Anonimleştirme..



---

### Veri Bütünlüğü ve Erişim Kontrolü

Risk:

- Yetkisiz değişiklikler
- Veri sabotajı

Önlem:

- Rol Tabanlı Erişim Kontrolü (RBAC)
- TLS 1.3 şifreleme
- Günlük artımlı yedekleme



---

# Sonuç ve Taahhüt

Bu Veri Yönetim Planı;

- Ölçeklenebilirlik
- Performans
- Güvenlik

hedeflerini desteklemektedir.

Kurulan entegrasyon hattı, LOD yapıları ve optimizasyon mekanizmaları sayesinde sistem donanım kaynaklarını verimli kullanarak sürdürülebilir ve kesintisiz bir kullanıcı deneyimi sunacaktır.