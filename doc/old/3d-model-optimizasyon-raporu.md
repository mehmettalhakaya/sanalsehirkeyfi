# 🎮 3D Model Optimizasyon Teknikleri Araştırması ve Raporlama

**👤 Sorumlu:** Mehmet Talha Kaya  

---

## 📖 Giriş

Sanal Şehir Keşfi projesi, kullanıcıların tarihi ve kültürel mekanları sanal gerçeklik (VR) ortamında keşfetmesini amaçlayan bir simülasyon uygulamasıdır. VR uygulamalarında yüksek kare hızı (72–90 FPS) ve düşük gecikme süresi (20 ms altı) sağlamak, kullanıcı konforunu ve deneyim kalitesini doğrudan etkileyen kritik faktörlerdir.

Bu rapor, Unity ortamında 3D modellerin optimize edilmesi için kullanılan temel teknikleri araştırmakta, her tekniğin avantaj ve dezavantajlarını karşılaştırmakta ve projemiz için en uygun optimizasyon stratejilerini önermektedir.

---

## 🎯 Proje Bağlamı ve Performans Hedefleri

Projemiz **Unity + C# + Blender + Oculus SDK** teknolojileri üzerine inşa edilmektedir. Hedef platform olarak **Meta Quest (Oculus Quest)** serisi başlıklar seçilmiştir. Bu cihazlar mobil GPU mimarisine sahip olduğundan, optimizasyon özellikle önemlidir.

### 📊 Performans Hedeflerimiz

| Metrik | Hedef Değer | Açıklama |
| --- | --- | --- |
| **FPS** | 72–90 FPS | VR hareket hastalığını önlemek için minimum |
| **Gecikme** | < 20 ms | Baş hareketi ile görüntü senkronizasyonu |
| **Draw Call** | < 100 / göz | Mobil VR GPU sınırlamaları |
| **Poligon Sayısı** | < 100K tri / sahne | Quest işlemci kapasitesine uygun |

---

## 🛠️ 3D Model Optimizasyon Teknikleri

### 1️⃣ Poligon Azaltma (Polygon Reduction)

Poligon azaltma, 3D modellerdeki üçgen (triangle) ve köşe (vertex) sayısını düşürerek GPU yükünü hafifletmeyi amaçlar. Bu, özellikle mobil VR cihazlarında en temel optimizasyon adımıdır.

#### 🔹 Blender Decimate Modifier

Blender'ın Decimate Modifier aracı, modellerin poligon sayısını otomatik olarak azaltır. Üç farklı modu vardır:

| Mod | Açıklama | Kullanım Alanı |
| --- | --- | --- |
| **Collapse** | Köşeleri birleştirerek poligon sayısını azaltır | Genel amaçlı, en yaygın kullanım |
| **Un-Subdivide** | Alt bölümleme işlemini tersine çevirir | Subdivide ile oluşturulmuş modeller |
| **Planar** | Düz yüzeylerdeki gereksiz poligonları kaldırır | Mimari modeller, düz yüzeyler |

📌 Ratio değeri 1.0'dan 0'a doğru azaltıldıkça daha fazla poligon kaldırılır.

#### 🔹 Manuel Retopoloji

Yüksek poligonlu modelin üzerine elle yeni, düşük poligonlu bir mesh oluşturma tekniğidir. En kaliteli sonuçları verir ancak zaman alıcıdır. Oyun endüstrisinde standart yaklaşımdır.

#### 🔹 Temel Kurallar

* Modellerde görünmeyen yüzeyleri (kamera açısından asla görülmeyecek olan) kaldırın.
* Düz alanlarda poligon sayısını azaltın, detayı silüet kısımlarında koruyun.
* UV dikişleri ve sert kenarları mümkün olduğunca az tutun (bunlar vertex sayısını artırır).
* Küçük detaylar için normal map ve texture kullanın, geometri yerine.

---

### 2️⃣ Level of Detail (LOD) – Detay Seviyesi Sistemi

LOD sistemi, bir nesnenin kameraya olan uzaklığına göre farklı detay seviyelerinde modellerle değiştirilmesini sağlar. Unity'de **LOD Group** bileşeni ile uygulanır. Yakındaki nesneler yüksek detaylı, uzaktaki nesneler düşük detaylı gösterilir; böylece toplam poligon sayısı önemli ölçüde düşer.

#### 🔹 LOD Seviyeleri

| Seviye | Mesafe | Poligon Oranı | Kullanım Alanı |
| --- | --- | --- | --- |
| **LOD 0** | Yakın (0–10m) | %100 (orijinal) | Etkileşime geçilen nesneler |
| **LOD 1** | Orta (10–30m) | %50 | Arka plan yapıları |
| **LOD 2** | Uzak (30–60m) | %25 | Uzak görünüm |
| **Culled** | Çok uzak (60m+) | %0 (gösterilmez) | Render edilmez |

📌 LOD modellerini Blender'da Decimate Modifier ile farklı ratio değerleriyle oluşturabilir, ardından Unity'ye import edip LOD Group bileşenine atayabilirsiniz. Unity 8 adede kadar LOD seviyesi destekler, ancak çoğu proje için **2–3 seviye** yeterlidir.

---

### 3️⃣ Occlusion Culling (Engelleme Ayıklama)

Occlusion Culling, kamera tarafından görülmeyen (başka nesnelerin arkasında kalan) nesnelerin render edilmesini engelleyen bir tekniktir. Unity'de sahne verileri önceden **bake** edilir ve çalışma zamanında bu veriler kullanılarak hangi nesnelerin görünür olduğu belirlenir.

#### 🔹 Nasıl Çalışır

1. Duvarlar, binalar gibi büyük statik nesneler **"Occluder Static"** olarak işaretlenir.
2. Gizlenebilecek küçük nesneler **"Occludee Static"** olarak işaretlenir.
3. **Window > Rendering > Occlusion Culling** menüsünden sahne bake edilir.
4. Kamera görüş alanı dışındaki nesneler otomatik olarak devre dışı bırakılır.

📌 **Önemli:** Bu teknik özellikle sanal şehir ortamımız gibi duvarlar ve binalarla ayrılmış bölgeler içeren sahnelerde çok etkilidir. Quest 2 üzerinde doğru uygulama ile kare hızında **%100'ün üzerinde artış** sağlanabilmektedir.

---

### 4️⃣ Texture (Doku) Optimizasyonu

Texture'lar GPU bellek tüketiminin en büyük kaynağı olabilir. Yüksek çözünürlüklü texture'lar VRAM'ı hızla tüketir ve performans düşüşlerine yol açar.

#### 🔹 Texture Sıkıştırma

| Format | Açıklama | Önerilen Kullanım |
| --- | --- | --- |
| **ASTC** | Adaptive Scalable Texture Compression. Esnek kalite/boyut dengesi | Mobil ve VR platformlar için birincil tercih |
| **Crunch Compression** | Unity'de ek sıkıştırma, dosya boyutunu küçültür | Depolama alanı kritik olduğunda |

#### 🔹 Mipmap Kullanımı

Mipmap'lar, texture'ın farklı çözünürlüklerde önceden hesaplanmış versiyonlarıdır. Uzaktaki nesneler düşük çözünürlüklü versiyonu kullanır, bu da bellek bant genişliğini azaltır.

* ✅ 3D nesnelerde mipmap **açık** olmalı
* ❌ Sabit boyuttaki UI elemanlarında mipmap **kapalı** olmalı

#### 🔹 Texture Atlas

Birden fazla küçük texture'ı tek bir büyük texture'a birleştirmek, materyal sayısını ve dolayısıyla draw call sayısını azaltır. Özellikle aynı materyali paylaşan statik nesnelerin batch edilmesi için önemlidir.

#### 🔹 VR'de Texture Farkı: Height Map vs Normal Map

Normal map'lar tek bir bakış açısından hesaplandığı için stereoskopik VR lenslerinde doğru görünmeyebilir. VR projelerinde **height map** (yükseklik haritası) kullanmak daha gerçekçi sonuçlar verir, ancak daha fazla işlem gücü gerektirir. Bu nedenle projenin ihtiyaçlarına göre dengeli bir karar verilmelidir.

---

### 5️⃣ Draw Call Optimizasyonu

Her draw call, CPU'dan GPU'ya yapılan bir render talimatıdır. Fazla draw call CPU darboğazına neden olur. VR'de her göz için ayrı render yapıldığından bu sayı iki katına çıkar.

#### 🔹 Static Batching

Hareket etmeyen nesneleri Inspector'da **"Static"** olarak işaretlemek, Unity'nin bunları tek bir mesh olarak birleştirip tek draw call ile çizmesini sağlar. Tüm nesnelerin aynı materyali kullanması gerekir. Sanal şehrimizdeki duvarlar, binalar, zemin gibi öğeler için idealdir.

#### 🔹 Dynamic Batching

Hareket eden küçük nesneler (900 vertex altı) için Unity otomatik olarak dynamic batching uygular. Aynı materyali kullanan dinamik nesnelerin birleştirilmesini sağlar ancak CPU maliyeti vardır.

#### 🔹 Materyal Birleştirme

Farklı materyaller yerine texture atlas kullanılarak tek bir materyal oluşturulabilir. Her ek materyal slotu bir draw call daha demektir. Mümkün olduğunca az materyal kullanılmalıdır.

---

### 6️⃣ Işık Optimizasyonu (Light Baking)

Gerçek zamanlı aydınlatma hesaplamaları GPU'ya büyük yük bindirir. **Lightmap baking**, ışık ve gölgelerin önceden hesalanıp texture olarak kaydedilmesidir. Çalışma zamanında ekstra hesaplama gerekmez. Statik sahnelerde (tarihi yapılar, müze içleri) en etkili yöntemdir.

* **Baked Lighting:** Önceden hesaplanır, çalışma zamanında maliyet sıfır. VR için önerilir.
* **Light Probe:** Baked ışıklandırma kullanılırken hareketli nesnelerin doğru aydınlanmasını sağlar.
* **Specular Baking:** Yansıma (specular) detaylarını texture'a işleyerek shader maliyetini azaltır.

---

### 7️⃣ VR'ye Özel Optimizasyon Teknikleri

#### 🔹 Single Pass Instanced Rendering

VR'de iki göz için sahne iki kez render edilir. Single Pass Instanced yöntemi, her iki gözü tek bir geçişte render ederek CPU yükünü önemli ölçüde azaltır. Multi-pass'a göre çok daha verimlidir.

#### 🔹 URP (Universal Render Pipeline) Kullanımı

Unity'nin URP'si, mobil ve VR platformlar için optimize edilmiş bir render pipeline'dır. Daha basit shader'lar ve daha az GPU kullanımı sağlar. Built-in Render Pipeline yerine **URP tercih edilmelidir.**

#### 🔹 Anti-Aliasing (4x MSAA)

VR'de kenar pürüzlülüğü (aliasing) çok belirgin olur. **4x MSAA** iyi bir performans/kalite dengesi sağlar. HDR kapatılmalı ve render scale 1 olarak ayarlanmalıdır.

---

## 📊 Tekniklerin Karşılaştırmalı Analizi

| Teknik | Avantajlar | Dezavantajlar | Projemize Uygunluğu |
| --- | --- | --- | --- |
| **Poligon Azaltma** | Doğrudan GPU yükünü azaltır. Kolay uygulanır. | Aşırı azaltma görsel kaliteyi bozar. | ✅ ÇOK YÜKSEK – Tüm 3D modellere uygulanmalı |
| **LOD Sistemi** | Uzak nesnelerde büyük performans kazancı. Otomatik geçiş. | Her model için birden fazla versiyon gerektirir. | ✅ ÇOK YÜKSEK – Şehir sahnesinde zorunlu |
| **Occlusion Culling** | Çizim sayısını ciddi biçimde düşürür. Bina/duvar sahneleri için ideal. | Bake süresi gerektirir. Dinamik sahnelerde etkisiz. | ✅ ÇOK YÜKSEK – Sanal şehir için mükemmel uyum |
| **Texture Sıkıştırma** | VRAM kullanımını önemli ölçüde azaltır. | Aşırı sıkıştırma görsel artefaktlara yol açabilir. | ✅ YÜKSEK – ASTC formatı ile uygulanmalı |
| **Static Batching** | Draw call sayısını drastik azaltır. | Bellek tüketimini artırır. Sadece statik nesnelere uygulanır. | ✅ YÜKSEK – Bina ve duvarlar için ideal |
| **Light Baking** | Çalışma zamanında ışık maliyeti sıfır. Görsel kalite yüksek. | Bake süresi uzun. Dinamik ışıklar için ek çözüm gerekir. | ✅ YÜKSEK – Müze ve tarihi yapılar için ideal |
| **Single Pass Instanced** | VR render maliyetini neredeyse yarıya indirir. | Tüm shader'lar desteklemeyebilir. | ✅ YÜKSEK – Oculus Quest desteği ile uygulanmalı |

---

## 🚀 Sanal Şehir Keşfi İçin Önerilen Optimizasyon Stratejisi

Projemizin sanal şehir ortamı yapısı, VR hedef platformu ve ekip kapasitesini göz önünde bulundurarak aşağıdaki çok katmanlı optimizasyon stratejisi önerilmektedir:

### 🔹 Öncelik 1: Model Hazırlık Aşaması (Blender)

1. Tüm 3D modelleri Blender'da **Decimate Modifier** ile optimize edin. Tarihi yapılar için ratio değerini **0.3–0.5** arasında tutun.
2. Her ana yapı için **2–3 LOD seviyesi** oluşturun: LOD0 (orijinal), LOD1 (ratio 0.5), LOD2 (ratio 0.2).
3. Görünmeyen yüzeyleri (yere bakan alt kısımlar, duvar arkası yüzler) elle kaldırın.
4. **Texture atlas** oluşturarak materyal sayısını minimumda tutun.

### 🔹 Öncelik 2: Unity Sahne Ayarları

* Tüm statik nesneleri (duvarlar, binalar, zemin) **"Static"** olarak işaretleyin.
* **Occlusion Culling**'i bake edin – sanal şehrin duvar/bina yapısı buna çok uygundur.
* **Lightmap baking** ile tüm statik aydınlatmayı önceden hesaplayın.
* **URP (Universal Render Pipeline)** kullanın ve **Single Pass Instanced** rendering'i etkinleştirin.
* Texture'larda **ASTC** sıkıştırma formatını seçin, mipmap'ları 3D nesneler için açık tutun.

### 🔹 Öncelik 3: Test ve İterasyon

* **Unity Profiler** ve **Oculus OVR Metrics Tool** ile düzenli performans testleri yapın.
* **Frame Debugger** ile draw call sayılarını, **Stats paneli** ile tri/vert sayılarını takip edin.
* Hedef metriklere ulaşamayana kadar iteratif optimizasyon uygulayın.

---

## ✅ Sonuç

3D model optimizasyonu, VR deneyiminin kalitesini doğrudan belirleyen kritik bir aşamadır. Tek bir teknik tek başına yeterli değildir; en iyi sonuçlar, **poligon azaltma + LOD + occlusion culling + texture optimizasyonu + light baking + draw call azaltma** tekniklerinin birlikte uygulanmasıyla elde edilir.

Sanal Şehir Keşfi projemiz, içerdiği duvarlar, binalar ve ayrı bölgeler ile occlusion culling için ideal bir yapıya sahiptir. LOD sistemi şehir görünümünde uzak yapıların performansını büyük ölçüde iyileştirecektir. Baked lighting ise müze ve tarihi yapı içleri için hem görsel kalite hem performans açısından en doğru tercihtir.

Bu stratejilerin disiplinli bir şekilde uygulanması ile **72–90 FPS** hedefimize ulaşmak ve kullanıcılara konforlu, sürükleyici bir VR deneyimi sunmak mümkündür.

---

## 📚 Kaynaklar

* Unity Documentation – Occlusion Culling (docs.unity3d.com)
* Unity Documentation – Optimizing Graphics Performance (docs.unity3d.com)
* Unity Learn – Optimizing Your VR/AR Experiences (learn.unity.com)
* Unity Learn – VR Optimization: 3.2 Optimization (learn.unity.com)
* Unity – Art Optimization Tips for Mobile (unity.com)
* Unity – Configuring Your Project for Stronger Performance (unity.com)
* Blender Documentation – Decimate Modifier (docs.blender.org)
* Intel – Unity Software Performance Optimizations Best Practices (intel.com)
